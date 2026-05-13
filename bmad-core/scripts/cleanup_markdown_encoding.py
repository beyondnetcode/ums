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
    "ÃƒÂ³": "ó",
    "ÃƒÂ¡": "á",
    "ÃƒÂ©": "é",
    "ÃƒÂ­": "í",
    "ÃƒÂº": "ú",
    "ÃƒÂ±": "ñ",
    "Ãƒâ€œ": "Ó",
    "Ãƒâ€˜": "Ñ",
    "Ãƒâ€°": "É",
    "ÃƒÂ": "á",
    "Ãƒï¿½": "Á",
    "ÃƒÅ¡": "Ú",
    "Ã³": "ó",
    "Ã¡": "á",
    "Ã©": "é",
    "Ã­": "í",
    "Ãº": "ú",
    "Ã±": "ñ",
    "Ã“": "Ó",
    "Ã‘": "Ñ",
    "Ã‰": "É",
    "Ã°Å¸â€œÅ“": "📜",
    "Ã°Å¸â€œâ€ž": "📄",
    "Ã°Å¸â€ºÂ¡Ã¯Â¸Â": "🛡️",
    "Ã°Å¸Â â€ºÃ¯Â¸Â": "🏛️",
    "Ã°Å¸Å¸Â¢": "🟢",
    "Ã°Å¸Å¸Â¡": "🟡",
    "Ã°Å¸â€ Â´": "🔴",
    "Ã°Å¸â€ºÂ Ã¯Â¸Â": "🛠️",
    "Ã°Å¸â€œË†": "📈",
    "Ã°Å¸â€œ": "📜",
    "Ã°Å¸â": "🛠️",
    "â€œ": "“",
    "â€": "”",
    "â€™": "'"
}

def scan_and_repair(root_dir):
    print(f"[*] Starting recursive scan for Markdown encoding artifacts in: {root_dir}")
    repaired_count = 0
    scanned_count = 0

    for root, dirs, files in os.walk(root_dir):
        # Skip version control and dependency folders
        if any(skip in root for skip in [".git", "node_modules", "bin", "obj"]):
            continue
            
        for file in files:
            if file.lower().endswith(".md"):
                scanned_count += 1
                file_path = os.path.join(root, file)
                
                try:
                    with open(file_path, "rb") as f:
                        content_bytes = f.read()
                    
                    # Attempt to decode as standard UTF-8
                    text = content_bytes.decode("utf-8", errors="replace")
                    original_text = text
                    
                    # Perform replacements for known patterns
                    for corrupted, restored in REPLACEMENTS.items():
                        text = text.replace(corrupted, restored)
                    
                    if text != original_text:
                        # Save back as clean UTF-8 without BOM
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
