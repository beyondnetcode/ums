# Domain Aggregate Architecture

> **Language:** [English](./index.md) | [Español](../domain-es/index.md)

Detailed architecture documents for every Aggregate Root and Owned Entity in the UMS domain model, organised by Bounded Context. Each document covers the full 8-section structure: Aggregate Overview · Object Model · Sequence Diagrams · ER Model · Bounded Context Model · API Contract · Persistence Notes · Security and Audit.

---

## Identity BC — `Ums.Domain.Identity`

| Aggregate / Entity | Type | Document |
|---|---|---|
| `Tenant` | Aggregate Root | [tenant.md](./identity/tenant.md) |
| `Branch` | Owned Entity (Tenant) | [branch.md](./identity/branch.md) |
| `Branding` | Owned Entity (Tenant) | [branding.md](./identity/branding.md) |
| `IdentityProvider` | Owned Entity (Tenant) | [identity-provider.md](./identity/identity-provider.md) |
| `UserAccount` | Aggregate Root | [user-account.md](./identity/user-account.md) |
| `PasswordCredential` | Owned Entity (UserAccount) | [password-credential.md](./identity/password-credential.md) |
| `MfaEnrollment` | Owned Entity (UserAccount) | [mfa-enrollment.md](./identity/mfa-enrollment.md) |

## Authorization BC — `Ums.Domain.Authorization`

| Aggregate / Entity | Type | Document |
|---|---|---|
| `SystemSuite` | Aggregate Root | [system-suite.md](./authorization/system-suite.md) |
| `Module` | Owned Entity (SystemSuite) | [module.md](./authorization/module.md) |
| `Menu` | Owned Entity (Module) | [menu.md](./authorization/menu.md) |
| `SubMenu` | Owned Entity (Menu) | [sub-menu.md](./authorization/sub-menu.md) |
| `Option` | Owned Entity (SubMenu) | [option.md](./authorization/option.md) |
| `Action` | Owned Entity (Option) | [action.md](./authorization/action.md) |
| `PermissionTemplate` | Aggregate Root | [permission-template.md](./authorization/permission-template.md) |
| `PermissionTemplateItem` | Owned Entity (PermissionTemplate) | [permission-template-item.md](./authorization/permission-template-item.md) |
| `Profile` | Aggregate Root | [profile.md](./authorization/profile.md) |
| `ProfilePermission` | Owned Entity (Profile) | [profile-permission.md](./authorization/profile-permission.md) |

## Configuration BC — `Ums.Domain.Configuration`

| `AppConfiguration` | Aggregate Root | [app-configuration.md](./configuration/app-configuration.md) |
| `FeatureFlag` | Aggregate Root | [feature-flag.md](./configuration/feature-flag.md) |
| `FlagEvaluationLog` | Owned Entity (FeatureFlag) | [flag-evaluation-log.md](./configuration/flag-evaluation-log.md) |
| `IdpConfiguration` | Aggregate Root | [idp-configuration.md](./configuration/idp-configuration.md) |

## Approvals BC — `Ums.Domain.Approvals`

| Aggregate / Entity | Type | Document |
|---|---|---|
| `ApprovalWorkflow` | Aggregate Root | [approval-workflow.md](./approvals/approval-workflow.md) |
| `ApprovalRequiredDocument` | Owned Entity (ApprovalWorkflow) | [approval-required-document.md](./approvals/approval-required-document.md) |
| `ApprovalRequest` | Aggregate Root | [approval-request.md](./approvals/approval-request.md) |
| `DocumentType` | Aggregate Root | [document-type.md](./approvals/document-type.md) |
| `NotificationRule` | Owned Entity (DocumentType) | [notification-rule.md](./approvals/notification-rule.md) |
| `UserDocument` | Aggregate Root | [user-document.md](./approvals/user-document.md) |
| `AccessNotification` | Owned Entity (UserDocument) | [access-notification.md](./approvals/access-notification.md) |
| `AccessEnforcementPolicy` | Aggregate Root | [access-enforcement-policy.md](./approvals/access-enforcement-policy.md) |

## IGA BC — `Ums.Domain.IGA`

| Aggregate / Entity | Type | Document |
|---|---|---|
| `PromotionRequest` | Aggregate Root | [promotion-request.md](./iga/promotion-request.md) |
| `PromotionImpactAnalysis` | Owned Entity (PromotionRequest) | [promotion-impact-analysis.md](./iga/promotion-impact-analysis.md) |
| `RoleMaturityStatus` | Aggregate Root | [role-maturity-status.md](./iga/role-maturity-status.md) |

## Audit BC — `Ums.Domain.Audit`

| Aggregate / Entity | Type | Document |
|---|---|---|
| `AuditRecord` | Aggregate Root | [audit-record.md](./audit/audit-record.md) |

---

**[Back to Master Index](../MASTER_INDEX.md)** | **[DDD Design Portal](../governance/construction/ddd-design/index.md)**
