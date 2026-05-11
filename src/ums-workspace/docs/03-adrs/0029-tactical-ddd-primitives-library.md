# 📜 ADR-0029 — Adoption of `@nestjslatam/ddd` for Optional Tactical DDD Primitives

**Status:** Accepted  
**Date:** 2026-05-09  
**Deciders:** Solutions Architect, Principal Software Architect, Lead Developer  
**ADR Type:** Tactical Design Standard  
**Related Specs:** [`nestjslatam-ddd-evaluation.md`](../02-architecture/nestjslatam-ddd-evaluation.md)

---

## 📋 Context

The **User Management System (UMS)** is designed as an abstract, modular identity and authorization kernel. While the adoption of Domain-Driven Design (DDD) is currently **optional** (governed by context complexity), standardizing tactical design components (Value Objects, Entities, Aggregate Roots, and Domain Events) is critical to stabilizing business rules and preventing technical drift.

Writing custom base classes for deep property equality, entity ID comparisons, and aggregate domain-event accumulators introduces significant boilerplate and maintenance overhead. To accelerate development and enforce uniform software patterns, we require a lightweight, pure TypeScript library of DDD primitives that integrates seamlessly with our NestJS architecture while respecting our non-negotiable **zero-dependency hexagonal domain rule**.

---

## ⚖️ Decision

We will adopt the **`@nestjslatam/ddd`** package as the authoritative, pre-approved standard library of tactical primitives in any UMS bounded context where Domain-Driven Design is utilized.

### 🏛️ Key Capabilities Approved:
1.  **`ValueObject`**: Used for properties with structural equality (e.g., Email, IPAddress, Geolocation coordinates) and immutable invariants.
2.  **`Entity<ID>`**: Used for domain models with durable identities (e.g., `UserIdentity`, `AuthPolicy`).
3.  **`AggregateRoot<ID>`**: Used as transactional boundaries managing state and collecting in-memory **Domain Events** for dispatching.
4.  **`DomainEvent`**: Used to record state mutations and communicate asynchronously across bounded contexts.

---

## 📐 Architecture Constraints & Guidelines

To prevent library coupling and ensure complete domain sovereignty, the adoption of `@nestjslatam/ddd` is subject to three strict constraints:

### 1. Barrel Export Abstraction (Anti-Coupling)
Developers must never import `@nestjslatam/ddd` directly inside domain logic. All primitives must be imported and re-exported via a central domain barrel file inside our Nx Monorepo (e.g., `libs/domain/src/core-primitives.ts`). Domain classes must import from this local file, making replacement or customization a zero-impact change.

### 2. Strict Immutability
All properties defined on classes extending `ValueObject` must be marked as `readonly` to prevent side effects and enforce structural immutability.

### 3. Absolute Decoupling from Database ORMs
Relational persistence decorators (such as TypeORM's `@Entity` or `@Column`) are **strictly prohibited** on classes extending `@nestjslatam/ddd` primitives. Mappings between domain entities and relational schemas must be performed exclusively in the Infrastructure Adapters layer using specialized Mappers.

---

## ✅ Consequences

### Positive
*   **Reduced Boilerplate:** Dramatically reduces code volume for deep property comparisons, ID allocations, and event accumulators.
*   **Engineering Standardization:** Standardizes core POJO models across development teams, ensuring readable and high-quality domain code.
*   **NestJS Alignment:** Designed from the ground up for NestJS environments, enabling clean integration with NestJS CQRS modules.
*   **Preserved Sovereignty:** Library abstractions are pure TypeScript and have no dependencies on databases, transport protocols, or HTTP frameworks.

### Negative
*   **Library Dependency:** Introduces an external tactical dependency, completely mitigated by the local barrel abstraction.
