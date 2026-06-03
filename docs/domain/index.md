# Domain Aggregate Architecture

> **Language:** [English](./index.md) | [Español](../domain-es/index.md)

Detailed architecture documents for every Aggregate Root in the UMS domain model, organised by Bounded Context. There are 23 Aggregate Roots across 6 Bounded Contexts. Child entities (Branch, Branding, IdentityProvider, PasswordCredential, MfaEnrollment, ProfilePermission, functional menus/modules/options/actions, flag evaluation logs, promotion impact analyses, etc.) are documented inside their respective parent Aggregate Root document — not as separate documents.

---

## Identity BC — `Ums.Domain.Identity`

| Aggregate Root | Document | Owned Child Entities (documented inline) |
|---|---|---|
| `Tenant` | [tenant.md](./identity/tenant.md) | `Branch`, `Branding`, `IdentityProvider` |
| `TenantSignupRequest` | [tenant-signup-request.md](./identity/tenant-signup-request.md) | None |
| `UserAccount` | [user-account.md](./identity/user-account.md) | `PasswordCredential`, `MfaEnrollment` |
| `UserManagementDelegation` | [user-management-delegation.md](./identity/user-management-delegation.md) | None |

---

## Authorization BC — `Ums.Domain.Authorization`

| Aggregate Root | Document | Owned Child Entities (documented inline) |
|---|---|---|
| `SystemSuite` | [system-suite.md](./authorization/system-suite.md) | `Module`, `Menu`, `SubMenu`, `Option`, `Action` |
| `Role` | [role.md](./authorization/role.md) | None |
| `PermissionTemplate` | [permission-template.md](./authorization/permission-template.md) | `PermissionTemplateItem` |
| `Profile` | [profile.md](./authorization/profile.md) | `ProfilePermission` |

---

## Configuration BC — `Ums.Domain.Configuration`

| Aggregate Root | Document | Owned Child Entities (documented inline) |
|---|---|---|
| `IdpConfiguration` | [idp-configuration.md](./configuration/idp-configuration.md) | None |
| `AppConfiguration` | [app-configuration.md](./configuration/app-configuration.md) | None |
| `FeatureFlag` | [feature-flag.md](./configuration/feature-flag.md) | `FlagEvaluationLog` |
| `ParameterDefinition` | [parameter-definition.md](./configuration/parameter-definition.md) | None |
| `ParameterGlobalValue` | [parameter-global-value.md](./configuration/parameter-global-value.md) | None |
| `ParameterTenantValue` | [parameter-tenant-value.md](./configuration/parameter-tenant-value.md) | None |

---

## Approvals BC — `Ums.Domain.Approvals`

| Aggregate Root | Document | Owned Child Entities (documented inline) |
|---|---|---|
| `ApprovalWorkflow` | [approval-workflow.md](./approvals/approval-workflow.md) | `ApprovalRequiredDocument` |
| `ApprovalRequest` | [approval-request.md](./approvals/approval-request.md) | `ApprovalLog` (inline) |
| `DocumentType` | [document-type.md](./approvals/document-type.md) | `EnforcementPolicy` |
| `NotificationRule` | [notification-rule.md](./approvals/notification-rule.md) | None |
| `UserDocument` | [user-document.md](./approvals/user-document.md) | `AccessNotification` |
| `AccessEnforcementPolicy` | [access-enforcement-policy.md](./approvals/access-enforcement-policy.md) | None |

---

## IGA BC — `Ums.Domain.IGA`

| Aggregate Root | Document | Owned Child Entities (documented inline) |
|---|---|---|
| `PromotionRequest` | [promotion-request.md](./iga/promotion-request.md) | `PromotionImpactAnalysis` |
| `RoleMaturityStatus` | [role-maturity-status.md](./iga/role-maturity-status.md) | None |

---

## Audit BC — `Ums.Domain.Audit`

| Aggregate Root | Document | Owned Child Entities (documented inline) |
|---|---|---|
| `AuditRecord` | [audit-record.md](./audit/audit-record.md) | None (Append-only) |

---

---

## Consistency Rules

Domain-wide state-machine rules, dependency guards, broken rules registry, and orphan-risk analysis:

**[Consistency Rules Portal →](./consistency-rules/index.md)**

---

**[Back to Master Index](../MASTER_INDEX.md)** | **[DDD Design Portal](../governance/construction/ddd-design/index.md)**
