# UMS — Enterprise User Management System

> **Standardized Modular Monolith for Unified Identity & Authorization.**
>
> ![Status](https://img.shields.io/badge/Status-Active-success) ![Architecture](https://img.shields.io/badge/Architecture-Modular_Monolith-blue) ![Methodology](https://img.shields.io/badge/Methodology-BMAD--METHOD-success)

---

## Language / Bilingüe
- **English** | [Español](./README.es.md)

---

## FOR DIRECTORS / CTO / INVESTORS — START HERE (5 min)

> **Have 5 minutes for an investment decision?**

| # | Document | Time | Purpose |
|---|----------|------|---------|
| 1 | **[Executive Summary (ES/EN)](./docs/governance/RESUMEN-EJECUTIVO-DIRECTORES.md)** | 5 min | Investment S/ 141K-195K, ROI Y1 84-112%, timeline 8.5-12 weeks, GO/NO-GO |
| 2 | **[Decision Matrix](./docs/governance/DECISION-MATRIX.md)** | 5 min | Sign-off form: CTO + CFO + Head of Engineering |
| 3 | **[Board Presentation (12 slides)](./docs/governance/BOARD-PRESENTATION.md)** | 20 min | Markdown deck for board meeting (exportable to PDF/PPTX) |

**Key numbers:** S/ 141K AI-Driven (8.5 weeks) | S/ 182K Human (12 weeks) | ROI Y1 84%-112% | Payback 3 months | LTV/CAC 14.9x

---

## Master Navigation Index
Start here if you are new to UMS. This index gives each reader a fast route into the repository without needing to know the folder structure.

| I want to... | Start Here | Then Read |
| :--- | :--- | :--- |
| Understand the product | [Product Vision](./docs/governance/product/product-vision.md) | [Business Context](./docs/governance/product/business-context.md) → [Scope](./docs/governance/product/scope.md) |
| See Epics & Priorities | [MVP Product Backlog](./docs/governance/project/mvp-product-backlog.md) | [MVP Prioritization](./docs/governance/roadmap/mvp-functional-prioritization.md) → [Functional Stories](./docs/governance/requirements/functional-stories/index.md) |
| Review functional requirements | [Requirements Index](./docs/governance/requirements/index.md) | [Functional Stories](./docs/governance/requirements/functional-stories/index.md) → [Glossary](./docs/governance/requirements/glossary.md) |
| Validate the data and domain model | [Conceptual Data Model](./docs/governance/requirements/conceptual-data-model.md) | [ER Export Formats](./docs/architecture/blueprints/er-export-formats.md) → [Database Design ER](./docs/architecture/blueprints/database-design-er.md) |
| Understand the architecture | [Architecture Portal](./docs/architecture/index.md) | [C4 Architecture Spec](./docs/architecture/blueprints/architecture-spec.md) → [ADR Registry](./docs/architecture/adrs/index.md) |
| **Approve Investment / Decide** | [**Executive Summary**](./docs/governance/RESUMEN-EJECUTIVO-DIRECTORES.md) | [Decision Matrix](./docs/governance/DECISION-MATRIX.md) → [Board Presentation](./docs/governance/BOARD-PRESENTATION.md) → [Revenue Model](./docs/governance/construction/REVENUE-MODEL-YEAR-1.md) |
| **Plan & Execute Construction** | [**Construction Index**](./docs/governance/construction/ESTIMATION-INDEX.md) | [MVP Scope (EN)](./docs/governance/construction/REDUCED-MVP-SCOPE-AND-ESTIMATION.md) → [Cost Analysis](./docs/governance/construction/ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) → [Execution Models](./docs/governance/construction/MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md) |
| Build or operate the system | [Engine Room](./src/) | [Technical Enablers](./docs/architecture/blueprints/technical-enablers/index.md) → [Operations Portal](./docs/operations/index.md) |
| Browse everything | [Master Index](./docs/MASTER_INDEX.md) | Complete document tree by lifecycle phase. |
## Quick Start (Engine Room)
```powershell
cd src
npm install; npx nx run app-web:dev
```

---

## Knowledge Hub
| Domain | Portal Index | Contents |
| :--- | :--- | :--- |
| **Governance** | [Governance Portal](./docs/governance/index.md) | Product direction, standards, roadmap, project backlog, and audits. |
| **Project Delivery** | [Project Backlog](./docs/governance/project/index.md) | MVP epics, user stories, priority order, delivery phases, and cut line. |
| **Construction Planning** | [**Construction Index**](./docs/governance/construction/ESTIMATION-INDEX.md) | **MVP scope (168 pts, 12 weeks), cost analysis (3 scenarios), execution models (Human vs AI-Driven), team composition, technical stories (89 TS), risk mitigation.** [See Full Index →](./docs/governance/construction/README.md) |
| **Requirements** | [Requirements Index](./docs/governance/requirements/index.md) | Functional stories, business glossary, permission model, and conceptual data model. |
| **Architecture** | [Architecture Portal](./docs/architecture/index.md) | Stack, ADR registry, C4 specs, bounded contexts, and technical enablers. |
| **Infrastructure** | [Infra Setup](./docs/infrastructure/index.md) | Docker, Kong, Kubernetes, and environment setup. |
| **Operations** | [Ops Portal](./docs/operations/index.md) | Runbooks, observability, SQL operations, and SRE practices. |
| **Knowledge** | [Knowledge Base](./docs/knowledge/index.md) | Recommended reading paths, POCs, research, and onboarding.
## Recommended Reading by Role
| Role | Objective | Learning Path (Direct Links) |
| :--- | :--- | :--- |
| **Executive / Director / Investor** | Investment decision: cost, ROI, timeline, risk | [**Resumen Ejecutivo (5 min)**](./docs/governance/RESUMEN-EJECUTIVO-DIRECTORES.md) → [**Decision Matrix**](./docs/governance/DECISION-MATRIX.md) → [**Board Presentation**](./docs/governance/BOARD-PRESENTATION.md) → [Product Vision](./docs/governance/product/product-vision.md) |
| **Product Owner** | Functional scope, sequencing, and backlog ownership | [Product Vision](./docs/governance/product/product-vision.md) → [Requirements](./docs/governance/requirements/index.md) → [MVP Prioritization](./docs/governance/roadmap/mvp-functional-prioritization.md) → [Product Backlog](./docs/governance/project/mvp-product-backlog.md) → [**MVP Scope & TS Mapping**](./docs/governance/construction/README.md) |
| **Business Analyst** | Business narrative, rules, and acceptance criteria | [Requirements Index](./docs/governance/requirements/index.md) → [Functional Stories](./docs/governance/requirements/functional-stories/index.md) → [Glossary](./docs/governance/requirements/glossary.md) → [Product Backlog](./docs/governance/project/mvp-product-backlog.md) |
| **Software Architect** | Technical design, decisions, and domain boundaries | [Product Vision](./docs/governance/product/product-vision.md) → [Architecture Portal](./docs/architecture/index.md) → [C4 Architecture Spec](./docs/architecture/blueprints/architecture-spec.md) → [ADR Registry](./docs/architecture/adrs/index.md) |
| **Developer (BE/FE)** | What to build, why it matters, and how it fits | [Product Vision](./docs/governance/product/product-vision.md) → [Product Backlog](./docs/governance/project/mvp-product-backlog.md) → [Functional Stories](./docs/governance/requirements/functional-stories/index.md) → [Technical Enablers](./docs/architecture/blueprints/technical-enablers/index.md) → [Engine Room](./src/) |
| **DevOps / SRE** | Environments, reliability, and observability | [Product Backlog](./docs/governance/project/mvp-product-backlog.md) → [Infrastructure](./docs/infrastructure/index.md) → [Operations Portal](./docs/operations/index.md) → [Observability Strategy](./docs/architecture/artifacts/observability-strategy.md) |
| **QA / Security** | Quality gates, risks, and verification strategy | [Product Backlog](./docs/governance/project/mvp-product-backlog.md) → [Contract Testing](./docs/architecture/artifacts/contract-testing-plan.md) → [Enterprise IAM](./docs/architecture/artifacts/enterprise-iam-ums-specification.md) → [Maturity Model](./docs/architecture/artifacts/architecture-maturity-model.md) |
| **Finance / Budget Owner** | Cost estimation, ROI, and infrastructure trade-offs | [**Cost Analysis (3 scenarios)**](./docs/governance/construction/ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) → [**Execution Models Comparison**](./docs/governance/construction/MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md) → [MVP Scope](./docs/governance/construction/ESTIMATION-INDEX.md) |
| **Engineering Lead / Tech Architect** | What to build, team composition, and critical path | [Product Backlog](./docs/governance/project/mvp-product-backlog.md) → [**Construction Index (89 TS, team, dependencies)**](./docs/governance/construction/ESTIMATION-INDEX.md) → [Technical Stories](./docs/governance/construction/README.md) → [Architecture Portal](./docs/architecture/index.md) |
| **AI / Agents** | BMAD-METHOD and repository rules | [Agent Rules](./AGENTS.md) → [BMAD Audit](./docs/architecture/artifacts/bmad-master-audit-alignment-report.md) → [Taxonomy Audit](./docs/governance/audits/2026-05-13-taxonomy-normalization-audit.md) |

---

## Executive & Board Portal — Investment Decision (MVP S/ 141K-195K)

**For Directors, CTO, Investors — Decision-Ready Documents:**

| Need | Document | Time | Key Info |
| :--- | :--- | :--- | :--- |
| **5-min executive summary** | [Resumen Ejecutivo (ES/EN)](./docs/governance/RESUMEN-EJECUTIVO-DIRECTORES.md) | **5 min** | Investment, ROI, timeline, GO/NO-GO recommendation |
| **Sign Go/No-Go decision** | [Decision Matrix](./docs/governance/DECISION-MATRIX.md) | 5 min | Sign-off form: CTO + CFO + Head of Engineering |
| **Board meeting pitch** | [Board Presentation](./docs/governance/BOARD-PRESENTATION.md) | 20 min | 12 slides: problem → solution → cost → ROI → ask |
| **Revenue & unit economics** | [Revenue Model Y1](./docs/governance/construction/REVENUE-MODEL-YEAR-1.md) | 15 min | CAC, LTV (14.9x), 50-client ramp-up plan, sensitivity |
| **Competitive positioning** | [Competitive Analysis](./docs/governance/construction/COMPETITIVE-ANALYSIS.md) | 10 min | vs Okta/Auth0/Azure AD; TCO saves S/ 164K-352K (3y) |

---

## Construction Planning Portal (MVP Reduced — 8.5-12 weeks)

**Quick Navigation by Need:**

| Need | Document | Time | Key Info |
| :--- | :--- | :--- | :--- |
| **See complete construction plan** | [Construction Index](./docs/governance/construction/ESTIMATION-INDEX.md) | 5 min | Master bilingual index with role-based reading paths |
| **Understand MVP scope & team** | [README (Construction)](./docs/governance/construction/README.md) | 10 min | 8 FS, 168 pts, 4-person team, 12 weeks, DoD per épica |
| **Budget & Cost Analysis** | [Cost/Benefit Analysis](./docs/governance/construction/ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) | 15 min | 10 profiles, 3 scenarios (On-Prem/Hybrid/Cloud), S/ 182K recommended, ROI 84% Y1 |
| **Human vs AI-Driven Decision** | [Execution Models](./docs/governance/construction/MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md) | 20 min | Human (S/ 182K, 12w, ROI 382%) vs AI (S/ 141K, 8.5w, ROI 729%), risk matrix |
| **AI Timeline Credibility** | [AI Timeline Justification](./docs/governance/construction/JUSTIFICACION-TIMELINE-AI-DRIVEN.md) | 10 min | Honest breakdown: agents 12%, validation 59%, setup 15%, rework 10% |
| **Technical story details** | [Technical Stories & Team](./docs/governance/construction/TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md) | 60 min | 89 TS across 8 épicas, team profiles, critical paths |
| **FS-to-TS traceability** | [FS Mapping](./docs/governance/construction/FS-TO-TS-MAPPING.md) | 30 min | Requirements coverage validation, acceptance criteria alignment |
| **RLS & Security Design** | [Construction README](./docs/governance/construction/README.md) | 30 min | Two-layer model, SESSION_CONTEXT, error handling, failover |
| **Architecture & Implementation** | [Service Plan](./docs/governance/project-es/SERVICE-IMPLEMENTATION-PLAN.md) | 40 min | .NET 8 structure, test pyramid, CI/CD, DI patterns |

---

**Reading Paths by Role:**

| Role | Path | Duration |
| :--- | :--- | :--- |
| **Directorio / Inversores** | [Resumen Ejecutivo](./docs/governance/RESUMEN-EJECUTIVO-DIRECTORES.md) → [Board Presentation](./docs/governance/BOARD-PRESENTATION.md) → [Decision Matrix](./docs/governance/DECISION-MATRIX.md) | **30 min** |
| **C-Level / Finance** | [Resumen Ejecutivo](./docs/governance/RESUMEN-EJECUTIVO-DIRECTORES.md) → [Cost Analysis](./docs/governance/construction/ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) → [Revenue Model](./docs/governance/construction/REVENUE-MODEL-YEAR-1.md) → [Competitive](./docs/governance/construction/COMPETITIVE-ANALYSIS.md) | 50 min |
| **Product Owner** | [Construction README](./docs/governance/construction/README.md) → [FS Mapping](./docs/governance/construction/FS-TO-TS-MAPPING.md) → [Validation Matrix](./docs/governance/construction/ESTIMATION-VALIDATION-MATRIX.md) | 40 min |
| **Engineering Lead** | [Construction Index](./docs/governance/construction/ESTIMATION-INDEX.md) → [Technical Stories](./docs/governance/construction/TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md) → [RLS Deep Dive](./docs/governance/construction/README.md) | 90 min |
| **QA Lead** | [Construction README](./docs/governance/construction/README.md) → [FS Mapping](./docs/governance/construction/FS-TO-TS-MAPPING.md) → [Validation Matrix](./docs/governance/construction/ESTIMATION-VALIDATION-MATRIX.md) | 45 min |
| **DevOps / SRE** | [Service Plan](./docs/governance/project-es/SERVICE-IMPLEMENTATION-PLAN.md) → [RLS Implementation](./docs/governance/construction/README.md) | 60 min |
| **Developer** | [Construction README](./docs/governance/construction/README.md) → [Technical Stories](./docs/governance/construction/TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md) → [Service Plan](./docs/governance/project-es/SERVICE-IMPLEMENTATION-PLAN.md) | 90 min |

---

## Contribution & Governance
- **Workflow**: This repo uses [BMAD-METHOD](./AGENTS.md) for spec-driven documentation.
- **Navigation**: Visit the [**Master Index**](./docs/MASTER_INDEX.md) for the full document tree.
