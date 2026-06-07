<div align="center">

# UMS — Enterprise User Management System

> **Bilingual Navigation:** [Versión en Español](./docs/README.es.md)

[![Status](https://img.shields.io/badge/Status-Active-brightgreen?style=for-the-badge)]()
[![Platform](https://img.shields.io/badge/.NET_10_%7C_React_18-informational?style=for-the-badge)]()
[![Architecture](https://img.shields.io/badge/Evolith-Satellite_Product-blueviolet?style=for-the-badge)](https://github.com/beyondnetcode/evolith_arch32)
[![ADRs](https://img.shields.io/badge/ADRs-32_decisions-orange?style=for-the-badge)](./docs/architecture/adrs/)
[![License](https://img.shields.io/badge/License-Proprietary-red?style=for-the-badge)]()

<br/>

<a href="https://raw.githubusercontent.com/beyondnetcode/evolith_arch32/main/reference/governance/sdlc/assets/master-view.png" title="Evolith E2E Architecture — UMS is a satellite product · click to enlarge">
  <img src="https://raw.githubusercontent.com/beyondnetcode/evolith_arch32/main/reference/governance/sdlc/assets/master-view.png"
       alt="Evolith E2E Architecture — UMS is a satellite product"
       width="780"
       style="border-radius: 8px; box-shadow: 0 4px 20px rgba(0,0,0,0.3);" />
</a>

<sub>↑ <a href="https://github.com/beyondnetcode/evolith_arch32">Evolith</a> E2E Architecture Framework · <strong>UMS is an official satellite product</strong> · <i>click to enlarge</i></sub>

<br/>

**UMS is a modular monolith for identity, authorization, configuration, approvals, compliance, IGA, and audit.**<br/>
Built on **.NET 10 · SQL Server 2022 · React 18 · TypeScript · Nx**.<br/>
Applied satellite of the [Evolith](https://github.com/beyondnetcode/evolith_arch32) corporate architecture framework.

</div>

---

## 📑 Quick Navigation Menu

| Category | Entry Point | Description |
|---|---|---|
| 📐 **Architecture** | [Architecture Portal](./docs/architecture/index.md) | Blueprints, ADRs, TEs, canonical patterns |
| 🏛️ **ADRs** | [ADR Registry](./docs/architecture/adrs/) | 32 architecture decisions |
| 🧩 **Domain Model** | [Aggregate Index](./docs/domain/index.md) | Bounded contexts · aggregates · entities |
| 📦 **SDK** | [SDK Portal](./docs/sdk/index.md) | .NET · TypeScript · NestJS |
| 📋 **Requirements** | [Functional Stories](./docs/governance/requirements/functional-stories/index.md) | Full product backlog |
| 🚦 **Planning** | [Project Backlog](./docs/governance/project/index.md) | Epics · MVP · gap tracker |
| ⚙️ **Operations** | [Operations Portal](./docs/operations/index.md) | Runbooks · metrics |
| 🧪 **QA** | [QA Report](./docs/qa/qa_report.md) | Test results · coverage · evidences |
| 🏗️ **Infrastructure** | [K8s Plan](./infra/UMS_K8s_Deployment_Plan.md) | Kubernetes deployment guide |
| 📖 **Full Index** | [Master Index](./docs/MASTER_INDEX.md) | Complete lifecycle navigation |
| 🔺 **Evolith Upstream** | [Evolith Framework](https://github.com/beyondnetcode/evolith_arch32) | Architecture reference base |

---

## 🎯 Start Here — Choose Your Path

### Path 1 — 5-Minute Overview

📄 [Product Vision](./docs/governance/product/product-vision.md) · [Architecture Overview](./docs/architecture/overview.md) · [Traceability Matrix](./docs/architecture/traceability-matrix.md)

*What is UMS? What problem does it solve? How does it fit Evolith?*

### Path 2 — By Role

| Role | Start Here | Then Read |
|---|---|---|
| 🏛️ **Architect** | [Architecture Portal](./docs/architecture/index.md) | [ADR Registry](./docs/architecture/adrs/) · [Traceability Matrix](./docs/architecture/traceability-matrix.md) |
| 👨‍💻 **Backend Dev** | [Canonical Patterns](./docs/architecture/artifacts/canonical-patterns/index.md) | [Domain Aggregates](./docs/domain/index.md) · [.NET SDK](./docs/sdk/dotnet/README.md) |
| 🖥️ **Frontend Dev** | [ADR-0056: Clean Architecture](./docs/architecture/adrs/0056-clean-architecture-frontend.md) | [TypeScript SDK](./docs/sdk/typescript/README.md) · [ADR-0057: State Mgmt](./docs/architecture/adrs/0057-zustand-tanstack-query-state.md) |
| 🛠️ **DevOps / SRE** | [Infrastructure Plan](./infra/infrastructure_plan.md) | [Runbooks](./docs/operations/runbooks/) · [Metrics](./docs/operations/metrics/index.md) |
| 📦 **Product / PM** | [Product Vision](./docs/governance/product/product-vision.md) | [Gap Tracker](./docs/governance/project/functional-story-gap-tracker.md) · [OKRs](./docs/governance/product/objectives.md) |
| 🤖 **AI Contributor** | [AGENTS.md](./AGENTS.md) | [ADR Template](./docs/governance/sdlc/adr-template.md) |

### Path 3 — Make an Architectural Decision

1. Check [ADR Registry](./docs/architecture/adrs/) — does the decision already exist?
2. If not, use the [ADR Template](./docs/governance/sdlc/adr-template.md)
3. Verify against [Evolith ADR Matrix](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/adr-matrix.md) — should this be promoted upstream?

---

## 📂 Repository Structure (Deep Dive)

### 📐 Architecture & Patterns

| Artifact | Purpose |
|---|---|
| [Architecture Portal](./docs/architecture/index.md) | Central entry for all architecture artifacts |
| [Architecture Overview](./docs/architecture/overview.md) | System-wide diagram and layered model |
| [Traceability Matrix](./docs/architecture/traceability-matrix.md) | FS → ADR → Technical Enabler coverage |
| [Blueprints](./docs/architecture/blueprints/) | Database ER, service-entity map, shell library design |
| [Technical Enablers](./docs/architecture/blueprints/technical-enablers/index.md) | TE-01 through TE-07 |
| [Canonical Patterns](./docs/architecture/artifacts/canonical-patterns/index.md) | CP-01 through CP-08 |
| [API Reference (.NET)](./docs/architecture/api-dotnet/) | HTTP contract reference |

### 🏛️ Architecture Decision Records

| ADR | Title | Evolith |
|---|---|---|
| [0050](./docs/architecture/adrs/0050-naming-taxonomy-standard.md) | Naming & Taxonomy Standard | Adopts ADR-0056 |
| [0051](./docs/architecture/adrs/0051-event-bus-injectable-port.md) | Event Bus Injectable Port | Adopts ADR-0015 |
| [0052](./docs/architecture/adrs/0052-immutable-audit-trail-enforcement.md) | Immutable Audit Trail | Adopts ADR-0016 |
| [0053](./docs/architecture/adrs/0053-opentelemetry-observability.md) | OpenTelemetry Observability | Adopts ADR-0007 |
| [0054](./docs/architecture/adrs/0054-shell-library-isolation.md) | Shell Library Isolation | UMS-specific |
| [0055](./docs/architecture/adrs/0055-graphql-rest-hybrid-api.md) | GraphQL/REST Hybrid API | Implements ADR-0012 |
| [0056](./docs/architecture/adrs/0056-clean-architecture-frontend.md) | Clean Architecture Frontend | → Evolith nodejs/0044 |
| [0057](./docs/architecture/adrs/0057-zustand-tanstack-query-state.md) | Zustand + TanStack Query | → Evolith nodejs/0045 |
| [0058](./docs/architecture/adrs/0058-api-gateway-yarp-evolution.md) | API Gateway YARP | Implements ADR-0008 |
| [0059](./docs/architecture/adrs/0059-single-api-tier-decision.md) | Single API Tier | Override of Evolith baseline |
| [0060](./docs/architecture/adrs/0060-aop-cross-cutting-concern-strategy.md) | AOP Cross-Cutting Concern | → Evolith dotnet/0072 |
| [0061](./docs/architecture/adrs/0061-execution-context-accessor.md) | Execution Context Accessor | → Evolith dotnet/0064 |
| [0062](./docs/architecture/adrs/0062-pii-safe-serilog-configuration.md) | PII-Safe Serilog | → Evolith dotnet/0065 |
| [0063](./docs/architecture/adrs/0063-idempotency-middleware.md) | Idempotency Middleware | → Evolith dotnet/0066 |
| [0064](./docs/architecture/adrs/0064-lean-root-repository-taxonomy.md) | Lean Root Repository Taxonomy | → Evolith core/0070 |
| [0065](./docs/architecture/adrs/0065-no-raw-guids-in-ui.md) | No Raw GUIDs in UI | → Evolith nodejs/0046 |
| [0066](./docs/architecture/adrs/0066-actionable-user-error-contract.md) | Actionable User Error Contract | → Evolith nodejs/0047 |
| [0067](./docs/architecture/adrs/0067-modular-monolith-schema-per-domain.md) | Modular Monolith Schema | → Evolith core/0067 |
| [0068](./docs/architecture/adrs/0068-feature-flag-system-scope.md) | Feature Flag System Scope | → Evolith nodejs/0048 |
| [0069](./docs/architecture/adrs/0069-domain-inheritance-strategy.md) | Domain Inheritance Strategy | → Evolith core/0071 |
| [0070](./docs/architecture/adrs/0070-database-schema-strategy-decision.md) | Database Schema Strategy | Adopts ADR-0067 |
| [0071](./docs/architecture/adrs/0071-auth-graph-engine.md) | Authorization Graph Engine | UMS-specific |
| [0072](./docs/architecture/adrs/0072-dynamic-auth-method-resolution.md) | Dynamic Auth Method Resolution | UMS-specific |
| [0073](./docs/architecture/adrs/0073-ums-sdk-multi-runtime.md) | UMS SDK Multi-Runtime | UMS-specific |
| [0074](./docs/architecture/adrs/0074-auth-graph-schema-versioning.md) | Auth Graph Schema Versioning | UMS-specific |
| [0075](./docs/architecture/adrs/0075-onboarding-approval-inbox-and-scope-based-authorization.md) | Onboarding Approval Inbox | UMS-specific |
| [0076](./docs/architecture/adrs/0076-utc-dates-timezone-language-resolution.md) | UTC Dates & Language Resolution | → Evolith core/0072 |
| [0077](./docs/architecture/adrs/0077-tenant-portal-management-authorization-boundary.md) | Tenant Portal Auth Boundary | UMS-specific |
| [0078](./docs/architecture/adrs/0078-ddd-domain-resource-hierarchy.md) | DDD Domain Resource Hierarchy | UMS-specific |
| [0079](./docs/architecture/adrs/0079-dependency-guard-policy.md) | Dependency Guard Policy | UMS-specific |
| [0080](./docs/architecture/adrs/0080-auth-graph-preview-internal-pipeline.md) | Auth Graph Preview Pipeline | UMS-specific |
| [0081](./docs/architecture/adrs/0081-semantic-auth-graph-client-contract.md) | Semantic Auth Graph Contract | UMS-specific |

### 🧩 Domain Model

| Bounded Context | Aggregates |
|---|---|
| **Identity** | [Tenant](./docs/domain/identity/tenant.md) · [UserAccount](./docs/domain/identity/user-account.md) · [Delegation](./docs/domain/identity/user-management-delegation.md) · [Auth Graph](./docs/domain/identity/auth-graph.md) · [Auth Method](./docs/domain/identity/auth-method-resolution.md) |
| **Authorization** | [SystemSuite](./docs/domain/authorization/system-suite.md) · [PermissionTemplate](./docs/domain/authorization/permission-template.md) · [Profile](./docs/domain/authorization/profile.md) |
| **Configuration** | [IdpConfiguration](./docs/domain/configuration/idp-configuration.md) · [AppConfiguration](./docs/domain/configuration/app-configuration.md) · [FeatureFlag](./docs/domain/configuration/feature-flag.md) |
| **Approvals** | [ApprovalWorkflow](./docs/domain/approvals/approval-workflow.md) · [ApprovalRequest](./docs/domain/approvals/approval-request.md) · [DocumentType](./docs/domain/approvals/document-type.md) |
| **IGA** | [PromotionRequest](./docs/domain/iga/promotion-request.md) · [RoleMaturityStatus](./docs/domain/iga/role-maturity-status.md) |
| **Audit** | [AuditRecord](./docs/domain/audit/audit-record.md) |

Also: [Bounded Context Map](./docs/governance/construction/ddd-design/01-bounded-context-map.md) · [Cross-Context Flows](./docs/governance/construction/ddd-design/10-cross-context-flows.md) · [DDD Primitives](./docs/governance/construction/ddd-design/11-ddd-primitives.md)

### 📦 SDK

| Runtime | README | Quickstart |
|---|---|---|
| **.NET** | [README](./docs/sdk/dotnet/README.md) | [Quickstart](./docs/sdk/dotnet/quickstart.md) |
| **TypeScript** | [README](./docs/sdk/typescript/README.md) | [Quickstart](./docs/sdk/typescript/quickstart.md) |
| **NestJS** | [README](./docs/sdk/nestjs/README.md) | [Quickstart](./docs/sdk/nestjs/quickstart.md) |

Contracts: [Schema Overview](./docs/sdk/contracts/schema-overview.md) · [Error Codes](./docs/sdk/contracts/error-codes.md) · [Compatibility Matrix](./docs/sdk/contracts/compatibility-matrix.md) · [Semantic Client Contract](./docs/sdk/contracts/semantic-client-contract.md)

### 📋 Product & Requirements

| Artifact | Purpose |
|---|---|
| [Product Vision](./docs/governance/product/product-vision.md) | Strategy, goals, and market positioning |
| [Business Context](./docs/governance/product/business-context.md) | Problem space and market context |
| [Scope & Boundaries](./docs/governance/product/scope.md) | What UMS is and is not |
| [Objectives (OKRs)](./docs/governance/product/objectives.md) | Measurable success criteria |
| [Stakeholders](./docs/governance/product/stakeholders.md) | Roles and responsibilities |
| [Glossary](./docs/governance/requirements/glossary.md) | Ubiquitous language |
| [Functional Stories](./docs/governance/requirements/functional-stories/index.md) | Full requirements backlog |
| [Conceptual Data Model](./docs/governance/requirements/conceptual-data-model.md) | High-level domain model |
| [Permission Matrix](./docs/governance/requirements/permission-matrix-example.md) | Role/permission reference |

### 🚦 Planning & Backlog

| Artifact | Purpose |
|---|---|
| [Project Backlog](./docs/governance/project/index.md) | Epics and sprint planning |
| [MVP Product Backlog](./docs/governance/project/mvp-product-backlog.md) | Prioritized MVP scope |
| [Gap Tracker](./docs/governance/project/functional-story-gap-tracker.md) | Implementation status per story |
| [Epic 06: Approvals](./docs/governance/project/ep-06-approvals-detailed-design.md) | Approval workflow detailed design |
| [Epic 07: Compliance](./docs/governance/project/ep-07-compliance-detailed-design.md) | Compliance module design |
| [Epic 08: IGA](./docs/governance/project/ep-08-iga-detailed-design.md) | Identity Governance design |

### ⚙️ Operations

| Artifact | Purpose |
|---|---|
| [Metrics Dashboard](./docs/operations/metrics/index.md) | API, frontend, tests, aggregate metrics |
| [RB-01: Incident Response](./docs/operations/runbooks/rb-01-incident-response.md) | On-call incident playbook |
| [RB-02: Rollback Procedure](./docs/operations/runbooks/rb-02-rollback-procedure.md) | Safe rollback steps |
| [RB-03: Cache Failure Recovery](./docs/operations/runbooks/rb-03-cache-failure-recovery.md) | Redis failure recovery |
| [RB-04: Database Failover](./docs/operations/runbooks/rb-04-database-failover.md) | SQL Server failover procedure |
| [Dev DB Anonymization](./docs/operations/runbooks/dev-db-anonymization.md) | PII anonymization for dev environments |
| [GDPR Backup Retention](./docs/operations/runbooks/gdpr-backup-retention-policy.md) | Backup retention compliance |

### 🧪 QA & Testing

| Artifact | Purpose |
|---|---|
| [QA Report](./docs/qa/qa_report.md) | Overall QA status |
| [Unit Testing Results](./docs/governance/testing/unit-testing-results.md) | Unit test coverage and results |
| [Integration Testing Results](./docs/governance/testing/integration-testing-results.md) | Integration test status |
| [Performance Testing Plan](./docs/governance/testing/performance-testing-plan.md) | Load testing strategy |
| [Performance Testing Results](./docs/governance/testing/performance-testing-results.md) | Load test outcomes |
| [QA Evidences](./docs/qa/evidences/) | US-001 through US-008 screenshots |

### 🏗️ Infrastructure

| Artifact | Purpose |
|---|---|
| [K8s Deployment Plan](./infra/UMS_K8s_Deployment_Plan.md) | Full Kubernetes deployment guide |
| [Infrastructure Plan](./infra/infrastructure_plan.md) | Infrastructure design |
| [Implementation Plan](./infra/implementation_plan.md) | Deployment implementation steps |
| [Local Access Guide](./infra/accesos_local.md) | Local environment endpoints |

---

## 🔧 Local Development

```bash
# Install all dependencies
cd src && npm install

# Frontend (React · Vite · port 5173)
npx nx run app-web:dev

# Backend (.NET 10 · port 7114)
cd src/apps/ums.api && dotnet build && dotnet run

# Run all tests
cd src && npx nx run-many --target=test --all

# Validate documentation consistency
python .bmad-core/scripts/validate_docs_consistency.py README.md docs/
```

> **Note:** After any change requiring a server reload, kill backend (:7114) and frontend (:5173) and restart both.

---

## 🔺 UMS vs Evolith — What Goes Where

| Question | Evolith (Upstream) | UMS (Satellite Product) |
|---|---|---|
| **What belongs here?** | Reusable standards, cross-product ADRs, canonical patterns, governance | Product-specific implementation, domain aggregates, schemas, seeds |
| **How does a product contribute?** | Promote an ADR backed by real UMS evidence | Provide executable proof, then propose upstream |
| **What stays local?** | Enterprise policies require governance review | Product routes, tenant configs, branding, UMS-specific ADRs |

13 UMS ADRs have been promoted to Evolith. See the [ADR table above](#-architecture-decision-records) (→ Evolith column).

---

## 🤝 Contributing

Before contributing, read:

- [AGENTS.md](./AGENTS.md) — Agent rules and conventions
- [Standards](./docs/STANDARDS.md) — Engineering standards
- [ADR Template](./docs/governance/sdlc/adr-template.md) — How to propose a decision
- [Evolith Child Repository Inheritance Guide](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/governance/standards/onboarding/child-repository-inheritance-guide.md) — How UMS inherits from Evolith

---

## 📋 All Navigation Indexes

| Index | Purpose |
|---|---|
| [Master Index](./docs/MASTER_INDEX.md) | Complete lifecycle navigation (Phase 00–06) |
| [Architecture Index](./docs/architecture/index.md) | All architecture artifacts |
| [Domain Index](./docs/domain/index.md) | All aggregates by bounded context |
| [SDK Portal](./docs/sdk/index.md) | All SDK runtimes and contracts |
| [Operations Portal](./docs/operations/index.md) | Runbooks and metrics |

---

<div align="center">
  <sub>UMS — Enterprise User Management System · Satellite of <a href="https://github.com/beyondnetcode/evolith_arch32">Evolith</a> · .NET 10 · React 18 · Modular Monolith</sub>
</div>
