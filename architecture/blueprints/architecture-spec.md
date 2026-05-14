# 🏛️ Software Architecture Design Document (UMS)

This document details the formal system design specification for the **`arc-nodejs-workspace`** monorepo. It adopts the **C4 Model** software modeling standard (Level 1: System Context, Level 2: Containers, Level 3: Components) and presents the unified and audited technical inventory of the project.

> [!IMPORTANT]
> **Engineering Standards Mandate:** All architectural decisions described here are strictly governed by the **[Global Engineering Standards & BMAD Manifesto](../04-artifacts/engineering-standards.md)**. Principles like SOLID, Clean Architecture, optional DDD, and the avoidance of anti-patterns are mandatory and automatically enforced via CI/CD pipelines.

---

## 🎯 1. Architectural Deliverables & Requirements Baseline

The following table defines the mandatory deliverables, strategic scope, and contractual design requirements governing the software architecture of this monorepo:

| Priority | Deliverable | Description (Strategic Level – Executive Rationale) |
| :--- | :--- | :--- |
| **1** | [Bounded Context Map](./bounded-context-map.md) | Representation of the bounded contexts of the UMS IAM domain, their responsibilities, how they relate, and how they will evolve. Establishes a clear functional scope for teams and budgeting. |
| **2** | [Platform Core Definition](#-7-centralized-authorization-engine-architecture-peppdppappip) | Strategy that identifies cross-cutting capabilities (Identity, Master Data, Event Bus, API Gateway), their common purpose, and reuse principles. Justifies investments in shared components. |
| **3** | [C4 Diagram (Context, Container, Component)](#-2-c4-model) | Architectural vision at levels 1 and 2: external systems, large containers, and communication between them. Sizes technical complexity and allows estimating effort without detailing classes or internal components. |
| **4** | [Database Strategy](#-8-database-strategy--multi-tenant-isolation-rls) | Substantiates the choice of persistence pattern (Database-per-Module), guidelines for distributed transactions, and general backup and recovery policies. Details the impact on costs and operations. |
| **5** | [Event Domain Model (Event Storming)](#-10-asynchronous-communication--event-model) | Map of relevant business events, their producers, and consumers, along with delivery and ordering principles. Guides integration and the effort associated with orchestration/choreography. |
| **6** | [End-to-End Observability Strategy](#-9-observability--distributed-telemetry-strategy) | Approach to distributed telemetry: traceability of complete business processes, key metrics, and logging models at the architectural level. Used to estimate monitoring tools and costs. |
| **7** | [Identity & Authorization Design](#-7-centralized-authorization-engine-architecture-peppdppappip) | Strategy for the identity and authorization model: Identity Provider (IdP), authentication flow between contexts, and session guidelines. Helps size security across all domains. |
| **8** | Documented Non-Functional Requirements (NFRs) | Definition of measurable non-functional requirements that condition the architecture: latency, throughput, availability, and graceful degradation mechanisms. Represents contractual targets that the design must meet. |
| **9** | Master Data Management Strategy | Work plan for master data: key entities, migration approach from SAP, quality guidelines, and phases. Justifies the integration and data cleansing effort in the budget. |
| **10** | API Versioning & Evolution Strategy | Guidelines for contract evolution (APIs and events): how changes are introduced without breaking dependencies. Forecasts technical governance and the cost of maintaining compatibility. |
| **11** | Multi-Domain Synchronization Strategy | Approach to eventual consistency between contexts: definition of sources of truth, duplication guidelines, and conflict resolution. Reveals integration complexity and its impact on timelines. |
| **12** | [Initial Architecture Decision Records (ADRs)](#-4-architectural-decision-matrix) | Log of the most influential architectural decisions, their justification, and alternatives. Backs up why a specific path was chosen, clarifying risks and assumed costs. |
| **13** | [Integration Contract Testing Plan](#-12-quality-strategy--contract-testing) | Strategy to ensure interactions between contexts comply with their contracts, integrated into the CI/CD pipeline. Justifies quality assurance in integrations without detailing specific tools. |
| **14** | [Deployment Infrastructure](#-11-deployment-infrastructure--cloud-topology) | Layout of the topology (cloud/on-premise/hybrid), key managed services, and operational cost estimations. Provides a financial and technical baseline for sizing. |
| **15** | [Work Breakdown Structure & Plan](#-5-technical-debt-management--architectural-roadmap-backlog) | Roadmap with phases, sprints, profiles, milestones, and acceptance criteria. Translates strategy into a schedule and justifies workload and costs for each stage. |

---

## 🗺️ 2. C4 Model

The architectural design of UMS is modeled at three progressive levels of abstraction to align business vision with physical code implementation.

### Level 1: System Context Diagram
Defines the boundary of the User Management System (UMS) interacting with corporate users and external identity services.

```mermaid
graph TD
    User["Multi-Tenant Users (Tenant Staff)"]
    ExternalUser["B2B External Users (Clients/Suppliers)"]
    Sponsor["Sponsor User (Internal Approver)"]
    Admin["Global Admin / Tenant Admin / SRE"]
    UMS["UMS Authorization & Config Core"]
    UMSConsole["UMS Admin Web Console (PAP)"]
    ExternalAuth["External Identity Service (OAuth / Tenant IdP)"]
    InternalAuth["Internal Credential DB (Native Login)"]
    ExternalFlags["Feature Flag Providers (LaunchDarkly/Unleash)"]
    Downstream["Downstream SaaS Services"]

    User -->|Logs in via Auth Gateway| UMS
    ExternalUser -->|Submits Access Request| UMS
    Sponsor -->|Approves External Request (UC-12)| UMSConsole
    Admin -->|Manages profiles, configs, flags| UMSConsole
    UMSConsole -->|Calls Auth & Config APIs| UMS
    
    UMS -->|Branch A: Federated Auth| ExternalAuth
    UMS -->|Branch B: Local Native Auth| InternalAuth
    
    UMS -->|Evaluates Flags via Adapters| ExternalFlags
    UMS -->|Injects Auth Graph & Config State| Downstream
```

---

### Level 2: Container Diagram
Maps the physical subsystems (React Frontend, NestJS API, PostgreSQL Database) that make up the monorepo and how they communicate using secure protocols.

```mermaid
graph TD
    subgraph Clients["Client Applications"]
        ReactApp["Frontend React Web App (Client Portal)"]
        AdminApp["UMS Admin Console (PAP React App)"]
        MobileApp["Future Mobile App (iOS/Android)"]
    end

    subgraph Gateways["BFF Gateways"]
        WebBFF["Web BFF Gateway (.NET 8)"]
        MobileBFF["Mobile BFF Gateway (Payload Optimizer)"]
    end

    subgraph Server["Application Services (Tenant Isolated)"]
        BackendAPI["UMS Core (.NET 8 Auth, Identity, Profiles)"]
        ConfigAPI["Config & Feature Flag Module"]
        PostgresDB["PostgreSQL 16 Database (Shared Schema + RLS)"]
        AuditDB["Audit Ledger & Access Requests (UC-12)"]
        RedisCache["Redis Cluster (Auth Graph + Cfg + Flags)"]
    end

    subgraph ExternalServices["External Services"]
        ExternalIdP["Tenant IdPs (Zitadel / Azure AD / Okta)"]
        ExternalProviders["Flag Providers (LaunchDarkly / ConfigCat)"]
    end

    ReactApp -->|1. HTTPS / JWT + Tenant Header| WebBFF
    AdminApp -->|1. HTTPS / JWT + SuperAdmin Token| WebBFF
    MobileApp -->|1. HTTPS / Optimized Payload| MobileBFF
    WebBFF -->|2. Internal TCP / gRPC| BackendAPI
    WebBFF -->|2. Internal TCP / gRPC| ConfigAPI
    MobileBFF -->|2. Internal TCP / gRPC| BackendAPI
    BackendAPI -->|3. Sets LOCAL tenant context via RLS| PostgresDB
    ConfigAPI -->|3. Sets LOCAL tenant context via RLS| PostgresDB
    BackendAPI -->|4. Read-Aside cache lookup| RedisCache
    ConfigAPI -->|4. Read/Write config & flags| RedisCache
    
    BackendAPI -->|5a. [IDP Auth Branch] Verifies Token| ExternalIdP
    BackendAPI -->|5b. [Internal Auth Branch] Bcrypt check| PostgresDB
    
    ConfigAPI -->|5c. Pluggable Flag Eval via IFeatureFlagPort| ExternalProviders
    BackendAPI -->|6. Streams mutation events & Request Approvals| AuditDB
    ConfigAPI -->|6. Streams config events| AuditDB
```

---

### Level 3: API Component Diagram
An interactive zoom into the **.NET 8 API** structure, demonstrating the flow of control towards the core (*Inversion of Control*) of the Hexagonal Architecture and the use of **MediatR**.

```mermaid
graph TD
    subgraph Presentation["Ums.Presentation (Web API)"]
        Controller["UsersController / Minimal APIs (Entry Layer)"]
    end

    subgraph Application["Ums.Application (Use Cases)"]
        Handler["CreateUserCommandHandler (MediatR Handler)"]
        Command["CreateUserCommand (Application Contract)"]
        Validator["CreateUserValidator (FluentValidation)"]
    end

    subgraph Domain["Ums.Domain (Pure POCO Core)"]
        Entity["User Aggregate Root (Pure Business Entity)"]
        IUserRepo["IUserRepository (Persistence Port)"]
    end

    subgraph Infrastructure["Ums.Infrastructure (Adapters)"]
        EfRepo["EfUserRepository (EF Core Implementation)"]
        Postgres["Npgsql (Native PostgreSQL Driver)"]
    end

    Controller -->|Sends Command| Handler
    Handler -->|Validates with| Validator
    Handler -->|Instantiates and creates| Entity
    Handler -.->|Depends on| IUserRepo
    
    EfRepo -.->|Implements| IUserRepo
    EfRepo -->|Access via| Postgres
```

---

## 📊 3. Dependency Technical Inventory (Sovereign Tech Inventory)

This inventory details all tools, libraries, plugins, and components per workspace with their respective installed version, technical lifecycle recommendation (*Staff Recommendation*), and official reference URL.

### 🚀 A. Backend (.NET 8 API Layer)

| Dependency / Library | Installed Version | Technical Recommendation | Reference URL |
| :--- | :--- | :--- | :--- |
| `Microsoft.AspNetCore.App` | `8.0.x` | **Keep (Stable)** - High-performance framework for modern APIs. | [.NET 8 Docs](https://learn.microsoft.com/en-us/aspnet/core/) |
| `MediatR` | `^12.0.0` | **Keep (Critical)** - Decoupling implementation via Mediator pattern. | [MediatR GitHub](https://github.com/jbogard/MediatR) |
| `Microsoft.EntityFrameworkCore`| `8.0.x` | **Keep (Stable)** - High-performance ORM with RLS and migration support. | [EF Core Docs](https://learn.microsoft.com/en-us/ef/core/) |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | `8.0.x` | **Keep (Stable)** - Optimized driver for PostgreSQL. | [Npgsql Docs](https://www.npgsql.org/efcore/) |
| `FluentValidation` | `^11.0.0` | **Keep (Stable)** - Strongly-typed validation for commands and queries. | [FluentValidation](https://fluentvalidation.net/) |
| `BCrypt.Net-Next` | `^4.0.3` | **Keep (Stable)** - Secure hashing for credential storage. | [BCrypt.Net](https://github.com/BcryptNet/bcrypt.net) |
| `Swashbuckle.AspNetCore` | `^6.5.0` | **Keep (Stable)** - Automatic generation of OpenAPI 3 specifications. | [Swashbuckle Docs](https://github.com/domaindrivendev/Swashbuckle.AspNetCore) |
| `OpenTelemetry` | `^1.7.0` | **Keep (Critical)** - Industry standard for observability and traceability. | [OpenTelemetry .NET](https://opentelemetry.io/docs/instrumentation/net/) |

---

### ⚛️ B. Frontend (React Web Client)

| Dependency / Library | Installed Version | Technical Recommendation | Reference URL |
| :--- | :--- | :--- | :--- |
| `react` | `^18.3.1` | **Keep (Stable)** - Ultra-stable version compatible with mature ecosystems. | [React Documentation](https://react.dev/) |
| `vite` | `^5.4.10` | **Keep (Stable)** - Ultra-fast bundler compatible with Node 18. | [Vite JS](https://vitejs.dev/) |
| `@tanstack/react-query`| `^5.100.9` | **Keep (Critical)** - Asynchronous server state synchronization and smart caching. | [TanStack Query Docs](https://tanstack.com/query/latest) |
| `zustand` | `^5.0.13` | **Keep (Stable)** - Lightweight global state manager alternative to Redux. | [Zustand GitHub](https://github.com/pmndrs/zustand) |
| `tailwindcss` | `^3.4.19` | **Keep (Stable)** - High-performance utility-first CSS engine. | [Tailwind CSS](https://tailwindcss.com/) |
| `axios` | `^1.16.0` | **Keep (Stable)** - Robust HTTP client with global interceptor support. | [Axios Docs](https://axios-http.com/) |
| `lucide-react` | `^1.14.0` | **Keep (Stable)** - Modern collection of reactive SVG icons. | [Lucide Icons](https://lucide.dev/) |

---

### 🛠️ C. Quality and Global Governance (Root Monorepo)

| Dependency / Library | Installed Version | Technical Recommendation | Reference URL |
| :--- | :--- | :--- | :--- |
| `nx` | `^20.3.0` | **Keep (Critical)** - High-performance task runner with caching support. | [Nx Dev Docs](https://nx.dev/) |
| `eslint-plugin-boundaries`| `^5.0.0` | **Keep (Stable)** - Strict governance for Hexagonal boundaries. | [eslint-plugin-boundaries](https://github.com/javierguzman/eslint-plugin-boundaries) |
| `eslint-plugin-sonarjs` | `^3.0.0` | **Keep (Stable)** - Zero-cost Sonar static analysis for local projects. | [SonarJS ESLint](https://github.com/SonarSource/eslint-plugin-sonarjs) |
| `husky` | `^9.0.0` | **Keep (Stable)** - Interception and automation of Git Hooks. | [Husky Docs](https://typicode.github.io/husky/) |
| `lint-staged` | `^15.0.0` | **Keep (Stable)** - Optimized execution of linters only on Git Staged files. | [lint-staged GitHub](https://github.com/lint-staged/lint-staged) |

---

## 🧠 4. Architectural Decision Matrix

This matrix maps foundational technical decisions to their targeted Quality Attributes, summarizing the architectural strategy and enforcement mechanisms to ensure a verifiable and sustainable system under the **spec-driven AI strategy BMAD-METHOD**:

| Decision / Focus | ADR Reference | Primary Quality Attributes | Decision Summary & Technical Strategy | Enforcement & Verification Mechanism |
| :--- | :--- | :--- | :--- | :--- |
| **Monorepo Orchestration** | [ADR 0001](../03-adrs/0001-monorepo-orchestration-nx.md) | Modularity, Build Performance | Uses Nx & npm workspaces to manage decoupled modules with localized configurations. | Nx cache verification and localized dependency schema checks. |
| **Hexagonal Boundaries** | [ADR 0002](../03-adrs/0002-clean-architecture-nestjs.md) | Decoupling, Testability, Agnosticism | Implements three strict layers: Core (Entities), Application (Use Cases), Infrastructure (Adapters). | `eslint-plugin-boundaries` blocks unauthorized outer-to-inner imports. |
| **Observability Telemetry** | [ADR 0007](../03-adrs/0007-observability-telemetry-loki-opentelemetry.md) | Observability, Performance, Monitoring | Grafana LGTM Stack (Loki + Grafana + Tempo) with OpenTelemetry (OTel). | OpenTelemetry integration tests and Grafana dashboards monitoring. |
| **Dependency Governance** | [ADR 0009](../03-adrs/0009-strict-dependency-pinning-vulnerability-management.md) | Security, Stability, Determinism | Enforces zero-tolerance for dynamic versions (`^`/`~` removed) to guarantee reproducible builds. | `npm audit --audit-level=high` runs in CI to block vulnerable PRs. |
| **Multi-Tenancy SaaS** | [ADR 0010](../03-adrs/0010-multi-tenancy-architecture-strategy.md) | Security, Data Isolation, Cost Efficiency | Shared Database schema with PostgreSQL Row-Level Security (RLS) to enforce tenant isolation. | `AsyncLocalStorage` propagates Tenant Context; TypeORM Subscribers validate RLS. |
| **Fault Tolerance & Resiliency** | [ADR 0011](../03-adrs/0011-fault-tolerance-resiliency-patterns.md) | Resilience, Reliability, Consistency | Circuit Breaker (`opossum`) + Exponential Backoff retries strictly wrapped inside Infrastructure Adapters. | Jest mocks simulating HTTP failures and verifying circuit state transitions. |
| **Granular Authorization** | [ADR 0012](../03-adrs/0012-advanced-authorization-rbac-abac.md) | Security, Traceability, SoC | Tenant-aware RBAC/ABAC using JWT claim decoders and NestJS execution context Guards. | Integration tests simulating cross-tenant access attempts. |
| **Distributed Caching** | [ADR 0014](../03-adrs/0014-distributed-caching-strategy-redis.md) | Performance, Database Offloading | Read-Aside caching with Redis store, completely hidden behind a pure Core `ICachePort` abstraction. | Redis integration tests and strict TTL verification. |
| **Event-Driven Decoupling** | [ADR 0015](../03-adrs/0015-event-driven-architecture-intra-domain.md) | Decoupling, Scalability, Extensibilidad | Monolith modules communicate asynchronously using an internal event bus hidden behind `IEventBusPort`. | Unit tests verifying asynchronous execution paths and payload formats. |
| **Immutable Auditing** | [ADR 0016](../03-adrs/0016-immutable-business-audit-trail.md) | Traceability, Compliance, Security | Automatically tracks business-critical mutations (Old Value -> New Value) using database subscribers. | TypeORM Lifecycle Hook interceptors with strictly isolated tables. |
| **Tactical Domain Integrity** | [ADR 0019](../03-adrs/0019-tactical-design-patterns-future-proofing.md) | Decoupling, Clarity, Dapr Readiness | Uses Result Pattern, Null Objects, and Decorators to protect the Core from throwing HTTP/external framework errors. | Mandatory return types and custom ESLint boundaries rules. |
| **Identity Provider Abstraction** | [ADR 0020](../03-adrs/0020-identity-provider-abstraction-strategy.md) | Decoupling, Vendor Neutrality, Extensibilidad | Abstracts external directories (Zitadel, Okta, SAML) via the Strategy Pattern wrapped under a Hexagonal Port. | Jest unit tests verifying agnostic credential routing. |
| **High-Performance Compilation** | [ADR 0021](../03-adrs/0021-high-performance-auth-and-graph-compilation.md) | Performance, Ultra-Low Latency, Scalability | Resolves dynamic hierarchical permission graphs under 5ms using Redis read-aside caching. | Locust load tests and SRE telemetry tracing. |
| **Contextual Authentication** | [ADR 0022](../03-adrs/0022-contextual-auth-and-pluggable-projections.md) | Multi-Tenancy, Customization, Extensibilidad | Supports localized corporate branch context resolution and projects compiled graphs into multiple output formats. | Integration tests verifying branch-specific (sedes) dynamic menu structures. |
| **Centralized Auth Kernel** | [ADR 0023](../03-adrs/0023-centralized-ums-vs-decentralized-access.md) | Security, SoC, Governance | Establishes the UMS as a centralized authorization core shared across all enterprise applications. | Strict ESLint boundary checks and centralized access ledger audits. |


---

## 📈 5. Technical Debt Management & Architectural Roadmap (Backlog)

To guarantee the healthy evolution of the monorepo towards distributed models and production telemetry, the following items are established in the architecture backlog:

*   **[ADR 0006: Future Microservices Transition with Dapr](../03-adrs/0006-future-microservices-transition-dapr.md)**: Establishes the technical criteria and triggers that will determine when to split the modular monolith into independent microservices governed by Dapr sidecars.
*   **[ADR 0008: Progressive Multi-Module Evolution with API Gateway and BFF](../03-adrs/0008-progressive-multimodule-evolution-gateway-bff.md)**: Establishes the progressive design to transform this 100% Node.js reference solution into a multi-module portal capable of integrating independent systems (TMS, WMS, etc.) exposed as services with isolated databases, consumed via a central API Gateway and optimized through Backend For Frontend (BFF) gateways for Web and Mobile clients.
*   **[ADR 0009: Strict Dependency Pinning and Automated Vulnerability Management](../03-adrs/0009-strict-dependency-pinning-vulnerability-management.md)**: Establishes the strategy of zero-tolerance for dynamic dependency versions, enforcing static versions across the monorepo, with automated dependency bot updates and high/critical CI vulnerability checks.
*   **[ADR 0010: Multi-Tenancy Architecture Strategy for SaaS Evolution](../03-adrs/0010-multi-tenancy-architecture-strategy.md)**: Establishes the hybrid pooled multi-tenancy strategy utilizing a shared PostgreSQL schema coupled with Row-Level Security (RLS) to enforce absolute data isolation at the engine level for cost-effective SaaS scalability.
*   **[ADR 0013: Cloud Infrastructure Topology & DR](../03-adrs/0013-cloud-infrastructure-topology-dr.md)**: Establishes high availability and disaster recovery topologies across multiple availability zones.
*   **[ADR 0024: Configuration & Feature Management Platform](../03-adrs/0024-configuration-feature-management-platform.md)**: Extends UMS to handle dynamic system configuration and multi-IdP setups.
*   **[ADR 0025: Feature Flag Provider Abstraction](../03-adrs/0025-feature-flag-provider-abstraction.md)**: Pluggable framework for external flag providers via `IFeatureFlagPort`.


---

## 🛡️ 6. Financial & Operational Risk Assessment

An exhaustive evaluation of **"Build vs. Buy"** decisions, licensing implications, and operational costs associated with the sovereign tech stack. 

*   **[Vendor Lock-In & Financial Risk Assessment](./vendor-risk-assessment.md)**: Baseline documentation analyzing Identity Providers, Redis licensing, Feature Flag platforms, and Nx Cloud caching to prevent unexpected financial burdens.

---

## 🏛️ 7. Centralized Authorization Engine Architecture (PEP/PDP/PAP/PIP)

To support secure, context-aware, and highly scalable access control across all corporate applications, the system adopts a centralized **User Management System (UMS)** serving as a shared "authorization kernel". This architecture decouples identity validation from dynamic permission resolution by implementing standard **XACML Architectural Reference Model** layers:

1.  **Policy Enforcement Point (PEP)**: Intercepts incoming client requests at the API Gateway or individual NestJS Guards, enforcing access rules by reading the returned authorization graph.
2.  **Policy Decision Point (PDP)**: The core UMS Engine. It compiles and resolves fine-grained permissions into a cached, hierarchical graph under 5ms using Redis.
3.  **Policy Administration Point (PAP)**: The UMS administrative portal where security teams manage baseline templates, tenant profiles, and explicit permission rules.
4.  **Policy Information Point (PIP)**: Relational PostgreSQL registries supplying active tenant, branch (sedes), and user attributes during graph evaluation.

By utilizing the **Strategy Pattern** for dynamic output projections, the UMS can format the compiled graph into a variety of target structures on-the-fly (including frontend-optimized JSON, cryptographically signed JWT scopes, or Claims-based lists), ensuring high adaptability and complete zero-lock-in longevity. For a complete analysis of the Business Analyst reference model and API contracts, consult **[enterprise-iam-ums-specification.md](../04-artifacts/enterprise-iam-ums-specification.md)**.

---

## 🗄️ 8. Database Strategy & Multi-tenant Isolation (RLS)

The system utilizes a **Shared Database** model with logical isolation reinforced via PostgreSQL **Row-Level Security (RLS)**. This ensures that no tenant can ever access data from another, even if an application-layer error occurs.

```mermaid
sequenceDiagram
    participant App as .NET 8 API (DbContext)
    participant PG as PostgreSQL 16 Engine
    participant Table as Protected Table (e.g., Users)

    App->>PG: Opens connection from pool
    App->>PG: Executes SET LOCAL app.current_tenant = 'tenant_uuid'
    App->>PG: SELECT * FROM users WHERE ...
    PG->>PG: Evaluates native RLS Policy
    PG->>Table: Filters rows by tenant_id
    Table-->>PG: Returns only authorized rows
    PG-->>App: Filtered and secure dataset
```

---

## 📊 9. Observability & Distributed Telemetry Strategy

Strict adherence to the **OpenTelemetry** standard to guarantee sovereign monitoring data and avoid cloud-provider lock-in (Cloud-Agnostic).

```mermaid
graph LR
    App["Application (.NET / React)"]
    OTelCol["OpenTelemetry Collector (Sidecar)"]
    Prom["Prometheus (Metrics)"]
    Loki["Grafana Loki (Logs)"]
    Tempo["Grafana Tempo (Tracing)"]
    Grafana["Grafana Dashboards"]

    App -->|Push OTLP| OTelCol
    OTelCol -->|Scrape| Prom
    OTelCol -->|Push| Loki
    OTelCol -->|Push| Tempo
    Prom --> Grafana
    Loki --> Grafana
    Tempo --> Grafana
```

---

## 🔄 10. Asynchronous Communication & Event Model

Clear differentiation between **Domain Events** (within the same bounded context) and **Integration Events** (between different contexts or external systems) to maintain module autonomy.

```mermaid
graph TD
    subgraph ContextA["Identity Context"]
        DomainEvent["Domain Event (UserCreated)"]
        HandlerA["Internal Handler (Ums.Application)"]
    end

    subgraph Messaging["Message Bus (RabbitMQ)"]
        IntegrationEvent["Integration Event (UserProvisioned)"]
    end

    subgraph ContextB["Audit Context"]
        HandlerB["Audit Subscriber"]
    end

    DomainEvent --> HandlerA
    HandlerA --> IntegrationEvent
    IntegrationEvent --> HandlerB
```

---

## ☁️ 11. Deployment Infrastructure & Cloud Topology

Design optimized for **Kubernetes** with the capability to deploy in public clouds or private on-premise environments.

```mermaid
graph TD
    Internet((Internet))
    LB["Load Balancer (Cloud/Metal)"]
    Ingress["NGINX Ingress Controller"]
    Vault["HashiCorp Vault (Secrets)"]
    
    subgraph K8s["Kubernetes Cluster"]
        subgraph NS_UMS["Namespace: ums-prod"]
            BFF["Pod: Web-BFF"]
            API["Pod: Core-API"]
            Redis["Pod: Redis-Sentinel"]
        end
    end

    DB[(PostgreSQL 16 Cluster)]

    Internet --> LB
    LB --> Ingress
    Ingress --> BFF
    BFF --> API
    API --> Redis
    API --> DB
    API -.->|Reads secrets| Vault
```

---

## 🧪 12. Quality Strategy & Contract Testing

To ensure that changes in one context do not break its consumers, automated contract tests are implemented.

- **Unit Tests**: Pure logic in `Ums.Domain`.
- **Integration Tests**: Using **Testcontainers** to validate real behavior with PostgreSQL and Redis.
- **Contract Tests**: Validation of OpenAPI schemas and asynchronous events.



