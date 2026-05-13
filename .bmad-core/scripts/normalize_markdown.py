#!/usr/bin/env python3
import os
import sys

def normalize_file(file_path):
    try:
        # Read as binary to handle all types of encoding
        with open(file_path, 'rb') as f:
            content = f.read()

        # Remove UTF-8 BOM if present
        if content.startswith(b'\xef\xbb\xbf'):
            content = content[3:]

        # Attempt to decode, fix common mojibake, and re-encode
        try:
            text = content.decode('utf-8')
        except UnicodeDecodeError:
            # If UTF-8 fails, try latin-1 which is common in Windows corruption
            text = content.decode('latin-1')

        # Advanced Mojibake Correction
        replacements = {
            "Ã³": "ó", "Ã¡": "á", "Ã©": "é", "Ã­": "í", "Ãº": "ú", "Ã±": "ñ",
            "Ã“": "Ó", "Ã‘": "Ñ", "Ã‰": "É", "Ã€": "À",
            "â†’": "→", "â‡’": "⇒",
            "Ã°Å¸Â§Âª": "🧪", "Ã°Å¸Â â€ºÃ¯Â¸Â": "📑", "Ã°Å¸â€œÂ": "🔄",
            "Ã°Å¸â€ºÂ¡Ã¯Â¸Â": "🛡️", "â€œ": "“", "â€": "”", "â€™": "'"
        }
        
        for corrupted, restored in replacements.items():
            text = text.replace(corrupted, restored)

        # Normalize Line Endings to LF (\n)
        text = text.replace('\r\n', '\n').replace('\r', '\n')

        # Write as pure UTF-8 without BOM
        with open(file_path, 'w', encoding='utf-8', newline='\n') as f:
            f.write(text)
        
        return True
    except Exception as e:
        print(f"[!] Error normalizing {file_path}: {e}")
        return False

def run_normalization(root_dir):
    print(f"[*] Normalizing all Markdown files in: {root_dir}")
    count = 0
    for root, dirs, files in os.walk(root_dir):
        if any(skip in root for skip in [".git", "node_modules", ".nx"]):
            continue
        for file in files:
            if file.lower().endswith(".md"):
                if normalize_file(os.path.join(root, file)):
                    count += 1
    print(f"[*] Success! Normalized {count} files.")

if __name__ == "__main__":
    path = sys.argv[1] if len(sys.argv) > 1 else "."
    run_normalization(path)
