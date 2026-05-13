# 🛡️ Document Integrity & Compatibility Standard (UTF-8/LF)

> **Goal**: Ensure 100% consistent rendering across GitHub, IDEs, and Operating Systems.

## 1. Mandatory Encoding Standards
All documentation artifacts must strictly adhere to the following binary standards:
- **Encoding**: `UTF-8` without Byte Order Mark (BOM).
- **Line Endings**: `LF` (Unix-style `\n`). **CRLF is strictly prohibited**.
- **Indentation**: 2 spaces (no tabs).
- **Final Newline**: Every file must end with a single newline character.

## 2. Prohibited Anti-Patterns
- **Copy-Paste Corruption**: Do not paste text from Microsoft Word or Outlook without passing it through a plain-text sanitizer.
- **Emoji Overload**: Use only standard Unicode emojis. Avoid system-specific icons.
- **Table Complexity**: Maintain simple Markdown tables. Do not use HTML tags within tables unless explicitly required for accessibility.

## 3. Tooling & Enforcement
To maintain this standard, the following tools are integrated:
1.  **EditorConfig**: Enforces encoding on file save.
2.  **GitAttributes**: Forces LF normalization on commit.
3.  **Normalization Script**: Run `python ./.bmad-core/scripts/normalize_markdown.py` before any major release.

## 4. Visual Consistency
- **Diagrams**: Use Mermaid.js exclusively. Avoid embedding raw base64 images for diagrams.
- **Bilinguality**: Maintain symmetry between English and Spanish indices using the `-es` folder suffix strategy.

---
*Failure to comply with this standard results in a "Critical Quality Gate" failure (R-03).*
