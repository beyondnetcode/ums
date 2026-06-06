# Outbox / Inbox Decision

## Context
Even though we are using an In-Memory Service Bus (MediatR) for Phase 1, we need resilience against process crashes or handler failures immediately after a database commit.

## Decision
**We will use the Transactional Outbox Pattern from Day 1 (Phase 1).**

### How it works:
1. When an Aggregate Root is modified, it registers Domain Events internally.
2. During `DbContext.SaveChangesAsync()`, an interceptor reads these events and serializes them into the `OutboxMessages` table in the *same* database transaction.
3. A background worker (e.g., Quartz.NET or .NET HostedService) polls the `OutboxMessages` table or is triggered by a channel, and dispatches them via MediatR.
4. Once successfully handled by all subscribers, the message is marked as Processed.

### Inbox Pattern Decision
**Inbox Pattern is deferred.** 
In Phase 1, idempotency will be handled explicitly in the Handlers by checking the database state (e.g. checking if a record with `IdempotencyKey` exists, or if the aggregate is already in the target state). We will not implement a generic Inbox table until we introduce a real distributed message broker (RabbitMQ/ASB) in Phase 2.

## Storage
- **SQL Server:** `[identity].[OutboxMessages]` (or dedicated schema).
- **PostgreSQL:** `"identity"."OutboxMessages"`.
- Clean-up jobs will archive or delete processed messages after 7 days.
