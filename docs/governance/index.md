# UMS Governance Portal

**Version:** 1.0 | **Date:** 2026-05-14 | **Status:** Construction Ready
**[Back to Main README](../README.md)**

---

## Governance Index

### Quick Links by Phase

#### **Product Readiness** (Complete)
- [Product Readiness Assessment](./project/product-readiness-final-assessment.md) — 100% readiness, 8 épicas validated, Go/No-Go approved
- [Product Backlog](./project/mvp-product-backlog.md) — 16 functional stories (FS-01 to FS-16)

#### **Construction Planning** (Ready)
- [Construction README](./construction/README.md) — How-to guide by role (PO, engineer, QA, hiring, DevOps)
- [Technical Stories & Team Composition](./construction/TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md) — **PRIMARY**: 89 TS, 8 team profiles, effort validation (578 pts)
- [FS-to-TS Mapping](./construction/FS-TO-TS-MAPPING.md) — Requirement traceability (16 FS → 89 TS)
- [Service Implementation Plan](./construction/SERVICE-IMPLEMENTATION-PLAN.md) — .NET 8 monorepo, architecture, CI/CD
- [TE-03 RLS Implementation](./construction/TE-03-rls-sql-server-implementation.md) — Two-layer RLS deep dive

#### **Detailed Designs** (Post-MVP Épicas)
- [EP-06: Approvals (MFA, B2B, Delegation)](./project/ep-06-approvals-detailed-design.md)
- [EP-07: Compliance (Docs, Notifications, Enforcement)](./project/ep-07-compliance-detailed-design.md)
- [EP-08: IGA (Role Promotion)](./project/ep-08-iga-detailed-design.md)

#### **Architecture & Strategy**
- [Product Vision](./project/index.md)
- [Requirements](./requirements/index.md)
- [Roadmap](./roadmap/index.md)
- [ADR Library](../architecture/blueprints/) — 49 architectural decision records

---

## Read This Based on Your Role

| Role | Start Here | Next | Time |
|------|-----------|------|------|
| **Product Owner** | construction/README.md (PO section) | FS-TO-TS-MAPPING.md | 20 min |
| **Engineering Lead** | construction/TECHNICAL-STORIES (PART 1,6,7) | SERVICE-IMPLEMENTATION-PLAN.md | 60 min |
| **QA Lead** | FS-TO-TS-MAPPING.md | construction/TECHNICAL-STORIES (PART 3) | 30 min |
| **Hiring** | construction/TECHNICAL-STORIES (PART 3,5) | construction/README.md | 15 min |
| **DBA** | TE-03-RLS-IMPLEMENTATION.md | construction/TECHNICAL-STORIES (PART 3) | 30 min |
| **DevOps** | SERVICE-IMPLEMENTATION-PLAN.md | construction/README.md (DevOps) | 30 min |
| **Architect** | product-readiness-final-assessment.md | ep-06/07/08-detailed-design.md | 45 min |

---

## Key Metrics

- **Readiness:** 100% (8/8 épicas validated)
- **Total Story Points:** 578 (MVP 253 + Post-MVP 325)
- **Technical Stories:** 89 (MVP 55 + Post-MVP 33)
- **Functional Stories:** 16 (FS-01 to FS-16)
- **Team Size:** 6.25 FTE (MVP) → 7.75 FTE (Post-MVP)
- **Timeline:** MVP 6-7 weeks, Full product 14-17 weeks
- **Risk Level:** LOW
- **Status:** AUTHORIZED FOR CONSTRUCTION

---

## Next Steps (Sprint 0)

1. **Leadership:** Approve readiness + budget for hiring
2. **Engineering:** Read `construction/README.md` (your role) + TECHNICAL-STORIES
3. **Hiring:** Post job descriptions (2 engineers: Senior Security + QA)
4. **DevOps:** Set up CI/CD + SQL Server test environment
5. **All:** Attend ADR training session

See `construction/README.md` Sprint 0 checklist for complete tasks.

---

*Last Updated: 2026-05-14 | Status: Construction Phase Ready*
