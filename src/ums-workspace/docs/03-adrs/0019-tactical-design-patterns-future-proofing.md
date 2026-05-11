# ADR 0019: Tactical Design Patterns for Domain Integrity & Future-Proofing

## Status
Approved

## Date
2026-05-09

## Context
While our macro-architecture is highly resilient (Hexagonal, Modular Monolith), the *micro-architecture* (tactical design patterns within the code) must also be protected. If the core domain throws HTTP-specific exceptions or relies heavily on `null` values, migrating from a REST Monolith to a distributed ecosystem (like gRPC or any microservice runtime) in the future will require massive refactoring, violating our goal of "zero-impact evolution."

## Decision
We mandate the adoption of specific GoF (Gang of Four) and Functional design patterns at the tactical code level to ensure the codebase remains completely agnostic to external frameworks:

1. **The Result Pattern (Error Handling)**: 
   * **Rule**: The Core Domain and Application Use Cases will **never** throw HTTP exceptions (e.g., `NotFoundException`).
   * **Implementation**: Methods will return a functional `Result<Value, Error>` (or `Either` monad). 
   * **Benefit**: When we move to gRPC or message brokers in the future, the adapter simply reads the `Result.isFailure()` and translates it to the appropriate protocol error, rather than trying to catch a NestJS HTTP exception that doesn't make sense outside of REST.

2. **Null Object Pattern (Behavioral)**:
   * **Rule**: Avoid returning `null` or `undefined` from domain queries.
   * **Implementation**: Return a safe "Null" representation of an entity (e.g., a `GuestTenant` instead of a `null` Tenant).
   * **Benefit**: Eliminates `NullReferenceExceptions` (the billion-dollar mistake) that crash systems unpredictably in production.

3. **Decorator Pattern (Structural)**:
   * **Rule**: Cross-cutting concerns like Observability (ADR 0007), caching, or Pub/Sub bindings must not pollute business logic.
   * **Implementation**: Use TypeScript decorators (e.g., `@Trace()`, `@Audit()`) to wrap Use Cases or Controllers. The business logic remains pure, while the decorator injects the necessary cross-cutting logic at runtime.

## Consequences
* **Pros**: Absolute decoupling. The code is predictable, highly testable, and ready to be physically split into distributed microservices over gRPC without rewriting the domain logic.
* **Cons**: The `Result` pattern introduces slight verbosity, forcing developers to explicitly handle successful and failed states (`result.isSuccess()`) instead of relying on generic `try/catch` blocks.
