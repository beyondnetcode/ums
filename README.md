# UMS — Enterprise User Management System

Enterprise-grade **Modular Monolith** designed for high-scale authorization and identity management. Built using **Clean Architecture**, **DDD (Domain-Driven Design)**, and **Hexagonal patterns**.

---

## 🧭 Project Navigation
Access the different layers of the product lifecycle:

### 📋 1. Governance & Requirements
- **[Vision & Context](./governance/product/business-context.md)**: Problem statement and solution overview.
- **[Product Roadmap](./governance/roadmap/versioning-and-audit-strategy.md)**: Versioning strategy and audit logs.
- **[Domain Requirements](./governance/requirements/glossary.md)**: Ubiquitous language and conceptual models.

### 🏗️ 2. Engineering & Architecture
- **[Architecture Blueprints](./architecture/blueprints/architecture-spec.md)**: C4 Models and technical specifications.
- **[ADR Registry](./architecture/adrs/)**: Historical record of architectural decisions.
- **[Engineering Standards](./architecture/artifacts/engineering-standards.md)**: Quality gates and coding standards.

### 🛠️ 3. Infrastructure & Ops
- **[DevOps & IaC](./infrastructure/)**: Docker, Kong Gateway, and K8s configs.
- **[Observability](./operations/)**: Monitoring, Tracing (Tempo/OTel), and Dashboards.

### 💻 4. Implementation (Source Code)
- **[Frontend Portal](./src/apps/app-web/)**: React 18 + Vite application.
- **[Backend API](./src/apps/app-api-dotnet/)**: .NET 8 LTS Core API.
- **[Shared Libs](./src/libs/)**: Internal domain and technical libraries.

---

### 🏛️ Technical DNA
- **Pattern**: Progressive/Modular Monolith with strict Bounded Contexts.
- **Security**: Row-Level Security (RLS) + OAuth2/OIDC.
- **Methodology**: **BMAD-METHOD** for AI-augmented documentation traceability.

### 🚦 Quick Start
```powershell
# Enter the Engine Room and Start
cd src
npm install; npx nx run app-web:dev
```

---
Detailed navigation is available in the [**Master Index**](./MASTER_INDEX.md).
