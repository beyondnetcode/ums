# UMS Documentation Alignment Report

**Date:** 2026-05-29
**Author:** Architecture Review
**Status:** Active
**Repository:** beyondnetcode/ums
**Parent:** beyondnetcode/evolith_arch32

---

## Executive Summary

This report analyzes the alignment between UMS documentation and Evolith architecture standards. UMS is designed as a satellite repository of Evolith, meaning it must:
1. Explicitly inherit from Evolith standards where applicable
2. Document UMS-specific implementation as evidence
3. Promote reusable patterns back to Evolith when validated

**Overall Alignment Score:** 75/100

| Category | Score | Notes |
|----------|-------|-------|
| Evolith References | 70% | Missing explicit references in key docs |
| Bilingual Compliance | 60% | R-01 violations in ADRs |
| ADR Governance | 85% | Well maintained, 1 numbering conflict resolved |
| Domain Documentation | 80% | 3/6 BCs fully bilingual |
| Architecture Documentation | 90% | Complete and well structured |

---

## 1. Repository Taxonomy

### 1.1 Separation Rule

**Evolith = Reusable Enterprise Standards**
- Architecture principles
- Governance rules
- Design/development standards
- ADRs (enterprise-level)
- Boilerplate guidance

**UMS = Applied Implementation Evidence**
- Concrete product decisions
- Local ADR implementations
- Source code examples
- Product-specific adaptations
- Implementation evidence

### 1.2 Current Structure

```
UMS Repository
├── docs/                          # UMS Applied Documentation
│   ├── architecture/              # Architecture decisions (ADRs, bluep

rints)
│   ├── domain/                    # Domain aggregates (en/es)
│   ├── governance/                # Product governance
│   ├── operations/                # Runbooks (EN only)
│   ├── qa/                        # QA (empty placeholder)
│   └── reference/                 # Local reference docs
│
├── reference/                     # Evolith parent references
│   └── architecture/adrs/core/    # Evolith parent ADRs
│
├── src/                           # Source code (implementation evidence)
│
├── .bmad-core/                    # BMAD-METHOD rules
│
└── .harness/                      # Project playbooks
```

---

## 2. Evolith Standards UMS Must Reference

The following Evolith standards are applicable to UMS and should be explicitly referenced:

### 2.1 Architecture & Governance

| Evolith Standard | UMS Reference Location | Status |
|-----------------|----------------------|--------|
| ADR Process | `docs/architecture/adrs/index.md` | ✅ Referenced |
| Repository Taxonomy | `AGENTS.md`, `docs/architecture/` | ✅ Referenced |
| Modular Monolith Pattern | `reference/architecture/adrs/core/0067-*.md` | ✅ Referenced (ADR-0070) |
| Onboarding Guide | `docs/architecture/index.md` | ✅ Referenced |

### 2.2 Engineering Standards

| Evolith Standard | UMS Applied Reference | Status |
|-----------------|----------------------|--------|
| DDD Primitives | `libs/shell/ddd/` | ✅ Implemented |
| React Web Frontend | `docs/architecture/web-frontend/` | ✅ Documented |
| .NET API Standard | `docs/architecture/api-dotnet/` | ✅ Documented |
| Result Pattern | `docs/architecture/artifacts/canonical-patterns/cp-03.md` | ✅ Documented |
| Hexagonal Architecture | `docs/architecture/artifacts/canonical-patterns/cp-01.md` | ✅ Documented |

### 2.3 Missing Evolith References

The following UMS documents should explicitly reference Evolith standards but don't:

| Document | Missing Reference |
|----------|-------------------|
| `docs/architecture/blueprints/database-design-er.md` | Evolith ER modeling standard |
| `docs/architecture/blueprints/shell-library-architecture.md` | Evolith DDD primitives |
| `docs/domain/identity/tenant.md` | Evolith multi-tenancy standards |
| `docs/operations/runbooks/*.md` | Evolith operations standards |
| `docs/governance/construction/ddd-design/*.md` | Evolith DDD patterns |

---

## 3. Documents to Update

### Priority P0 (Critical - Week 1)

| Document | Action | Reason |
|----------|--------|--------|
| ADR-0053 to ADR-0060, ADR-0064, ADR-0068 | Create Spanish versions | R-01 bilingual sync violation |
| `docs/governance/construction/ddd-design/index.md` | Fix language mismatch | Spanish content, English title |
| `docs/domain-es/` | Complete Approvals, IGA, Audit BCs | Missing bilingual coverage |

### Priority P1 (High - Week 2)

| Document | Action | Reason |
|----------|--------|--------|
| `docs/operations/runbooks/rb-01*.md` - `rb-04*.md` | Translate to Spanish | Operations knowledge gap |
| `docs/architecture/shell-libraries/*.md` | Add Spanish versions | Documentation completeness |
| `docs/architecture/blueprints/database-design-er.md` | Add Evolith references | Pattern traceability |
| `docs/architecture/blueprints/shell-library-architecture.md` | Add Evolith references | Pattern traceability |

### Priority P2 (Medium - Week 3)

| Document | Action | Reason |
|----------|--------|--------|
| `docs/architecture/artifacts/` | Add Evolith references | Canonical pattern provenance |
| `docs/domain/authorization/profile.md` | Add recent Profile redesign | Pattern cards documentation |
| `docs/domain/configuration/feature-flag.md` | Add criteria-based activation | Feature flag evolution |

---

## 4. Missing Documents

| Document | Location | Priority |
|----------|----------|----------|
| Feature Flag criteria model | `docs/domain/configuration/feature-flag-criteria.md` | P1 (exists but needs update) |
| TenantParameter documentation | `docs/domain/identity/tenant-parameter.md` | P1 |
| Profile Permission Graph View | `docs/domain/authorization/profile.md` | P1 |
| Export strategies (JSON/XML/YAML/CSV) | `docs/architecture/` | P2 |
| Factory-based exporter resolution | `docs/architecture/shell-libraries/factory.md` | P2 |

---

## 5. Proposed UMS ADRs

| ADR | Title | Trigger |
|-----|-------|---------|
| UMS-ADR-0071 | DDD Design Portal Bilingual Policy | DDD docs language inconsistency |
| UMS-ADR-0072 | Operations Runbook Bilingual Scope | Runbooks EN-only |
| UMS-ADR-0073 | Shell Library Documentation Scope | No ES translation defined |
| UMS-ADR-0074 | Domain BC ES Translation Scope | Incomplete domain mirror |
| UMS-ADR-0075 | Profile Permission Export Strategy | New feature documentation |
| UMS-ADR-0076 | TenantParameter as Tenant Child Entity | New domain pattern |

---

## 6. Proposed Evolith Promotions

The following UMS patterns have been validated in production and should be proposed to Evolith:

| Pattern | Evidence | Rationale |
|---------|----------|-----------|
| **Execution Context Accessor** | ADR-0061 | Proven pattern for user/tenant context |
| **PII-Safe Serilog Configuration** | ADR-0062 | Enterprise security requirement |
| **Idempotency Key Middleware** | ADR-0063 | Cross-cutting concern |
| **No Raw GUIDs in UI** | ADR-0065 | UX/DDD rule |
| **Actionable User Error Contract** | ADR-0066 | UX quality pattern |
| **Domain Inheritance Strategy** | ADR-0069 | C# DDD base class decision |
| **Feature Flag SystemSuite Ownership** | ADR-0068 | Configuration pattern |
| **Database Schema Per Module** | ADR-0070 | Modular monolith pattern |
| **Shell Library Architecture** | `docs/architecture/shell-libraries/` | DDD/Factory/AOP/Bootstrapper |
| **Transactional Outbox** | `te-04-transactional-outbox.md` | Enterprise integration pattern |

---

## 7. Bilingual Compliance (R-01)

### 7.1 Current Status

| Area | English | Spanish | Status |
|------|---------|---------|--------|
| Architecture Overview | ✅ | ✅ | Complete |
| ADRs (0050-0070) | ✅ | ⚠️ 10/22 | Inconsistent |
| API .NET Applied Reference | ✅ | ✅ | Complete |
| Web Frontend Applied Reference | ✅ | ✅ | Complete |
| Shell Libraries | ✅ | ❌ | Missing |
| Domain (all BCs) | ✅ | ⚠️ 3/6 | Incomplete |
| DDD Design Portal | ⚠️ | ✅ | Mismatch |
| Functional Stories (FS-01-18) | ✅ | ✅ | Complete |
| Operations Runbooks | ✅ | ❌ | Missing |
| Blueprints | ✅ | ⚠️ | Partial |

### 7.2 ADRs Missing Spanish Version

```
ADR-0053 (OpenTelemetry)         - EN only
ADR-0054 (Shell Library)         - EN only
ADR-0055 (GraphQL/REST)          - EN only
ADR-0056 (Clean Architecture)    - EN only
ADR-0057 (Zustand+TanStack)      - EN only
ADR-0058 (API Gateway YARP)      - EN only
ADR-0059 (Single API Tier)       - EN only
ADR-0060 (AOP Strategy)          - EN only
ADR-0064 (Lean Repository)       - EN only
ADR-0068 (Feature Flags)         - EN only (has ES draft)
```

---

## 8. Link/Diagram/Index Validation

### 8.1 Verified Links

| Link | Status | Notes |
|------|--------|-------|
| MASTER_INDEX.md → architecture | ✅ | Valid |
| architecture/index.md → adrs/index.md | ✅ | Valid |
| ADR-0070 → reference/0067 | ✅ | Valid |
| ADR index → Parent Evolith | ✅ | Valid |

### 8.2 Mermaid Diagrams

| Document | Diagram | Status |
|----------|---------|--------|
| `docs/architecture/overview.md` | Architecture diagram | ⚠️ Needs validation |
| `docs/governance/construction/ddd-design/` | Interactive DDD viewer | ✅ Exists |
| `docs/architecture/blueprints/` | ER diagrams | ⚠️ Needs validation |
| `docs/architecture/blueprints/interactive-er-viewer.html` | Interactive ER | ✅ Exists |

### 8.3 Index Coherence

| Index | Status | Notes |
|-------|--------|-------|
| MASTER_INDEX.md | ✅ | Complete with en/es |
| architecture/index.md | ✅ | Well structured |
| adrs/index.md | ✅ | Recently updated with ADR-0070 |
| domain/index.md | ✅ | Complete |
| governance/index.md | ✅ | Complete with en/es |

---

## 9. Recent Concepts to Document

The following recent UMS decisions need explicit documentation:

### 9.1 Architecture Intelligence

- Pattern Cards concept for reusable solutions
- No cross-domain joins rule
- Contract-first integration

### 9.2 Profile Permission Redesign

- Modules ownership hierarchy
- Domain Resources classification
- System Actions delegation

### 9.3 Feature Flags Evolution

- SystemSuite as owner (ADR-0068)
- Criteria-based activation/deactivation
- Dynamic evaluation

### 9.4 Tenant Context

- Tenant selected at login (immutable session context)
- Tenant read-only display rule
- Tenant-based filtering enforcement (backend)
- Human-readable tenant identifiers

### 9.5 Export System

- TenantParameter as export configuration
- Export formats (JSON, XML, YAML, CSV)
- Factory-based exporter resolution
- Profile Permission Graph View/Export

### 9.6 UI/UX Standards

- Dynamic searchable combo boxes
- Connected user/profile panel
- Human-readable statistics (no raw GUIDs)
- Material Design 3 review rules

---

## 10. Recommended Next Commits

### Commit 1: Fix P0 Bilingual Sync (ADR-0053 to ADR-0060)
```
Create Spanish versions for:
- ADR-0053 (OpenTelemetry)
- ADR-0054 (Shell Library Isolation)
- ADR-0055 (GraphQL/REST Hybrid)
- ADR-0056 (Clean Architecture Frontend)
- ADR-0057 (Zustand + TanStack Query)
- ADR-0058 (API Gateway YARP)
- ADR-0059 (Single API Tier)
- ADR-0060 (AOP Cross-Cutting)
```

### Commit 2: Fix DDD Design Portal Language
```
- docs/governance/construction/ddd-design/index.md
  Change title to Spanish or translate content
```

### Commit 3: Complete domain-es/ BCs
```
Translate missing Approvals, IGA, Audit bounded contexts
```

### Commit 4: Add Evolith References
```
Add explicit Evolith references to:
- database-design-er.md
- shell-library-architecture.md
- All domain/*.md files
```

### Commit 5: Update ADR Index
```
- Add ADR-0070 entry
- Fix any broken cross-references
- Validate all links
```

---

## 11. Improvement Plan

### Week 1 (P0)
1. ✅ Fixed ADR-0066 numbering conflict (done - ADR-0070)
2. Complete ADR bilingual sync (0053-0060, 0064, 0068)
3. Fix DDD Design index language mismatch

### Week 2 (P1)
1. Translate runbooks RB-01 to RB-04
2. Complete domain-es/ coverage
3. Add Evolith references to key architecture docs

### Week 3 (P2)
1. Translate shell-libraries docs
2. Document Profile Permission redesign
3. Document export strategies

### Ongoing
1. Use documentation-audit-playbook.md for compliance
2. Validate links and diagrams quarterly
3. Review ADR bilingual coverage on each new ADR

---

## 12. Separation Evidence

### Evolith Standards (Inherited)

| Standard | UMS Evidence |
|----------|-------------|
| Clean Architecture | `docs/architecture/overview.md` |
| DDD Patterns | `src/apps/ums.api/Ums.Domain/` |
| Result Pattern | `Ums.Domain/Kernel/Result.cs` |
| CQRS | `Ums.Application/Authorization/Profile/Commands/` |
| Modular Monolith | `docs/architecture/adrs/0070-*.md` |

### UMS Local Decisions (Product-Specific)

| Decision | Evidence |
|----------|----------|
| SQL Server specific schema per BC | ADR-0070 |
| SystemSuite as FeatureFlag owner | ADR-0068 |
| React + Zustand + TanStack Query | ADR-0057 |
| GraphQL + REST hybrid | ADR-0055 |
| Shell.Ddd inheritance | ADR-0069 |
| TenantId as immutable session context | Recent implementation |

---

## Appendix A: ADR Status

| ADR | Title | Bilingual | Evolith Candidate | UMS Specific |
|-----|-------|-----------|-------------------|--------------|
| 0050 | Naming Taxonomy | ✅ | ✅ Adopted | No |
| 0051 | Event Bus Port | ✅ | No | Yes |
| 0052 | Audit Trail | ✅ | No | Yes |
| 0053 | OpenTelemetry | ❌ | No | Yes |
| 0054 | Shell Library | ❌ | No | Yes |
| 0055 | GraphQL/REST | ❌ | No | Yes |
| 0056 | Clean Arch Frontend | ❌ | No | Yes |
| 0057 | Zustand+TanStack | ❌ | No | Yes |
| 0058 | API Gateway | ❌ | Proposed | Yes |
| 0059 | Single API Tier | ❌ | No | Yes |
| 0060 | AOP Strategy | ❌ | No | Yes |
| 0061 | Exec Context | ✅ | ✅ Candidate | No |
| 0062 | PII Serilog | ✅ | ✅ Candidate | No |
| 0063 | Idempotency | ✅ | ✅ Candidate | No |
| 0064 | Lean Repository | ✅ | No | Yes |
| 0065 | No Raw GUIDs | ✅ | ✅ Candidate | No |
| 0066 | User Error Contract | ✅ | ✅ Candidate | No |
| 0068 | Feature Flags | ⚠️ | No | Yes |
| 0069 | Domain Inheritance | ✅ | No | Yes |
| 0070 | DB Schema Per Module | ✅ | No | Yes |

---

**Document Version:** 1.0
**Next Review:** 2026-06-29
**Owner:** Architecture Team