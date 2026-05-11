# 🏛️ Progressive Node.js Reference Architecture (Monolith-to-Microservices)

Welcome to the **Node.js Progressive Architecture Reference**, a blueprints for enterprise-grade solutions built to evolve. This repository serves as the canonical baseline for systems that need to start fast as a **Modular Monolith** but are architected to scale out seamlessly into **Distributed Microservices**.

The solution leverages the **BMAD Method**, enforcing strict **Clean Architecture (Hexagonal)** principles, explicit bounded contexts, and rigorous quality gates. 

> [!IMPORTANT]
> **Evolutionary Strategy**: This system adopts a **"Monolith-First"** design. By isolating domains within a highly decoupled Nx Monorepo, business domains can later be extracted as standalone Microservices (using Dapr Sidecars, gRPC, or Event-Driven mechanisms) with **zero rewrite** of core business rules.



## 🛠️ Technology Stack
- **Backend**: NestJS (v10), TypeORM, PostgreSQL 16.
- **Frontend**: React (v18), Vite, Zustand, TanStack React Query.
- **Monorepo Orchestration**: Nx & npm Workspaces.
- **Security & Quality**: Husky, ESLint (SonarJS + Boundaries), CodeQL, GitHub Actions.

## 📚 Documentation Index & Navigation Guide

This repository contains extensive technical documentation following the **bMAD Method** and industry standards (C4 Model and Markdown Architectural Decision Records - MADR). Use the following curated guides to navigate the codebase:

### 🏛️ bMAD Structure and Taxonomy (Interactive Navigation)
Click on the phases or files to navigate quickly and directly through the knowledge base:

*   🗺️ **[Main Phase - Master Index and Navigation Guides](./arc-nodejs-workspace/docs/index.md)** (Unified central index)
*   🎯 **[Phase 00 - Product Vision](./arc-nodejs-workspace/docs/00-product/)**:
    *   [Product Vision](./arc-nodejs-workspace/docs/00-product/product-vision.md) | [Business Context](./arc-nodejs-workspace/docs/00-product/business-context.md) | [Scope and Boundaries](./arc-nodejs-workspace/docs/00-product/scope.md) | [Objectives](./arc-nodejs-workspace/docs/00-product/objectives.md) | [Stakeholders](./arc-nodejs-workspace/docs/00-product/stakeholders.md)
*   📋 **[Phase 01 - Domain Requirements](./arc-nodejs-workspace/docs/01-requirements/)**:
    *   [Atomic Use Cases](./arc-nodejs-workspace/docs/01-requirements/usecases/) | [Conceptual Data Model](./arc-nodejs-workspace/docs/01-requirements/conceptual-data-model.md) | [Permission Matrix](./arc-nodejs-workspace/docs/01-requirements/permission-matrix-example.md) | [DDD Glossary](./arc-nodejs-workspace/docs/01-requirements/glossary.md)
*   🏗️ **[Phase 02 - Architectural Design](./arc-nodejs-workspace/docs/02-architecture/)**:
    *   **[Node.js Reference Architecture](./arc-nodejs-workspace/docs/02-architecture/reference-architecture-nodejs-arc42.md)** | [C4 Master Specification](./arc-nodejs-workspace/docs/02-architecture/architecture-spec.md)
*   📜 **[Phase 03 - Architectural Decision Records (ADRs)](./arc-nodejs-workspace/docs/03-adrs/)**:
    *   [23 ADRs History](./arc-nodejs-workspace/docs/03-adrs/)
*   🛠️ **[Phase 04 - Engineering Standards and Artifacts](./arc-nodejs-workspace/docs/04-artifacts/)**:
    *   [Global Standards](./arc-nodejs-workspace/docs/04-artifacts/engineering-standards.md) | [Gap Analysis](./arc-nodejs-workspace/docs/04-artifacts/gap-analysis-and-optimization-plan.md) | [QA Quality Plan](./arc-nodejs-workspace/docs/04-artifacts/contract-testing-plan.md) | [Observability](./arc-nodejs-workspace/docs/04-artifacts/observability-strategy.md) | [IAM Spec](./arc-nodejs-workspace/docs/04-artifacts/enterprise-iam-ums-specification.md) | [High-Concurrency](./arc-nodejs-workspace/docs/04-artifacts/high-concurrency-auth-specification.md) | [Console Spec](./arc-nodejs-workspace/docs/04-artifacts/ums-web-console-product-scope.md)
*   📈 **[Phase 05 - Release Roadmap](./arc-nodejs-workspace/docs/05-roadmap/)**:
    *   [Semantic Versioning](./arc-nodejs-workspace/docs/05-roadmap/versioning-and-audit-strategy.md)

---

### 📖 1. Standards & Mandates (Core)
*   👉 **[Global Engineering Standards & BMAD Manifesto](./arc-nodejs-workspace/docs/04-artifacts/engineering-standards.md)**: **MANDATORY reading**. Establishes the non-negotiable coding standards, SOLID, Clean Code, OWASP compliance, and optional DDD guidelines.

### 🏗️ 2. Architectural Design
*   👉 **[ULPMS Master Documentation Index & Guides](./arc-nodejs-workspace/docs/index.md)**: The central navigation map linking all 6 phases of product vision, requirements, architecture specifications, ADRs, engineering standards, and deployment roadmaps.
*   👉 **[Corporate Node.js Reference Architecture](./arc-nodejs-workspace/docs/02-architecture/reference-architecture-nodejs-arc42.md)**: The official corporate blueprint for API-driven systems following the international **arc42** standard. UMS acts as the canonical technical instance of this architecture.
*   👉 **[C4 Architecture Spec & Technical Inventory](./arc-nodejs-workspace/docs/02-architecture/architecture-spec.md)**: The legacy/detailed C4 mapping (Level 1, 2, and 3) along with physical technology inventories.
*   👉 **[Gap Analysis & Optimization Roadmap](./arc-nodejs-workspace/docs/04-artifacts/gap-analysis-and-optimization-plan.md)**: Analyzes the platform's architectural maturity against 16 Enterprise Quality Criteria and tracks active ADR implementations.

### 📜 3. Architectural Decision Records (ADRs)
Foundational engineering decisions grouped by architectural focus:

#### 🟢 General & Core Monorepo
*   [ADR 0001: Monorepo Orchestration with Nx and npm Workspaces](./arc-nodejs-workspace/docs/03-adrs/0001-monorepo-orchestration-nx.md)
*   [ADR 0002: Clean Architecture and Hexagonal Boundaries on NestJS](./arc-nodejs-workspace/docs/03-adrs/0002-clean-architecture-nestjs.md)
*   [ADR 0003: Strict TypeScript Standards and SonarJS Static Analysis](./arc-nodejs-workspace/docs/03-adrs/0003-strict-typescript-standards.md)
*   [ADR 0005: Zero-Cost Security and CI Pipeline with CodeQL](./arc-nodejs-workspace/docs/03-adrs/0005-ci-cd-quality-codeql.md)
*   [ADR 0009: Strict Dependency Pinning and Automated Vulnerability Management](./arc-nodejs-workspace/docs/03-adrs/0009-strict-dependency-pinning-vulnerability-management.md)

#### 🔵 Frontend & Client Integration
*   [ADR 0004: Frontend State Management and React Query Offline Architecture](./arc-nodejs-workspace/docs/03-adrs/0004-frontend-offline-resilience.md)
*   [ADR 0008: Progressive Multi-Module Evolution with API Gateway and BFF Patterns](./arc-nodejs-workspace/docs/03-adrs/0008-progressive-multimodule-evolution-gateway-bff.md)

#### 🟠 SaaS, Scalability & Resilience
*   [ADR 0006: Future Microservices Transition with Dapr Sidecars](./arc-nodejs-workspace/docs/03-adrs/0006-future-microservices-transition-dapr.md)
*   [ADR 0007: Observability Telemetry with Grafana Loki and OpenTelemetry](./arc-nodejs-workspace/docs/03-adrs/0007-observability-telemetry-loki-opentelemetry.md)
*   [ADR 0010: Multi-Tenancy Architecture Strategy for SaaS Evolution](./arc-nodejs-workspace/docs/03-adrs/0010-multi-tenancy-architecture-strategy.md)
*   [ADR 0011: Fault Tolerance & Resiliency Patterns (Circuit Breakers)](./arc-nodejs-workspace/docs/03-adrs/0011-fault-tolerance-resiliency-patterns.md)
*   [ADR 0012: Advanced Authorization (RBAC/ABAC)](./arc-nodejs-workspace/docs/03-adrs/0012-advanced-authorization-rbac-abac.md)
*   [ADR 0013: Cloud Infrastructure Topology & DR](./arc-nodejs-workspace/docs/03-adrs/0013-cloud-infrastructure-topology-dr.md)
*   [ADR 0014: Distributed Caching Strategy (Redis)](./arc-nodejs-workspace/docs/03-adrs/0014-distributed-caching-strategy-redis.md)
*   [ADR 0015: Event-Driven Architecture (EDA)](./arc-nodejs-workspace/docs/03-adrs/0015-event-driven-architecture-intra-domain.md)
*   [ADR 0016: Immutable Business Audit Trail (CDC)](./arc-nodejs-workspace/docs/03-adrs/0016-immutable-business-audit-trail.md)
*   [ADR 0017: Feature Flagging Strategy](./arc-nodejs-workspace/docs/03-adrs/0017-feature-flagging-strategy.md)
*   [ADR 0018: Testing Pyramid & Automated Quality Gates](./arc-nodejs-workspace/docs/03-adrs/0018-testing-pyramid-quality-gates.md)
*   [ADR 0019: Tactical Design Patterns for Domain Integrity (Result Pattern)](./arc-nodejs-workspace/docs/03-adrs/0019-tactical-design-patterns-future-proofing.md)

### 🏷️ 4. Versioning & Release Cycles
*   [BMAD Automated Versioning Strategy](./arc-nodejs-workspace/docs/05-roadmap/versioning-and-audit-strategy.md) - Learn how we automate Semantic Versioning and Release cycles.
*   **[View the Official CHANGELOG](./arc-nodejs-workspace/CHANGELOG.md)** - The pristine audit log of all merged features and fixes across the monorepo. fixes across the monorepo.

---

## 🚀 Quick Start
```bash
# 0. Navigate to the Monorepo workspace
cd arc-nodejs-workspace

# 1. Install dependencies and link workspaces
npm install

# 2. Start the NestJS API (Port 3000)
npx nx run api:serve

# 3. Start the React Client (Port 5173)
npx nx run apps-web:dev
```
