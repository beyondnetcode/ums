# UMS — Tenant Projection (from MMS)

> UMS consumes the master Tenant from MMS (ADR-0106 / [ADR-0083](../architecture/adrs/0083-consume-mms-tenant-projection.md))
> and projects it into a local read model used as the authorization boundary.
> Canonical design: [`mms/docs/architecture/tenant-master-data-projection.md`](../../../mms/docs/architecture/tenant-master-data-projection.md).

## Consumer flow

```mermaid
flowchart LR
  X{{RabbitMQ<br/>evolith.masterdata<br/>x-consistent-hash by tenantId}}
  X --> Q[[queue: ums.tenant-projection<br/>quorum + DLX]]
  Q --> C[TenantProjectionConsumer<br/>MassTransit IConsumer]
  C --> I{Inbox: seen eventId?}
  I -- yes --> ACK[ack — dedup no-op]
  I -- no --> V{seq > stored.version?}
  V -- no --> DIS[ack — stale, discard]
  V -- yes --> UP[UPSERT identity.tenant_projection]
  UP --> AUTHZ[users / roles / permissions<br/>scope to tenant]
  Q -. poison .-> DLQ[[ums.tenant-projection.dlq]]
```

## Transport correction (critical)

```mermaid
flowchart LR
  subgraph Now["Today (broken for this flow)"]
    B1[MassTransit UsingInMemory<br/>single-process] -.->|cannot receive broker msgs| Void((✗))
    OB1[(Outbox/Inbox tables migrated<br/>but NOT wired)]
  end
  subgraph Target["Target"]
    B2[MassTransit UsingRabbitMq] --> Q2[[ums.tenant-projection]]
    OB2[AddEntityFrameworkOutbox<br/>Inbox active — dedup]
  end
  Now ==> Target
```

## Notes
- Switch `UsingInMemory` → `UsingRabbitMq`; wire `AddEntityFrameworkOutbox` (activates Inbox).
- UMS already **authors** a `Tenant` aggregate → **demote to read-only projection** + backfill
  to avoid two writers.
- Instrument the consumer with OpenTelemetry + Prometheus (lag, retries, DLQ).
