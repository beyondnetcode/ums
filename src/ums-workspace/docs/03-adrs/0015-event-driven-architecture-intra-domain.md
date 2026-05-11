# ADR 0015: Event-Driven Architecture (EDA) for Intra-Domain Communication

## Status
Approved

## Date
2026-05-08

## Context
As the Modular Monolith grows, allowing domains (e.g., Inventory, Billing, Operations) to call each other synchronously leads to tight coupling. If the Billing module is slow or crashes, an Inventory operation should not fail as a consequence.

## Decision
We will adopt an asynchronous **Event-Driven Architecture (EDA)** for intra-domain communication:

1. **Internal Event Bus**: Utilize an in-memory event bus (like NestJS `EventEmitter2`) for the current Modular Monolith stage. Domains will publish Domain Events (e.g., `Tarja.Completed`) instead of directly invoking services of other domains.
2. **Independent Consumers**: Other bounded contexts will subscribe to these events and process them independently (e.g., Billing module listening for `Tarja.Completed` to generate an invoice).
3. **Future Microservices Readiness**: This event-driven pattern ensures that if we split the monolith into microservices later (ADR 0006), the internal event bus can easily be swapped for a distributed message broker (e.g., Kafka or RabbitMQ) with zero changes to domain logic.

## Consequences
* **Pros**: High decoupling, superior fault isolation, and smooth transition to microservices.
* **Cons**: Tracing execution flows becomes harder. Requires handling eventual consistency scenarios across domains.
