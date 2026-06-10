<div align="center">

# UMS: Enterprise User Management System

> **Bilingual Navigation:** [Versión en Español](./docs/README.es.md)

[![Status](https://img.shields.io/badge/Status-Active-brightgreen?style=for-the-badge)]()
[![Platform](https://img.shields.io/badge/.NET_10_%7C_PostgreSQL_%7C_React_18-informational?style=for-the-badge)]()
[![Architecture](https://img.shields.io/badge/Evolith-Satellite_Product-blueviolet?style=for-the-badge)](https://github.com/beyondnetcode/evolith_arch32)
[![ADRs](https://img.shields.io/badge/ADRs-66_decisions-orange?style=for-the-badge)](./docs/architecture/adrs/)
[![License](https://img.shields.io/badge/License-Proprietary-red?style=for-the-badge)]()

<br/>

<a href="./docs/diagrams/evolith-ums-satellite.png" title="Evolith E2E Architecture - UMS Satellite Product - click to enlarge">
  <img src="./docs/diagrams/evolith-ums-satellite.png"
       alt="Evolith E2E Architecture - UMS Satellite Product"
       width="780"
       style="border-radius: 8px; box-shadow: 0 4px 20px rgba(0,0,0,0.3);" />
</a>

<sub>Evolith E2E Architecture Framework - UMS official satellite product - click to enlarge</sub>

<br/>

**UMS is a modular monolith for identity, authorization, configuration, approvals, compliance, IGA, and audit.**<br/>
Built on **.NET 10, PostgreSQL, EF Core through Npgsql, React 18, TypeScript, and Nx**.<br/>
It specializes the [Evolith](https://github.com/beyondnetcode/evolith_arch32) corporate architecture reference for a product-grade user management system.

> *Inherit the standard, specialize the product.*

</div>

---

## Start Here

<details>
<summary><strong>Primary entry points</strong></summary>

- [Product Vision](./docs/governance/product/product-vision.md) - strategy, product goals, and business positioning.
- [Architecture Portal](./docs/architecture/index.md) - architectural overview, ADRs, blueprints, and applied reference material.
- [Domain Model](./docs/domain/index.md) - bounded contexts, aggregates, entities, and domain rules.
- [Functional Stories](./docs/governance/requirements/functional-stories/index.md) - business-readable product backlog.
- [Master Index](./docs/MASTER_INDEX.md) - complete documentation navigation.
- [Evolith Upstream](https://github.com/beyondnetcode/evolith_arch32) - corporate reference base inherited by UMS.

</details>

<details>
<summary><strong>Getting started by role</strong></summary>

- **Architects:** start with [Architecture Portal](./docs/architecture/index.md), then review [ADR Registry](./docs/architecture/adrs/) and [Traceability Matrix](./docs/architecture/traceability-matrix.md).
- **Backend developers:** start with [API .NET Reference](./docs/architecture/api-dotnet/README.md), then review [Domain Aggregates](./docs/domain/index.md) and [.NET SDK](./docs/sdk/dotnet/README.md).
- **Frontend developers:** start with [Frontend Clean Architecture ADR](./docs/architecture/adrs/0056-clean-architecture-frontend.md), then review [TypeScript SDK](./docs/sdk/typescript/README.md) and [State Management ADR](./docs/architecture/adrs/0057-zustand-tanstack-query-state.md).
- **Product and PM:** start with [Product Vision](./docs/governance/product/product-vision.md), then review [Scope](./docs/governance/product/scope.md), [Objectives](./docs/governance/product/objectives.md), and [Gap Tracker](./docs/governance/project/functional-story-gap-tracker.md).
- **DevOps and SRE:** start with [Infrastructure Plan](./infra/infrastructure_plan.md), then review [Operations Portal](./docs/operations/index.md), [Runbooks](./docs/operations/runbooks/index.md), and [Metrics](./docs/operations/metrics/index.md).
- **AI contributors:** start with [AGENTS.md](./AGENTS.md), then review [Documentation Control Agents](./docs/governance/documentation-control-agents.md) and [ADR Template](./docs/governance/sdlc/adr-template.md).

</details>

## SDLC Navigation

Open the lifecycle area you are working in. Each section groups the documents and repository anchors that support its gate.

<details>
<summary><strong>Phase 00 - Product and Governance</strong></summary>

| Documento | Tipo |
| :--- | :--- |
| [Product Vision](./docs/governance/product/product-vision.md) | Guía |
| [Business Context](./docs/governance/product/business-context.md) | Guía |
| [Scope and Boundaries](./docs/governance/product/scope.md) | Guía |
| [Objectives](./docs/governance/product/objectives.md) | Guía |
| [Governance Hub](./docs/governance/index.md) | Índice |
| [Stakeholders](./docs/governance/product/stakeholders.md) | Registro |

</details>

<details>
<summary><strong>Phase 01 - Requirements</strong></summary>

| Documento | Tipo |
| :--- | :--- |
| [Functional Story Standard](./docs/governance/requirements/functional-stories/functional-story-standard.md) | Estándar |
| [Requirements Hub](./docs/governance/requirements/index.md) | Índice |
| [Functional Stories](./docs/governance/requirements/functional-stories/index.md) | Índice |
| [Permission Matrix Example](./docs/governance/requirements/permission-matrix-example.md) | Matriz |
| [Conceptual Data Model](./docs/governance/requirements/conceptual-data-model.md) | Referencia |
| [Glossary](./docs/governance/requirements/glossary.md) | Referencia |

</details>

<details>
<summary><strong>Phase 02 - Design and Architecture</strong></summary>

| Documento | Tipo |
| :--- | :--- |
| [Canonical Patterns](./docs/architecture/artifacts/canonical-patterns/index.md) | Guía |
| [Architecture Portal](./docs/architecture/index.md) | Índice |
| [ADR Registry](./docs/architecture/adrs/) | Índice |
| [DDD Design Hub](./docs/governance/construction/ddd-design/index.md) | Índice |
| [Traceability Matrix](./docs/architecture/traceability-matrix.md) | Matriz |
| [Evolith ADR Matrix](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/adr-matrix.md) | Matriz |
| [Architecture Overview](./docs/architecture/overview.md) | Referencia |
| [Blueprints](./docs/architecture/blueprints/) | Referencia |

</details>

<details>
<summary><strong>Phase 03 - Construction</strong></summary>

| Documento | Tipo |
| :--- | :--- |
| [Construction Hub](./docs/governance/construction/index.md) | Índice |
| [SDK Portal](./docs/sdk/index.md) | Índice |
| [Bounded Context Map](./docs/governance/construction/ddd-design/01-bounded-context-map.md) | Referencia |
| [Cross-Context Flows](./docs/governance/construction/ddd-design/10-cross-context-flows.md) | Referencia |
| [DDD Primitives](./docs/governance/construction/ddd-design/11-ddd-primitives.md) | Referencia |
| [API .NET Applied Reference](./docs/architecture/api-dotnet/ums-api-dotnet-applied-reference.md) | Referencia |
| [Project Backlog](./docs/governance/project/index.md) | Registro |

</details>

<details>
<summary><strong>Phase 04 - Validation and QA</strong></summary>

| Documento | Tipo |
| :--- | :--- |
| [Performance Testing Plan](./docs/governance/testing/performance-testing-plan.md) | Guía |
| [QA Report](./docs/qa/qa_report.md) | Registro |
| [Unit Testing Results](./docs/governance/testing/unit-testing-results.md) | Registro |
| [Integration Testing Results](./docs/governance/testing/integration-testing-results.md) | Registro |
| [Performance Testing Results](./docs/governance/testing/performance-testing-results.md) | Registro |
| [QA Evidences](./docs/qa/evidences/) | Registro |

</details>

<details>
<summary><strong>Phase 05 - Delivery and Operations</strong></summary>

| Documento | Tipo |
| :--- | :--- |
| [Runbooks](./docs/operations/runbooks/index.md) | Guía |
| [Kubernetes Deployment Plan](./infra/UMS_K8s_Deployment_Plan.md) | Guía |
| [Infrastructure Plan](./infra/infrastructure_plan.md) | Guía |
| [Implementation Plan](./infra/implementation_plan.md) | Guía |
| [Documentation Release Process](./docs/releases/bmad-documentation-release-process.md) | Guía |
| [Operations Portal](./docs/operations/index.md) | Índice |
| [Metrics](./docs/operations/metrics/index.md) | Referencia |

</details>

## Cross-Cutting References

<details>
<summary><strong>Architecture, domain, and product reference</strong></summary>

- [Identity Domain](./docs/domain/identity/index.md)
- [Authorization Domain](./docs/domain/authorization/index.md)
- [Configuration Domain](./docs/domain/configuration/index.md)
- [Approvals Domain](./docs/domain/approvals/index.md)
- [IGA Domain](./docs/domain/iga/index.md)
- [Audit Domain](./docs/domain/audit/index.md)
- [Consistency Rules](./docs/domain/consistency-rules/index.md)
- [SDK Contracts](./docs/sdk/contracts/schema-overview.md)
- [Documentation Standards](./docs/STANDARDS.md)
- [Bilingual Documentation Control](./docs/governance/documentation-control-agents.md)

</details>

<details>
<summary><strong>UMS and Evolith inheritance</strong></summary>

- UMS inherits reusable architecture standards, governance rules, ADR patterns, and documentation practices from [Evolith](https://github.com/beyondnetcode/evolith_arch32).
- UMS keeps product-specific implementation, bounded contexts, schemas, seed strategy, and runtime behavior in this repository.
- Product ADRs may be promoted upstream when UMS provides executable evidence that the decision is reusable across products.
- Multi-tenancy is enforced primarily at the application layer. PostgreSQL policies, constraints, schema ownership, and row-level security are secondary infrastructure failsafes.

</details>

## Tools and Automation

<details>
<summary><strong>Local development commands</strong></summary>

Run technical commands from `src/` unless the command explicitly targets the backend solution.

```bash
# Install frontend dependencies
cd src
npm install

# Frontend: React 18 and Vite
npx nx run app-web:dev

# Backend: .NET 10
cd apps/ums.api
dotnet build
dotnet run

# Backend tests
dotnet test
```

</details>

<details>
<summary><strong>Documentation validation</strong></summary>

```bash
# From the repository root
python3 .bmad-core/scripts/cleanup_markdown_encoding.py

# From src/, when Context7 setup is needed
cd src
npx ctx7 setup
```

Documentation changes must keep English and Spanish artifacts synchronized, preserve UTF-8 integrity, and avoid decorative icons or non-standard Markdown characters.

</details>

---

## Contribution

Before contributing, read:

- [AGENTS.md](./AGENTS.md) - agent rules and repository conventions.
- [Standards](./docs/STANDARDS.md) - engineering and documentation standards.
- [ADR Template](./docs/governance/sdlc/adr-template.md) - how to propose a decision.
- [Child Repository Inheritance Guide](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/onboarding/child-repository-inheritance-guide.md) - how UMS inherits from Evolith.

---

## License

This repository is proprietary unless a separate license file states otherwise.

---

<div align="center">
  <sub>UMS - Enterprise User Management System | Evolith Satellite Product | .NET 10, React 18, PostgreSQL</sub>
</div>
