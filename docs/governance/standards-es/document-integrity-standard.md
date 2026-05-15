# Document Integrity & Compatibility Standard (UTF-8/LF)

> **Goal**: Ensure 100% consistent rendering across GitHub, IDEs, and Operating Systems.

## 1. Mandatory Encoding Standards

All documentation artifacts must strictly adhere to the following binary standards:

- **Encoding**: `UTF-8` without Byte Order Mark (BOM).
- **Line Endings**: `LF` (Unix-style `\n`). **CRLF is strictly prohibited**.
- **Indentation**: 2 spaces (no tabs).
- **Final Newline**: Every file must end with a single newline character.

## 2. Prohibited Anti-Patterns

### 2.1 No emojis (strict)

Documentation under `/docs/` must NOT contain emoji or pictograph characters.
This includes — but is not limited to — the following blocks:

| Unicode range | Examples | Status |
|---------------|----------|--------|
| `U+1F300 - U+1F9FF` | smileys, objects, food, animals | FORBIDDEN |
| `U+2600 - U+26FF` | weather, warning sign, star outline | FORBIDDEN |
| `U+2700 - U+27BF` | dingbats (check mark, cross mark) | FORBIDDEN |
| `U+2B50, U+2B55` | white medium star, heavy large circle | FORBIDDEN |
| `U+FE0F` | variation selector-16 (forces emoji style) | FORBIDDEN |

What IS allowed (standard typography, not emoji):

- Em dash `—` (U+2014), en dash `–` (U+2013)
- Arrows `→` (U+2192), `←`, `↑`, `↓`
- Bullet `•` (U+2022)
- Spanish accents and `ñ`
- Box-drawing characters (`├──`, `└──`, `│`) **only** when used for directory trees

**Rationale**: Emojis render inconsistently across GitHub, IDEs, terminals, and
when documents are exported to PDF for board meetings. Plain text scales.

### 2.2 No copy-paste corruption

Do not paste text from Microsoft Word or Outlook without passing it through a
plain-text sanitizer first. The normalization script handles common mojibake
(`Ã³` -> `o`, `â†'` -> `->`), but pass-through is unreliable.

### 2.3 Simple tables only

Maintain simple Markdown tables. Do not use HTML tags within tables unless
explicitly required for accessibility.

## 3. Visual Consistency

### 3.1 Diagrams

**Use Mermaid.js exclusively for all diagrams.** This is a hard rule.

- Forbidden: ASCII art using box-drawing characters (`┌──┐ │ └──┘`) for diagrams
- Forbidden: Raw base64-embedded images
- Allowed: Mermaid `flowchart`, `gantt`, `pie`, `sequenceDiagram`, `erDiagram`,
  `classDiagram`, `stateDiagram-v2`, `quadrantChart`, `journey`, `timeline`,
  `mindmap`, `C4Context/Container/Component`
- Allowed: Plain Markdown tables for tabular data (not "diagrams")
- Allowed: ASCII directory trees (`├── file.md`) for showing folder structure

### 3.2 Bilinguality

Maintain symmetry between English and Spanish indices using the `-es` folder
suffix strategy (e.g. `docs/governance/standards/` and
`docs/governance/standards-es/`).

## 4. Tooling & Enforcement

To maintain this standard, the following scripts are integrated under
`/.bmad-core/scripts/`:

| Script | Purpose | When to run |
|--------|---------|-------------|
| `normalize_markdown.py` | Fix mojibake, BOM, line endings | Before any major release |
| `strip_emojis.py` | Remove all emoji/pictograph characters | After any large edit |
| `fix_bold_spacing.py` | Restore spacing after `**Label:**Word` cases | Always after `strip_emojis.py` |
| `validate_mermaid.py` | Validate every Mermaid block compiles | Before commit if diagrams changed |

**Recommended workflow before each commit that touches documentation:**

```bash
python3 .bmad-core/scripts/normalize_markdown.py .
python3 .bmad-core/scripts/strip_emojis.py .
python3 .bmad-core/scripts/fix_bold_spacing.py .
python3 .bmad-core/scripts/validate_mermaid.py .
```

The Mermaid validator exits with code `1` if any block has errors, so it can
be used as a pre-commit hook or CI gate.

## 5. Editor Configuration

These configuration files at the repository root enforce parts of the standard
automatically:

- `.editorconfig` - encoding on file save
- `.gitattributes` - LF normalization on commit

---

*Failure to comply with this standard results in a "Critical Quality Gate"
failure (R-03).*
