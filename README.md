# UMS — Enterprise User Management System

> **Standardized Modular Monolith for Unified Identity & Authorization.**
>
> ![Status](https://img.shields.io/badge/Status-Active-success) ![Architecture](https://img.shields.io/badge/Architecture-Modular_Monolith-blue) ![Methodology](https://img.shields.io/badge/Methodology-BMAD--METHOD-success)

---

## 🌍 Language / Bilingüe
- **English** | [Español](./README.es.md)

---

## 🚀 Quick Start (Engine Room)
Ready to build? Run these from the `src/` directory:
```powershell
# Frontend: React 18 + Vite
npm install; npx nx run app-web:dev
# Backend: .NET 8 LTS
dotnet build ./apps/app-api-dotnet/Ums.sln
```

---

## 🗺️ Knowledge Navigation Hub
Access all project domains directly. No more folder digging.

| Domain | Index & Portal | Scope |
| :--- | :--- | :--- |
| ⚖️ **Governance** | [Governance Portal](./governance/index.md) | Business Vision, Product Roadmap, & Stakeholders. |
| 🏗️ **Architecture** | [Architecture Portal](./architecture/index.md) | ADR Registry, C4 Spec, & Context Maps. |
| 📋 **Requirements** | [Requirements Index](./governance/requirements/index.md) | Glossary, User Stories, & Domain Models. |
| 🛠️ **Infrastructure** | [Infra Setup](./infrastructure/index.md) | Docker, Kong Gateway, & K8s Config. |
| 🚀 **Operations** | [Ops Portal](./operations/index.md) | Observability (OTel), Monitoring, & SRE. |
| 🎓 **Knowledge** | [Knowledge Base](./knowledge/index.md) | POCs, Research, & Onboarding Guides. |

---

## 👥 Recommended Reading by Role
Tailored onboarding paths to maximize context and minimize noise.

<details>
<summary><b>🏢 Executive / Director</b></summary>

- **Goal**: Understand business value, vision, and strategic impact.
1. [Product Vision](./governance/product/product-vision.md)
2. [Business Context](./governance/product/business-context.md)
3. [Product Roadmap](./governance/roadmap/index.md)
</details>

<details>
<summary><b>📦 Product Owner / Analyst</b></summary>

- **Goal**: Define behavior, requirements, and functional boundaries.
1. [Requirements Index](./governance/requirements/index.md)
2. [User Stories Registry](./governance/requirements/functional-stories/index.md)
3. [Glossary (Ubiquitous Language)](./governance/requirements/glossary.md)
</details>

<details>
<summary><b>🏗️ Software Architect / Team Lead</b></summary>

- **Goal**: Ensure technical coherence, scalability, and decision traceability.
1. [Architecture Spec (C4 Models)](./architecture/blueprints/architecture-spec.md)
2. [ADR Registry (Decision Records)](./architecture/adrs/index.md)
3. [Context Map & Boundaries](./architecture/blueprints/bounded-context-map.md)
4. [Engineering Standards](./architecture/artifacts/engineering-standards.md)
</details>

<details>
<summary><b>⚙️ Backend & Frontend Developers</b></summary>

- **Goal**: Implementation, code quality, and technical patterns.
1. [Engineering Standards](./architecture/artifacts/engineering-standards.md)
2. [Technology Stack](./architecture/blueprints/stack.md)
3. [Source Code Engine Room](./src/)
4. [Functional Story Registry](./governance/requirements/functional-stories/index.md)
</details>

<details>
<summary><b>☁️ DevOps / SRE / Security</b></summary>

- **Goal**: Infrastructure, deployment, security gates, and observability.
1. [Infrastructure Setup](./infrastructure/index.md)
2. [Observability Strategy](./architecture/artifacts/observability-strategy.md)
3. [IAM Security Specification](./architecture/artifacts/enterprise-iam-ums-specification.md)
4. [Operations Portal](./operations/index.md)
</details>

<details>
<summary><b>🧪 QA / Automation</b></summary>

- **Goal**: Testing strategies, contract validation, and quality gates.
1. [Contract Testing Plan](./architecture/artifacts/contract-testing-plan.md)
2. [Architecture Maturity Model](./architecture/artifacts/architecture-maturity-model.md)
3. [User Stories (Acceptance Criteria)](./governance/requirements/functional-stories/index.md)
</details>

<details>
<summary><b>🤖 AI & Automation Contributors</b></summary>

- **Goal**: Efficient collaboration with AI agents using BMAD-METHOD.
1. [Agent Rules (AGENTS.md)](./AGENTS.md)
2. [BMAD-METHOD Standard](./architecture/artifacts/bmad-master-audit-alignment-report.md)
3. [Repository Taxonomy Guide](./governance/audits/2026-05-13-taxonomy-normalization-audit.md)
</details>

---

## 🤝 Contribution & Governance
- **Workflow**: This repo uses [BMAD-METHOD](./AGENTS.md) for spec-driven documentation.
- **Full Navigation**: Visit the [**Master Index**](./MASTER_INDEX.md) for a complete document tree.
