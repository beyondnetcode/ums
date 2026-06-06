# Transaction Boundary Design

## Philosophy
Transactions must align with the **Aggregate Root** boundary. A single transaction should generally modify exactly one aggregate. 

## Multi-Aggregate Operations
If an operation requires modifying multiple aggregates or spanning multiple modules, we employ **Eventual Consistency** via Domain Events and the Outbox Pattern.

## Transaction Flow (The Standard Operation)
1. **Command Handler:** Application layer receives a Command.
2. **Retrieve:** Repository retrieves Aggregate.
3. **Modify:** Aggregate mutates state and queues **Domain Events** internally.
4. **Unit of Work (Commit):**
   - The UoW opens a local database transaction.
   - EF Core persists Aggregate changes.
   - **Intra-BC Communication:** Handlers for Domain Events run (synchronously or within the same transaction scope) to update other aggregates within the SAME Bounded Context.
   - **Inter-BC Communication:** If the state change must notify OTHER Bounded Contexts, an **Application/Integration Event** is generated and the Outbox Interceptor serializes it into the `OutboxMessages` table in the *same* database transaction.
   - UoW Commits.
5. **Post-Commit Dispatch:** The Message Bus (or Outbox Dispatcher) reads the Outbox and dispatches Application Events to other bounded contexts.

## Error Handling
- **Error Before Commit:** No data is persisted, no messages are sent.
- **Error During Commit:** Rollback. No data persisted.
- **Error Post Commit (Handler Failure):** The Aggregate is persisted. The Outbox message remains pending. A background worker retries the Outbox message.
- **Concurrency:** Optimistic concurrency via `RowVersion` / `xmin`. If a conflict occurs, `DbUpdateConcurrencyException` is thrown, transaction rolls back, and the client receives a 409 Conflict.
