#!/usr/bin/env python3
"""
Check bilingual documentation sync.
Ensures English docs have Spanish counterparts and vice versa.
"""

import os
import sys

def get_doc_pairs(docs_dir):
    """Get pairs of English/Spanish documentation files."""
    pairs = []
    seen_english = set()
    seen_spanish = set()

    for root, dirs, files in os.walk(docs_dir):
        for file in files:
            if not file.endswith('.md'):
                continue

            filepath = os.path.join(root, file)

            if file.endswith('.es.md'):
                # Spanish file
                english_version = file[:-7] + '.md'  # Remove .es and add .md
                english_path = os.path.join(root, english_version)
                seen_spanish.add(english_path.lower())
                pairs.append((filepath, english_path if os.path.exists(english_path) else None))
            elif '-es.' not in file and '/es/' not in filepath.replace('\\', '/'):
                # English file (not in es subdirectory)
                english_version = file
                spanish_version = file[:-3] + '.es.md'
                spanish_path = os.path.join(root, spanish_version)
                seen_english.add(filepath.lower())
                pairs.append((filepath, spanish_path if os.path.exists(spanish_path) else None))

    return pairs

def main():
    docs_dir = sys.argv[1] if len(sys.argv) > 1 else 'docs'

    pairs = get_doc_pairs(docs_dir)

    mismatches = []
    for english, spanish in pairs:
        english_name = os.path.basename(english)
        spanish_name = os.path.basename(spanish) if spanish else 'MISSING'

        if spanish is None:
            mismatches.append(f"Missing Spanish: {english}")
        elif not os.path.exists(spanish):
            mismatches.append(f"Missing Spanish: {english} -> {spanish}")

    print(f"Bilingual Sync Check:")
    print(f"  Total files checked: {len(pairs)}")
    print(f"  Mismatches: {len(mismatches)}")

    if mismatches:
        print(f"\nMissing translations:")
        for mismatch in mismatches:
            print(f"  - {mismatch}")
        return 1
    else:
        print(f"  All files have translations")
        return 0

if __name__ == '__main__':
    sys.exit(main())