# 🛠️€”ÂºÃ¯Â¸Â Master Navigation Map - UMS Knowledge Base

> Ã°Å¸Å’Â **Language Selector:** [🛠️€¡Âª🛠️€¡Â¸ Español](../es/index.md) | [🛠️€¡Âº🛠️€¡Â¸ English](./index.md)

Welcome to the master technical documentation for the **User Management System (UMS)**. This knowledge base is structured under the **spec-driven AI strategy BMAD-METHOD (numerical sequential phases)** to guarantee maximum discoverability, traceability, and seamless support for both human developers and autonomous AI copilots.

---

## Ã°Å¸Â§Â­ Phase-Based Navigation Index

### Ã°Å¸Å½Â¯ [Phase 00 - Product Vision](./00-product/)
Defines the business context, product strategic pillars, and the map of stakeholders.
*   📄 **[Product Vision](./00-product/product-vision.md)**: Sovereign identity pillars, authentication delegation, and dynamic multi-tenancy.
*   📄 **[Business Context](./00-product/business-context.md)**: Problem statement, proposed solution, and conceptual integration diagrams.
*   📄 **[Scope and Boundaries](./00-product/scope.md)**: Detailed In-Scope and Out-of-Scope capabilities.
*   📄 **[Strategic Objectives (OKRs)](./00-product/objectives.md)**: Quantifiable metrics and KRs for security, latency, and self-service.
*   📄 **[Stakeholders Map](./00-product/stakeholders.md)**: Responsibility matrix and expectation mappings for technical and business roles.

---

### 📜”¹ [Phase 01 - Domain Requirements](./01-requirements/)
Details business rules, interactive sequences, conceptual database diagrams, and the formal Ubiquitous Language definition.
*   📄 **[Glossary of Terms (Ubiquitous Language)](./01-requirements/glossary.md)**: Formal DDD dictionary of the core domain.
*   📄 **[Conceptual Data Model](./01-requirements/conceptual-data-model.md)**: PostgreSQL relational logic and Row-Level Security (RLS) policies.
*   📄 **[Granular Permission Matrix](./01-requirements/permission-matrix-example.md)**: Detailed access logic (RBAC/ABAC) and the explicit-deny precedence rule.
*   📜”š **[Functional Stories](./01-requirements/functional-stories/)**:
    *   [FS-01: User Authentication via External IdP](./01-requirements/functional-stories/fs-01-user-authentication.md)
    *   [TE-01: Compile Authorization Graph](./01-requirements/../02-architecture/technical-enablers/te-01-build-authorization-graph.md)
    *   [FS-02: Create & Instantiate Authorization Template](./01-requirements/functional-stories/fs-02-create-authorization-template.md)
    *   [FS-03: Register Organization & Configure IdP Strategy](./01-requirements/functional-stories/fs-03-register-organization.md)
    *   [FS-04: Register System & Define Menu Topology](./01-requirements/functional-stories/fs-04-register-system-topology.md)
    *   [FS-05: Create Profile & Manually Assign Template](./01-requirements/functional-stories/fs-05-create-profile-manual-template.md)
    *   [FS-06: Auto-Assign Template on Profile Creation](./01-requirements/functional-stories/fs-06-auto-assign-template.md)
    *   [FS-07: Diagnose Permissions via Visual Graph Resolver](./01-requirements/functional-stories/fs-07-visual-graph-resolver.md)
    *   [TE-02: Resolve Hierarchical System Configuration](./01-requirements/../02-architecture/technical-enablers/te-02-resolve-hierarchical-config.md)
    *   [FS-08: Authenticate via Customizable Hosted Login Page](./01-requirements/functional-stories/fs-08-hosted-login-redirection.md)
    *   [FS-09: Multi-Factor & Passwordless Adaptive Authentication](./01-requirements/functional-stories/fs-09-mfa-passwordless-adaptive-auth.md)
    *   [FS-10: External B2B Access Request & Approval Workflow](./01-requirements/functional-stories/fs-10-external-b2b-access-request-approval.md)

---

### Ã°Å¸Â””Ã¯Â¸Â [Phase 02 - Software Architecture](./02-architecture/)
Contains the system's architectural specification based on the C4 Model standard and the authoritative technology stack.
*   📄 **[Bounded Context Map](./02-architecture/bounded-context-map.md)**: DDD context boundaries, integration patterns, and Anti-Corruption Layers.
*   📄 **[C4 Architecture Spec & Technical Inventory](./02-architecture/architecture-spec.md)**: Level 1 (Context), Level 2 (Container), and Level 3 (Component) technical diagrams.
*   📄 **[Authoritative Technology Stack Definition](./02-architecture/stack.md)**: 100% cloud-agnostic and on-premise capable technology stack definitions and risk registers.
*   📄 **[.NET Core Migration & Tech Stack Plan](./02-architecture/dotnet-migration-and-tech-stack-plan.md)**: Active architectural roadmap detailing the transition to .NET 8.
*   📄 **[Technology Stack Cheat Sheet](./02-architecture/stack-summary.md)**: High-density quick-reference sheet of all selected tools and layers.
*   📄 **[Tactical DDD Evaluation](./02-architecture/nestjslatam-ddd-evaluation.md)**: Architectural evaluation of tactical DDD primitives for the Domain Core.


---

### 📜 [Phase 03 - Architectural Decision Records (ADRs)](./03-adrs/)
The chronological and immutable ledger of critical design decisions in MADR format.
*   📄 **[ADR Ledger](./03-adrs/)**: Access the complete index of **29 active architectural decisions** (ranging from Nx Monorepo, Clean Architecture, RLS, to Identity Provider Abstraction, High-Performance Graph Compilation, Pluggable Output Projections, Centralized Authorization Kernel, Configuration Platform, Feature Flag Provider Abstraction, Adaptive MFA/Passwordless, Dual-Protocol APIs, Self-Hosted Infrastructure, and Tactical DDD Primitives).
*   📄 **[ADR-0024: Configuration & Feature Management Platform](./03-adrs/0024-configuration-feature-management-platform.md)**: Establishes Multi-IdP config, System Behavioral Config, and Feature Flag framework.
*   📄 **[ADR-0025: Feature Flag Provider Abstraction](./03-adrs/0025-feature-flag-provider-abstraction.md)**: Defines `IFeatureFlagPort` pluggable pattern Ã¢â‚¬” supports Internal engine, LaunchDarkly, Unleash, ConfigCat, Azure App Config.
*   📄 **[ADR-0026: Multi-Tenant Adaptive MFA and Passwordless Authentication](./03-adrs/0026-mfa-passwordless-adaptive-authentication.md)**: Establishes dynamic adaptive MFA, WebAuthn/Passkeys, cryptographic trusted devices, and self-service recovery.
*   📄 **[ADR-0027: Dual-Protocol REST & gRPC API Structure with Kong Gateway](./03-adrs/0027-dual-protocol-rest-grpc-api-gateway.md)**: Establishes dual public-facing REST and internal-facing high-performance gRPC interfaces.
*   📄 **[ADR-0028: Self-Hosted, Open-Source Infrastructure for Hybrid & On-Premise Deployments](./03-adrs/0028-self-hosted-hybrid-infrastructure-on-premise.md)**: Guarantees cloud-agnostic capability and localized deployments with zero cloud lock-in via MinIO, RabbitMQ, and HashiCorp Vault.
*   📄 **[ADR-0029: Tactical DDD Primitives Library](./03-adrs/0029-tactical-ddd-primitives-library.md)**: Adopts `@nestjslatam/ddd` as the pre-approved standard library for tactical DDD primitives in case DDD is selected as part of the design.

#### 🛡️ Operational Risk Management
*   📄 **[Vendor Lock-In & Financial Risk Assessment](./02-architecture/vendor-risk-assessment.md)**: Baseline documentation analyzing Identity Providers, Redis licensing, Feature Flag platforms, and Nx Cloud caching to prevent unexpected financial burdens.

#### Ã°Å¸Â”ºÃ¯Â¸Â Architectural Governance & ADR Status Matrix

Before starting the coding phase, the Product Owner (PO) has absolute authority to approve, defer, or veto any Architectural Decision Record (ADR). Below is the exhaustive classification of all 29 active decisions matching their file statuses:

### 🟢 1. APPROVED & ACCEPTED (Línea Base Autorizada Ã¢â‚¬” Ready for Coding)
These decisions are officially **Approved** and form the system's baseline architecture. Development must strictly adhere to these patterns:

| ADR ID | Decision Title | Status | Impact / Scope | Next Steps / Action |
| :--- | :--- | :--- | :--- | :--- |
| **ADR-0001** | [Monorepo Orchestration with Nx](./03-adrs/0001-monorepo-orchestration-nx.md) | 🟢 **Accepted** | Core monorepo organization and speed optimization. | Approved baseline. |
| **ADR-0002** | [Clean Architecture & Hexagonal Boundaries](./03-adrs/0002-clean-architecture-nestjs.md) | 🟢 **Accepted** | Decoupling core domain rules from database and frameworks. | Approved baseline. |
| **ADR-0003** | [Strict TypeScript Standards](./03-adrs/0003-strict-typescript-standards.md) | 🟢 **Accepted** | Static analysis quality gate and type enforcement. | Approved baseline. |
| **ADR-0004** | [React Query Offline Architecture](./03-adrs/0004-frontend-offline-resilience.md) | 🟢 **Accepted** | Local caching & fallback mechanism for client resilience. | Approved baseline. |
| **ADR-0005** | [Zero-Cost Security via CodeQL](./03-adrs/0005-ci-cd-quality-codeql.md) | 🟢 **Accepted** | Automated vulnerability scanning inside CI pipeline. | Approved baseline. |
| **ADR-0007** | [Loki & OpenTelemetry Strategy](./03-adrs/0007-observability-telemetry-loki-opentelemetry.md) | 🟢 **Accepted** | Distributed tracing and centralized log collection. | Approved baseline. |
| **ADR-0008** | [Gateway and BFF Patterns](./03-adrs/0008-progressive-multimodule-evolution-gateway-bff.md) | 🟢 **Accepted** | Optimizes network requests for multi-module clients. | Approved baseline. |
| **ADR-0009** | [Strict Dependency Pinning](./03-adrs/0009-strict-dependency-pinning-vulnerability-management.md) | 🟢 **Accepted** | Mitigates supply chain injection vulnerabilities. | Approved baseline. |
| **ADR-0010** | [Multi-Tenancy SaaS Strategy](./03-adrs/0010-multi-tenancy-architecture-strategy.md) | 🟢 **Accepted** | Defines database isolation strategy per corporate tenant. | Approved baseline. |
| **ADR-0011** | [Fault Tolerance & Resiliency](./03-adrs/0011-fault-tolerance-resiliency-patterns.md) | 🟢 **Accepted** | Circuit breakers (`opossum`) and exponential retries. | Approved baseline. |
| **ADR-0012** | [Advanced Authorization (RBAC/ABAC)](./03-adrs/0012-advanced-authorization-rbac-abac.md) | 🟢 **Accepted** | Fine-grained contextual permission modeling. | Approved baseline. |
| **ADR-0014** | [Distributed Caching (Redis)](./03-adrs/0014-distributed-caching-strategy-redis.md) | 🟢 **Accepted** | Memory caches for auth validations and active sessions. | Approved baseline. |
| **ADR-0015** | [Event-Driven Architecture](./03-adrs/0015-event-driven-architecture-intra-domain.md) | 🟢 **Accepted** | Asynchronous events publishing for state sync. | Approved baseline. |
| **ADR-0016** | [Immutable Business Audit Trail](./03-adrs/0016-immutable-business-audit-trail.md) | 🟢 **Accepted** | Application-level audit strategy using Domain Events. | Approved baseline. |
| **ADR-0017** | [Feature Flagging Strategy](./03-adrs/0017-feature-flagging-strategy.md) | 🟢 **Accepted** | Infrastructure-injected Feature Flags (Unleash/LaunchDarkly). | Approved baseline. |
| **ADR-0018** | [Testing Pyramid Quality Gates](./03-adrs/0018-testing-pyramid-quality-gates.md) | 🟢 **Accepted** | Coverage limits for E2E, Contract, and Unit tests (>70%). | Approved baseline. |
| **ADR-0019** | [Tactical Domain Patterns](./03-adrs/0019-tactical-design-patterns-future-proofing.md) | 🟢 **Accepted** | Result Pattern, Null Objects, and Decorators. | Approved baseline. |
| **ADR-0020** | [Identity Provider Abstraction](./03-adrs/0020-identity-provider-abstraction-strategy.md) | 🟢 **Accepted** | Decouples UMS from Auth0, Keycloak, or Entra ID. | Approved baseline. |
| **ADR-0021** | [High Performance Auth Graph](./03-adrs/0021-high-performance-auth-and-graph-compilation.md) | 🟢 **Accepted** | Optimized permission compiling under <5ms latency limit. | Approved baseline. |
| **ADR-0022** | [Pluggable Output Projections](./03-adrs/0022-contextual-auth-and-pluggable-projections.md) | 🟢 **Accepted** | Context-aware read projection layers outside the core. | Approved baseline. |
| **ADR-0023** | [Centralized vs Decentralized Access](./03-adrs/0023-centralized-ums-vs-decentralized-access.md) | 🟢 **Accepted** | Establishes the authoritative access kernel boundary. | Approved baseline. |
| **ADR-0024** | [Configuration & Feature Management](./03-adrs/0024-configuration-feature-management-platform.md) | 🟢 **Accepted** | Multi-IdP parameter dynamic engine. | Approved baseline. |
| **ADR-0025** | [Feature Flag Provider Abstraction](./03-adrs/0025-feature-flag-provider-abstraction.md) | 🟢 **Accepted** | Pluggable `IFeatureFlagPort` for Unleash/ConfigCat. | Approved baseline. |
| **ADR-0026** | [MFA and Passwordless Authentication](./03-adrs/0026-mfa-passwordless-adaptive-authentication.md) | 🟢 **Accepted** | WebAuthn, Passkeys, TOTP, and Adaptive Risk MFA. | Approved baseline. |
| **ADR-0027** | [Dual-Protocol REST & gRPC API Structure](./03-adrs/0027-dual-protocol-rest-grpc-api-gateway.md) | 🟢 **Accepted** | Public RESTful APIs and internal gRPC services. | Approved baseline. |
| **ADR-0028** | [Self-Hosted Hybrid Infrastructure](./03-adrs/0028-self-hosted-hybrid-infrastructure-on-premise.md) | 🟢 **Accepted** | Cloud-agnostic capability (MinIO, RabbitMQ, Vault OSS). | Approved baseline. |
| **ADR-0029** | [Tactical DDD Primitives Library](./03-adrs/0029-tactical-ddd-primitives-library.md) | 🟢 **Accepted** | Standardizes `@nestjslatam/ddd` for optional DDD use. | Approved baseline. |

### 🟡 2. PROPOSED & PENDING REVIEW (Pendientes de Revisión/Aprobación por el PO)
These decisions are currently **Proposed** and represent strategic backlogs. They **must be formally approved by the PO before starting coding**:

| ADR ID | Decision Title | Status | Impact / Scope | Required PO Action |
| :--- | :--- | :--- | :--- | :--- |
| **ADR-0006** | [Future Microservices via Dapr](./03-adrs/0006-future-microservices-transition-dapr.md) | 🟡 **Proposed** | Sidecar integration for distributed state and messaging. | **PO review/approve** to activate microservice migration. |
| **ADR-0013** | [Cloud Infrastructure & DR](./03-adrs/0013-cloud-infrastructure-topology-dr.md) | 🟡 **Proposed** | Multi-region disaster recovery replication limits. | **PO review/approve** to authorize deployment budget. |
| **ADR-0031** | [Identity Domain Abstraction to Subject](./03-adrs/0031-abstract-identity-domain-subject.md) | 🟡 **Proposed** | Transition Employee concept to agnostic Subject tied to Organization. | **PO review/approve** to enable complete B2B and multi-tenant support. |
| **ADR-0032** | [Organization as the Strategic Domain Boundary](./03-adrs/0032-organization-as-strategic-domain-boundary.md) | 🟡 **Proposed** | Establish Organization as the absolute container of Subjects and Systems for governance and security. | **PO review/approve** to formalize ownership and Zero Trust boundaries. |

### 🛠️€Â´ 3. CANCELLED, REJECTED OR VETOED (Rechazados o Descartados por el PO)
These architectural decisions have been **Vetoed / Rejected** or **Cancelled** by the Product Owner and **must never be implemented**:

*   *Currently, there are no rejected or cancelled decisions. You have full authority to move any ADR to this section to veto its implementation.*



---

### 🛠️ [Phase 04 - Engineering Standards and Artifacts](./04-artifacts/)
Technical guidelines, clean code rules, security standards, and technical quality plans.
*   📄 **[Global Engineering Standards](./04-artifacts/engineering-standards.md)**: SOLID, Clean Architecture, OWASP compliance, and DDD guidelines.
*   📄 **[Architecture Maturity Model (AMM)](./04-artifacts/architecture-maturity-model.md)**: TOGAF ACMM and Well-Architected Framework maturity assessment.
*   📄 **[Contract Testing Plan](./04-artifacts/contract-testing-plan.md)**: Safe microservices integration using Pact JS.
*   📄 **[Distributed Observability Strategy](./04-artifacts/observability-strategy.md)**: Unified telemetry using OpenTelemetry and Grafana Loki.
*   📄 **[Gap Analysis & Technical Debt](./04-artifacts/gap-analysis-and-optimization-plan.md)**: Architectural maturity assessment and technical mitigation plan.
*   📄 **[Enterprise IAM Spec](./04-artifacts/enterprise-iam-ums-specification.md)**: Dynamic authorization graph contracts and specifications.
*   📄 **[High-Concurrency Auth Spec](./04-artifacts/high-concurrency-auth-specification.md)**: Performance caching and token rotation schemas.
*   📄 **[UMS Web Console Product Spec](./04-artifacts/ums-web-console-product-scope.md)**: Administrative PAP control panel and SRE monitors.
*   📄 **[Configuration & Feature Management Platform Spec](./04-artifacts/ums-configuration-platform-spec.md)**: Multi-IdP config engine, system behavioral config, and feature flag framework.
*   📄 **[MFA & Passwordless Authentication Spec](./04-artifacts/mfa-passwordless-security-spec.md)**: High-assurance multi-factor (WebAuthn/Passkeys, TOTP, Email/SMS OTP) and adaptive risk-based authentication spec.
*   📄 **[BMAD Master Audit & Enterprise Spec](./04-artifacts/bmad-master-audit-alignment-report.md)**: Comprehensive business-models-architecture-delivery spec.
*   📄 **[Multi-Tenant Governance & Organizational Structure Report](./04-artifacts/enterprise-multitenant-governance-report.md)**: Domain evaluation, hierarchy definition, and Tenant-Aware / RLS strategy.



---

### 📈 [Phase 05 - Release Roadmap](./05-roadmap/)
Code release strategies, continuous deployment, and deployment automation.
*   📄 **[Versioning & Release Strategy](./05-roadmap/versioning-and-audit-strategy.md)**: Tags management and publications utilizing Nx Release.
