#!/usr/bin/env python3
"""
strip_emojis.py

Removes all emojis and pictograph characters from Markdown files to enforce
the "no emojis, no strange characters" standard defined in
docs/governance/standards-es/document-integrity-standard.md.

Preserves:
- Spanish accents (a, e, i, o, u, n, etc.)
- Standard typography: em dash, en dash, arrows, bullets, smart quotes
- Mathematical symbols when used in context

Removes:
- Emoji ranges (U+1F300-U+1FAFF, U+2600-U+27BF except arrows/bullets)
- Variation selectors (U+FE0F)
- Zero-width characters
- Misc pictographs

Usage:
    python strip_emojis.py [path]   (defaults to current dir)
    python strip_emojis.py --dry-run  (preview without modifying)
"""

import os
import re
import sys
import unicodedata

# Emoji ranges to remove (broad Unicode pictograph coverage)
EMOJI_RANGES = [
    (0x1F300, 0x1F9FF),   # Misc symbols and pictographs, emoticons, transport
    (0x1FA00, 0x1FAFF),   # Symbols and pictographs extended-A
    (0x1F000, 0x1F02F),   # Mahjong, dominoes
    (0x1F0A0, 0x1F0FF),   # Playing cards
    (0x2600,  0x26FF),    # Misc symbols (sun, snowflake, etc.) - includes warning sign
    (0x2700,  0x27BF),    # Dingbats - includes check marks, X marks, stars
    (0x1F100, 0x1F1FF),   # Enclosed alphanumeric supplement, regional indicators
]

# Specific characters to preserve (within the above ranges, do not remove)
PRESERVE = set([])

# Specific characters to ALSO remove (outside main ranges).
# Carefully selected to avoid removing standard typography:
# - Em dash (U+2014), en dash (U+2013), arrows (U+2190-U+21FF) ARE PRESERVED.
# - Box drawing chars (U+2500-U+257F) ARE PRESERVED (used in ASCII diagrams).
# - We only remove characters that are clearly pictograms/emoji.
EXTRA_REMOVE = set([
    # Misc symbols pictograms (rango 2B00-2BFF: includes both arrows and emojis)
    0x2B50,   # WHITE MEDIUM STAR (emoji)
    0x2B55,   # HEAVY LARGE CIRCLE (emoji)
    0x2B1B,   # BLACK LARGE SQUARE (emoji)
    0x2B1C,   # WHITE LARGE SQUARE (emoji)
    0x2B05,   # LEFTWARDS BLACK ARROW (emoji-styled arrow)
    0x2B06,   # UPWARDS BLACK ARROW
    0x2B07,   # DOWNWARDS BLACK ARROW
    0x2B08,   # NORTH EAST BLACK ARROW
    0x2B09,   # NORTH WEST BLACK ARROW
    0x2B0A,   # SOUTH EAST BLACK ARROW
    0x2B0B,   # SOUTH WEST BLACK ARROW
    # Variation selectors and zero-width chars
    0xFE0F,   # Variation Selector-16 (makes preceding char emoji style)
    0xFE0E,   # Variation Selector-15 (text style)
    0x200D,   # Zero-Width Joiner (used in compound emojis)
    0x200B,   # Zero-Width Space
    0x200C,   # Zero-Width Non-Joiner
    0x2060,   # Word Joiner
    0xFEFF,   # Zero-Width No-Break Space (BOM)
])


def is_emoji_codepoint(cp: int) -> bool:
    """Return True if codepoint should be removed as emoji/pictograph."""
    if cp in EXTRA_REMOVE:
        return True
    if cp in PRESERVE:
        return False
    for start, end in EMOJI_RANGES:
        if start <= cp <= end:
            return True
    return False


def strip_emojis(text: str) -> tuple[str, int]:
    """Remove emojis from text. Returns (clean_text, count_removed)."""
    out_chars = []
    removed = 0
    i = 0
    while i < len(text):
        ch = text[i]
        cp = ord(ch)

        # Handle surrogate pairs (Python 3 strings should already handle this,
        # but be defensive).
        if 0xD800 <= cp <= 0xDBFF and i + 1 < len(text):
            cp2 = ord(text[i + 1])
            if 0xDC00 <= cp2 <= 0xDFFF:
                actual_cp = 0x10000 + (cp - 0xD800) * 0x400 + (cp2 - 0xDC00)
                if is_emoji_codepoint(actual_cp):
                    removed += 1
                    i += 2
                    continue

        if is_emoji_codepoint(cp):
            removed += 1
            i += 1
            continue

        out_chars.append(ch)
        i += 1

    cleaned = ''.join(out_chars)

    # Collapse double spaces that may result from emoji removal
    cleaned = re.sub(r'  +', ' ', cleaned)
    # Fix "( Text)" -> "(Text)" (emoji was inside parens at start)
    cleaned = re.sub(r'\(\s+', '(', cleaned)
    # Fix " )" -> ")" (emoji was inside parens at end)
    cleaned = re.sub(r'\s+\)', ')', cleaned)
    # Fix "** Text" -> "**Text" (emoji was right after bold marker)
    cleaned = re.sub(r'\*\*\s+(\w)', r'**\1', cleaned)
    # Fix "Text **" -> "Text**"
    cleaned = re.sub(r'(\w)\s+\*\*', r'\1**', cleaned)
    # Fix "| Text" header rows with leading space
    cleaned = re.sub(r'\|\s\s+', '| ', cleaned)
    # Fix "## Text" with leading space after hash
    cleaned = re.sub(r'^(#+)\s\s+', r'\1 ', cleaned, flags=re.MULTILINE)
    # Clean up leading spaces on lines (from emoji at start of line)
    cleaned = re.sub(r'^[ \t]+([#\-*>|])', r'\1', cleaned, flags=re.MULTILINE)
    # Clean up trailing whitespace on lines
    cleaned = re.sub(r'[ \t]+$', '', cleaned, flags=re.MULTILINE)
    # Collapse 3+ blank lines to 2
    cleaned = re.sub(r'\n\n\n+', '\n\n', cleaned)

    return cleaned, removed


def process_file(path: str, dry_run: bool = False) -> int:
    try:
        with open(path, 'r', encoding='utf-8') as f:
            original = f.read()
    except (UnicodeDecodeError, OSError) as e:
        print(f"[!] Skipping {path}: {e}")
        return 0

    cleaned, removed = strip_emojis(original)

    if removed > 0 and not dry_run:
        with open(path, 'w', encoding='utf-8', newline='\n') as f:
            f.write(cleaned)

    return removed


def walk_and_clean(root_dir: str, dry_run: bool = False) -> None:
    total_files = 0
    total_modified = 0
    total_removed = 0

    skip_dirs = {'.git', 'node_modules', '.nx', 'dist', 'build', '.next', '.venv'}

    for root, dirs, files in os.walk(root_dir):
        # Mutate dirs in place to skip
        dirs[:] = [d for d in dirs if d not in skip_dirs]

        for fname in files:
            if not fname.lower().endswith('.md'):
                continue
            total_files += 1
            fpath = os.path.join(root, fname)
            removed = process_file(fpath, dry_run=dry_run)
            if removed > 0:
                total_modified += 1
                total_removed += removed
                action = "Would remove" if dry_run else "Removed"
                rel = os.path.relpath(fpath, root_dir)
                print(f"  {action:14} {removed:5} emojis in {rel}")

    mode = "DRY RUN" if dry_run else "EXECUTED"
    print(f"\n[{mode}] Scanned {total_files} files, modified {total_modified}, "
          f"removed {total_removed} emoji codepoints.")


if __name__ == '__main__':
    args = sys.argv[1:]
    dry_run = '--dry-run' in args
    args = [a for a in args if a != '--dry-run']
    root = args[0] if args else '.'
    walk_and_clean(root, dry_run=dry_run)
