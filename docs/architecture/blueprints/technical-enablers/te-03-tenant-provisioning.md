# TE-03: Tenant Provisioning Pipeline

| Field | Value |
|-------|-------|
| **TE ID** | TE-03 |
| **Status** | Approved |
| **ADR Reference** | ADR-0010 (Multi-Tenancy RLS), ADR-0048 (Closure Table Hierarchy) |
| **Satisfies** | FS-03 (Register Organization), FS-14 (Delegated Management), FS-16 (Access Enforcement Policy) |
| **Owner** | Platform Team |
| **Date** | 2026-05-18 |

---

## Problem

Registering a new tenant is not a single INSERT — it involves creating the root record, seeding the closure table hierarchy, provisioning a schema row in `ums_identity`, initializing default configuration in BC-C, and notifying downstream contexts (Authorization, IGA, Compliance) that a new tenant is available. If any of these steps fail partially, the platform ends up with phantom tenants in inconsistent state.

## Solution: Provisioning Pipeline via Transactional Outbox

The `RegisterTenantCommand` handler executes all local DB mutations inside a single transaction. Cross-context notifications are delivered reliably via TE-04 (Transactional Outbox), guaranteeing at-least-once delivery without distributed transactions.

```
RegisterTenantCommand
    │
    ▼
┌──────────────────────────────────────────────────────┐
│  RegisterTenantCommandHandler (Application Layer)     │
│                                                       │
│  BEGIN TRANSACTION                                    │
│  ├─ Tenant.Create(code, name, type, idpStrategy)     │
│  ├─ ITenantRepository.AddAsync(tenant)               │
│  │   └─ INSERT INTO ums_identity.TENANT              │
│  ├─ Seed TENANT_CLOSURE via SQL trigger              │
│  │   (trigger fires on INSERT, builds hierarchy)     │
│  ├─ INSERT INTO ums_identity.outbox_events           │
│  │   └─ TenantCreatedEvent payload                   │
│  COMMIT                                              │
└──────────────────────┬───────────────────────────────┘
                       │  (async, via TE-04 relay)
          ┌────────────┴────────────┐
          ▼                         ▼
  BC-C Configuration         BC-B Authorization
  Seed default IdP config    Init empty auth graph
          │                         │
          ▼                         ▼
  BC-H IGA                   BC-I Compliance
  Init role tracking          Init document catalog
```

## Closure Table (ADR-0048)

The `TENANT_CLOSURE` table is maintained by an SQL Server trigger on `ums_identity.TENANT`. The domain never writes to it directly — this is intentional per [12-design-decisions.md](../../../../governance/construction/ddd-design/12-design-decisions.md).

```sql
-- TENANT_CLOSURE schema
CREATE TABLE ums_identity.TENANT_CLOSURE (
    ancestor_id   UNIQUEIDENTIFIER NOT NULL,
    descendant_id UNIQUEIDENTIFIER NOT NULL,
    depth         INT              NOT NULL,
    root_tenant_id UNIQUEIDENTIFIER NOT NULL,
    PRIMARY KEY (ancestor_id, descendant_id)
);
```

## Hierarchical Invariants Enforced at Provisioning

| Invariant | Check | Error |
|-----------|-------|-------|
| INV-T1 | `child.TaxonomyRank > parent.TaxonomyRank` | `tenant.taxonomy_rank_violation` |
| INV-T2 | `BRANCH`/`DEPARTMENT` cannot have children | `tenant.leaf_cannot_have_children` |
| INV-T3 | ROOT tenant is its own `root_tenant_id` | Enforced by domain AR |
| INV-T4 | `CompanyReference` unique per type within same parent | `tenant.company_reference_not_unique` |

## RLS Composite Key Contract

Every table in `ums_identity` carries a `root_tenant_id` column. All repository queries **must** include `WHERE root_tenant_id = @rootTenantId` as the first predicate. The `IAggregateRepository<T>` interface enforces this at compile time by requiring `(tenantId, id)` in every `GetByIdAsync` overload.

## Delegated Tenant Management (FS-14)

A tenant administrator with the `TENANT_CREATE` permission may register sub-tenants within their own hierarchy boundary. The provisioning pipeline validates that:
- The parent tenant exists and is `ACTIVE`.
- The requesting user's `TenantId` is an ancestor of the target parent (closure table lookup).
- The new tenant's `TaxonomyRank` is strictly greater than the parent's rank (INV-T1).

## Outbox Events Published

| Event | Consumer |
|-------|----------|
| `TenantCreatedEvent` | BC-C, BC-B, BC-H, BC-I, BC-D (Audit) |
| `BranchCreatedEvent` | BC-B (scope binding), BC-D (Audit) |

---

**[Technical Enablers Index](./index.md)** | **[Traceability Matrix](../../traceability-matrix.md)**
