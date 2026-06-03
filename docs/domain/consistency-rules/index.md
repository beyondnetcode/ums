# Domain Consistency Rules — Master Index

> **Language:** English | [Español](../../domain-es/consistency-rules/index.md)
>
> **Source of truth:** `Ums.Domain` source code. This documentation tracks every aggregate's state machine, dependency guards, broken rules and orphan risks. **Update this section whenever you add a new aggregate, child entity, state transition, or domain rule.**

---

## Purpose

This section documents the **consistency rules** that every aggregate in UMS must enforce.  
A consistency rule is any invariant that must hold:

- before a state transition (`Suspend`, `Deactivate`, `Archive`, `Delete`, etc.)
- when adding or removing child entities
- when an aggregate's state affects the validity of another aggregate's state (cross-aggregate guard)

Rules are organised by Bounded Context. Each document lists:
- the aggregate's state machine (allowed transitions)
- intra-aggregate guards (things the aggregate itself can verify)
- cross-aggregate dependency guards (counts passed by the application layer)
- broken rules returned on violation
- orphan risks and cascade responsibilities

---

## Bounded Contexts

| BC | Document | Aggregates covered |
|----|----------|-------------------|
| Identity | [identity-bc.md](./identity-bc.md) | `Tenant`, `UserAccount`, `UserManagementDelegation`, `TenantSignupRequest` |
| Authorization | [authorization-bc.md](./authorization-bc.md) | `SystemSuite`, `Role`, `PermissionTemplate`, `Profile` |
| Configuration | [configuration-bc.md](./configuration-bc.md) | `AppConfiguration`, `IdpConfiguration`, `FeatureFlag`, `ParameterDefinition`, `ParameterGlobalValue`, `ParameterTenantValue` |
| Approvals | [approvals-bc.md](./approvals-bc.md) | `ApprovalWorkflow`, `ApprovalRequest`, `DocumentType`, `NotificationRule`, `UserDocument`, `AccessEnforcementPolicy` |
| IGA | [iga-bc.md](./iga-bc.md) | `PromotionRequest`, `RoleMaturityStatus` |
| Audit | Append-only — no state transitions | `AuditRecord` |

---

## Global Rules (apply to all aggregates)

1. **Aggregate owns its children.** Child entity state changes must go through the aggregate root — never call child methods directly from outside.
2. **No orphan child entities.** Deactivating or archiving an aggregate must cascade to or validate all owned child entities.
3. **Cross-aggregate guards use counts.** When a guard requires knowledge of another aggregate (e.g. "has active users"), the application layer queries the count and passes it to the aggregate method. The aggregate validates the count.
4. **Every blocked operation returns a typed broken rule.** Error codes use the format `AGGREGATE_REASON` (e.g. `TENANT_HAS_ACTIVE_USERS`).
5. **Every successful state change raises a domain event.**
6. **Audit is always updated** on every successful mutating operation.
7. **Archived state is terminal.** No aggregate may transition out of `Archived` status once set.

---

## Broken Rules Registry

See [broken-rules-registry.md](./broken-rules-registry.md) for the full, searchable list of every broken rule code, its aggregate, trigger operation, and implementation status.

---

## How to update this documentation

When you add a new aggregate, child entity, state transition or domain rule:

1. Find the relevant BC document in this folder.
2. Add a row to the aggregate's state-machine table.
3. Add the broken rule to the broken-rules-registry.
4. If a new aggregate is added, create a new section in the BC document **and** add it to the table above.
5. Synchronise the Spanish version at `docs/domain-es/consistency-rules/`.
6. Update `docs/domain/index.md` aggregate count.

---

*Last reviewed: 2026-06-03 — based on `Ums.Domain` source analysis.*
