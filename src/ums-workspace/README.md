# 🏢 User Management System (UMS) - Enterprise Monorepo

Welcome to the **User Management System (UMS)**, a highly resilient, enterprise-grade modular monolith built for **UMS**. This system is engineered to manage corporate identities, access control, and user lifecycles across the organization.

The UMS is built leveraging the **BMAD Method**, enforcing strict **Clean Architecture (Hexagonal)** principles, $0-cost observability (AOP), and rigorous CI/CD quality gates. 

> [!NOTE]
> **Progressive Design Disclaimer**: This repository serves as a reference base for 100% Node.js-based systems. While currently implemented as a single modular monolithic solution (UMS), the frontend architecture is built to preserve its structure as a unified entry portal. In the future, it will scale to integrate other corporate backends (such as TMS, WMS, etc.), which will be developed as independent, isolated services with their own databases. Communication will be routed through a central API Gateway, using the Backend For Frontend (BFF) pattern to optimize payloads for web and mobile clients (see [ADR 0008](./docs/architecture-design/adrs/0008-progressive-multimodule-evolution-gateway-bff.md)).


## 🛠️ Technology Stack
- **Backend**: NestJS (v10), TypeORM, PostgreSQL 16.
- **Frontend**: React (v18), Vite, Zustand, TanStack React Query.
- **Monorepo Orchestration**: Nx & npm Workspaces.
- **Security & Quality**: Husky, ESLint (SonarJS + Boundaries), CodeQL, GitHub Actions.

---

## 📚 Documentation Index & Navigation Guide

This repository contains extensive technical documentation following the **bMAD Method** and industry standards (C4 Model and Markdown Architectural Decision Records - MADR). Use the following curated guides to navigate the codebase:

### 🏛️ bMAD Structure and Taxonomy (Interactive Navigation)
Click on the phases or files to navigate quickly and directly through the knowledge base:

*   🗺️ **[Main Phase - Master Index and Navigation Guides](./docs/index.md)** (Unified central index)
*   🎯 **[Phase 00 - Product Vision](./docs/00-product/)**:
    *   [Product Vision](./docs/00-product/product-vision.md) | [Business Context](./docs/00-product/business-context.md) | [Scope and Boundaries](./docs/00-product/scope.md) | [Objectives](./docs/00-product/objectives.md) | [Stakeholders](./docs/00-product/stakeholders.md)
*   📋 **[Phase 01 - Domain Requirements](./docs/01-requirements/)**:
    *   [Atomic Use Cases](./docs/01-requirements/usecases/) | [Conceptual Data Model](./docs/01-requirements/conceptual-data-model.md) | [Permission Matrix](./docs/01-requirements/permission-matrix-example.md) | [DDD Glossary](./docs/01-requirements/glossary.md)
*   🏗️ **[Phase 02 - Architectural Design](./docs/02-architecture/)**:
    *   **[Node.js Reference Architecture](./docs/02-architecture/reference-architecture-nodejs-arc42.md)** | [C4 Master Specification](./docs/02-architecture/architecture-spec.md)
*   📜 **[Phase 03 - Architectural Decision Records (ADRs)](./docs/03-adrs/)**:
    *   [23 ADRs History](./docs/03-adrs/)
*   🛠️ **[Phase 04 - Engineering Standards and Artifacts](./docs/04-artifacts/)**:
    *   [Global Standards](./docs/04-artifacts/engineering-standards.md) | [Architecture Maturity Model](./docs/04-artifacts/architecture-maturity-model.md) | [Gap Analysis](./docs/04-artifacts/gap-analysis-and-optimization-plan.md) | [QA Quality Plan](./docs/04-artifacts/contract-testing-plan.md) | [Observability](./docs/04-artifacts/observability-strategy.md) | [IAM Spec](./docs/04-artifacts/enterprise-iam-ums-specification.md) | [High-Concurrency](./docs/04-artifacts/high-concurrency-auth-specification.md) | [Console Spec](./docs/04-artifacts/ums-web-console-product-scope.md)
*   📈 **[Phase 05 - Release Roadmap](./docs/05-roadmap/)**:
    *   [Semantic Versioning](./docs/05-roadmap/versioning-and-audit-strategy.md)

---

### 📖 1. Standards & Mandates (Core)
*   👉 **[Global Engineering Standards & BMAD Manifesto](./docs/04-artifacts/engineering-standards.md)**: **MANDATORY reading**. Establishes the non-negotiable coding standards, SOLID, Clean Code, OWASP compliance, and optional DDD guidelines.

### 🏗️ 2. Architectural Design
*   👉 **[ULPMS Master Documentation Index & Guides](./docs/index.md)**: The central navigation map linking all 6 phases of product vision, requirements, architecture specifications, ADRs, engineering standards, and deployment roadmaps.
*   👉 **[Corporate Node.js Reference Architecture](./docs/02-architecture/reference-architecture-nodejs-arc42.md)**: The official corporate blueprint for API-driven systems following the international **arc42** standard. UMS acts as the canonical technical instance of this architecture.
*   👉 **[C4 Architecture Spec & Technical Inventory](./docs/02-architecture/architecture-spec.md)**: Details the Level 1 (System Context), Level 2 (Container), and Level 3 (Component) diagrams of the UMS, along with the physical technology inventory.
*   👉 **[Gap Analysis & Optimization Roadmap](./docs/04-artifacts/gap-analysis-and-optimization-plan.md)**: Analyzes the platform's architectural maturity against 16 Enterprise Quality Criteria and tracks active ADR implementations.
*   👉 **[Vendor Lock-In & Financial Risk Assessment](./docs/02-architecture/vendor-risk-assessment.md)**: Analyzes financial and operational risks associated with commercial identity providers, caching mechanisms, and feature flag solutions.

### 📜 3. Architectural Decision Records (ADRs)
Foundational engineering decisions grouped by architectural focus:

#### 🟢 General & Core Monorepo
*   [ADR 0001: Monorepo Orchestration with Nx and npm Workspaces](./docs/03-adrs/0001-monorepo-orchestration-nx.md)
*   [ADR 0002: Clean Architecture and Hexagonal Boundaries on NestJS](./docs/03-adrs/0002-clean-architecture-nestjs.md)
*   [ADR 0003: Strict TypeScript Standards and SonarJS Static Analysis](./docs/03-adrs/0003-strict-typescript-standards.md)
*   [ADR 0005: Zero-Cost Security and CI Pipeline with CodeQL](./docs/03-adrs/0005-ci-cd-quality-codeql.md)
*   [ADR 0009: Strict Dependency Pinning and Automated Vulnerability Management](./docs/03-adrs/0009-strict-dependency-pinning-vulnerability-management.md)

#### 🔵 Frontend & Client Integration
*   [ADR 0004: Frontend State Management and React Query Offline Architecture](./docs/03-adrs/0004-frontend-offline-resilience.md)
*   [ADR 0008: Progressive Multi-Module Evolution with API Gateway and BFF Patterns](./docs/03-adrs/0008-progressive-multimodule-evolution-gateway-bff.md)

#### 🟠 SaaS, Scalability & Resilience
*   [ADR 0006: Future Microservices Transition with Dapr Sidecars](./docs/03-adrs/0006-future-microservices-transition-dapr.md)
*   [ADR 0007: Observability Telemetry with Grafana Loki and OpenTelemetry](./docs/03-adrs/0007-observability-telemetry-loki-opentelemetry.md)
*   [ADR 0010: Multi-Tenancy Architecture Strategy for SaaS Evolution](./docs/03-adrs/0010-multi-tenancy-architecture-strategy.md)
*   [ADR 0011: Fault Tolerance & Resiliency Patterns (Circuit Breakers)](./docs/03-adrs/0011-fault-tolerance-resiliency-patterns.md)
*   [ADR 0012: Advanced Authorization (RBAC/ABAC)](./docs/03-adrs/0012-advanced-authorization-rbac-abac.md)
*   [ADR 0013: Cloud Infrastructure Topology & DR](./docs/03-adrs/0013-cloud-infrastructure-topology-dr.md)
*   [ADR 0014: Distributed Caching Strategy (Redis)](./docs/03-adrs/0014-distributed-caching-strategy-redis.md)
*   [ADR 0015: Event-Driven Architecture (EDA)](./docs/03-adrs/0015-event-driven-architecture-intra-domain.md)
*   [ADR 0016: Immutable Business Audit Trail (CDC)](./docs/03-adrs/0016-immutable-business-audit-trail.md)
*   [ADR 0017: Feature Flagging Strategy](./docs/03-adrs/0017-feature-flagging-strategy.md)
*   [ADR 0018: Testing Pyramid & Automated Quality Gates](./docs/03-adrs/0018-testing-pyramid-quality-gates.md)
*   [ADR 0019: Tactical Design Patterns for Domain Integrity (Result Pattern)](./docs/03-adrs/0019-tactical-design-patterns-future-proofing.md)

#### 🏛️ Architectural Governance & ADR Status Matrix

Before starting the coding phase, the Product Owner (PO) has absolute authority to approve, defer, or veto any Architectural Decision Record (ADR). Below is the exhaustive classification of all 29 active decisions matching their file statuses:

### 🟢 1. APPROVED & ACCEPTED (Línea Base Autorizada — Ready for Coding)
These decisions are officially **Approved** and form the system's baseline architecture. Development must strictly adhere to these patterns:

| ADR ID | Decision Title | Status | Impact / Scope | Next Steps / Action |
| :--- | :--- | :--- | :--- | :--- |
| **ADR-0001** | [Monorepo Orchestration with Nx](./docs/03-adrs/0001-monorepo-orchestration-nx.md) | 🟢 **Accepted** | Core monorepo organization and speed optimization. | Approved baseline. |
| **ADR-0002** | [Clean Architecture & Hexagonal Boundaries](./docs/03-adrs/0002-clean-architecture-nestjs.md) | 🟢 **Accepted** | Decoupling core domain rules from database and frameworks. | Approved baseline. |
| **ADR-0003** | [Strict TypeScript Standards](./docs/03-adrs/0003-strict-typescript-standards.md) | 🟢 **Accepted** | Static analysis quality gate and type enforcement. | Approved baseline. |
| **ADR-0004** | [React Query Offline Architecture](./docs/03-adrs/0004-frontend-offline-resilience.md) | 🟢 **Accepted** | Local caching & fallback mechanism for client resilience. | Approved baseline. |
| **ADR-0005** | [Zero-Cost Security via CodeQL](./docs/03-adrs/0005-ci-cd-quality-codeql.md) | 🟢 **Accepted** | Automated vulnerability scanning inside CI pipeline. | Approved baseline. |
| **ADR-0008** | [Gateway and BFF Patterns](./docs/03-adrs/0008-progressive-multimodule-evolution-gateway-bff.md) | 🟢 **Accepted** | Optimizes network requests for multi-module clients. | Approved baseline. |
| **ADR-0009** | [Strict Dependency Pinning](./docs/03-adrs/0009-strict-dependency-pinning-vulnerability-management.md) | 🟢 **Accepted** | Mitigates supply chain injection vulnerabilities. | Approved baseline. |
| **ADR-0010** | [Multi-Tenancy SaaS Strategy](./docs/03-adrs/0010-multi-tenancy-architecture-strategy.md) | 🟢 **Accepted** | Defines database isolation strategy per corporate tenant. | Approved baseline. |
| **ADR-0020** | [Identity Provider Abstraction](./docs/03-adrs/0020-identity-provider-abstraction-strategy.md) | 🟢 **Accepted** | Decouples UMS from Auth0, Keycloak, or Entra ID. | Approved baseline. |
| **ADR-0021** | [High Performance Auth Graph](./docs/03-adrs/0021-high-performance-auth-and-graph-compilation.md) | 🟢 **Accepted** | Optimized permission compiling under <5ms latency limit. | Approved baseline. |
| **ADR-0022** | [Pluggable Output Projections](./docs/03-adrs/0022-contextual-auth-and-pluggable-projections.md) | 🟢 **Accepted** | Context-aware read projection layers outside the core. | Approved baseline. |
| **ADR-0023** | [Centralized vs Decentralized Access](./docs/03-adrs/0023-centralized-ums-vs-decentralized-access.md) | 🟢 **Accepted** | Establishes the authoritative access kernel boundary. | Approved baseline. |
| **ADR-0024** | [Configuration & Feature Management](./docs/03-adrs/0024-configuration-feature-management-platform.md) | 🟢 **Accepted** | Multi-IdP parameter dynamic engine. | Approved baseline. |
| **ADR-0025** | [Feature Flag Provider Abstraction](./docs/03-adrs/0025-feature-flag-provider-abstraction.md) | 🟢 **Accepted** | Pluggable `IFeatureFlagPort` for Unleash/ConfigCat. | Approved baseline. |
| **ADR-0026** | [MFA and Passwordless Authentication](./docs/03-adrs/0026-mfa-passwordless-adaptive-authentication.md) | 🟢 **Accepted** | WebAuthn, Passkeys, TOTP, and Adaptive Risk MFA. | Approved baseline. |
| **ADR-0027** | [Dual-Protocol REST & gRPC API Structure](./docs/03-adrs/0027-dual-protocol-rest-grpc-api-gateway.md) | 🟢 **Accepted** | Public RESTful APIs and internal gRPC services. | Approved baseline. |
| **ADR-0028** | [Self-Hosted Hybrid Infrastructure](./docs/03-adrs/0028-self-hosted-hybrid-infrastructure-on-premise.md) | 🟢 **Accepted** | Cloud-agnostic capability (MinIO, RabbitMQ, Vault OSS). | Approved baseline. |
| **ADR-0029** | [Tactical DDD Primitives Library](./docs/03-adrs/0029-tactical-ddd-primitives-library.md) | 🟢 **Accepted** | Standardizes `@nestjslatam/ddd` for optional DDD use. | Approved baseline. |

### 🟡 2. PROPOSED & PENDING REVIEW (Pendientes de Revisión/Aprobación por el PO)
These decisions are currently **Proposed** and represent strategic backlogs. They **must be formally approved by the PO before starting coding**:

| ADR ID | Decision Title | Status | Impact / Scope | Required PO Action |
| :--- | :--- | :--- | :--- | :--- |
| **ADR-0006** | [Future Microservices via Dapr](./docs/03-adrs/0006-future-microservices-transition-dapr.md) | 🟡 **Proposed** | Sidecar integration for distributed state and messaging. | **PO review/approve** to activate microservice migration. |
| **ADR-0007** | [Loki & OpenTelemetry Strategy](./docs/03-adrs/0007-observability-telemetry-loki-opentelemetry.md) | 🟡 **Proposed** | Distributed tracing and centralized log collection. | **PO review/approve** to authorize SRE monitoring infra. |
| **ADR-0011** | [Fault Tolerance & Resiliency](./docs/03-adrs/0011-fault-tolerance-resiliency-patterns.md) | 🟡 **Proposed** | Circuit breakers (`opossum`) and exponential retries. | **PO review/approve** to authorize resiliency policies. |
| **ADR-0012** | [Advanced Authorization (RBAC/ABAC)](./docs/03-adrs/0012-advanced-authorization-rbac-abac.md) | 🟡 **Proposed** | Fine-grained contextual permission modeling. | **PO review/approve** to authorize security model. |
| **ADR-0013** | [Cloud Infrastructure & DR](./docs/03-adrs/0013-cloud-infrastructure-topology-dr.md) | 🟡 **Proposed** | Multi-region disaster recovery replication limits. | **PO review/approve** to authorize deployment budget. |
| **ADR-0014** | [Distributed Caching (Redis)](./docs/03-adrs/0014-distributed-caching-strategy-redis.md) | 🟡 **Proposed** | Memory caches for auth validations and active sessions. | **PO review/approve** to authorize memory allocation. |
| **ADR-0015** | [Event-Driven Architecture](./docs/03-adrs/0015-event-driven-architecture-intra-domain.md) | 🟡 **Proposed** | Asynchronous events publishing for state sync. | **PO review/approve** to authorize event bus topology. |
| **ADR-0016** | [Immutable Business Audit Trail](./docs/03-adrs/0016-immutable-business-audit-trail.md) | 🟡 **Proposed** | Cryptographically signed logs for user activity tracing. | **PO review/approve** to authorize audit compliance rules. |
| **ADR-0017** | [Feature Flagging Strategy](./docs/03-adrs/0017-feature-flagging-strategy.md) | 🟡 **Proposed** | Live features toggling on development environments. | **PO review/approve** to authorize operational gating. |
| **ADR-0018** | [Testing Pyramid Quality Gates](./docs/03-adrs/0018-testing-pyramid-quality-gates.md) | 🟡 **Proposed** | Coverage limits for E2E, Contract, and Unit tests. | **PO review/approve** to authorize QA gate requirements. |
| **ADR-0019** | [Tactical Domain Patterns (Result Pattern)](./docs/03-adrs/0019-tactical-design-patterns-future-proofing.md) | 🟡 **Proposed** | Rigid functional error handling over exception throwing. | **PO review/approve** to authorize coding style mandate. |

### 🔴 3. CANCELLED, REJECTED OR VETOED (Rechazados o Descartados por el PO)
These architectural decisions have been **Vetoed / Rejected** or **Cancelled** by the Product Owner and **must never be implemented**:

*   *Currently, there are no rejected or cancelled decisions. You have full authority to move any ADR to this section to veto its implementation.*




### 🏷️ 4. Versioning & Release Cycles
*   [BMAD Automated Versioning Strategy](./docs/05-roadmap/versioning-and-audit-strategy.md) - Learn how we automate Semantic Versioning and Release cycles.
*   **[View the Official CHANGELOG](./CHANGELOG.md)** - The pristine audit log of all merged features and fixes across the monorepo.

---

## 🚀 Quick Start
```bash
# 1. Install dependencies and link workspaces
npm install

# 2. Start the NestJS API (Port 3000)
npx nx run api:serve

# 3. Start the React Client (Port 5173)
npx nx run apps-web:dev
```
