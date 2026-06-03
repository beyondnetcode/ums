# Approvals BC — Consistency Rules

> **Bounded Context:** `Ums.Domain.Approvals`  
> **Aggregates:** `ApprovalWorkflow`, `ApprovalRequest`, `DocumentType`, `NotificationRule`, `UserDocument`, `AccessEnforcementPolicy`

---

## ApprovalWorkflow

### State Machine

No explicit status enum currently. Consistency enforced via `RequiresApproval` flag and required documents collection.

**Planned:** `WorkflowStatus: { Draft, Active, Archived }` — not yet implemented.

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `AddRequiredDocument()` | DocumentType must not already be required | `approvals.document_type_already_required` |
| `Create()` | If `RequiresApproval = true`, at least one `RequiredDocument` must be added | `approvals.requires_documents_if_approval_required` |
| `RemoveRequiredDocument()` | If `RequiresApproval = true`, must leave ≥ 1 document | `approvals.requires_documents_if_approval_required` |

### Orphan Risks

| Risk | Scenario | Mitigation |
|------|----------|-----------|
| 🟠 Workflow with 0 required docs but RequiresApproval=true | `RemoveRequiredDocument()` empties the list | Guard: validate min count before remove |

---

## ApprovalRequest

### State Machine

```
Pending ──Approve()──► Approved (terminal)
Pending ──Reject()──► Rejected (terminal)
```

**Planned extensions:** `Revoked`, `Expired` states — not yet implemented.

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Approve()` / `Reject()` | Status must be `Pending` | `approvals.request_not_pending` |

### Cross-Aggregate Dependency Guards (application layer validates before calling)

| Operation | Validation | Notes |
|-----------|------------|-------|
| `Create()` | `WorkflowId` must exist and be active | Application layer pre-validates |
| `Create()` | `RequestedRoleId` must exist and be active | Application layer pre-validates |
| `Approve()` | `GrantedRoleId` must exist and be active | Application layer pre-validates |

### Orphan Risks

| Risk | Scenario | Mitigation |
|------|----------|-----------|
| 🔴 Pending request references deleted Role | Role deleted after request created | Application layer listens to `RoleDeactivatedEvent`, cancels pending requests |
| 🔴 Pending request never resolved | No timeout mechanism | Planned: background job to expire after N days |

---

## DocumentType

### State Machine

`DocumentType` does not have an explicit status state machine. It is managed via `Criticity` level and `IsActive` flag.

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Create()` | Code must be unique within tenant | `common.duplicate` |
| Criticity rules | `Critical` document type must have at least one active `AccessEnforcementPolicy` | Validated at query/reporting level |

### Design Decision: NotificationRule

> **Code is the source of truth.**  
> `NotificationRule` is implemented as an **independent Aggregate Root** in `Ums.Domain.Approvals.NotificationRule`.  
> It is **not** an owned entity of `DocumentType`.  
> `DocumentType` may reference `NotificationRule` entities by ID, but does not own their lifecycle.  
> The `document-type.md` documentation has been updated to reflect this.

---

## NotificationRule

### Aggregate Root: `Ums.Domain.Approvals.NotificationRule`

`NotificationRule` is an **independent Aggregate Root** — not an owned entity of `DocumentType`.

### State Machine

```
Active ──Deactivate()──► Inactive
```

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Deactivate()` | Must not already be inactive | `approvals.rule_already_inactive` |

### Orphan Risks

| Risk | Scenario | Mitigation |
|------|----------|-----------|
| 🟢 Deactivated rule referenced by DocumentType | DocumentType references inactive rule | Application layer should filter inactive rules from notifications |

---

## UserDocument

### State Machine

```
PendingReview ──Validate()──► Valid ──Expire()──► Expired
PendingReview ──Reject()──► Rejected
Rejected ──ReUpload()──► PendingReview
Expired ──ReUpload()──► PendingReview
```

**Planned:** `Archived` state after `Expired` with retention period.

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Validate()` / `Reject()` | Status must be `PendingReview` | `compliance.document_not_pending_review` |
| `Expire()` | Status must not already be `Expired` | `compliance.document_already_expired` |
| Any invalid transition | E.g. `Valid → PendingReview` | `compliance.document_cannot_transition` |
| `Upload()` | `ExpirationDate` must be after `IssueDate` | `compliance.expiration_before_issue_date` |

### Child Entity Cascade Rules

| Event | Cascade |
|-------|---------|
| Any state change | `AccessNotification` entries are append-only audit records. No cascade required. |

---

## AccessEnforcementPolicy

### State Machine

```
Active ──Deactivate()──► Inactive
```

### Intra-aggregate Guards

| Operation | Guard | Broken Rule |
|-----------|-------|-------------|
| `Create()` | Must target at least one Profile or Role | `approvals.policy_requires_profile_or_role` |
| `Deactivate()` | Must not already be inactive | `approvals.policy_already_inactive` |

---

*Part of [consistency-rules/index.md](./index.md)*
