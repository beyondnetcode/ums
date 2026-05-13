# UMS — Enterprise User Management System

> **Standardized Modular Monolith for Unified Identity & Authorization.**
>
> ![Status](https://img.shields.io/badge/Status-Active-success) ![Architecture](https://img.shields.io/badge/Architecture-Modular_Monolith-blue) ![Methodology](https://img.shields.io/badge/Methodology-BMAD--METHOD-success)

---

## 🌍 Language / Bilingüe
- **English** | [Español](./README.es.md)

---

## 🚀 Quick Start (Engine Room)
```powershell
cd src
npm install; npx nx run app-web:dev
```

---

## 📍 Knowledge Hub
| Domain | Portal Index | Contents |
| :--- | :--- | :--- |
| ⚖️ **Governance** | [Governance Portal](./governance/index.md) | Vision, Context, Roadmap, & Audits. |
| 🏗️ **Architecture** | [Architecture Portal](./architecture/index.md) | ADR Registry, C4 Specs, & Context Maps. |
| 📋 **Requirements** | [Requirements Index](./governance/requirements/index.md) | User Stories, Glossary, & Domain Models. |
| 🛠️ **Infrastructure** | [Infra Setup](./infrastructure/index.md) | Docker, Kong, & Kubernetes. |
| 🚀 **Operations** | [Ops Portal](./operations/index.md) | Observability (OTel), SQL, & SRE. |
| 🎓 **Knowledge** | [Knowledge Base](./knowledge/index.md) | POCs, Research, & Onboarding. |

---

## 👥 Recommended Reading by Role
| Role | Objective | Learning Path (Direct Links) |
| :--- | :--- | :--- |
| **Executive / Director** | Business Strategy & Value | [Vision](./governance/product/product-vision.md) → [Context](./governance/product/business-context.md) → [Roadmap](./governance/roadmap/index.md) |
| **Product Owner** | Requirements & Functional Scope | [Requirements](./governance/requirements/index.md) → [User Stories](./governance/requirements/functional-stories/index.md) → [Glossary](./governance/requirements/glossary.md) |
| **Software Architect** | Technical Design & Traceability | [C4 Specs](./architecture/blueprints/architecture-spec.md) → [ADR Registry](./architecture/adrs/index.md) → [Boundaries](./architecture/blueprints/bounded-context-map.md) |
| **Developer (BE/FE)** | Patterns, Standards & Code | [Standards](./architecture/artifacts/engineering-standards.md) → [Stories](./governance/requirements/functional-stories/index.md) → [Engine Room](./src/) |
| **DevOps / SRE** | Infra, Security & Observability | [Infra Setup](./infrastructure/index.md) → [Ops Portal](./operations/index.md) → [IAM Spec](./architecture/artifacts/enterprise-iam-ums-specification.md) |
| **QA / Security** | Quality Gates & Security Spec | [Testing Plan](./architecture/artifacts/contract-testing-plan.md) → [IAM Spec](./architecture/artifacts/enterprise-iam-ums-specification.md) → [Maturity](./architecture/artifacts/architecture-maturity-model.md) |
| **AI / Agents** | BMAD-METHOD & Automation | [Agent Rules](./AGENTS.md) → [BMAD Standard](./architecture/artifacts/bmad-master-audit-alignment-report.md) → [Taxonomy Audit](./governance/audits/2026-05-13-taxonomy-normalization-audit.md) |

---

## 🤝 Contribution & Governance
- **Workflow**: This repo uses [BMAD-METHOD](./AGENTS.md) for spec-driven documentation.
- **Navigation**: Visit the [**Master Index**](./MASTER_INDEX.md) for the full document tree.
