# ADR 0006: Future Microservices Transition with Dapr Sidecars

## Status
Proposed (Backlog / Technical Debt)

## Date
2026-05-08

## Context
Currently, the UMS monorepo is successfully structured as a **Modular Monolith** with a single API (`apps/api`) and a single frontend (`apps/web`). This keeps resource consumption at $0 and minimizes infrastructure complexity during the MVP phase. However, as business requirements scale (e.g., adding high-throughput telemetry, vessel routing, or massive client portals), we need a clear roadmap on when and how to transition to distributed microservices without rewriting core business logic.

## Proposed Decision
We propose to adopt **Dapr (Distributed Application Runtime)** as our microservices sidecar runtime when splitting the monolithic API:
1. Turn the Modular Monolith into independent microservices (e.g., `@ums/billing-service`, `@ums/vessels-service`).
2. Implement **Dapr Sidecars** next to each microservice to handle Service-to-Service invocation, Pub/Sub (via Redis or RabbitMQ), and state abstraction without introducing custom infrastructure SDKs in our Clean Architecture code.
3. Keep our business domain (`core`) 100% agnostic, only adapting the infrastructure adapters (`infrastructure`) to communicate via Dapr's HTTP/gRPC local endpoints (e.g., `localhost:3500`).
4. **Dapr Sidecar Abstraction (Dependency Inversion)**: Even Dapr SDKs must be hidden behind ports. For example, if using an `IPubSubPort`, the adapter logic will translate the port call into the Dapr PubSub component call. The core domain must change exactly **0 lines of code** when Dapr is introduced.

## Consequences

### Positive (Pros)
* **High Extensibility**: Adding or replacing databases, queues, or secrets stores requires zero code changes - only updating a declarative YAML component in Dapr.
* **Polyglot Architecture**: Microservices can be written in any language (Go, Python, NestJS) while sharing the same sidecar capabilities.
* **Resiliency**: Native support for retry policies, circuit breakers, and state locks.

### Negative (Cons)
* Adds architectural overhead (requires managing Kubernetes or local Dapr sidecars).
* Increases network latency slightly due to local HTTP/gRPC loopback calls.
