#!/usr/bin/env python3
"""
Validate ADR numbering and status consistency.
Ensures all ADRs have correct numbering and valid status.
"""

import os
import re
import sys

VALID_STATUSES = ['Proposed', 'Accepted', 'Deprecated', 'Superseded']

def extract_adr_info(filepath):
    """Extract ADR number, title, and status from a file."""
    with open(filepath, 'r', encoding='utf-8') as f:
        content = f.read()

    # Extract ADR number and title
    filename = os.path.basename(filepath)
    match = re.match(r'ADR-(\d+)-(.*)\.md', filename)
    if not match:
        return None

    adr_number = int(match.group(1))
    title = match.group(2).replace('-', ' ')

    # Extract status
    status_match = re.search(r'^status:\s*(\w+)', content, re.MULTILINE | re.IGNORECASE)
    status = status_match.group(1) if status_match else None

    return {
        'number': adr_number,
        'title': title,
        'status': status,
        'filepath': filepath
    }

def main():
    adrs_dir = sys.argv[1] if len(sys.argv) > 1 else 'docs/architecture/adrs'

    adrs = []
    errors = []

    # Collect all ADRs
    for root, dirs, files in os.walk(adrs_dir):
        for file in files:
            if file.startswith('ADR-') and file.endswith('.md'):
                filepath = os.path.join(root, file)
                adr_info = extract_adr_info(filepath)
                if adr_info:
                    adrs.append(adr_info)

    # Sort by number
    adrs.sort(key=lambda x: x['number'])

    # Check for duplicate numbers
    seen_numbers = {}
    for adr in adrs:
        num = adr['number']
        if num in seen_numbers:
            errors.append(f"Duplicate ADR number: ADR-{num:04d}")
            errors.append(f"  Files: {seen_numbers[num]} and {adr['filepath']}")
        else:
            seen_numbers[num] = adr['filepath']

    # Check for missing numbers
    numbers = [a['number'] for a in adrs]
    for i in range(1, max(numbers) + 1):
        if i not in numbers:
            print(f"WARNING: Missing ADR-{i:04d}")

    # Validate status
    for adr in adrs:
        if adr['status'] not in VALID_STATUSES:
            errors.append(f"Invalid status '{adr['status']}' in ADR-{adr['number']:04d}")

        if adr['status'] is None:
            errors.append(f"Missing status in ADR-{adr['number']:04d}: {adr['filepath']}")

    # Report results
    print(f"ADR Validation Summary:")
    print(f"  Total ADRs: {len(adrs)}")
    print(f"  Number range: {min(numbers) if numbers else 0} - {max(numbers) if numbers else 0}")

    if errors:
        print(f"\nErrors ({len(errors)}):")
        for error in errors:
            print(f"  - {error}")
        return 1
    else:
        print(f"  No errors found")

        # Show ADR table
        print(f"\nADR Registry:")
        print(f"| Number | Title | Status |")
        print(f"|--------|-------|--------|")
        for adr in adrs:
            print(f"| {adr['number']:04d} | {adr['title']} | {adr['status'] or 'MISSING'} |")

    return 0

if __name__ == '__main__':
    sys.exit(main())