# UMS — Enterprise User Management System

> **Standardized Modular Monolith for Unified Identity & Authorization.**
>
> ![Status](https://img.shields.io/badge/Status-Active-success) ![Architecture](https://img.shields.io/badge/Architecture-Modular_Monolith-blue) ![Methodology](https://img.shields.io/badge/Methodology-BMAD--METHOD-success)

---

## Language / Bilingüe
- **English** | [Español](./README.es.md)

---

## Master Navigation Index
Start here if you are new to UMS. This index gives each reader a fast route into the repository without needing to know the folder structure.

| I want to... | Start Here | Then Read |
| :--- | :--- | :--- |
| Understand the product | [Product Vision](./governance/product/product-vision.md) | [Business Context](./governance/product/business-context.md) → [Scope](./governance/product/scope.md) |
| See Epics & Priorities | [MVP Product Backlog](./governance/project/mvp-product-backlog.md) | [MVP Prioritization](./governance/roadmap/mvp-functional-prioritization.md) → [Functional Stories](./governance/requirements/functional-stories/index.md) |
| Review functional requirements | [Requirements Index](./governance/requirements/index.md) | [Functional Stories](./governance/requirements/functional-stories/index.md) → [Glossary](./governance/requirements/glossary.md) |
| Validate the data and domain model | [Conceptual Data Model](./governance/requirements/conceptual-data-model.md) | [ER Export Formats](./architecture/blueprints/er-export-formats.md) → [Database Design ER](./architecture/blueprints/database-design-er.md) |
| Understand the architecture | [Architecture Portal](./architecture/index.md) | [C4 Architecture Spec](./architecture/blueprints/architecture-spec.md) → [ADR Registry](./architecture/adrs/index.md) |
| **Plan & Execute Construction** 🚀 | [**Construction Index**](./governance/construction/ESTIMATION-INDEX.md) | [MVP Scope (ES/EN)](./governance/construction/README.md) → [Cost Analysis](./governance/construction/ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) → [Execution Models](./governance/construction/MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md) |
| Build or operate the system | [Engine Room](./src/) | [Technical Enablers](./architecture/blueprints/technical-enablers/index.md) → [Operations Portal](./operations/index.md) |
| Browse everything | [Master Index](./MASTER_INDEX.md) | Complete document tree by lifecycle phase.
## Quick Start (Engine Room)
```powershell
cd src
npm install; npx nx run app-web:dev
```

---

## Knowledge Hub
| Domain | Portal Index | Contents |
| :--- | :--- | :--- |
| **Governance** | [Governance Portal](./governance/index.md) | Product direction, standards, roadmap, project backlog, and audits. |
| **Project Delivery** | [Project Backlog](./governance/project/index.md) | MVP epics, user stories, priority order, delivery phases, and cut line. |
| **Construction Planning** 🚀 | [**Construction Index**](./governance/construction/ESTIMATION-INDEX.md) | **MVP scope (168 pts, 12 weeks), cost analysis (3 scenarios), execution models (Human vs AI-Driven), team composition, technical stories (89 TS), risk mitigation.** [See Full Index →](./governance/construction/README.md) |
| **Requirements** | [Requirements Index](./governance/requirements/index.md) | Functional stories, business glossary, permission model, and conceptual data model. |
| **Architecture** | [Architecture Portal](./architecture/index.md) | Stack, ADR registry, C4 specs, bounded contexts, and technical enablers. |
| **Infrastructure** | [Infra Setup](./infrastructure/index.md) | Docker, Kong, Kubernetes, and environment setup. |
| **Operations** | [Ops Portal](./operations/index.md) | Runbooks, observability, SQL operations, and SRE practices. |
| **Knowledge** | [Knowledge Base](./knowledge/index.md) | Recommended reading paths, POCs, research, and onboarding.
## Recommended Reading by Role
| Role | Objective | Learning Path (Direct Links) |
| :--- | :--- | :--- |
| **Executive / Director** | Business value, MVP scope, and delivery confidence | [Product Vision](./governance/product/product-vision.md) → [MVP Prioritization](./governance/roadmap/mvp-functional-prioritization.md) → [MVP Product Backlog](./governance/project/mvp-product-backlog.md) → [**Construction Index (cost, timeline, execution)**](./governance/construction/ESTIMATION-INDEX.md) |
| **Product Owner** | Functional scope, sequencing, and backlog ownership | [Product Vision](./governance/product/product-vision.md) → [Requirements](./governance/requirements/index.md) → [MVP Prioritization](./governance/roadmap/mvp-functional-prioritization.md) → [Product Backlog](./governance/project/mvp-product-backlog.md) → [**MVP Scope & TS Mapping**](./governance/construction/README.md) |
| **Business Analyst** | Business narrative, rules, and acceptance criteria | [Requirements Index](./governance/requirements/index.md) → [Functional Stories](./governance/requirements/functional-stories/index.md) → [Glossary](./governance/requirements/glossary.md) → [Product Backlog](./governance/project/mvp-product-backlog.md) |
| **Software Architect** | Technical design, decisions, and domain boundaries | [Product Vision](./governance/product/product-vision.md) → [Architecture Portal](./architecture/index.md) → [C4 Architecture Spec](./architecture/blueprints/architecture-spec.md) → [ADR Registry](./architecture/adrs/index.md) |
| **Developer (BE/FE)** | What to build, why it matters, and how it fits | [Product Vision](./governance/product/product-vision.md) → [Product Backlog](./governance/project/mvp-product-backlog.md) → [Functional Stories](./governance/requirements/functional-stories/index.md) → [Technical Enablers](./architecture/blueprints/technical-enablers/index.md) → [Engine Room](./src/) |
| **DevOps / SRE** | Environments, reliability, and observability | [Product Backlog](./governance/project/mvp-product-backlog.md) → [Infrastructure](./infrastructure/index.md) → [Operations Portal](./operations/index.md) → [Observability Strategy](./architecture/artifacts/observability-strategy.md) |
| **QA / Security** | Quality gates, risks, and verification strategy | [Product Backlog](./governance/project/mvp-product-backlog.md) → [Contract Testing](./architecture/artifacts/contract-testing-plan.md) → [Enterprise IAM](./architecture/artifacts/enterprise-iam-ums-specification.md) → [Maturity Model](./architecture/artifacts/architecture-maturity-model.md) |
| **Finance / Budget Owner** 💰 | Cost estimation, ROI, and infrastructure trade-offs | [**Cost Analysis (3 scenarios)**](./governance/construction/ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) → [**Execution Models Comparison**](./governance/construction/MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md) → [MVP Scope](./governance/construction/ESTIMATION-INDEX.md) |
| **Engineering Lead / Tech Architect** | What to build, team composition, and critical path | [Product Backlog](./governance/project/mvp-product-backlog.md) → [**Construction Index (89 TS, team, dependencies)**](./governance/construction/ESTIMATION-INDEX.md) → [Technical Stories](./governance/construction/README.md) → [Architecture Portal](./architecture/index.md) |
| **AI / Agents** | BMAD-METHOD and repository rules | [Agent Rules](./AGENTS.md) → [BMAD Audit](./architecture/artifacts/bmad-master-audit-alignment-report.md) → [Taxonomy Audit](./governance/audits/2026-05-13-taxonomy-normalization-audit.md) |

---

## 🚀 Construction Planning Portal (MVP Reduced — 12 weeks)

**Quick Navigation by Need:**

| Need | Document | Time | Key Info |
| :--- | :--- | :--- | :--- |
| **📋 See complete construction plan** | [Construction Index](./governance/construction/ESTIMATION-INDEX.md) | 5 min | Master bilingual index with role-based reading paths |
| **👥 Understand MVP scope & team** | [README (Construction)](./governance/construction/README.md) | 10 min | 8 FS, 168 pts, 4-person team, 12 weeks, DoD per épica |
| **💰 Budget & Cost Analysis** | [Cost/Benefit Analysis](./governance/construction/ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) | 15 min | 10 profiles, 3 scenarios (On-Prem/Hybrid/Cloud), S/ 182K recommended, ROI 84% Y1 |
| **🎯 Human vs AI-Driven Decision** | [Execution Models](./governance/construction/MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md) | 20 min | Human (S/ 182K, 12w, ROI 382%) vs AI (S/ 141K, 8.5w, ROI 729%), risk matrix |
| **🔍 AI Timeline Credibility** | [AI Timeline Justification](./governance/construction/JUSTIFICACION-TIMELINE-AI-DRIVEN.md) | 10 min | Honest breakdown: agents 12%, validation 59%, setup 15%, rework 10% |
| **📊 Technical story details** | [Technical Stories & Team](./governance/construction/TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md) | 60 min | 89 TS across 8 épicas, team profiles, critical paths |
| **✅ FS-to-TS traceability** | [FS Mapping](./governance/construction/FS-TO-TS-MAPPING.md) | 30 min | Requirements coverage validation, acceptance criteria alignment |
| **🔒 RLS & Security Design** | [RLS Deep Dive](./governance/construction/TE-03-rls-sql-server-implementation.md) | 30 min | Two-layer model, SESSION_CONTEXT, error handling, failover |
| **🛠️ Architecture & Implementation** | [Service Plan](./governance/construction/SERVICE-IMPLEMENTATION-PLAN.md) | 40 min | .NET 8 structure, test pyramid, CI/CD, DI patterns |

---

**Reading Paths by Role:**

| Role | Path | Duration |
| :--- | :--- | :--- |
| **💼 C-Level / Finance** | [Cost Analysis](./governance/construction/ANALISIS-COSTO-BENEFICIO-MVP-REDUCIDO.md) → [Execution Models](./governance/construction/MODELO-EJECUCION-HUMANO-VS-AI-DRIVEN.md) → [Timeline Check](./governance/construction/JUSTIFICACION-TIMELINE-AI-DRIVEN.md) | 45 min |
| **👨‍💼 Product Owner** | [Construction README](./governance/construction/README.md) → [FS Mapping](./governance/construction/FS-TO-TS-MAPPING.md) → [Validation Matrix](./governance/construction/ESTIMATION-VALIDATION-MATRIX.md) | 40 min |
| **🏗️ Engineering Lead** | [Construction Index](./governance/construction/ESTIMATION-INDEX.md) → [Technical Stories](./governance/construction/TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md) → [RLS Deep Dive](./governance/construction/TE-03-rls-sql-server-implementation.md) | 90 min |
| **🧪 QA Lead** | [Construction README](./governance/construction/README.md) → [FS Mapping](./governance/construction/FS-TO-TS-MAPPING.md) → [Validation Matrix](./governance/construction/ESTIMATION-VALIDATION-MATRIX.md) | 45 min |
| **🚀 DevOps / SRE** | [Service Plan](./governance/construction/SERVICE-IMPLEMENTATION-PLAN.md) → [RLS Implementation](./governance/construction/TE-03-rls-sql-server-implementation.md) | 60 min |
| **👨‍💻 Developer** | [Construction README](./governance/construction/README.md) → [Technical Stories](./governance/construction/TECHNICAL-STORIES-AND-TEAM-COMPOSITION.md) → [Service Plan](./governance/construction/SERVICE-IMPLEMENTATION-PLAN.md) | 90 min |

---

## Contribution & Governance
- **Workflow**: This repo uses [BMAD-METHOD](./AGENTS.md) for spec-driven documentation.
- **Navigation**: Visit the [**Master Index**](./MASTER_INDEX.md) for the full document tree.
