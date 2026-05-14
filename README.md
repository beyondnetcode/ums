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
| **Requirements** | [Requirements Index](./governance/requirements/index.md) | Functional stories, business glossary, permission model, and conceptual data model. |
| **Architecture** | [Architecture Portal](./architecture/index.md) | Stack, ADR registry, C4 specs, bounded contexts, and technical enablers. |
| **Infrastructure** | [Infra Setup](./infrastructure/index.md) | Docker, Kong, Kubernetes, and environment setup. |
| **Operations** | [Ops Portal](./operations/index.md) | Runbooks, observability, SQL operations, and SRE practices. |
| **Knowledge** | [Knowledge Base](./knowledge/index.md) | Recommended reading paths, POCs, research, and onboarding.
## Recommended Reading by Role
| Role | Objective | Learning Path (Direct Links) |
| :--- | :--- | :--- |
| **Executive / Director** | Business value, MVP scope, and delivery confidence | [Product Vision](./governance/product/product-vision.md) → [MVP Prioritization](./governance/roadmap/mvp-functional-prioritization.md) → [MVP Product Backlog](./governance/project/mvp-product-backlog.md) |
| **Product Owner** | Functional scope, sequencing, and backlog ownership | [Product Vision](./governance/product/product-vision.md) → [Requirements](./governance/requirements/index.md) → [MVP Prioritization](./governance/roadmap/mvp-functional-prioritization.md) → [Product Backlog](./governance/project/mvp-product-backlog.md) |
| **Business Analyst** | Business narrative, rules, and acceptance criteria | [Requirements Index](./governance/requirements/index.md) → [Functional Stories](./governance/requirements/functional-stories/index.md) → [Glossary](./governance/requirements/glossary.md) → [Product Backlog](./governance/project/mvp-product-backlog.md) |
| **Software Architect** | Technical design, decisions, and domain boundaries | [Product Vision](./governance/product/product-vision.md) → [Architecture Portal](./architecture/index.md) → [C4 Architecture Spec](./architecture/blueprints/architecture-spec.md) → [ADR Registry](./architecture/adrs/index.md) |
| **Developer (BE/FE)** | What to build, why it matters, and how it fits | [Product Vision](./governance/product/product-vision.md) → [Product Backlog](./governance/project/mvp-product-backlog.md) → [Functional Stories](./governance/requirements/functional-stories/index.md) → [Technical Enablers](./architecture/blueprints/technical-enablers/index.md) → [Engine Room](./src/) |
| **DevOps / SRE** | Environments, reliability, and observability | [Product Backlog](./governance/project/mvp-product-backlog.md) → [Infrastructure](./infrastructure/index.md) → [Operations Portal](./operations/index.md) → [Observability Strategy](./architecture/artifacts/observability-strategy.md) |
| **QA / Security** | Quality gates, risks, and verification strategy | [Product Backlog](./governance/project/mvp-product-backlog.md) → [Contract Testing](./architecture/artifacts/contract-testing-plan.md) → [Enterprise IAM](./architecture/artifacts/enterprise-iam-ums-specification.md) → [Maturity Model](./architecture/artifacts/architecture-maturity-model.md) |
| **AI / Agents** | BMAD-METHOD and repository rules | [Agent Rules](./AGENTS.md) → [BMAD Audit](./architecture/artifacts/bmad-master-audit-alignment-report.md) → [Taxonomy Audit](./governance/audits/2026-05-13-taxonomy-normalization-audit.md) |

## Contribution & Governance
- **Workflow**: This repo uses [BMAD-METHOD](./AGENTS.md) for spec-driven documentation.
- **Navigation**: Visit the [**Master Index**](./MASTER_INDEX.md) for the full document tree.
