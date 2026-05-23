# Domain Aggregate Architecture

> **Language:** [English](./index.md) | [Español](../domain-es/index.md)

Detailed architecture documents for every Aggregate Root and Owned Entity in the UMS domain model, organised by Bounded Context. Each document covers the full 8-section structure: Aggregate Overview · Object Model · Sequence Diagrams · ER Model · Bounded Context Model · API Contract · Persistence Notes · Security and Audit.

---

## Identity BC — `Ums.Domain.Identity`

### Aggregate Roots

| Aggregate Root | Document |
|---|---|
| `Tenant` | [tenant.md](./identity/tenant.md) |
| `UserAccount` | [user-account.md](./identity/user-account.md) |

### Owned Entities (documented within their parent AR)

| Owned Entity | Parent Aggregate | Document |
|---|---|---|
| `Branch` | Tenant | [branch.md](./identity/branch.md) |
| `Branding` | Tenant | [branding.md](./identity/branding.md) |
| `IdentityProvider` | Tenant | [identity-provider.md](./identity/identity-provider.md) |
| `PasswordCredential` | UserAccount | [password-credential.md](./identity/password-credential.md) |
| `MfaEnrollment` | UserAccount | [mfa-enrollment.md](./identity/mfa-enrollment.md) |

---

## Authorization BC — `Ums.Domain.Authorization`

### Aggregate Roots

| Aggregate Root | Document |
|---|---|
| `SystemSuite` | [system-suite.md](./authorization/system-suite.md) |
| `PermissionTemplate` | [permission-template.md](./authorization/permission-template.md) |
| `Profile` | [profile.md](./authorization/profile.md) |

### Owned Entities (documented within their parent AR)

| Owned Entity | Parent Aggregate | Document |
|---|---|---|
| `FunctionalModule` | SystemSuite | [module.md](./authorization/module.md) |
| `FunctionalMenu` | FunctionalModule | [menu.md](./authorization/menu.md) |
| `FunctionalSubMenu` | FunctionalMenu | [sub-menu.md](./authorization/sub-menu.md) |
| `FunctionalOption` | FunctionalSubMenu | [option.md](./authorization/option.md) |
| `Action` | SystemSuite / Module | [action.md](./authorization/action.md) |
| `PermissionTemplateItem` | PermissionTemplate | [permission-template-item.md](./authorization/permission-template-item.md) |
| `ProfilePermission` | Profile | [profile-permission.md](./authorization/profile-permission.md) |

---

## Configuration BC — `Ums.Domain.Configuration`

### Aggregate Roots

| Aggregate Root | Document |
|---|---|
| `IdpConfiguration` | [idp-configuration.md](./configuration/idp-configuration.md) |
| `AppConfiguration` | [app-configuration.md](./configuration/app-configuration.md) |
| `FeatureFlag` | [feature-flag.md](./configuration/feature-flag.md) |

### Owned Entities (documented within their parent AR)

| Owned Entity | Parent Aggregate | Document |
|---|---|---|
| `FlagEvaluationLog` | FeatureFlag | [flag-evaluation-log.md](./configuration/flag-evaluation-log.md) |

---

## Approvals BC — `Ums.Domain.Approvals`

### Aggregate Roots

| Aggregate Root | Document |
|---|---|
| `ApprovalWorkflow` | [approval-workflow.md](./approvals/approval-workflow.md) |
| `ApprovalRequest` | [approval-request.md](./approvals/approval-request.md) |
| `DocumentType` | [document-type.md](./approvals/document-type.md) |
| `UserDocument` | [user-document.md](./approvals/user-document.md) |

### Owned Entities (documented within their parent AR)

| Owned Entity | Parent Aggregate | Document |
|---|---|---|
| `ApprovalRequiredDocument` | ApprovalWorkflow | [approval-required-document.md](./approvals/approval-required-document.md) |
| `ApprovalLog` | ApprovalRequest | *(inline in approval-request.md)* |
| `NotificationRule` | DocumentType | [notification-rule.md](./approvals/notification-rule.md) |
| `AccessEnforcementPolicy` | DocumentType | [access-enforcement-policy.md](./approvals/access-enforcement-policy.md) |
| `AccessNotification` | UserDocument | [access-notification.md](./approvals/access-notification.md) |

---

## IGA BC — `Ums.Domain.IGA`

### Aggregate Roots

| Aggregate Root | Document |
|---|---|
| `PromotionRequest` | [promotion-request.md](./iga/promotion-request.md) |
| `RoleMaturityStatus` | [role-maturity-status.md](./iga/role-maturity-status.md) |

### Owned Entities (documented within their parent AR)

| Owned Entity | Parent Aggregate | Document |
|---|---|---|
| `PromotionImpactAnalysis` | PromotionRequest | [promotion-impact-analysis.md](./iga/promotion-impact-analysis.md) |

---

## Audit BC — `Ums.Domain.Audit`

### Aggregate Roots

| Aggregate Root | Document |
|---|---|
| `AuditRecord` | [audit-record.md](./audit/audit-record.md) |

> Append-only — no owned entities.

---

**[Back to Master Index](../MASTER_INDEX.md)** | **[DDD Design Portal](../governance/construction/ddd-design/index.md)**
