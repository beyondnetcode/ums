# 🗺️ Global Master Index (UMS Entry Point)

> 🌍 **Bilingual Navigation:** [🇪🇸 Versión en Español (Índice Maestro)](./MASTER_INDEX.es.md) | [🇺🇸 English](./MASTER_INDEX.md)

Welcome to the **User Management System (UMS)** central nervous system. This master index serves as the canonical routing gate for all actors interacting with this repository. Locate your designated profile below to access your tailored, accelerated reading path ensuring technical and procedural compliance.

---

## 🚀 1. Accelerated Paths by Role (Role-Based Navigation)

Identify your current relationship with the project to unlock the tailored compulsory reading hierarchy:

| Enterprise Role | Recommended Reading Path | Compliance Expected |
| :--- | :--- | :--- |
| **External Software Vendor** | 1. [bMAD Master Documentation](./src/ums-workspace/docs/en/index.md)<br>2. [Global Engineering Standards](./src/ums-workspace/docs/en/04-artifacts/engineering-standards.md)<br>3. [C4 Master Specification](./src/ums-workspace/docs/en/02-architecture/architecture-spec.md) | Validate tech baseline, environment isolation, and boundary checks before initiating work. |
| **Backend Developer / QA** | 1. [Plan for .NET Migration & Hexagonal Boundaries](./src/ums-workspace/docs/en/02-architecture/dotnet-migration-and-tech-stack-plan.md)<br>2. [Contract Testing Plan](./src/ums-workspace/docs/en/04-artifacts/contract-testing-plan.md)<br>3. [Testing Pyramid Quality Gates (ADR-0018)](./src/ums-workspace/docs/en/03-adrs/0018-testing-pyramid-quality-gates.md) | Enforce Test thresholds, DDD patterns, lint validations, and avoid leakages into the core domain. |
| **Solutions Architect** | 1. [Bounded Context Map](./src/ums-workspace/docs/en/02-architecture/bounded-context-map.md)<br>2. [ADR Registry](./src/ums-workspace/docs/en/03-adrs/)<br>3. [Distributed Observability Strategy](./src/ums-workspace/docs/en/04-artifacts/observability-strategy.md) | Assess overall pattern integrity, evaluate risk indices, and approve microservice readiness gates. |
| **Product Owner / PM** | 1. [Product Vision](./src/ums-workspace/docs/en/00-product/product-vision.md)<br>2. [Atomic Use Cases Directory](./src/ums-workspace/docs/en/01-requirements/usecases/)<br>3. [Gap Analysis & Debt Strategy](./src/ums-workspace/docs/en/04-artifacts/gap-analysis-and-optimization-plan.md) | Align release milestones with ADR maturity and govern use case coverage definitions. |

---

## 🛡️ 2. Mandatory Compliance Path (Global Baseline)

All contributors—regardless of seniority—MUST adhere to and enforce the foundational anchors below:

*   📄 **[Global Engineering Standards](./src/ums-workspace/docs/en/04-artifacts/engineering-standards.md)**: Non-negotiable coding standards, SOLID, Clean Code, and OWASP.
*   📄 **[Architecture Maturity Model (AMM)](./src/ums-workspace/docs/en/04-artifacts/architecture-maturity-model.md)**: Comprehensive criteria for corporate enterprise readiness.
*   📄 **[.NET 8 Core Migration Plan](./src/ums-workspace/docs/en/02-architecture/dotnet-migration-and-tech-stack-plan.md)**: Mandatory decoupling criteria for domain models under C#.
*   📄 **[Contract Testing Strategy](./src/ums-workspace/docs/en/04-artifacts/contract-testing-plan.md)**: Quality gates for decoupled integration validations.

---

## 🏛️ 3. bMAD Structure and Taxonomy (Phase-Based Navigation)

This repository's documentation follows the **bMAD Method** (numerical sequential phases). Click the references to navigate through the knowledge base:

### 🎯 **[Phase 00 - Product Vision](./src/ums-workspace/docs/en/00-product/)**
*   [Product Vision](./src/ums-workspace/docs/en/00-product/product-vision.md) | [Business Context](./src/ums-workspace/docs/en/00-product/business-context.md) | [Scope and Boundaries](./src/ums-workspace/docs/en/00-product/scope.md) | [Objectives (OKRs)](./src/ums-workspace/docs/en/00-product/objectives.md) | [Stakeholders](./src/ums-workspace/docs/en/00-product/stakeholders.md)

### 📋 **[Phase 01 - Domain Requirements](./src/ums-workspace/docs/en/01-requirements/)**
*   [Atomic Use Cases Catalog](./src/ums-workspace/docs/en/01-requirements/usecases/) | [Conceptual Data Model](./src/ums-workspace/docs/en/01-requirements/conceptual-data-model.md) | [Permission Matrix](./src/ums-workspace/docs/en/01-requirements/permission-matrix-example.md) | [DDD Glossary](./src/ums-workspace/docs/en/01-requirements/glossary.md)

### 🏗️ **[Phase 02 - Architectural Design](./src/ums-workspace/docs/en/02-architecture/)**
*   **[.NET 8 Migration Plan](./src/ums-workspace/docs/en/02-architecture/dotnet-migration-and-tech-stack-plan.md)** | [C4 Master Specification](./src/ums-workspace/docs/en/02-architecture/architecture-spec.md) | [Bounded Context Map](./src/ums-workspace/docs/en/02-architecture/bounded-context-map.md) | [Vendor Lock-In Risk Assessment](./src/ums-workspace/docs/en/02-architecture/vendor-risk-assessment.md)

### 📜 **[Phase 03 - Architectural Decision Records (ADRs)](./src/ums-workspace/docs/en/03-adrs/)**
*   [Nx Monorepo Strategy](./src/ums-workspace/docs/en/03-adrs/0001-monorepo-orchestration-nx.md) | [Clean Arch Boundaries](./src/ums-workspace/docs/en/03-adrs/0002-clean-architecture-nestjs.md) | [Offline Resiliency](./src/ums-workspace/docs/en/03-adrs/0004-frontend-offline-resilience.md) | [Zero-Cost CodeQL](./src/ums-workspace/docs/en/03-adrs/0005-ci-cd-quality-codeql.md) | [BFF Gateway](./src/ums-workspace/docs/en/03-adrs/0008-progressive-multimodule-evolution-gateway-bff.md) | [View All 29 ADRs History](./src/ums-workspace/docs/en/03-adrs/)

### 🛠️ **[Phase 04 - Engineering Standards and Artifacts](./src/ums-workspace/docs/en/04-artifacts/)**
*   [Architecture Maturity](./src/ums-workspace/docs/en/04-artifacts/architecture-maturity-model.md) | [Observability Strategy](./src/ums-workspace/docs/en/04-artifacts/observability-strategy.md) | [Enterprise IAM Spec](./src/ums-workspace/docs/en/04-artifacts/enterprise-iam-ums-specification.md) | [High-Concurrency Auth Spec](./src/ums-workspace/docs/en/04-artifacts/high-concurrency-auth-specification.md) | [MFA Spec](./src/ums-workspace/docs/en/04-artifacts/mfa-passwordless-security-spec.md)

### 📈 **[Phase 05 - Release Roadmap](./src/ums-workspace/docs/en/05-roadmap/)**
*   [Versioning & Audit Strategy](./src/ums-workspace/docs/en/05-roadmap/versioning-and-audit-strategy.md) | [Monorepo CHANGELOG](./src/ums-workspace/CHANGELOG.md)

---

👉 **[Back to Main README](./README.md)**

