#!/usr/bin/env python3
"""
BMAD-METHOD Utility Script: cleanup_markdown_encoding.py
Recursively scans the repository for Markdown files (.md) and restores corrupted UTF-8 
encoding (mojibake) back to clean, renderable UTF-8 format.
"""

import os
import sys

# Define replacements map for common Windows double-encoding (mojibake) artifacts
REPLACEMENTS = {
    "Ã³": "ó", "Ã¡": "á", "Ã©": "é", "Ã­": "í", "Ãº": "ú", "Ã±": "ñ",
    "Ã“": "Ó", "Ã‘": "Ñ", "Ã‰": "É", "Ã€": "À",
    "â†’": "→", "â‡’": "⇒",
    "Ã°Å¸Â§Âª": "🧪",
    "Ã°Å¸Â â€ºÃ¯Â¸Â": "📑",
    "Ã°Å¸â€œÂ": "🔄",
    "Ã°Å¸â€ºÂ¡Ã¯Â¸Â": "🛡️",
    "â€œ": "“", "â€": "”", "â€™": "'"
}

def fix_mojibake(text):
    # Try to fix double encoding: text was encoded as utf-8, then misread as latin-1, then encoded as utf-8 again
    try:
        # If the text contains characters that look like mojibake
        if any(c in text for c in "ÃÂâ"):
            # This is a common trick to reverse double encoding
            fixed = text.encode('latin-1').decode('utf-8')
            return fixed
    except:
        pass
    return text

def scan_and_repair(root_dir):
    print(f"[*] Starting recursive scan for Markdown encoding artifacts in: {root_dir}")
    repaired_count = 0
    scanned_count = 0

    for root, dirs, files in os.walk(root_dir):
        if any(skip in root for skip in [".git", "node_modules", "bin", "obj"]):
            continue
            
        for file in files:
            if file.lower().endswith(".md"):
                scanned_count += 1
                file_path = os.path.join(root, file)
                
                try:
                    # First read as UTF-8
                    with open(file_path, "r", encoding="utf-8", errors="replace") as f:
                        text = f.read()
                    
                    original_text = text
                    
                    # 1. Apply dynamic fix for double encoding
                    text = fix_mojibake(text)
                    
                    # 2. Apply static replacements for specific artifacts
                    for corrupted, restored in REPLACEMENTS.items():
                        text = text.replace(corrupted, restored)
                    
                    if text != original_text:
                        with open(file_path, "w", encoding="utf-8", newline="\n") as f:
                            f.write(text)
                        print(f"[+] Repaired: {file_path}")
                        repaired_count += 1
                except Exception as e:
                    print(f"[!] Error processing {file_path}: {e}")
                    
    print(f"\n[*] Scan complete.")
    print(f"    Scanned files: {scanned_count}")
    print(f"    Repaired files: {repaired_count}")

if __name__ == "__main__":
    # Resolve the base directory of the project
    script_dir = os.path.dirname(os.path.abspath(__file__))
    project_root = os.path.abspath(os.path.join(script_dir, "..", ".."))
    
    if len(sys.argv) > 1:
        target_path = sys.argv[1]
    else:
        target_path = project_root
        
    scan_and_repair(target_path)
