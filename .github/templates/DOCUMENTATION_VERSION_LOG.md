# Documentation Version Log

**Purpose**: Track documentation changes across releases to ensure traceability and compliance with governance standards.

---

## Version Log Format

```markdown
## Release vX.Y.Z (YYYY-MM-DD)

| Field | Value |
|-------|-------|
| Version | X.Y.Z |
| Date | YYYY-MM-DD |
| Commit SHA | abc1234def5678 |
| Author | author@company.com |
| Summary | Brief description of documentation changes |

### Files Changed
- `path/to/file1.md`
- `path/to/file2.md`
- ...

### ADRs Changed
- `ADR-NNNN-title.md`
- ...

### Diagrams Changed
- `path/to/diagram.md`
- ...

### Validation Results
- Internal links: PASS/FAIL
- External links: PASS/FAIL (N verified)
- Mermaid syntax: PASS/FAIL (N diagrams)
- Bilingual sync: PASS/FAIL
- Evolith alignment: PASS/FAIL

### Known Issues
- None / List issues

### Approval Status
- [ ] Architecture Team
- [ ] Security Team
- [ ] Product Owner
```

---

## Release History

<!-- Add new entries at the top -->

---

## Guidelines

### When to Update
- Every production release
- Major documentation restructure
- ADR additions or significant updates

### What to Include
- All markdown files changed
- All diagrams changed (Mermaid, PlantUML, etc.)
- ADR decisions made
- Bilingual sync changes

### Validation Requirements
- All internal links must pass
- All external links must be verified
- Mermaid/PlantUML syntax must be valid
- Bilingual docs must be synchronized

---

**Document Version**: 1.0.0
**Last Updated**: 2026-05-29