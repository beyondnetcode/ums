---
adr: 0083
title: Consume MMS Tenant Projection (UMS as downstream consumer)
status: Proposed
date: 2026-07-09
tags: [EvolithSatellite, event-driven, tenant, projection]
supersedes: none
relates: [ADR-0051 event-bus injectable port, ADR-0050 naming/CloudEvents, ADR-0063 idempotency]
authority: Evolith Core ADR-0106 (master tenant projections)
canonical: mms/docs/architecture/tenant-master-data-projection.md
---

# ADR-0083 — Consume the MMS Tenant Projection

## Status
Proposed.

## Context
Per Core **ADR-0106**, **MMS is the sole authority** for the master Tenant. UMS must
**consume** MMS tenant events and project them into a local read model used as the
authorization boundary (users/roles/permissions scope to the projected tenant). The
canonical design is [`mms/docs/architecture/tenant-master-data-projection.md`](../../../../mms/docs/architecture/tenant-master-data-projection.md).

**Current UMS reality (critical):**
- The MassTransit transport is **`UsingInMemory`** (`Ums.Infrastructure/DependencyInjection.cs`) —
  a single-process bus that **cannot receive messages from an external broker**. ADR-0051
  claims the RabbitMQ implementation record; the code contradicts it (doc/code drift).
- MassTransit `OutboxMessage/OutboxState/InboxState` tables were migrated
  (`20260606233924_...MassTransitOutbox`) but **`AddEntityFrameworkOutbox` is never called** —
  they are orphaned; there is **no active message-level idempotency**.
- The only consumers are user-revocation handlers; there is **no tenant projection consumer**.
- UMS already **authors** a `Tenant` aggregate (`Ums.Domain/Identity/Tenant/Tenant.cs` + create
  commands + repositories) — making MMS the master creates a **two-writer conflict**.

## Decision
UMS becomes a **downstream projection consumer** of MMS tenant events:

1. **Switch transport** `UsingInMemory` → `UsingRabbitMq` (bind queue `ums.tenant-projection`,
   quorum, DLX to `evolith.masterdata.dlx`).
2. **Wire `AddEntityFrameworkOutbox<UmsPlatformDbContext>`** to activate the Inbox
   (idempotency/dedup by `eventId`, per ADR-0063).
3. Add `IConsumer<TenantEvent>` that performs a **versioned upsert** into a dedicated
   `identity.tenant_projection` read-model table (apply only if `event.sequence > row.version`;
   dedup by `eventId`; soft-delete on `Deactivated/Deleted`).
4. **Demote the local `Tenant` aggregate** to read-only (or replace with the projection) and
   **backfill** existing rows from MMS — UMS stops authoring tenants.
5. **Instrument** the consumer with OpenTelemetry traces + Prometheus metrics (lag, retries, DLQ).

## Consequences
- **+** Single source of truth; UMS authorization scopes to a consistent tenant identity.
- **+** Resilient (durable queue, DLQ, replay from MMS event-store), independent from Tracker.
- **−** Requires the transport switch, outbox/inbox wiring, a new consumer, and a **tenant
  ownership migration** (the largest cost — must backfill and stop local authoring).
- **Risk:** until the transport is RabbitMQ and the ownership is migrated, the flow is inert.

## Validation
Follow the mandatory suite in
[`tenant-projection-test-matrix.md`](../../../../mms/docs/architecture/tenant-projection-test-matrix.md)
(F1–F7, R1–R5, S1–S3), asserting UMS==MMS after each scenario.
