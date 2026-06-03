# ADR-0078: DDD Domain Resource Hierarchy — Aggregate, Entity, DomainMethod

**Status:** Accepted  
**Date:** 2026-06-03  
**Decision Owner:** Architecture  
**Related:**
- [ADR-0071: Auth Graph Engine](./0071-auth-graph-engine.md)
- [ADR-0074: Auth Graph Schema Versioning](./0074-auth-graph-schema-versioning.md)

---

## Context

The authorization system models access control at the domain level through `DomainResource` objects. Initially only `Aggregate` and `Entity` types were supported. Domain-Driven Design also defines *DomainMethods* — operations exposed by an Aggregate that carry business logic and require explicit permission grants (e.g. `ResetPassword()`, `ApproveOrder()`).

Three design questions arose:

1. Should DomainMethod be a separate entity type or extend `DomainResource`?
2. Should the parent–child relationship use a join table or a `ParentResourceId` column?
3. Where should DDD hierarchy rules be enforced (domain, application, or persistence)?

---

## Decision

### 1. DomainMethod as a `DomainResourceType` value

`DomainMethod` is added as a third variant of the existing `DomainResourceType` enum (`DomainMethod = new(3, ...)`), not as a separate entity. A `DomainResource` row can now be one of `Aggregate(1) | Entity(2) | DomainMethod(3)`.

| Option | Rejected reason |
|---|---|
| Separate `DomainMethod` entity | Duplicates query paths and complicates the auth graph builder |
| Custom action on the Aggregate | Loses the addressability needed by the auth graph |
| Extension via join table | Over-engineered for a two-level hierarchy |

### 2. `ParentResourceId` on the same table

A nullable `Guid? ParentResourceId` column on `DOMAIN_RESOURCE` provides the parent reference. This avoids a join table while keeping the hierarchy queryable in a single read.

Constraint rules enforced by the `SystemSuite` aggregate:
- A `DomainMethod` **must** have a non-null `ParentResourceId`.
- The parent resource **cannot** be a `DomainMethod` (prevents nesting beyond one level).
- The parent resource **must** exist within the same `SystemSuite`.
- An `Entity` may optionally reference a parent `Aggregate`; this is not enforced as a hard rule.

### 3. Rules enforced in the domain aggregate

All hierarchy invariants are enforced inside `SystemSuite.AddDomainResource(moduleId, parentResourceId, type, ...)`. The application layer passes the resolved `parentResourceId` and the aggregate validates it. This keeps the rules close to the data and testable without a database.

### Impact on the auth graph

The auth graph builder (`IAuthorizationGraphBuilder`) includes `DomainMethod` nodes as addressable permission targets, linked to their parent Aggregate or Entity. Permission templates can now grant access to specific domain operations, not just whole resources.

---

## Consequences

- **Positive:** Single table, single query path, clean enum extension.
- **Positive:** Auth graph can represent fine-grained method-level permissions.
- **Negative:** One-level depth limit — if deeper nesting is needed, this ADR must be revisited.
- **Neutral:** Existing `Aggregate` and `Entity` rows have `ParentResourceId = null`; no migration data change required.
