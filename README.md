# UMS — Enterprise User Management System

> **High-scale Modular Monolith for Unified Identity & Authorization.**
>
> ![Architecture](https://img.shields.io/badge/Architecture-Modular_Monolith-blue) ![Language](https://img.shields.io/badge/Language-.NET_8_/_React-informational) ![Methodology](https://img.shields.io/badge/Methodology-BMAD--METHOD-success)

---

## 🌍 Language / Bilingüe
- 🇺🇸 [English](./README.md)
- 🇪🇸 [Español](./README.es.md)

---

## 🚀 Quick Start
For developers ready to spin up the environment:
```powershell
# Enter the Engine Room
cd src
# Start Frontend
npm install; npx nx run app-web:dev
# Build Backend
dotnet build ./apps/app-api-dotnet/Ums.sln
```

---

## 🏛️ Architecture & Principles
Built on **Clean Architecture**, **DDD**, and **Hexagonal** patterns.
- **Pattern**: Progressive/Modular Monolith with strict Bounded Contexts.
- **Persistence**: PostgreSQL 16 with Row-Level Security (RLS).
- **Security**: OAuth2/OIDC + Multi-tenant Authorization Graph.

---

## 📍 Global Documentation Hub

| Domain | Description | Content |
| :--- | :--- | :--- |
| [⚖️ **Governance**](./governance/) | Business & Product Strategy | Vision, Roadmap, Requirements (Phases 00, 01, 05). |
| [🏗️ **Architecture**](./architecture/) | Technical Blueprint | ADRs, C4 Models, Engineering Standards (Phases 02, 03, 04). |
| [🛠️ **Infrastructure**](./infrastructure/) | Platform & IaC | Docker, Kong Gateway, Kubernetes configs. |
| [🚀 **Operations**](./operations/) | Monitoring & SRE | Observability (OTel/Tempo), Grafana Dashboards, SQL init. |
| [🎓 **Knowledge**](./knowledge/) | Learning Center | POCs, Onboarding guides, Reference research. |
| [💻 **Source Code**](./src/) | Implementation | Product Source (`apps/` and `libs/`). |

---

## 👥 Recommended Reading by Role
Select your profile for a tailored onboarding path:

<details>
<summary><b>📦 Product Owner / Business</b></summary>

1. [Product Vision](./governance/product/product-vision.md)
2. [Business Context & Solution](./governance/product/business-context.md)
3. [Functional Stories](./governance/requirements/functional-stories/)
4. [Product Roadmap](./governance/roadmap/versioning-and-audit-strategy.md)
</details>

<details>
<summary><b>🏗️ Software Architect</b></summary>

1. [Architecture Spec (C4 Models)](./architecture/blueprints/architecture-spec.md)
2. [ADR Registry](./architecture/adrs/)
3. [Technology Stack](./architecture/blueprints/stack.md)
4. [Context Map](./architecture/blueprints/bounded-context-map.md)
</details>

<details>
<summary><b>⚙️ Backend Developer</b></summary>

1. [Engineering Standards](./architecture/artifacts/engineering-standards.md)
2. [Backend Source Code](./src/apps/app-api-dotnet/)
3. [Technical Enablers](./architecture/blueprints/technical-enablers/)
4. [.NET Migration Plan](./architecture/blueprints/dotnet-migration-and-tech-stack-plan.md)
</details>

<details>
<summary><b>💻 Frontend Developer</b></summary>

1. [Frontend Source Code](./src/apps/app-web/)
2. [Web Console Product Scope](./architecture/artifacts/ums-web-console-product-scope.md)
3. [Engineering Standards](./architecture/artifacts/engineering-standards.md)
</details>

<details>
<summary><b>☁️ DevOps / SRE</b></summary>

1. [Infrastructure Setup](./infrastructure/)
2. [Observability Strategy](./architecture/artifacts/observability-strategy.md)
3. [Operational Assets](./operations/)
4. [Kong Gateway Config](./architecture/artifacts/kong-plugins-configuration-guide.md)
</details>

<details>
<summary><b>🛡️ Security / QA</b></summary>

1. [Enterprise IAM Spec](./architecture/artifacts/enterprise-iam-ums-specification.md)
2. [Contract Testing Plan](./architecture/artifacts/contract-testing-plan.md)
3. [MFA & Security Spec](./architecture/artifacts/mfa-passwordless-security-spec.md)
4. [Multi-Tenant Governance](./architecture/artifacts/enterprise-multitenant-governance-report.md)
</details>

---

## 🤝 Contributing
Please read the [**Master Index**](./MASTER_INDEX.md) and the [**Agent Rules**](./AGENTS.md) before making any changes. This project follows the **BMAD-METHOD** for spec-driven development.
