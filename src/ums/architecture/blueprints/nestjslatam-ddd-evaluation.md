# 📐 Architectural Evaluation — Utilizing `@nestjslatam/packages` for DDD in UMS Core

**Document Type:** Architectural Evaluation & Standard  
**Status:** Approved (Conditional)  
**Date:** 2026-05-09  
**Deciders:** Solutions Architect, Principal Software Architect, Lead Developer  
**Scope:** Optional DDD Implementation in UMS Bounded Contexts  

---

## 🧭 1. Introduction & Context

The **User Management System (UMS)** is an abstract, standalone identity & access governance kernel. While implementing tactical Domain-Driven Design (DDD) patterns (such as Aggregate Roots, Entities, and Value Objects) is currently **optional** for core contexts, standardizing these patterns is critical to stabilizing complex business logic and preventing technical drift.

If the team chooses to utilize DDD patterns for any UMS bounded context (e.g., `Identity`, `Authorization`, or `Configuration`), we require a unified, lightweight, and pre-approved library of building blocks. This evaluation examines the **`@nestjslatam`** organization's DDD packages (specifically `@nestjslatam/ddd`) as the official tactical primitives standard for the UMS Core.

---

## 📊 2. Architectural Alignment Analysis

To be approved for use within the UMS Core Domain, any external library must satisfy our non-negotiable architectural guardrails:

### A. Zero-Infrastructure Dependency Constraint (Compliance: ✅ FULLY COMPLIANT)
*   **Guardrail:** The core Domain layer must have zero dependencies on external database ORMs (e.g., TypeORM), cloud provider SDKs, or web/HTTP frameworks.
*   **Evaluation:** `@nestjslatam/ddd` provides pure TypeScript abstractions for tactical DDD primitives with no external runtime dependencies. The base building blocks (`Entity`, `ValueObject`, `AggregateRoot`) run entirely in memory and are highly testable in complete isolation.

### B. Standard Tactical DDD Building Blocks (Compliance: ✅ FULLY COMPLIANT)
The library provides complete, reliable implementations of standard DDD tactical components:
1.  **`ValueObject`**: Supports structural equality based on properties (rather than memory reference equality) and immutable invariant validations.
2.  **`Entity<ID>`**: Standardizes entities with durable, unique identities that persist over time.
3.  **`AggregateRoot<ID>`**: Manages transaction boundaries, groups related entities, and incorporates internal **Domain Event** accumulation for safe transactional dispatching.
4.  **`DomainEvent`**: Lightweight event interfaces that record state changes dynamically and enable eventual consistency across bounded contexts.

---

## 📈 3. Benefits of Adoption

1.  **Boilerplate Eradication:** Eliminates the need for the team to write custom abstract classes for deep equality, entity comparison, or in-memory domain event tracking, significantly accelerating initial velocity.
2.  **Unified Engineering Standards:** Provides a pre-approved, unified blueprint for the development team, preventing ad-hoc or inconsistent implementations of DDD tactical patterns.
3.  **NestJS Ecosystem Alignment:** Designed specifically to integrate smoothly with NestJS applications and NestJS CQRS modules, while maintaining pure POJO models in the core domain layers.

---

## 🛠️ 4. Formal Approval & Mandatory Implementation Guidelines

The use of `@nestjslatam/ddd` is formally **APPROVED** for any UMS bounded context where the team chooses to adopt Domain-Driven Design, subject to the following mandatory guidelines:

### Guideline 1: Barrel Export Abstraction (Anti-Coupling)
Developers must **never** import `@nestjslatam/ddd` directly inside individual domain entity files. To prevent direct library lock-in, all approved primitives must be re-exported via a local domain abstractions file inside the Nx Monorepo:
*   **Abstractions Entrypoint:** `libs/domain/src/core-primitives.ts`
*   **Usage:** Domain files must import from `@ums/domain/core-primitives` instead of `@nestjslatam/ddd` directly, allowing a seamless replacement or local override if the library becomes deprecated.

### Guideline 2: Strict Immutability for Value Objects
All properties defined on classes extending `ValueObject` must be declared as `readonly`. Value Objects are immutable by definition and must never be mutated after instantiation.

### Guideline 3: Strict Decoupling from Database ORMs
Database-specific decorators (such as `@Entity`, `@Column`, or `@ManyToOne` from TypeORM) are **strictly prohibited** inside domain entities or classes extending `@nestjslatam/ddd` primitives. Relational schemas and persistence mapping must be handled exclusively in the Infrastructure Adapters layer using specialized Mappers.

---

## 🔗 5. Vendor Lock-in & Risk Mitigation

| Risk | Level | Mitigation Strategy |
| :--- | :--- | :--- |
| **Direct Library Lock-in** | **Medium** | Mitigated completely by **Guideline 1 (Barrel Export Abstraction)**. The domain layer depends on local exports, isolating external package changes. |
| **Performance Overhead** | **Low** | Primitives are highly optimized. Deep property comparison overhead is negligible under p95 < 5ms permission graph SLA limits. |
| **Community Maintenance** | **Medium** | The package is open-source. If necessary, it can be easily forked, customized, or maintained internally as a local utility library under `libs/domain/core`. |
