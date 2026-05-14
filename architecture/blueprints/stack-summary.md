# Polyglot Enterprise Technology Stack Cheat Sheet (Quick Reference)

This cheat sheet serves as the authoritative, high-density tool reference by architectural layer for developers and autonomous agents working on the Enterprise Reference Architecture.

---

### 1. Runtime & Language
*   **Enterprise Core:** .NET 8 LTS (C#)
*   **Satellite Services:** Node.js v20 LTS (TypeScript v5.4+)
*   **Compiler Engine:** Roslyn (.NET) / SWC (Node)
*   **Code Quality:** SonarJS / SonarLint / ESLint v8 + Prettier v3
*   **Git Quality Gates:** Husky + lint-staged / .NET Pre-commit hooks

### 2. API Layer
*   **Internal Protocols:** gRPC (gRPC-dotnet / NestJS Microservices)
*   **External Protocols:** REST API (ASP.NET Core / NestJS Express)
*   **Validation Standard:** FluentValidation / class-validator + class-transformer
*   **API Documentation:** OpenAPI v3 (Swagger) via Swashbuckle / NestJS decorators

### 3. Gateway Layer
*   **API Gateway:** Kong Gateway (Open Source Edition)
*   **Session Management:** RS256 Signed JSON Web Tokens (JWT)
*   **Internal Security:** Mutual TLS (mTLS) via Istio Service Mesh
*   **Rate Limiting:** Sliding-Window Rate Limiter (Kong Redis plugin)

### 4. Domain & Application Layer
*   **Architectural Pattern:** Hexagonal Architecture (Ports & Adapters)
*   **Monorepo Strategy:** Nx Monorepo
*   **Execution Pattern:** Modular Monolith (Dapr-Ready)
*   **Segregation Pattern:** Internal CQRS (MediatR / NestJS CQRS Module)
*   **Dependency Injection:** Native Container (.NET / NestJS)

### 5. Data Layer
*   **Primary Database (.NET):** SQL Server 2022
*   **Primary Database (Node):** SQL Server 2022 *(UMS decision: all services use SQL Server 2022 per ADR-0041)*
*   **Relational Mapping (ORM):** EF Core (.NET) / TypeORM (Node)
*   **Schema Migration Engine:** EF Migrations / TypeORM Migrations via K8s Init-Containers
*   **In-Memory Caching:** Redis v7.2 (Sentinel / Cluster)
*   **Object & Asset Store:** MinIO (S3-Compatible, Self-hosted)
*   **Asynchronous Message Broker:** RabbitMQ (AMQP v0.9.1, Self-hosted)

### 6. Multi-tenancy Strategy
*   **Data Isolation Model:** Shared Database with Row-Level Security (RLS)
*   **Implementation (.NET):** SQL Server SESSION_CONTEXT + Security Policies
*   **Implementation (Node):** SQL Server SESSION_CONTEXT + Security Policies *(aligned with .NET core; per ADR-0041)*
*   **Context Resolution:** JWT claim extraction via Middleware / Guards

### 7. Infrastructure & Deployment
*   **Container Engine:** Docker v25 (Multi-Stage Distroless images)
*   **Orchestárator Platform:** Kubernetes (K8s v1.28+)
*   **Secrets & Key Management:** HashiCorp Vault (OSS, Self-hosted)
*   **Deployment Packager:** Helm v3 parameterized charts

### 8. Observability
*   **Instrumentation Standard:** OpenTelemetry (Vendor-Neutral SDKs)
*   **Log Aggregator:** Grafana Loki (OSS)
*   **Distributed Traces:** Jaeger (OSS)
*   **Metric Server:** Prometheus Pulling Engine

### 9. Security
*   **Auth Registries:** Federated OIDC & SAML + UMS Native BCrypt Store
*   **Access Control:** Hierarchical RBAC + Attribute-Based Access Control (ABAC)
*   **Dependency Audit:** Snyk CLI + `npm audit` / `dotnet list package --vulnerable`

### 10. Developer Experience (DevEx)
*   **Local Services:** Docker Compose Spec (SQL Server, Redis, etc.)
*   **Unit Testing Framework:** xUnit (.NET) / Jestá (Node)
*   **Integration Testing:** Testácontainers (SQL Server / PostgreSQL / Redis)
*   **End-to-End (E2E) Testing:** Playwright
