#!/usr/bin/env python3
"""
BMAD-METHOD Utility Script: validate_docs_consistency.py

Audits Markdown/HTML documentation for enterprise documentation consistency.

Checks covered:
- Broken local Markdown/HTML links and anchors.
- Prohibited emoji/pictograph characters in Markdown files (R-14).
- Common mojibake/encoding artifacts (R-03).
- Obsolete or forbidden stack references in docs (R-16/R-20).
- Missing bilingual counterpart files for common English/Spanish doc patterns (R-01).

The script is read-only and exits non-zero when issues are found.
"""

from __future__ import annotations

import argparse
import os
import re
import sys
import unicodedata
from dataclasses import dataclass
from pathlib import Path
from urllib.parse import unquote, urlparse

REPO_ROOT = Path(__file__).resolve().parents[2]
DEFAULT_TARGETS = [REPO_ROOT / "README.md", REPO_ROOT / "docs"]

SKIP_DIRS = {
    ".git",
    ".nx",
    ".venv",
    "bin",
    "build",
    "dist",
    "node_modules",
    "obj",
}

LOCAL_LINK_RE = re.compile(r"(?<!!)\[[^\]]+\]\(([^)]+)\)|(?:href|src)=[\"']([^\"']+)[\"']", re.IGNORECASE)
HEADING_RE = re.compile(r"^(#{1,6})\s+(.+?)\s*$", re.MULTILINE)

MOJIBAKE_TOKENS = (
    "Ã",
    "Â",
    "â€œ",
    "â€",
    "â€™",
    "â€“",
    "â€”",
    "ï»¿",
)

FORBIDDEN_STACK_PATTERNS = [
    (re.compile(r"\.NET\s+8\b", re.IGNORECASE), ".NET 8 reference found. UMS authoritative backend stack is .NET 10."),
    (re.compile(r"SQL\s+Server\s+2019\b", re.IGNORECASE), "SQL Server 2019 reference found. Current UMS persistence baseline is PostgreSQL."),
]

EMOJI_RANGES = [
    (0x1F000, 0x1FAFF),
    (0x2600, 0x27BF),
    (0x1F100, 0x1F1FF),
]
EXTRA_EMOJI_CODEPOINTS = {0x2B50, 0x2B55, 0x2B1B, 0x2B1C, 0xFE0F, 0xFE0E, 0x200D, 0x200B, 0x200C, 0x2060, 0xFEFF}


@dataclass(frozen=True)
class Issue:
    severity: str
    rule: str
    path: Path
    line: int
    issue_type: str
    message: str
    recommendation: str


def iter_files(targets: list[Path]) -> list[Path]:
    files: list[Path] = []
    for target in targets:
        if not target.exists():
            continue
        if target.is_file() and target.suffix.lower() in {".md", ".html"}:
            files.append(target)
            continue
        if target.is_dir():
            for root, dirs, names in os.walk(target):
                dirs[:] = [d for d in dirs if d not in SKIP_DIRS]
                for name in names:
                    path = Path(root) / name
                    if path.suffix.lower() in {".md", ".html"}:
                        files.append(path)
    return sorted(set(files))


def line_number(text: str, index: int) -> int:
    return text.count("\n", 0, index) + 1


def is_emoji(ch: str) -> bool:
    cp = ord(ch)
    if cp in EXTRA_EMOJI_CODEPOINTS:
        return True
    return any(start <= cp <= end for start, end in EMOJI_RANGES)


def slugify_heading(heading: str) -> str:
    heading = re.sub(r"<[^>]+>", "", heading)
    heading = re.sub(r"[`*_~]", "", heading)
    heading = unicodedata.normalize("NFKD", heading)
    heading = "".join(ch for ch in heading if not unicodedata.combining(ch))
    heading = heading.strip().lower()
    heading = re.sub(r"[^a-z0-9\s-]", "", heading)
    heading = re.sub(r"\s+", "-", heading)
    heading = re.sub(r"-+", "-", heading).strip("-")
    return heading


def collect_anchors(text: str) -> set[str]:
    anchors: set[str] = set()
    seen: dict[str, int] = {}
    for match in HEADING_RE.finditer(text):
        base = slugify_heading(match.group(2))
        if not base:
            continue
        count = seen.get(base, 0)
        slug = base if count == 0 else f"{base}-{count}"
        seen[base] = count + 1
        anchors.add(slug)
    anchors.update(re.findall(r"<a\s+(?:[^>]*?\s+)?name=[\"']([^\"']+)[\"']", text, flags=re.IGNORECASE))
    anchors.update(re.findall(r"id=[\"']([^\"']+)[\"']", text, flags=re.IGNORECASE))
    return anchors


def normalize_link(raw: str) -> str:
    raw = raw.strip()
    if raw.startswith("<") and raw.endswith(">"):
        raw = raw[1:-1].strip()
    return raw


def strip_code_blocks(text: str) -> str:
    def preserve_newlines(match: re.Match[str]) -> str:
        return "\n" * match.group(0).count("\n")

    text = re.sub(r"```.*?```", preserve_newlines, text, flags=re.DOTALL)
    text = re.sub(r"<pre\b.*?</pre>", preserve_newlines, text, flags=re.DOTALL | re.IGNORECASE)
    return text


def is_external_or_special(link: str) -> bool:
    parsed = urlparse(link)
    return bool(parsed.scheme in {"http", "https", "mailto", "tel", "data"}) or link.startswith("#")


def validate_links(path: Path, text: str, file_cache: dict[Path, str]) -> list[Issue]:
    issues: list[Issue] = []
    scan_text = strip_code_blocks(text) if path.suffix.lower() == ".md" else text
    for match in LOCAL_LINK_RE.finditer(scan_text):
        link = normalize_link(match.group(1) or match.group(2) or "")
        if not link or is_external_or_special(link):
            continue
        if link.startswith(".") or link.startswith("/") or not urlparse(link).scheme:
            link_path, _, anchor = link.partition("#")
            clean_path = unquote(link_path)
            clean_path = clean_path.split("?", 1)[0]
            target = (path.parent / clean_path).resolve() if clean_path else path.resolve()
            if clean_path and not target.exists():
                issues.append(Issue(
                    "critical",
                    "R-10/R-13",
                    path,
                    line_number(text, match.start()),
                    "broken-link",
                    f"Broken local link: {link}",
                    "Fix the relative path from the current document or remove the stale link.",
                ))
                continue
            if anchor and target.suffix.lower() in {".md", ".html"} and target.exists():
                target_text = file_cache.get(target)
                if target_text is None:
                    target_text = target.read_text(encoding="utf-8", errors="replace")
                    file_cache[target] = target_text
                if anchor and unquote(anchor).lower() not in collect_anchors(target_text):
                    issues.append(Issue(
                        "warning",
                        "R-10/R-13",
                        path,
                        line_number(text, match.start()),
                        "broken-anchor",
                        f"Anchor not found: {link}",
                        "Update the anchor to match the target heading generated by GitHub Markdown.",
                    ))
    return issues


def validate_encoding_and_professionalism(path: Path, text: str) -> list[Issue]:
    issues: list[Issue] = []
    for token in MOJIBAKE_TOKENS:
        index = text.find(token)
        if index >= 0:
            issues.append(Issue(
                "critical",
                "R-03",
                path,
                line_number(text, index),
                "encoding",
                f"Possible mojibake token found: {token}",
                "Run cleanup_markdown_encoding.py and review the affected sentence manually.",
            ))
    if path.suffix.lower() == ".md":
        for index, ch in enumerate(text):
            if is_emoji(ch):
                issues.append(Issue(
                    "warning",
                    "R-14",
                    path,
                    line_number(text, index),
                    "decorative-character",
                    f"Prohibited emoji/decorative character found: U+{ord(ch):04X}",
                    "Remove the emoji/icon and keep the Markdown enterprise-professional.",
                ))
                break
    return issues


def validate_stack(path: Path, text: str) -> list[Issue]:
    issues: list[Issue] = []
    lower_path = str(path).replace("\\", "/").lower()
    if "/docs/" not in lower_path and not lower_path.endswith("readme.md"):
        return issues
    for pattern, message in FORBIDDEN_STACK_PATTERNS:
        for match in pattern.finditer(text):
            context = text[max(0, match.start() - 100): match.end() + 100].lower()
            if "external comparison" in context or "comparación externa" in context:
                continue
            issues.append(Issue(
                "critical",
                "R-16/R-20",
                path,
                line_number(text, match.start()),
                "stack-consistency",
                message,
                "Align the document with .NET 10 + PostgreSQL + EF Core through Npgsql, or mark the reference as legacy context or an explicit external comparison.",
            ))
    return issues


def counterpart_candidates(path: Path) -> list[Path]:
    s = str(path)
    candidates: list[str] = []
    if s.endswith(".es.md"):
        candidates.append(s[:-6] + ".md")
    elif s.endswith(".md"):
        candidates.append(s[:-3] + ".es.md")
    candidates.append(s.replace("/product-es/", "/product/"))
    candidates.append(s.replace("/product/", "/product-es/"))
    candidates.append(s.replace("/project-es/", "/project/"))
    candidates.append(s.replace("/project/", "/project-es/"))
    candidates.append(s.replace("/requirements-es/", "/requirements/"))
    candidates.append(s.replace("/requirements/", "/requirements-es/"))
    candidates.append(s.replace("/testing-es/", "/testing/"))
    candidates.append(s.replace("/testing/", "/testing-es/"))
    candidates.append(s.replace("/architecture-es/", "/architecture/"))
    candidates.append(s.replace("/architecture/", "/architecture-es/"))
    candidates.append(s.replace("/blueprints-es/", "/blueprints/"))
    candidates.append(s.replace("/blueprints/", "/blueprints-es/"))
    candidates.append(s.replace("/domain-es/", "/domain/"))
    candidates.append(s.replace("/domain/", "/domain-es/"))
    candidates.append(s.replace("/sdk-es/", "/sdk/"))
    candidates.append(s.replace("/sdk/", "/sdk-es/"))
    candidates.append(s.replace("/knowledge/articles/", "/knowledge/articles/").replace("/en.md", "/es.md"))
    candidates.append(s.replace("/knowledge/articles/", "/knowledge/articles/").replace("/es.md", "/en.md"))
    candidates.append(re.sub(r"/es-([^/]+)\.md$", r"/\1.md", s))
    candidates.append(re.sub(r"/([^/]+)\.md$", r"/es-\1.md", s))
    return [Path(c) for c in candidates if c != s]


def validate_bilingual(files: list[Path]) -> list[Issue]:
    file_set = {p.resolve() for p in files if p.suffix.lower() == ".md"}
    issues: list[Issue] = []
    for path in file_set:
        rel = path.relative_to(REPO_ROOT) if path.is_relative_to(REPO_ROOT) else path
        rel_s = str(rel).replace("\\", "/")
        if not rel_s.startswith("docs/"):
            continue
        if rel_s.startswith("docs/qa/"):
            continue
        candidates = [c.resolve() for c in counterpart_candidates(path)]
        if not any(c in file_set for c in candidates):
            issues.append(Issue(
                "warning",
                "R-01",
                path,
                1,
                "bilingual-sync",
                "No obvious bilingual counterpart found for this documentation file.",
                "Create or link the English/Spanish counterpart, or document why the file is intentionally single-language.",
            ))
    return issues


def format_issue(issue: Issue) -> str:
    rel = issue.path.relative_to(REPO_ROOT) if issue.path.is_relative_to(REPO_ROOT) else issue.path
    return (
        f"- [{issue.severity.upper()}] {issue.rule} {rel}:{issue.line}\n"
        f"  Type: {issue.issue_type}\n"
        f"  Issue: {issue.message}\n"
        f"  Fix: {issue.recommendation}"
    )


def main() -> int:
    parser = argparse.ArgumentParser(description="Validate UMS documentation consistency.")
    parser.add_argument("paths", nargs="*", help="Files or directories to audit. Defaults to README.md and docs/.")
    parser.add_argument("--no-bilingual", action="store_true", help="Skip bilingual counterpart scan.")
    args = parser.parse_args()

    targets = [Path(p).resolve() for p in args.paths] if args.paths else DEFAULT_TARGETS
    files = iter_files(targets)
    file_cache: dict[Path, str] = {}
    issues: list[Issue] = []

    for path in files:
        text = path.read_text(encoding="utf-8", errors="replace")
        file_cache[path.resolve()] = text
        issues.extend(validate_encoding_and_professionalism(path, text))
        issues.extend(validate_stack(path, text))
        issues.extend(validate_links(path, text, file_cache))

    if not args.no_bilingual:
        issues.extend(validate_bilingual(files))

    if issues:
        print("Documentation consistency audit failed.\n")
        for issue in sorted(issues, key=lambda x: (str(x.path), x.line, x.rule, x.issue_type)):
            print(format_issue(issue))
        print(f"\nTotal issues: {len(issues)}")
        return 1

    print(f"Documentation consistency audit passed. Scanned {len(files)} files.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
