# Documentation Control Agents -- UMS Governance

## Overview

UMS documentation inherits governance standards from [Evolith Architecture Reference](https://github.com/beyondnetcode/evolith_arch32) while maintaining UMS-specific implementation evidence locally. This document establishes rules and processes for BMAD agents to maintain documentation quality, bilingual consistency, and architectural traceability.

## Inheritance Model

```
Evolith (parent repository)
    │
    ├── Defines global documentation standards
    ├── Provides reusable governance rules
    ├── Establishes canonical patterns for documentation
    │
    ▼
UMS (satellite repository)
    │
    ├── Inherits applicable Evolith rules by reference
    ├── Adapts rules to UMS context where needed
    ├── Documents UMS-specific implementation evidence
    └── Proposes successful patterns back to Evolith
```

### Rule Classification

| Rule Type | Source | Example |
|---|---|---|
| Global enterprise standard | Evolith | Naming conventions, ADR format, Clean Architecture principles |
| Applied implementation evidence | UMS | API applied references, React applied references, domain design |
| Local adaptation | UMS | UMS-specific routing, module organization, runtime values |
| Candidate for promotion | UMS → Evolith | Patterns with zero UMS dependencies that are applicable to any satellite |

## Bilingual Consistency Rules

### Core Requirement

Every documentation artifact that exists in English MUST have a Spanish equivalent, and vice versa. The English and Spanish versions MUST be:

1. **Structurally homogeneous** -- Same section order, same table structure, same heading hierarchy
2. **Technically equivalent** -- Same concepts, same links, same references
3. **Naturally worded** -- Not literal translations; use appropriate technical terminology in each language

### Language Switch Links

All documentation entry points MUST include language switch links:

- English pages: `[Leer en español](./README.es.md)` (or equivalent path)
- Spanish pages: `[Read in English](./README.md)` (or equivalent path)

### File Naming Convention

- English: `<filename>.md`
- Spanish: `<filename>.es.md`

When a Spanish version exists, internal links in the Spanish document MUST point to `*.es.md` versions of other Spanish documents.

### Terminology Standards

Use accepted technical terms appropriately in each language:

| English | Spanish | Context |
|---|---|---|
| ADR | ADR | Decision records (keep acronym) |
| Backend | Backend / API | Context-dependent |
| Frontend | Frontend / Web | Context-dependent |
| Bounded Context | Bounded Context | DDD concept (keep English) |
| Aggregate | Agregado | DDD concept |
| Quick Access | Acceso Rapido | Standards page |
| Master Index | Indice Maestro | Navigation hub |
| Applied Reference | Referencia Aplicada | UMS-specific implementation |

## Documentation Entry Points

UMS has the following primary entry points that MUST remain synchronized:

| Entry Point | English | Spanish |
|---|---|---|
| Root README | `/README.md` | `/docs/README.es.md` |
| Standards | `/docs/STANDARDS.md` | `/docs/STANDARDS.es.md` |
| Master Index | `/docs/MASTER_INDEX.md` | `/docs/MASTER_INDEX.es.md` |
| Architecture Portal | `/docs/architecture/index.md` | `/docs/architecture/index.es.md` |

## BMAD Agent Validation Process

### Before Committing Documentation Changes

1. **Run bilingual consistency check**
   - Verify English and Spanish versions have same structure
   - Verify links point to correct language variants
   - Verify terminology is appropriate for each language

2. **Run link validation**
   - All internal links resolve to existing files
   - External Evolith links are valid and point to correct language version
   - No broken references remain

3. **Run diagram validation**
   - All Mermaid diagrams are syntactically correct
   - Diagrams match the language of their document

4. **Check encoding compliance**
   - No mojibake or encoding artifacts
   - No emojis or non-standard decorative characters (per BMAD Rule R-03 and R-14)
   - Run cleanup if needed: `python ../.bmad-core/scripts/cleanup_markdown_encoding.py`

### Where to Start

If you are checking documentation coverage or a gap in a story, start with:

1. [Functional Story Gap Tracker](./project/functional-story-gap-tracker.md)
2. [API Aggregate Implementation Tracker](./project/api-aggregate-implementation-tracker.md)
3. [DDD Design Portal](./construction/ddd-design/index.md)

### Guide Usage

Use this local [Documentation Control Agents guide](./documentation-control-agents.md) together with the [Functional Story Gap Tracker](./project/functional-story-gap-tracker.md) for:
- Functional story readability verification
- Bilingual synchronization validation
- ADR and stack coherence checks
- Diagram traceability verification

## Evolith Rules Inherited by UMS

UMS agents MUST enforce these Evolith rules where applicable:

1. **R-03 and R-14 (BMAD Global Rules)**: No encoding artifacts or decorative characters
2. **ADR Format Standard**: All architectural decisions follow Evolith ADR template
3. **Functional Story Standard**: Business narrative remains readable; technical details in dedicated section
4. **Configuration Catalog Standard**: `code`, `value`, `description` mandatory contract
5. **Diagram Validation**: Mermaid syntax must be correct before commit

## UMS-Local Documentation Rules

These rules apply specifically to UMS documentation:

1. **Applied References**: Document UMS-specific implementation evidence in `/docs/architecture/`
2. **Evolith Links**: All upstream standards link to Evolith repository with correct language variant
3. **Product Scope**: UMS-specific decisions remain local; do not generalize to Evolith without ADR
4. **Promotion Process**: Patterns with zero UMS dependencies should be proposed to Evolith via ADR
5. **Complex Change Documentation Gate**: Any complex, cross-cutting, architectural, or evolutionary change must include a documented impact check, a documentation update plan, and synchronized English/Spanish updates before the change can be considered complete.

## Validation Checklist

Before any documentation commit, verify:

- [ ] English and Spanish versions have identical structure
- [ ] Language switch links are present and correct
- [ ] Internal links resolve to existing files
- [ ] External Evolith links are valid
- [ ] No encoding artifacts (mojibake) present
- [ ] No emojis or decorative characters
- [ ] Mermaid diagrams are syntactically valid
- [ ] Terminology is appropriate for each language
- [ ] UMS/Evolith separation is clear
- [ ] Configuration catalogs follow `code`, `value`, `description` standard
- [ ] Complex or evolutionary changes include a documentation impact check and updated artifacts in both languages

## Repository Map for Documentation

| Area | Entry Point | Purpose |
|---|---|---|
| Standards | `docs/STANDARDS*.md` | Quick access to Evolith standards and UMS applied references |
| Architecture | `docs/architecture/index*.md` | ADRs, blueprints, applied references, traceability |
| Governance | `docs/governance/index*.md` | Product vision, requirements, delivery documentation |
| Documentation Control | `docs/governance/documentation-control-agents*.md` | Documentation governance, validation workflow, bilingual sync |
| Construction | `docs/governance/construction/index*.md` | DDD design, bounded contexts, aggregates |
| Operations | `docs/operations/index.md` | Metrics, runbooks, operational documentation |
| Master Index | `docs/MASTER_INDEX*.md` | Complete document tree |

## External References

- [Evolith Architecture Reference](https://github.com/beyondnetcode/evolith_arch32)
- [Evolith Quick Access by Stack](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/quick-access/README.md)
- [BMAD Global Rules](../../.bmad-core/rules/global-rules.md)
- [BMAD Structuring Standard](../../.bmad-core/rules/structuring-standard.md)
- [UMS AGENTS.md](../../AGENTS.md)

## Change Process

When updating documentation:

1. Identify whether change affects English, Spanish, or both
2. Ensure both versions are updated together
3. Run validation checklist
4. If adding a new pattern applicable to other satellites, propose to Evolith via ADR
5. Update MASTER_INDEX if adding new top-level documents
6. For complex or evolutionary changes, add or update the corresponding ADR, functional story, or architecture note before closing the work

---

**[Back to AGENTS.md](../../AGENTS.md)** | **[Back to Master Index](./MASTER_INDEX.md)**
