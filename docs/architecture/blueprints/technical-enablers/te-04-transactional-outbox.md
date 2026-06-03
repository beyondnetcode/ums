# TE-04: Transactional Outbox Pattern

| Field | Value |
|-------|-------|
| **TE ID** | TE-04 |
| **Status** | Approved |
| **ADR Reference** | [ADR-0033: Transactional Outbox](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/core/0033-transactional-outbox-pattern.md) |
| **Satisfies** | FS-06 (Auto-Assign), FS-11 (Document Upload), FS-15 (Notification Rules) |
| **Owner** | Platform Team |
| **Date** | 2026-05-15 |

---

## Problem

In a distributed system, writing to the database and publishing a domain event are two separate operations. If the application crashes after the DB write but before the event publish, downstream consumers never receive the event — producing silent data loss and broken workflows.

## Solution: Transactional Outbox

Persist domain events into an **outbox table** in the **same ACID transaction** as the aggregate mutation. A separate **relay worker** polls (or uses CDC) the outbox table and publishes events to the message bus, then marks them as processed.

```
┌────────────────────────────────────────────────────────────────────┐
│  Application Transaction                                           │
│                                                                    │
│  1. UPDATE aggregates SET ...                                      │
│  2. INSERT INTO outbox_events (id, type, payload, status, ts) ...  │
│  └── COMMIT ──────────────────────────────────────────────────────►│
└────────────────────────────────────────────────────────────────────┘
                                     │
                      ┌──────────────▼──────────────┐
                      │       Relay Worker           │
                      │  poll WHERE status='PENDING' │
                      │  publish to Message Bus      │
                      │  UPDATE status='PROCESSED'   │
                      └──────────────────────────────┘
                                     │
                      ┌──────────────▼──────────────┐
                      │     Message Bus (Dapr)       │
                      │  (at-least-once delivery)    │
                      └──────────────────────────────┘
```

---

## Outbox Schema (SQL Server)

```sql
CREATE TABLE outbox_events (
    id            UUID          NOT NULL DEFAULT gen_random_uuid(),
    aggregate_id  UUID          NOT NULL,
    aggregate_type VARCHAR(128) NOT NULL,
    event_type    VARCHAR(256)  NOT NULL,   -- e.g. "ums.identity.user.registered"
    payload       JSONB         NOT NULL,
    status        VARCHAR(16)   NOT NULL DEFAULT 'PENDING',  -- PENDING | PROCESSED | FAILED
    created_at    TIMESTAMPTZ   NOT NULL DEFAULT now(),
    processed_at  TIMESTAMPTZ,
    retry_count   SMALLINT      NOT NULL DEFAULT 0,
    CONSTRAINT pk_outbox PRIMARY KEY (id),
    CONSTRAINT ck_outbox_status CHECK (status IN ('PENDING','PROCESSED','FAILED'))
);

CREATE INDEX idx_outbox_status_created ON outbox_events (status, created_at)
    WHERE status = 'PENDING';
```

---

## NestJS Implementation — Domain Side

```typescript
// domain/events/user-registered.event.ts
export class UserRegisteredEvent {
  constructor(
    public readonly userId: string,
    public readonly tenantId: string,
    public readonly email: string,
    public readonly occurredAt: Date,
  ) {}
}

// domain/aggregates/user.aggregate.ts
export class User extends AggregateRoot {
  private _domainEvents: unknown[] = [];

  static register(id: string, tenantId: string, email: string): User {
    const user = new User(id, tenantId, email);
    user._domainEvents.push(new UserRegisteredEvent(id, tenantId, email, new Date()));
    return user;
  }

  pullEvents(): unknown[] {
    const events = [...this._domainEvents];
    this._domainEvents = [];
    return events;
  }
}
```

```typescript
// infrastructure/repositories/user.repository.ts
@Injectable()
export class UserRepository implements IUserRepository {
  constructor(
    @InjectDataSource() private readonly ds: DataSource,
  ) {}

  async save(user: User): Promise<void> {
    await this.ds.transaction(async (em) => {
      await em.save(UserEntity, toEntity(user));

      const events = user.pullEvents();
      for (const event of events) {
        await em.save(OutboxEventEntity, {
          aggregateId: user.id,
          aggregateType: 'User',
          eventType: event.constructor.name,
          payload: JSON.stringify(event),
          status: 'PENDING',
        });
      }
    });
  }
}
```

---

## Relay Worker

```typescript
// infrastructure/relay/outbox-relay.worker.ts
@Injectable()
export class OutboxRelayWorker implements OnApplicationBootstrap {
  private readonly logger = new Logger(OutboxRelayWorker.name);

  constructor(
    @InjectRepository(OutboxEventEntity)
    private readonly outboxRepo: Repository<OutboxEventEntity>,
    private readonly daprClient: DaprClient,
  ) {}

  onApplicationBootstrap() {
    setInterval(() => this.relay(), 2_000);
  }

  private async relay() {
    const pending = await this.outboxRepo.find({
      where: { status: 'PENDING' },
      order: { createdAt: 'ASC' },
      take: 50,
    });

    for (const event of pending) {
      try {
        await this.daprClient.pubsub.publish(
          'ums-pubsub',
          event.eventType,
          JSON.parse(event.payload),
        );
        await this.outboxRepo.update(event.id, {
          status: 'PROCESSED',
          processedAt: new Date(),
        });
      } catch (err) {
        this.logger.warn(`Relay failed for ${event.id}: ${err.message}`);
        await this.outboxRepo.increment({ id: event.id }, 'retryCount', 1);
        if (event.retryCount >= 5) {
          await this.outboxRepo.update(event.id, { status: 'FAILED' });
        }
      }
    }
  }
}
```

---

## Guarantees and Trade-offs

| Property | Value |
|----------|-------|
| Delivery semantics | At-least-once |
| Ordering | Per aggregate (ordered by `created_at`) |
| Idempotency requirement | Consumers **must** be idempotent |
| Latency overhead | ~2 s polling interval (configurable) |
| CDC alternative | Debezium / SQL Server CDC for sub-second relay |
| DLQ | Events with `retry_count >= 5` move to `status='FAILED'`; separate alert monitors this |

---

## Acceptance Criteria

- [ ] Domain event persisted in same transaction as aggregate mutation
- [ ] Relay worker publishes within configurable polling interval (default 2 s)
- [ ] Failed events (≥5 retries) flagged as `FAILED` and trigger alert
- [ ] Consumers handle duplicate delivery without side effects
- [ ] Outbox table has index on `(status, created_at)` for relay efficiency
- [ ] Integration test verifies event appears in consumer after aggregate save

---

**[Back to Blueprints Index](../index.md)** | **[Back to Traceability Matrix](../../traceability-matrix.md)**
