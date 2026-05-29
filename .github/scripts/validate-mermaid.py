#!/usr/bin/env python3
"""
Validate Mermaid diagrams in markdown files.
Checks syntax and ensures diagrams are renderable.
"""

import os
import re
import sys

def extract_mermaid_blocks(content):
    """Extract mermaid blocks from markdown content."""
    pattern = r'```mermaid\s*\n(.*?)\n```'
    return re.findall(pattern, content, re.DOTALL)

def validate_mermaid_syntax(diagram_content):
    """Basic Mermaid syntax validation."""
    errors = []

    # Check for common diagram types
    valid_starters = [
        'graph', 'flowchart', 'sequenceDiagram', 'classDiagram',
        'stateDiagram', 'erDiagram', 'gantt', 'pie', 'mindmap'
    ]

    has_valid_starter = any(diagram_content.strip().startswith(s) for s in valid_starters)

    # Check for balanced brackets
    open_braces = diagram_content.count('{')
    close_braces = diagram_content.count('}')
    if open_braces != close_braces:
        errors.append(f"Unbalanced braces: {open_braces} open, {close_braces} close")

    # Check for balanced parentheses
    open_parens = diagram_content.count('(')
    close_parens = diagram_content.count(')')
    if open_parens != close_parens:
        errors.append(f"Unbalanced parentheses: {open_parens} open, {close_parens} close")

    return errors, has_valid_starter

def main():
    docs_dir = sys.argv[1] if len(sys.argv) > 1 else 'docs'

    total_diagrams = 0
    error_diagrams = 0

    for root, dirs, files in os.walk(docs_dir):
        for file in files:
            if file.endswith('.md'):
                filepath = os.path.join(root, file)

                try:
                    with open(filepath, 'r', encoding='utf-8') as f:
                        content = f.read()

                    diagrams = extract_mermaid_blocks(content)
                    total_diagrams += len(diagrams)

                    for i, diagram in enumerate(diagrams):
                        errors, has_valid_starter = validate_mermaid_syntax(diagram)

                        if not has_valid_starter:
                            print(f"WARNING: {filepath} - Diagram {i+1} doesn't start with recognized diagram type")
                            error_diagrams += 1

                        if errors:
                            print(f"ERROR: {filepath} - Diagram {i+1}:")
                            for error in errors:
                                print(f"  - {error}")
                            error_diagrams += 1

                except Exception as e:
                    print(f"ERROR: Failed to process {filepath}: {e}")

    print(f"\nMermaid Validation Summary:")
    print(f"  Total diagrams: {total_diagrams}")
    print(f"  Errors: {error_diagrams}")

    return 0 if error_diagrams == 0 else 1

if __name__ == '__main__':
    sys.exit(main())