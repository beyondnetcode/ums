# TE-06: CQRS Projection Rebuild

| Field | Value |
|-------|-------|
| **TE ID** | TE-06 |
| **Status** | Approved |
| **ADR Reference** | [ADR-0034: CQRS Applicability](../../../../../reference/architecture/adrs/core/0034-cqrs-pattern-applicability-matrix.md) |
| **Satisfies** | FS-04 (Register System Topology), FS-07 (Visual Graph Resolver), FS-13 (Hierarchical Configuration) |
| **Owner** | Platform Team |
| **Date** | 2026-05-15 |

---

## Problem

Read models for complex views — such as the authorization graph (FS-07) or hierarchical configuration (FS-13) — require aggregated data from multiple bounded contexts. Computing these on-the-fly from the write store is expensive and couples the query model to the domain model.

## Solution: Shadow Read Store with Atomic Swap

Maintain a **separate read store** (projection) optimized for queries. When the write side changes, events trigger an incremental update to the projection. For full rebuilds (schema migration, recovery), build a **shadow projection** and atomically swap the pointer.

```
Write Side                         Read Side
──────────                         ─────────
Command                            
  │                                ┌──────────────────────────┐
  ▼                                │  Projection Store        │
Domain Event ──► Event Store ──────► (Redis / PostgreSQL view)│
                                   │                          │
                     ▲             │  auth_graph_v2 (shadow)  │
                     │             │         │                 │
              Projection           │  SWAP ──►auth_graph (live)│
              Rebuilder            │                          │
                                   └──────────────────────────┘
```

---

## Projection Architecture

### Read Model Interface

```typescript
// application/queries/auth-graph.query.ts
export interface AuthGraphProjection {
  userId: string;
  tenantId: string;
  roles: string[];
  permissions: string[];
  effectiveAt: Date;
}

export abstract class IAuthGraphRepository {
  abstract findByUser(userId: string, tenantId: string): Promise<AuthGraphProjection | null>;
}
```

### Projection Handler

```typescript
// infrastructure/projections/auth-graph.projector.ts
@Injectable()
export class AuthGraphProjector {
  constructor(
    @InjectRepository(AuthGraphReadEntity)
    private readonly readRepo: Repository<AuthGraphReadEntity>,
  ) {}

  @OnEvent('ums.authorization.role.assigned')
  async onRoleAssigned(event: RoleAssignedEvent): Promise<void> {
    await this.readRepo.upsert(
      {
        userId: event.userId,
        tenantId: event.tenantId,
        roles: () => `array_append(roles, '${event.roleId}')`,
        effectiveAt: new Date(),
      },
      ['userId', 'tenantId'],
    );
  }

  @OnEvent('ums.authorization.role.revoked')
  async onRoleRevoked(event: RoleRevokedEvent): Promise<void> {
    await this.readRepo.upsert(
      {
        userId: event.userId,
        tenantId: event.tenantId,
        roles: () => `array_remove(roles, '${event.roleId}')`,
        effectiveAt: new Date(),
      },
      ['userId', 'tenantId'],
    );
  }
}
```

---

## Full Rebuild Process (Shadow Swap)

```typescript
// infrastructure/projections/auth-graph-rebuild.service.ts
@Injectable()
export class AuthGraphRebuildService {
  private readonly logger = new Logger(AuthGraphRebuildService.name);

  constructor(
    private readonly eventStore: EventStoreService,
    private readonly ds: DataSource,
  ) {}

  async rebuild(): Promise<void> {
    const shadowTable = `auth_graph_shadow_${Date.now()}`;
    this.logger.log(`Starting projection rebuild into ${shadowTable}`);

    await this.ds.query(`CREATE TABLE ${shadowTable} (LIKE auth_graph INCLUDING ALL)`);

    try {
      const events = await this.eventStore.getAllOrderedEvents(['role.assigned', 'role.revoked']);

      for (const event of events) {
        await this.applyToShadow(shadowTable, event);
      }

      await this.ds.transaction(async (em) => {
        await em.query(`ALTER TABLE auth_graph RENAME TO auth_graph_old`);
        await em.query(`ALTER TABLE ${shadowTable} RENAME TO auth_graph`);
        await em.query(`DROP TABLE auth_graph_old`);
      });

      this.logger.log('Projection rebuild complete — shadow swapped');
    } catch (err) {
      await this.ds.query(`DROP TABLE IF EXISTS ${shadowTable}`);
      throw err;
    }
  }

  private async applyToShadow(table: string, event: StoredEvent): Promise<void> {
    // apply event to shadow table using same projector logic
  }
}
```

---

## Redis Cache Layer (for hot projections)

```typescript
// infrastructure/projections/cached-auth-graph.repository.ts
@Injectable()
export class CachedAuthGraphRepository implements IAuthGraphRepository {
  constructor(
    @Inject(REDIS_CLIENT) private readonly redis: Redis,
    private readonly dbRepo: AuthGraphDbRepository,
  ) {}

  async findByUser(userId: string, tenantId: string): Promise<AuthGraphProjection | null> {
    const key = `auth:graph:${tenantId}:${userId}`;
    const cached = await this.redis.get(key);
    if (cached) return JSON.parse(cached);

    const projection = await this.dbRepo.findByUser(userId, tenantId);
    if (projection) {
      await this.redis.set(key, JSON.stringify(projection), 'EX', 300);
    }
    return projection;
  }

  async invalidate(userId: string, tenantId: string): Promise<void> {
    await this.redis.del(`auth:graph:${tenantId}:${userId}`);
  }
}
```

---

## Rebuild Trigger Policy

| Trigger | Action |
|---------|--------|
| Schema migration | Manual rebuild via admin API `POST /admin/projections/auth-graph/rebuild` |
| Event replay recovery | Automated rebuild on startup if projection checksum mismatch |
| Performance degradation | Incremental re-project last N events via DLQ replay |

---

## Acceptance Criteria

- [ ] Auth graph projection updates within 500 ms of role assignment event
- [ ] Full rebuild completes without downtime (shadow swap)
- [ ] Cache invalidated on role change; stale reads not returned after 300 s TTL
- [ ] Rebuild endpoint protected by admin role guard
- [ ] Event replay order guaranteed (monotonic sequence or `created_at` ordering)
- [ ] Unit test: projector applies events in correct order and produces valid read model

---

**[Back to Blueprints Index](../index.md)** | **[Back to Traceability Matrix](../../traceability-matrix.md)**
