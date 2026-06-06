# In-Memory Service Bus Transactionality

## Context
During Phase 1, an **In-Memory Service Bus** (MediatR) is utilized as the default transport for Application Events. However, the architecture strictly abstracts the bus, allowing an immediate switch to **Redis**, RabbitMQ, or Azure Service Bus without touching domain or application logic.

## Rules
1. **Domain Events (Intra-BC):** Used strictly for communication *within* the same Bounded Context. Handled locally to guarantee strong consistency inside the aggregate or bounded context boundaries.
2. **Application / Integration Events (Inter-BC):** Used strictly for communication *across* Bounded Contexts. These are saved to the Outbox and published to the external bus.
3. **No Implicit Distributed Transactions:** The Service Bus does NOT wrap multiple module handlers into a single database transaction. 
4. **Handlers are Independent:** Each handler for an Application Event runs in its own Scope and uses its own `DbContext` instance, creating its own local database transaction.
5. **Delivery Guarantee:** To guarantee "At-Least-Once" delivery across modules, the system must utilize the Transactional Outbox pattern. 
6. **Idempotency:** Because messages can be retried if a transient failure occurs during dispatch, ALL integration event handlers must be idempotent.

## Message Types
- **Commands:** Synchronous execution. One handler. Return `Result<T>`.
- **Queries:** Synchronous execution. One handler. Read-only.
- **Domain Events:** Intra-BC logic. Not saved to Outbox.
- **Application / Integration Events:** Inter-BC facts. Triggered via Outbox background worker.

## Separation of Concerns & Adaptability
Handlers must not depend on a specific broker tool directly. The Application layer interfaces with an abstract `IEventBus`. In Phase 1, the implementation uses MediatR in-memory, but it is 100% adaptable to use **Redis Pub/Sub or Redis Streams** by simply swapping the infrastructure adapter.
