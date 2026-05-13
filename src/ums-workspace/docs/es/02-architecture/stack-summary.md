> ?? **Nota de Arquitectura:** Este documento se encuentra actualmente en su versión original (Inglés) y está programado para traducción oficial en la hoja de ruta.

# âš¡ Progressive Node.js Technology Stack Cheat Sheet (Quick Reference)

This cheat sheet serves as the authoritative, high-density tool reference by architectural layer for developers and autonomous agents working on the Progressive Node.js Reference Architecture.

---

### 1. Runtime & Language
*   **Runtime Environment:** Node.js v20 LTS
*   **Language:** TypeScript v5.4+ (Strict Mode)
*   **Compiler Engine:** SWC (`@swc/core`) inside Nx Monorepo
*   **Code Quality:** ESLint v8 + Prettier v3
*   **Git Quality Gates:** Husky + lint-staged

### 2. API Layer
*   **Internal Protocols:** gRPC (NestJS Microservices)
*   **External Protocols:** REST API (NestJS Express)
*   **Validation Standard:** `class-validator` + `class-transformer`
*   **API Documentation:** OpenAPI v3 (Swagger) via NestJS decorators

### 3. Gateway Layer
*   **API Gateway:** Kong Gateway (Open Source Edition)
*   **Session Management:** RS256 Signed JSON Web Tokens (JWT)
*   **Internal Security:** Mutual TLS (mTLS) via Istio Service Mesh
*   **Rate Limiting:** Sliding-Window Rate Limiter (Kong Redis plugin)

### 4. Domain & Application Layer
*   **Architectural Pattern:** Hexagonal Architecture (Ports & Adapters)
*   **Monorepo Strategy:** Nx Monorepo
*   **Execution Pattern:** Modular Monolith (Dapr-Ready)
*   **Segregation Pattern:** Internal CQRS (NestJS CQRS Module)
*   **Dependency Injection:** Native NestJS DI Container

### 5. Data Layer
*   **Primary Relational Database:** PostgreSQL v16
*   **Relational Mapping (ORM):** TypeORM (TypeScript)
*   **High-Performance Queries:** Native `pg` driver
*   **Schema Migration Engine:** TypeORM Migrations via Kubernetes Init-Containers
*   **In-Memory Caching:** Redis v7.2 (Sentinel / Cluster Replications)
*   **Object & Asset Store:** MinIO (S3-Compatible, Self-hosted)
*   **Asynchronous Message Broker:** RabbitMQ (AMQP v0.9.1, Self-hosted)

### 6. Multi-tenancy Strategy
*   **Data Isolation Model:** Shared Database with Row-Level Security (RLS)
*   **Tenant Resolution Context:** JWT claim extraction via NestJS Guards
*   **Isolation Enforcement:** Dynamic database transaction session injection (`SET LOCAL app.current_tenant`)

### 7. Infrastructure & Deployment
*   **Container Engine:** Docker v25 (Multi-Stage Distroless node images)
*   **Orchestrator Platform:** Kubernetes (K8s v1.28+)
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
*   **Dependency Audit:** Snyk CLI + `npm audit` inside CI/CD pipelines

### 10. Developer Experience (DevEx)
*   **Local Services:** Docker Compose Spec
*   **Unit Testing Framework:** Jest
*   **Integration Testing:** Jest + Supertest with **Testcontainers**
*   **End-to-End (E2E) Testing:** Playwright

