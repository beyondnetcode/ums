# ADR 0002: Clean Architecture and Hexagonal Boundaries on NestJS

## Status
Accepted

## Date
2026-05-08

## Context
Our core backend business logic for UMS must be highly decoupled from databases, HTTP frameworks, and third-party libraries to prevent technical debt, ensure easy testing, and maintain long-term stability. Clean Architecture (Hexagonal Architecture) was chosen, but without automated boundaries, developers could easily import outer layer details (e.g., infrastructure entities or controllers) into inner layer components (e.g., pure domain entities or use cases).

## Decision
We decided to strictly enforce **Clean and Hexagonal Architecture** boundaries:
1. Divide `apps/api/src` into three distinct architectural layers:
   * **`core/`**: Pure Domain (Entities, interfaces of ports/repositories, exceptions). No outer dependencies.
   * **`application/`**: Use cases, orchestrators, payload DTOs. Can only depend on `core`.
   * **`infrastructure/`**: Adapters (controllers, database entities, repositories, security hashing implementations). Can depend on `core` and `application`.
2. Configure **`eslint-plugin-boundaries`** inside `.eslintrc.js` to map these folder patterns and block invalid imports (e.g., blocking `core` from importing from `infrastructure`) with custom, human-readable compilation errors.
3. **Absolute Tool Transparency (Dependency Inversion Principle)**: Any third-party tool, library, or framework (e.g., TypeORM, Redis, Axios, Opossum, Unleash) **MUST NOT** be imported directly into the `core` or `application` layers. Instead, the `core` defines abstract Interfaces (Ports), and the `infrastructure` provides implementations (Adapters) that wrap these tools. If a tool is replaced in the future, exactly zero lines of code in the core business logic will change.

## Consequences

### Positive (Pros)
* **Architectural Integrity**: The architecture is automatically governed by the compiler/linter. Developers are prevented from violating boundaries locally (via Husky hooks) and in CI.
* **Flawless Testability**: Since the `core` and `application` layers do not depend on databases, they can be tested using simple, fast, and secure Jest mocks.
* **Framework Agnosticism**: If UMS decides to migrate from NestJS/TypeORM to another framework, the core business domain remains 100% untouched.

### Negative (Cons)
* Requires minor initial overhead to define interfaces (ports) and implement them in the infrastructure layer (adapters).
* Requires the linter to run on every commit (already optimized to under 2 seconds with `lint-staged`).
