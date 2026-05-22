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
| `SystemSuite` | Aggregate Root | *(pending)* |
| `Module` | Owned Entity (SystemSuite) | *(pending)* |
| `Menu` | Owned Entity (Module) | *(pending)* |
| `SubMenu` | Owned Entity (Menu) | *(pending)* |
| `Option` | Owned Entity (SubMenu) | *(pending)* |
| `Action` | Owned Entity (Option) | *(pending)* |
| `PermissionTemplate` | Aggregate Root | *(pending)* |
| `PermissionTemplateItem` | Owned Entity (PermissionTemplate) | *(pending)* |
| `Profile` | Aggregate Root | *(pending)* |
| `ProfilePermission` | Owned Entity (Profile) | *(pending)* |

## Configuration BC — `Ums.Domain.Configuration`

| Aggregate / Entity | Type | Document |
|---|---|---|
| `AppConfiguration` | Aggregate Root | *(pending)* |
| `FeatureFlag` | Aggregate Root | *(pending)* |
| `FlagEvaluationLog` | Owned Entity (FeatureFlag) | *(pending)* |
| `IdpConfiguration` | Aggregate Root | *(pending)* |

## Approvals BC — `Ums.Domain.Approvals`

| Aggregate / Entity | Type | Document |
|---|---|---|
| `ApprovalWorkflow` | Aggregate Root | *(pending)* |
| `ApprovalRequiredDocument` | Owned Entity (ApprovalWorkflow) | *(pending)* |
| `ApprovalRequest` | Aggregate Root | *(pending)* |
| `DocumentType` | Aggregate Root | *(pending)* |
| `NotificationRule` | Aggregate Root | *(pending)* |
| `UserDocument` | Aggregate Root | *(pending)* |
| `AccessNotification` | Aggregate Root | *(pending)* |
| `AccessEnforcementPolicy` | Aggregate Root | *(pending)* |

## IGA BC — `Ums.Domain.IGA`

| Aggregate / Entity | Type | Document |
|---|---|---|
| `PromotionRequest` | Aggregate Root | *(pending)* |
| `PromotionImpactAnalysis` | Owned Entity (PromotionRequest) | *(pending)* |
| `RoleMaturityStatus` | Aggregate Root | *(pending)* |

## Audit BC — `Ums.Domain.Audit`

| Aggregate / Entity | Type | Document |
|---|---|---|
| `AuditRecord` | Aggregate Root | *(pending)* |

---

**[Back to Master Index](../MASTER_INDEX.md)** | **[DDD Design Portal](../governance/construction/ddd-design/index.md)**
