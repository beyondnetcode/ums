# Approvals BC — Aggregate Architecture

> **Language:** [English](./index.md) | [Español](../../domain-es/approvals/index.md)

**Bounded Context:** Approvals (`Ums.Domain.Approvals`)  
**Aggregate Roots:** `ApprovalWorkflow`, `ApprovalRequest`, `DocumentType`, `UserDocument`, `AccessEnforcementPolicy`

---

### Workflows and Requests Model
The core workflow elements govern dynamic approval routing and verification events:
- [ApprovalWorkflow](./approval-workflow.md) (Aggregate Root) — Defines sequential or parallel verification steps required for sensitive actions.
- [ApprovalRequiredDocument](./approval-required-document.md) (Owned Entity) — Mappings of specific document types required to authorize a workflow.
- [ApprovalRequest](./approval-request.md) (Aggregate Root) — A concrete runtime execution request containing verification status and signatures.

### Document Classification & Policies
- [DocumentType](./document-type.md) (Aggregate Root) — Classification of verification documents (e.g., Identification, Address Proof).
- [NotificationRule](./notification-rule.md) (Owned Entity) — Defines days before expiration, channels (email, SMS), and frequencies to trigger alert notifications.
- [UserDocument](./user-document.md) (Aggregate Root) — The uploaded physical file record belonging to a user, storing verification state.
- [AccessNotification](./access-notification.md) (Owned Entity) — History of alerts sent for document compliance.
- [AccessEnforcementPolicy](./access-enforcement-policy.md) (Aggregate Root) — Defines automatic account lockouts or security profile downgrades on document non-compliance.

---

**[Back to Domain Index](../index.md)**
