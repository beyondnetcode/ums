#!/usr/bin/env python3
"""
fix_bold_spacing.py

Fixes a regression introduced by strip_emojis.py: when an emoji was placed
between a bold label and the following text, removing the emoji caused
"**Label:**Text" instead of "**Label:** Text".

This script restores the missing space after **...:** and similar patterns.

Usage:
    python fix_bold_spacing.py [path]
    python fix_bold_spacing.py --dry-run [path]
"""

import os
import re
import sys


# Pattern 1: "**Label:**Word" -> "**Label:** Word"
# Pattern 2: "**Label**Word" where Label ends with letter/digit -> "**Label** Word"
# Pattern 3: "Word**Label**" -> "Word **Label**" (less common)

PATTERNS = [
    # 1. "**Label:**Word" -> "**Label:** Word"
    (re.compile(r'(\*\*[^*\n]+:\*\*)([A-Za-zÀ-ÿÑñ¿¡0-9])'), r'\1 \2'),
    # 2. "**bold**Capital" -> "**bold** Capital" (close bold then uppercase text)
    (re.compile(r'(\*\*[A-Za-zÀ-ÿÑñ][^*\n]+\*\*)([A-ZÀ-ÿÑ][a-zà-ÿñ])'), r'\1 \2'),
    # 3. "wordEnd**Bold:**" -> "wordEnd **Bold:**" (lowercase letter immediately
    # followed by ** opening a new bold label - clearly missing a space).
    (re.compile(r'([a-zà-ÿñ0-9])(\*\*[A-ZÀ-ÿÑ][^*\n]{0,80}:\*\*)'), r'\1 \2'),
    # 4. ".**Bold**" -> ". **Bold**" (sentence punctuation immediately followed by bold)
    (re.compile(r'([.!?])(\*\*[A-ZÀ-ÿÑ])'), r'\1 \2'),
    # 5. "letter**Bold..." (any letter followed by bold opening) -> add space.
    # This catches "Definition**FS-09**" and "Documentation**Comprehensive**".
    (re.compile(r'([a-zà-ÿñ])(\*\*[A-Za-z0-9À-ÿÑ])'), r'\1 \2'),
    # 6. "**Bold**letter" -> "**Bold** letter" (close bold then letter without space).
    # We require that the preceding char inside ** is a letter/digit to avoid
    # breaking emphasis like ":**" + ":" (already handled).
    (re.compile(r'([A-Za-z0-9À-ÿÑ]\*\*)([A-Za-z0-9À-ÿÑ])'), r'\1 \2'),
]


def fix_text(text: str) -> tuple[str, int]:
    fixed = text
    total = 0
    for pat, repl in PATTERNS:
        fixed, n = pat.subn(repl, fixed)
        total += n
    return fixed, total


def process_file(path: str, dry_run: bool) -> int:
    try:
        with open(path, 'r', encoding='utf-8') as f:
            original = f.read()
    except (UnicodeDecodeError, OSError) as e:
        print(f"[!] Skipping {path}: {e}")
        return 0

    fixed, count = fix_text(original)
    if count > 0 and not dry_run:
        with open(path, 'w', encoding='utf-8', newline='\n') as f:
            f.write(fixed)
    return count


def walk(root: str, dry_run: bool) -> None:
    skip = {'.git', 'node_modules', '.nx', 'dist', 'build', '.next', '.venv'}
    total_files = 0
    modified = 0
    total_fixes = 0

    for r, dirs, files in os.walk(root):
        dirs[:] = [d for d in dirs if d not in skip]
        for f in files:
            if not f.lower().endswith('.md'):
                continue
            total_files += 1
            p = os.path.join(r, f)
            n = process_file(p, dry_run)
            if n > 0:
                modified += 1
                total_fixes += n
                rel = os.path.relpath(p, root)
                action = "Would fix" if dry_run else "Fixed"
                print(f"  {action:10} {n:4} spacings in {rel}")

    mode = "DRY RUN" if dry_run else "EXECUTED"
    print(f"\n[{mode}] Scanned {total_files} files, modified {modified}, "
          f"applied {total_fixes} fixes.")


if __name__ == '__main__':
    args = sys.argv[1:]
    dry_run = '--dry-run' in args
    args = [a for a in args if a != '--dry-run']
    root = args[0] if args else '.'
    walk(root, dry_run)
