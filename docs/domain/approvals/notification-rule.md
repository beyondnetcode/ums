# NotificationRule — Aggregate Architecture

**Bounded Context:** Approvals  
**Aggregate Root:** `NotificationRule`  
**Module:** `Ums.Domain.Approvals.NotificationRule`  
**Status:** Production

---

## 1. Aggregate Overview

### Purpose
`NotificationRule` defines a notification configuration that specifies the communication channel and recipient for a notification event. It is a tenant-scoped, independent aggregate that can be referenced by `DocumentType` entities but manages its own lifecycle.

### Design Decision
`NotificationRule` is modelled as an **Aggregate Root** (not an Owned Entity of `DocumentType`). This allows a single notification rule to be independently managed, activated, or deactivated without going through a parent document type, and enables reuse of notification configurations across multiple document types.

### Aggregate Root
`NotificationRule` owns its own lifecycle. It does not depend on `DocumentType` to be created, deactivated, or deleted.

### State Machine

```
Active ──Deactivate()──► Inactive
```

### Invariants
1. `Recipient` must be a non-empty string.
2. `Channel` must be a valid `NotificationChannel` value.
3. `TenantId` must be provided and valid.

---

## 2. Child Entities

None. `NotificationRule` has no owned child entities.

---

## 3. Public Operations

| Method | State Precondition | Description |
|--------|-------------------|-------------|
| `Create(tenantId, channel, recipient, createdBy)` | — | Creates a new notification rule. |
| `Deactivate(updatedBy)` | Must be Active | Deactivates the rule. |
| `UpdateRecipient(newRecipient, updatedBy)` | Any state | Updates the notification recipient. |

---

## 4. Broken Rules

| Code | Trigger |
|------|---------|
| `approvals.rule_already_inactive` | `Deactivate()` when already inactive |
| `value_object.property_required` | `Create()` with empty recipient |

---

## 5. Value Objects

| Name | Type | Description |
|------|------|-------------|
| `NotificationRuleId` | GUID-based ID | Aggregate identifier |
| `TenantId` | GUID-based ID | Tenant scope |
| `NotificationChannel` | Enum | Email, SMS, WebPortal, etc. |
| `TextValueObject` | String wrapper | Recipient address/identifier |

---

## 6. Domain Events

None registered yet. Event raising to be added when notification dispatch is implemented.

---

**[Back to Approvals BC Index](./index.md)** | **[Consistency Rules](../consistency-rules/approvals-bc.md)**
