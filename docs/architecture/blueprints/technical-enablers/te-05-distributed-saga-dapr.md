# TE-05: Distributed Saga with Dapr

| Field | Value |
|-------|-------|
| **TE ID** | TE-05 |
| **Status** | Approved |
| **ADR Reference** | [ADR-0035: Distributed Sagas](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/core/0035-distributed-saga-pattern-strategy.md) |
| **Satisfies** | FS-10 (External B2B Access Request/Approval), FS-12 (Role Promotion Process) |
| **Owner** | Platform Team |
| **Date** | 2026-05-15 |

---

## Problem

Long-running business processes — such as B2B access approval or role promotion — span multiple bounded contexts and may take minutes or hours. A single distributed transaction is impractical. Instead we need a coordinated sequence of steps where each step can be compensated if a subsequent step fails.

## Solution: Choreography-Based Saga via Dapr Pub/Sub

Each bounded context publishes events and reacts to events from other contexts. No central orchestrator — each participant owns its own step and its compensation logic.

```
FS-10: External B2B Access Request Saga
──────────────────────────────────────

[Requester]                [Approval Context]          [Identity Context]
    │                            │                            │
    │── AccessRequested ────────►│                            │
    │                            │── ApprovalInitiated ──────►│
    │                            │                            │── AccountProvisioned ──►
    │                            │◄── AccountProvisioned ─────│
    │◄── AccessGranted ──────────│                            │
    
    On failure at any step → compensation events published backwards
```

---

## Saga State Machine (FS-12: Role Promotion)

```
PENDING ──► ACCESS_REQUESTED ──► APPROVAL_INITIATED ──► APPROVED ──► PROVISIONED
    │               │                     │                │
    │               └── REJECTED ◄────────┘                │
    │                                                       │
    └── CANCELLED ◄─────────────────────────────────────────┘ (compensation)
```

---

## Implementation Pattern (NestJS + Dapr)

### Saga State Entity

```typescript
// domain/sagas/role-promotion.saga.ts
export type SagaStatus =
  | 'PENDING'
  | 'APPROVAL_REQUESTED'
  | 'APPROVED'
  | 'REJECTED'
  | 'PROVISIONED'
  | 'COMPENSATING'
  | 'CANCELLED';

export class RolePromotionSaga {
  constructor(
    public readonly id: string,
    public readonly userId: string,
    public readonly requestedRole: string,
    public status: SagaStatus,
    public readonly createdAt: Date,
    public updatedAt: Date,
  ) {}

  transitionTo(next: SagaStatus): Result<RolePromotionSaga> {
    const allowed: Partial<Record<SagaStatus, SagaStatus[]>> = {
      PENDING: ['APPROVAL_REQUESTED'],
      APPROVAL_REQUESTED: ['APPROVED', 'REJECTED'],
      APPROVED: ['PROVISIONED', 'COMPENSATING'],
      COMPENSATING: ['CANCELLED'],
    };
    if (!allowed[this.status]?.includes(next)) {
      return Result.fail(`Invalid transition ${this.status} → ${next}`);
    }
    return Result.ok(
      new RolePromotionSaga(this.id, this.userId, this.requestedRole, next, this.createdAt, new Date()),
    );
  }
}
```

### Event Handler (Dapr Subscriber)

```typescript
// infrastructure/subscribers/role-promotion.subscriber.ts
@Controller()
export class RolePromotionSubscriber {
  constructor(
    private readonly promotionService: RolePromotionService,
    private readonly daprClient: DaprClient,
  ) {}

  @DaprSubscribe({ pubsubName: 'ums-pubsub', topic: 'ums.iga.promotion-request.approved' })
  async onApprovalGranted(@Body() event: ApprovalGrantedEvent): Promise<void> {
    const result = await this.promotionService.provisionRole(event.sagaId);
    if (result.isFailure) {
      // publish compensation
      await this.daprClient.pubsub.publish('ums-pubsub', 'ums.iga.promotion-request.compensation-requested', {
        sagaId: event.sagaId,
        reason: result.error,
      });
    }
  }

  @DaprSubscribe({ pubsubName: 'ums-pubsub', topic: 'ums.iga.promotion-request.rejected' })
  async onApprovalRejected(@Body() event: ApprovalRejectedEvent): Promise<void> {
    await this.promotionService.cancelPromotion(event.sagaId, event.reason);
  }
}
```

---

## Compensation Map

| Step | Forward Event | Compensation Event | Compensation Handler |
|------|--------------|-------------------|---------------------|
| Request submitted | `ums.iga.promotion-request.submitted` | `ums.iga.promotion-request.cancelled` | Delete saga record |
| Approval initiated | `ums.iga.promotion-request.approval-initiated` | `ums.iga.promotion-request.compensation-requested` | Notify requester |
| Role provisioned | `ums.iga.promotion-request.executed` | `ums.iga.promotion-request.revoked` | Remove role grant |

---

## Dapr Component Config

```yaml
# deploy/dapr/components/pubsub.yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: ums-pubsub
spec:
  type: pubsub.redis
  version: v1
  metadata:
    - name: redisHost
      value: redis:6379
    - name: enableTLS
      value: "false"
```

---

## Idempotency

Every saga step checks whether the transition has already been applied (optimistic lock on `updated_at`). If a duplicate event arrives, the handler returns `200 OK` without re-applying — Dapr's at-least-once delivery is safe.

---

## Acceptance Criteria

- [ ] Happy path: saga reaches `PROVISIONED` status end-to-end
- [ ] Rejection path: saga reaches `REJECTED`, requester is notified
- [ ] Compensation path: failure after `APPROVED` triggers `COMPENSATING` → `CANCELLED`
- [ ] Duplicate event: idempotent — no state corruption on redelivery
- [ ] Saga state persisted; queryable via `/sagas/:id`
- [ ] Dapr pub/sub component deployed and verified in dev environment

---

**[Back to Blueprints Index](../index.md)** | **[Back to Traceability Matrix](../../traceability-matrix.md)**
