# 📐 Authoritative Technology Stack Definition — UMS

**Document Type:** Architecture Blueprint  
**Status:** Approved  
**Framework:** bMAD Phase 02 (Architecture)  
**Sovereignty:** 100% Cloud-Agnostic / On-Premise Capable  

---

## 🧭 Executive Context Reference

*   **Product Name:** User Management System (UMS)
*   **Product Type:** Hybrid (SaaS & Localized On-Premise deployments)
*   **Primary Users:** Integrated Application Users (Operators, Business Analysts), B2B Tenant Admins
*   **Expected Scale (Initial):** < 1,000 tenants, ~50 concurrent users per tenant (~50,000 active concurrent connections total)
*   **Expected Scale (Target):** > 10,000 tenants, ~500 concurrent users per tenant (~5,000,000 active concurrent connections total)
*   **Team Size:** ~5–10 Engineers
*   **Team Expertise:** Strong NestJS & TypeScript/JavaScript, some DevOps (Docker, Kubernetes), no Java expertise
*   **Existing Constraints:** Transitioning to .NET 8 for Core API, SQL Server relational engine, high-performance Redis cache, Dapr-ready architecture, strict on-premise K8s deployment capability.
*   **Non-Negotiables:** Absolutely zero cloud-provider SDK dependencies in the core domain layer (strict Hexagonal Architecture); 100% self-hostable open-source infrastructure alternatives.
*   **Polyglot Strategy (ADR 0026):** .NET 8 (Core) & Node.js (Satellites) -> **Unified SQL Server 2022 Engine**.

---

## 1. Runtime & Language

### 1.1 Language + Version
*   **Primary Enterprise Core:** **C# running on .NET 8 LTS**
    *   **Why:** Superior performance for compute-intensive tasks, enterprise-grade tooling, and strict type safety.
*   **Satellite/Lightweight Services:** **TypeScript v5.4+ running on Node.js v20 LTS**
    *   **Why:** High developer velocity and rich ecosystem for non-core modules.
*   **Alternatives Rejected:**
    *   *Golang*: Rejected due to lack of team expertise; introducing a new language would delay time-to-market.
    *   *Java (Spring Boot)*: Rejected due to higher memory footprint and lack of team expertise.

### 1.2 Type System / Compiler Setup
*   **Chosen Tool:** **Strict TypeScript Compiling compiled via SWC (`@swc/core`) inside Nx Monorepo**
*   **Why Chosen:** `strict: true` enforces zero-implicit-any and strict null checks, preventing common runtime errors. SWC compiles TypeScript up to 20x faster than traditional `tsc`, significantly accelerating local dev cycles and CI/CD runs.
*   **Alternatives Rejected:**
    *   *tsc (standard TypeScript compiler)*: Rejected as primary build engine due to slow compilation times under high-concurrency monorepo setups, but retained solely for type-checking (`tsc --noEmit`).

### 1.3 Linting & Formatting Toolchain
*   **Chosen Tool:** **ESLint v8 + Prettier v3 integrated via Husky and lint-staged**
*   **Why Chosen:** Guarantees automated, uniform code formatting and static analysis at the pre-commit stage, preventing unformatted or buggy code from entering the repository.
*   **Alternatives Rejected:**
    *   *TSLint*: Deprecated; no longer supported.

---

## 2. API Layer

### 2.1 Primary API Protocol and Framework
*   **Chosen Tool:** **Dual-Protocol Engine: REST (via NestJS Express) + gRPC (via NestJS Microservices)**
*   **Why Chosen:**
    *   *REST (JSON)*: Serves as the public-facing API for downstream client integrations and the UMS Hosted Login page due to its universal compatibility.
    *   *gRPC (Protocol Buffers)*: Enforces high-performance, low-latency, and type-safe internal communication (BFF to UMS, or service-to-service) to guarantee permission graphs resolve in **under 5ms**.
*   **Alternatives Rejected:**
    *   *GraphQL*: Rejected due to high caching complexity, query-complexity overhead, and the difficulty of maintaining predictable sub-5ms latency limits under massive B2B multi-tenant nesting.

### 2.2 API Documentation Standard
*   **Chosen Tool:** **OpenAPI v3 (Swagger) generated dynamically via NestJS decorators**
*   **Why Chosen:** Ensures that public-facing REST APIs are automatically documented, interactive, and completely synchronized with the codebase, with zero manual document maintenance.
*   **Alternatives Rejected:**
    *   *Manual Postman Collections*: High maintenance overhead and prone to falling out-of-sync with production APIs.

### 2.3 Validation Library
*   **Chosen Tool:** **`class-validator` + `class-transformer`**
*   **Why Chosen:** Integrates natively with NestJS Pipes to enforce declarative, decorator-based validation directly on DTOs (Data Transfer Objects) at the network ingress.
*   **Alternatives Rejected:**
    *   *Joi / Zod*: While highly performant, they do not integrate as seamlessly with NestJS's class-based dependency injection and decorators as `class-validator`.

---

## 3. Gateway Layer

### 3.1 Gateway Solution per Client
*   **Chosen Tool:** **Kong API Gateway (Open Source Edition)**
*   **Why Chosen:** A cloud-agnostic, lightweight, and extremely high-performance API gateway (built on Nginx) that handles B2B rate-limiting, IP allowlisting, and routing. It runs natively in Kubernetes and is completely self-hostable on-premise.
*   **Alternatives Rejected:**
    *   *AWS API Gateway / Azure API Management*: Rejected because they are proprietary, cloud-locked, and cannot be run on-premise inside local client networks.

### 3.2 Authentication Mechanism
*   **Chosen Tool:** **RS256 Signed JSON Web Tokens (JWT) + mutual TLS (mTLS)**
*   **Why Chosen:** JWTs enable stateless, cryptographically verified user sessions. mTLS (managed by Istio/Linkerd or Kong) secures all internal container-to-container traffic, adhering to Zero Trust guidelines.
*   **Alternatives Rejected:**
    *   *Stateful Session Cookies*: Restricts horizontal scaling by requiring session synchronization or sticky sessions across distributed container instances.

### 3.3 Rate Limiting Strategy
*   **Chosen Tool:** **Sliding-Window Rate Limiting enforced at Kong Gateway using Redis**
*   **Why Chosen:** Prevents brute-force or denial-of-service attempts. Enforcing this at the Gateway layer using Redis saves application-level CPU cycles by dropping bad traffic before it reaches NestJS pods.
*   **Alternatives Rejected:**
    *   *Application-level memory rate-limiting*: Risks memory exhaustion and does not share rate-limiting state across multiple horizontal application pods.

---

## 4. Domain & Application Layer

### 4.1 Architectural Pattern
*   **Chosen Tool:** **Hexagonal Architecture (Ports & Adapters) / Clean Architecture**
*   **Why Chosen:** Mandated to ensure the core Domain layer has **absolutely zero dependencies** on NestJS, TypeORM, SQL Server, or external cloud SDKs. Core logic communicates exclusively with interfaces (Ports), making the kernel completely sovereign and future-proof.
*   **Alternatives Rejected:**
    *   *Standard 3-Tier Layered Architecture*: Creates strong coupling between business logic, database ORMs, and network frameworks, violating our non-negotiable sovereignty constraint.

### 4.2 Module / Bounded Context Strategy
*   **Chosen Tool:** **Modular Monolith inside Nx Monorepo (Dapr-ready)**
*   **Why Chosen:** Minimizes initial operational and deployment complexity for our 5-10 engineer team. All contexts (`Identity`, `Authorization`, `Configuration`, `Audit`) are isolated within strict library boundaries in Nx, allowing them to be split into independent Dapr microservices without refactoring core domain models.
*   **Alternatives Rejected:**
    *   *Distributed Microservices from Day 1*: High operational complexity, deployment overhead, and network latency that would overwhelm a small engineering team.

### 4.3 CQRS Approach
*   **Chosen Tool:** **Internal CQRS via NestJS CQRS module**
*   **Why Chosen:** Decouples heavy permission graph compilation (Queries) from basic identity mutations (Commands), optimizing read performance. Caches read-projections in Redis while writing sequentially to **SQL Server 2022**.
*   **Alternatives Rejected:**
    *   *Direct CRUD*: Directly querying and compiling relational tables on every read request degrades database performance under high concurrent loads.

---

## 5. Data Layer

### 5.1 Primary Database + ORM/Query Builder
*   **Unified Relational Engine:** **SQL Server 2022 (All Services)**
    *   **For .NET 8 Services:** EF Core with SQL Server Provider.
    *   **For Node.js Services:** TypeORM / Prisma with `mssql` driver.
    *   **Why:** Official corporate standard, superior RLS via `SESSION_CONTEXT`, and operational simplicity for on-premise deployments.

### 5.2 Migration Strategy
*   **Chosen Tool:** **TypeORM Migrations executed via K8s Init-Containers**
*   **Why Chosen:** Guarantees that database schemas are versioned and migrations are executed sequentially and successfully prior to application pods spinning up, preventing schema desynchronization during rolling updates.
*   **Alternatives Rejected:**
    *   *TypeORM `synchronize: true`*: Extremely dangerous for production environments as it can cause accidental data loss.

### 5.3 Caching Layer
*   **Chosen Tool:** **Redis v7.2 (Self-hosted Sentinel / Cluster)**
*   **Why Chosen:** Provides an ultra-low latency, distributed, and self-hostable in-memory cache. Replicated Sentinel/Cluster setups ensure high availability and sub-3ms read times for compiled authorization graphs.
*   **Alternatives Rejected:**
    *   *Memcached*: Lacks robust data structure support (hashes, sets, sorted sets) and native replication failover.

### 5.4 Object / File Storage
*   **Chosen Tool:** **MinIO (Self-hosted, S3-Compatible)**
*   **Why Chosen:** Completely self-hostable, high-performance object storage. It implements the exact AWS S3 API contract, allowing local deployments without cloud lock-in.
*   **Alternatives Rejected:**
    *   *AWS S3*: Rejected as a primary choice because it is a proprietary cloud service that cannot run on-premise for localized deployments.

### 5.5 Message Queue / Event Bus
*   **Chosen Tool:** **RabbitMQ (Self-hosted, AMQP v0.9.1)**
*   **Why Chosen:** High-performance, lightweight, and fully self-hostable broker with robust routing capabilities. Fits on-premise networks perfectly and carries low administrative overhead.
*   **Alternatives Rejected:**
    *   *Apache Kafka*: Offers higher throughput but carries massive administrative and infrastructure overhead (Zookeeper/KRaft) that is unnecessary for our initial scale.

---

## 6. Multi-tenancy Strategy

### 6.1 Isolation Model
*   **Strategy:** **Shared Database with Native Row-Level Security (RLS)**
*   **Implementation (Unified):** Uses SQL Server `SESSION_CONTEXT('TenantId', @value)` and Security Policies with iTVF predicates for both .NET and Node.js.
*   **Why Chosen:** High packing density and absolute isolation at the engine level with a single implementation to maintain.
*   **Alternatives Rejected:**
    *   *Database-per-tenant*: High infrastructure cost and severe administrative overhead when managing thousands of databases.
    *   *Schema-per-tenant*: Becomes hard to scale and migrate when tenant counts exceed 1,000, causing connection pool exhaustion.

### 6.2 Tenant Resolution Mechanism
*   **Chosen Tool:** **NestJS Interceptor + SQL Server SESSION_CONTEXT**
*   **Why Chosen:** Resolves the `tenant_id` from JWT claims or `X-Tenant-ID` headers at ingress, and uses a database interceptor/wrapper to inject the tenant context into the active SQL Server session using `sp_set_session_context`.
*   **Alternatives Rejected:**
    *   *Application-level filtering*: Prone to developer omissions (forgetting a `WHERE tenant_id = x` clause), leading to critical data leak vulnerabilities. RLS prevents this at the database level across all runtimes.

---

## 7. Infrastructure & Deployment

### 7.1 Containerization
*   **Chosen Tool:** **Docker v25 with Multi-Stage distroless builds**
*   **Why Chosen:** Reduces the container image size to the absolute minimum and eliminates standard shell utilities, significantly hardening the production containers against remote execution exploits.

### 7.2 Orchestration
*   **Chosen Tool:** **Kubernetes (K8s v1.28+)**
*   **Why Chosen:** Standardizes deployment, scaling, and self-healing. Works identically across public clouds (EKS, GKE) and local on-premise clusters (MicroK8s, Rancher K3s, OpenShift).

### 7.3 Configuration & Secrets Management
*   **Chosen Tool:** **HashiCorp Vault (OSS, Self-hosted)**
*   **Why Chosen:** A highly secure, enterprise-grade, and self-hostable secrets store. Secrets are injected dynamically into K8s pods via Vault Agent Injector sidecars, ensuring credentials are never exposed in plaintext.
*   **Alternatives Rejected:**
    *   *K8s Standard Secrets*: Stored in base64 plaintext inside etcd, which is insecure without complex KMS integration.

### 7.4 Helm Chart / Deployment Strategy
*   **Chosen Tool:** **Helm v3 parameterized charts**
*   **Why Chosen:** Allows package-based deployments, templating all UMS resources while enabling easy parameter swaps (e.g., toggling local MinIO versus cloud S3) between environments.

---

## 8. Observability

### 8.1 Instrumentation Standard
*   **Chosen Tool:** **OpenTelemetry (W3C Trace Context standard)**
*   **Why Chosen:** Mandated by our non-negotiables. Guarantees that the application code remains entirely vendor-neutral. If we switch from Jaeger/Loki to Datadog or New Relic, we do not have to modify a single line of application code.

### 8.2 Metrics
*   **Chosen Tool:** **Prometheus pulling from OpenTelemetry Collector**
*   **Why Chosen:** Standard, self-hostable, and extremely high-performance metrics aggregator for Kubernetes environments.

### 8.3 Distributed Tracing
*   **Chosen Tool:** **Jaeger (OSS, Self-hosted)**
*   **Why Chosen:** Highly reliable, self-hostable distributed tracing engine that receives standard OpenTelemetry trace spans.

### 8.4 Log Aggregation
*   **Chosen Tool:** **Grafana Loki (OSS, Self-hosted)**
*   **Why Chosen:** Extremely efficient log aggregation system that uses the same metadata labels as Prometheus, enabling seamless correlation between metrics and logs inside Grafana dashboards.

---

## 9. Security

### 9.1 Auth & Identity
*   **Chosen Tool:** **Federated OIDC/SAML Resolvers with UMS Native BCrypt Store Fallback**
*   **Why Chosen:** Ensures enterprise-grade identity federation (Okta, Keycloak, Azure AD) out-of-the-box via OIDC/SAML configurations, while retaining a local, secure BCrypt-hashed user table to support localized on-premise operations.

### 9.2 RBAC / ABAC Approach
*   **Chosen Tool:** **Hierarchical RBAC compiled into fine-grained permission graphs with ABAC context evaluation**
*   **Why Chosen:** Satisfies both role-based routing (for UI rendering) and precise attribute evaluation (geofencing, action thresholds) required by modern integrated client portals.

### 9.3 Dependency Audit Tooling
*   **Chosen Tool:** **Snyk Open Source CLI + `npm audit` run in CI/CD pipeline**
*   **Why Chosen:** Blocks builds automatically if critical vulnerabilities (CVEs) are detected in npm packages, protecting the software supply chain.

---

## 10. Developer Experience

### 10.1 Local Development Setup
*   **Chosen Tool:** **Docker Compose Spec**
*   **Why Chosen:** Allows developers to spin up the entire UMS dependency suite (**SQL Server 2022**, Redis, RabbitMQ, MinIO) locally with a single command (`docker compose up -d`).

### 10.2 Monorepo vs. Multi-repo
*   **Chosen Tool:** **Nx Monorepo**
*   **Why Chosen:** Simplifies dependency management, allows sharing TypeScript types between frontend and backend instantly, and uses advanced build caching to minimize CI compile times.

### 10.3 Testing Pyramid
*   **Chosen Tool:**
    *   *Unit Tests*: Jest (aiming for >80% coverage).
    *   *Integration Tests*: Jest + Supertest with **Testcontainers** (spinning up ephemeral **SQL Server Edge/Express** and Redis instances in local Docker for realistic testing).
    *   *End-to-End*: Playwright for Web Console regression testing.

---

## 11. Third-party Services

To avoid cloud-provider lock-in and support offline, on-premise environments, **zero external SaaS integrations are mandatory**. Optional integrations are fully abstracted behind Domain Ports.

| Service Name | Purpose | Why NOT Internally | Cloud-Agnostic Alternative | Domain Interface |
| :--- | :--- | :--- | :--- | :--- |
| **Twilio** | SMS OTP Delivery | Telecommunication carrier gateways require complex global agreements. | Local SMTP-to-SMS Gateway or self-hosted SMS Modem | `ISmsPort` |
| **SendGrid** | Transactional Emails | Managing IP reputation and mail delivery queues is a massive operational overhead. | Self-hosted Postfix / Haraka SMTP server | `IEmailPort` |

---

## 12. Vendor Lock-in Risk Register

| Component | Chosen Solution | Lock-in Risk | Mitigation Strategy | Re-evaluate Trigger |
| :--- | :--- | :--- | :--- | :--- |
| **Database** | SQL Server 2022 | **Medium** | Standard SQL compliance. Domain layer has no direct dependency (decoupled via Ports). Use of RLS creates some lock-in. | Licensing cost change |
| **Object Store** | MinIO | **Low** | MinIO uses the exact AWS S3 API contract. Swapping requires a simple config change. | Performance bottlenecks |
| **Secrets Store**| HashiCorp Vault | **Low** | Secret resolution is abstracted via K8s secrets injection or custom Adapter. | Licensing model changes |
| **Gateway** | Kong Gateway | **Low** | Configuration is managed via standard K8s Ingress resources. | Custom routing constraints |

---

## 13. Decision Log

### Decision 1: Polyglot Backend Strategy
*   **Decision:** **.NET 8 for Enterprise Core; Node.js for Satellite Services.**
*   **Rationale:** Balances high-performance/safety requirements for the core with development speed for utilities.
*   **Revisit When:** Organizational skills shift significantly or a unified single-runtime model is required.

### Decision 2: Unified Database Engine (ADR 0026)
*   **Decision:** **SQL Server 2022 for all services (.NET and Node.js).**
*   **Rationale:** Eliminates infrastructure fragmentation and ensures unified security enforcement (RLS).
*   **Revisit When:** Licensing costs exceed budget or a specific context requires native NoSQL features.

### Decision 3: Single Database Engine Strategy — SQL Server 2022 for all services
*   **Decision:** All services, including NestJS satellites (Config, Template, Profile managers), must persist exclusively in SQL Server 2022.
*   **Rationale:** Polyglot persistence (PG/Mongo) was rejected to minimize operational overhead in on-premise deployments and unify the Row-Level Security (RLS) implementation.
*   **Impact:** All services share a single backup, security, and maintenance strategy. Node.js services must use the `mssql` driver.
*   **Revisit When:** Re-evaluate if unstructured data volume requires a specialized NoSQL engine for non-relational workloads.

---

## 14. Open Questions

1.  **On-premise SMS Gateways:** What local SMS hardware or telecommunication providers are pre-approved by localized enterprise clients?
    *   *Information Needed:* Active local SMS contracts or SMS gateway hardware specs.
    *   *BMAD Resolver:* **Dev Agent / Infra Agent** during Phase 05 onboarding.
2.  **OIDC Dynamic Registration:** Should on-premise installations support dynamic OIDC client registrations, or must they be pre-configured statically via Helm?
    *   *Information Needed:* Client IT infrastructure onboarding capabilities.
    *   *BMAD Resolver:* **Product Owner / Solutions Architect** during pilot deployments.
