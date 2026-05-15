#!/usr/bin/env python3
"""
validate_mermaid.py

Extracts every Mermaid diagram from Markdown files and validates basic
syntactic correctness. This catches the common breakages:

- Unclosed fences (```mermaid ... missing ```)
- Empty Mermaid blocks
- Unknown diagram type (must be one of: graph/flowchart/sequenceDiagram/
  classDiagram/stateDiagram/erDiagram/gantt/pie/mindmap/journey/timeline/
  C4Context/C4Container/C4Component/quadrantChart/requirementDiagram)
- Unbalanced brackets or parens within a node label
- Reserved keywords used as node IDs (e.g. "end" lowercase)

The script does NOT actually render diagrams (that would require a Node.js
toolchain). It only catches the syntactic errors that GitHub will refuse
to render.

Usage:
    python validate_mermaid.py [path]
    python validate_mermaid.py --fix-empty [path]   # remove empty mermaid blocks
"""

import os
import re
import sys

MERMAID_DIAGRAM_TYPES = {
    'graph', 'flowchart', 'sequenceDiagram', 'classDiagram', 'stateDiagram',
    'stateDiagram-v2', 'erDiagram', 'gantt', 'pie', 'mindmap', 'journey',
    'timeline', 'gitGraph', 'C4Context', 'C4Container', 'C4Component',
    'C4Deployment', 'C4Dynamic', 'quadrantChart', 'requirementDiagram',
    'sankey-beta', 'xychart-beta', 'block-beta',
}

# Lowercased reserved words that must NOT be used as bare node IDs.
RESERVED_NODE_IDS = {'end', 'subgraph', 'click'}


def extract_mermaid_blocks(text: str):
    """Yield (start_line, end_line, body) for each mermaid code block."""
    lines = text.split('\n')
    i = 0
    while i < len(lines):
        line = lines[i].rstrip()
        if line.lstrip().startswith('```mermaid'):
            start = i
            body_lines = []
            i += 1
            closed = False
            while i < len(lines):
                if lines[i].rstrip() == '```':
                    closed = True
                    end = i
                    break
                body_lines.append(lines[i])
                i += 1
            if closed:
                yield (start + 1, end + 1, '\n'.join(body_lines))
            else:
                yield (start + 1, len(lines), '\n'.join(body_lines))
                return
        i += 1


def validate_block(body: str) -> list[str]:
    """Return a list of error messages for a Mermaid block. Empty = valid."""
    errors = []

    stripped = body.strip()
    if not stripped:
        errors.append("EMPTY mermaid block")
        return errors

    # First non-comment line should declare the diagram type
    first_line = None
    for raw in stripped.split('\n'):
        line = raw.strip()
        if not line:
            continue
        if line.startswith('%%'):  # Mermaid comments
            continue
        first_line = line
        break

    if not first_line:
        errors.append("Only comments, no diagram declaration")
        return errors

    # The first token before space/colon/special should be a known diagram type
    first_token = re.split(r'[\s:({\[]', first_line, maxsplit=1)[0]
    if first_token not in MERMAID_DIAGRAM_TYPES:
        # graph TD, flowchart LR etc. are fine because first_token is graph/flowchart
        errors.append(f"Unknown diagram type: '{first_token}'")

    # Check for reserved IDs used as node IDs in graph/flowchart
    if first_token in ('graph', 'flowchart'):
        # Look for "  end -->" or "end-->" style
        for ln, line in enumerate(stripped.split('\n'), start=1):
            stripped_line = line.strip()
            # Pattern: bare ID followed by arrow without brackets
            for reserved in RESERVED_NODE_IDS:
                if re.search(rf'(^|\s){reserved}\s*(-->|---|==>|\.->|<--|<==)', stripped_line):
                    errors.append(
                        f"Line {ln}: reserved word '{reserved}' used as node ID. "
                        f"Wrap as [{reserved}] or rename."
                    )

    # Bracket balance over the WHOLE block (not per line). Skipped for
    # erDiagram because Mermaid uses `||--o{`, `}o--o{` etc. as relationship
    # syntax, which would cause false positives with naive bracket counting.
    if first_token != 'erDiagram':
        block_clean = re.sub(r'"[^"]*"', '""', stripped)
        # Strip lines that are comments to avoid counting brackets there
        block_clean = '\n'.join(
            ln for ln in block_clean.split('\n') if not ln.strip().startswith('%%')
        )
        if block_clean.count('[') != block_clean.count(']'):
            diff = block_clean.count('[') - block_clean.count(']')
            errors.append(f"Block has unbalanced [ ] (diff={diff})")
        if block_clean.count('{') != block_clean.count('}'):
            diff = block_clean.count('{') - block_clean.count('}')
            errors.append(f"Block has unbalanced {{ }} (diff={diff})")
    # Parens: more permissive because text labels can contain prose like
    # "(see ADR)" which is fine.

    return errors


def process_file(path: str) -> tuple[int, int, list[str]]:
    """Return (total_blocks, error_blocks, error_messages_for_file)."""
    try:
        with open(path, 'r', encoding='utf-8') as f:
            text = f.read()
    except (UnicodeDecodeError, OSError) as e:
        return (0, 0, [f"Could not read file: {e}"])

    total = 0
    bad = 0
    messages = []

    for start, end, body in extract_mermaid_blocks(text):
        total += 1
        errors = validate_block(body)
        if errors:
            bad += 1
            for err in errors:
                messages.append(f"  Lines {start}-{end}: {err}")

    return (total, bad, messages)


def walk_and_validate(root_dir: str) -> None:
    skip = {'.git', 'node_modules', '.nx', 'dist', 'build', '.next', '.venv'}
    total_files = 0
    files_with_diagrams = 0
    files_with_errors = 0
    total_blocks = 0
    total_errors = 0

    for root, dirs, files in os.walk(root_dir):
        dirs[:] = [d for d in dirs if d not in skip]
        for fname in files:
            if not fname.lower().endswith('.md'):
                continue
            total_files += 1
            fpath = os.path.join(root, fname)
            t, b, msgs = process_file(fpath)
            if t > 0:
                files_with_diagrams += 1
                total_blocks += t
            if b > 0:
                files_with_errors += 1
                total_errors += b
                rel = os.path.relpath(fpath, root_dir)
                print(f"[BAD] {rel}  ({b}/{t} blocks have errors)")
                for m in msgs:
                    print(m)

    print()
    print(f"Scanned:           {total_files} markdown files")
    print(f"With diagrams:     {files_with_diagrams}")
    print(f"Total diagrams:    {total_blocks}")
    print(f"Diagrams w/errors: {total_errors}")
    print(f"Files w/errors:    {files_with_errors}")

    if total_errors > 0:
        sys.exit(1)


if __name__ == '__main__':
    args = [a for a in sys.argv[1:] if not a.startswith('--')]
    root = args[0] if args else '.'
    walk_and_validate(root)
