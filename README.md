# 🏛️ User Management System (UMS) - Enterprise Reference Monorepo

> 🌍 **Language Selector:** [🇺🇸 English](./README.md) | [🇪🇸 Español](./README.es.md)

---

## 💡 1. Introduction & Key Objectives

Welcome to the **User Management System (UMS)**, an enterprise-grade, highly resilient modular monolith engineered to manage corporate identities, access control, and user lifecycles. 

This solution is a .NET 8 LTS implementation that serves as a concrete instance of the [Unified Corporate Polyglot Reference Architecture](https://github.com/beyondnetcode/arc32_progresive_monolith). UMS implements a "Monolith-First" evolutionary design using the **spec-driven AI strategy BMAD-METHOD**, allowing business domains to be extracted as independent microservices in the future (using Dapr, gRPC, or event-driven architectures) with zero rewrite of core domain rules.

### 🎯 Primary Objectives:
*   **Strict Decoupling & Clean Architecture:** Domain-driven design using Hexagonal (Ports & Adapters) principles to ensure absolute framework independence.
*   **Progressive Evolution:** Engineered within a highly decoupled Nx Monorepo, facilitating a seamless transition to distributed microservices when KPIs dictate.
*   **Agnostic Identity Isolation:** Decouples core authorization from commercial Identity Providers (Auth0, Keycloak, Entra ID), eliminating vendor lock-in.
*   **Zero-Trust Authorization Kernel:** Delivers fine-grained contextual permissions (RBAC/ABAC) and high-performance auth graphs compiling in under <5ms.

---

## 🧭 2. Unified Master Navigation Hub

🚀 **Do not explore the directories randomly.** All technical and compliance paths are organized explicitly by persona:

1.  👉 **[Global Master Index](./MASTER_INDEX.md)**: The canonical entry point. Find your exact compulsory reading path according to your role (Vendor, Dev, Architect, PM).
2.  🏛️ **[bMAD Master Documentation](./src/ums-workspace/docs/en/index.md)**: The unified central index mapping Phase 00 through Phase 05.
3.  📜 **[ADR Registry Hub](./src/ums-workspace/docs/en/index.md#📜-phase-03---architectural-decision-records-adrs)**: Direct access to the 29 active Architectural Decision Records (ADRs).

---

## ⚠️ 3. Critical Disclaimers & Usage Recommendations

To interact safely with the UMS codebase, all developers MUST respect the following guidelines:

### 🛑 Important Disclaimers:
*   **Auth-Provider Agnostic:** Designed to work both as a standalone authorization block and in tandem with enterprise identity providers.
*   **Reference Instance:** Serves as the standard baseline for .NET/C# modular applications inside the corporation. It prioritizes demonstrative cleanliness over dense, optimized micro-efficiencies.

### ✅ Crucial Usage Recommendations:
1.  **Never Bypass Ports:** Never inject external framework (ASP.NET Core controllers), Database (EF Core), or library dependencies directly into core `/domain` logic folders.
2.  **Sync with the ADR Base:** Every technical design deviation demands consultation of the existing authorized architectural baseline before implementation.
3.  **Adopt Docs-as-Code:** Keep documentation maps updated following the strict **bMAD numerical phases** (00-Product, 01-Requirements, 02-Architecture, 03-ADRs, 04-Artifacts, 05-Roadmap).

---

## ⚡ 4. High-Level Architecture & Ecosystem Quick Map

### 🚀 Technology Stack (Horizon 2026)

| Component | Technology / Framework | Role |
| :--- | :--- | :--- |
| **Backend Core** | .NET 8 LTS / ASP.NET Core | Modular Monolith / Transactional API |
| **Frontend** | React (v18) / Vite / Zustand / Query | Unified Enterprise Portal |
| **Monorepo Engine** | Nx / npm Workspaces / .NET SLN | Dependency Orchestration & Build Performance |
| **Data Layer** | PostgreSQL 16 / EF Core | Persistent Identity and Permission Graph |
| **Security** | CodeQL / SonarJS / CSharpier / ESLint | Automated CI/CD Quality Gates |

### 🛠️ Fast Shortcuts Directory
*   🏛️ **[Corporate Reference Blueprint](https://github.com/beyondnetcode/arc32_progresive_monolith)**: Official corporate specifications for polyglot systems.
*   📊 **[Gap Analysis & Debt Strategy](./src/ums-workspace/docs/en/04-artifacts/gap-analysis-and-optimization-plan.md)**: Architecture assessment against 16 Enterprise Criteria.
*   ⚙️ **[Observability Strategy](./src/ums-workspace/docs/en/04-artifacts/observability-strategy.md)**: Zero-cost telemetry using OpenTelemetry and Grafana Loki.
*   📈 **[Release Roadmap](./src/ums-workspace/docs/en/05-roadmap/versioning-and-audit-strategy.md)**: Automated Semantic Versioning strategy.

---

🤖 **AI-Augmented Enablement:** This repository natively supports autonomous AI agents and LLM-assisted development following the BMAD Harness engineering standard.
👉 **[Explore AGENTS.md Developer Harness](./AGENTS.md)**

