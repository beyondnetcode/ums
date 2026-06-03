# Arquitectura de Agregados de Dominio

> **Idioma:** [English](../domain/index.md) | [Español](./index.md)

Documentos de arquitectura detallados para cada Aggregate Root en el modelo de dominio UMS, organizados por Bounded Context. Las entidades hijas (Branch, Branding, IdentityProvider, PasswordCredential, MfaEnrollment, ProfilePermission, módulos/menús/opciones/acciones funcionales, bitácoras de evaluación de banderas, análisis de impacto de promoción, etc.) se documentan dentro de su respectivo documento de Agregado Raíz (Aggregate Root) — no como documentos independientes.

---

## Identity BC — `Ums.Domain.Identity`

| Agregado Raíz | Documento | Entidades Hijas Propias (documentadas inline) |
|---|---|---|
| `Tenant` | [tenant.md](./identity/tenant.md) | `Branch`, `Branding`, `IdentityProvider` |
| `UserAccount` | [user-account.md](./identity/user-account.md) | `PasswordCredential`, `MfaEnrollment` |
| `UserManagementDelegation` | [user-management-delegation.md](./identity/user-management-delegation.md) | Ninguna |

---

## Authorization BC — `Ums.Domain.Authorization`

| Agregado Raíz | Documento | Entidades Hijas Propias (documentadas inline) |
|---|---|---|
| `SystemSuite` | [system-suite.md](./authorization/system-suite.md) | `Module`, `Menu`, `SubMenu`, `Option`, `Action` |
| `Role` | [role.md](./authorization/role.md) | Ninguna |
| `PermissionTemplate` | [permission-template.md](./authorization/permission-template.md) | `PermissionTemplateItem` |
| `Profile` | [profile.md](./authorization/profile.md) | `ProfilePermission` |

---

## Configuration BC — `Ums.Domain.Configuration`

| Agregado Raíz | Documento | Entidades Hijas Propias (documentadas inline) |
|---|---|---|
| `IdpConfiguration` | [idp-configuration.md](./configuration/idp-configuration.md) | Ninguna |
| `AppConfiguration` | [app-configuration.md](./configuration/app-configuration.md) | Ninguna |
| `FeatureFlag` | [feature-flag.md](./configuration/feature-flag.md) | `FlagEvaluationLog` |
| `ParameterDefinition` | [parameter-definition.md](./configuration/parameter-definition.md) | Ninguna |
| `ParameterGlobalValue` | [parameter-global-value.md](./configuration/parameter-global-value.md) | Ninguna |
| `ParameterTenantValue` | [parameter-tenant-value.md](./configuration/parameter-tenant-value.md) | Ninguna |

---

## Approvals BC — `Ums.Domain.Approvals`

| Agregado Raíz | Documento | Entidades Hijas Propias (documentadas inline) |
|---|---|---|
| `ApprovalWorkflow` | [approval-workflow.md](./approvals/approval-workflow.md) | `ApprovalRequiredDocument` |
| `ApprovalRequest` | [approval-request.md](./approvals/approval-request.md) | `ApprovalLog` (inline) |
| `DocumentType` | [document-type.md](./approvals/document-type.md) | `EnforcementPolicy` |
| `NotificationRule` | [notification-rule.md](./approvals/notification-rule.md) | Ninguna |
| `UserDocument` | [user-document.md](./approvals/user-document.md) | `AccessNotification` |
| `AccessEnforcementPolicy` | [access-enforcement-policy.md](./approvals/access-enforcement-policy.md) | Ninguna |

---

## IGA BC — `Ums.Domain.IGA`

| Agregado Raíz | Documento | Entidades Hijas Propias (documentadas inline) |
|---|---|---|
| `PromotionRequest` | [promotion-request.md](./iga/promotion-request.md) | `PromotionImpactAnalysis` |
| `RoleMaturityStatus` | [role-maturity-status.md](./iga/role-maturity-status.md) | Ninguna |

---

## Audit BC — `Ums.Domain.Audit`

| Agregado Raíz | Documento | Entidades Hijas Propias (documentadas inline) |
|---|---|---|
| `AuditRecord` | [audit-record.md](./audit/audit-record.md) | Ninguna (adición exclusiva) |

---

## Consistency Rules

Reglas de consistencia por Bounded Context, registro de broken rules y riesgos de orfandad:

**[Portal de Reglas de Consistencia →](./consistency-rules/index.md)**

---

**[Volver al Índice Maestro](../MASTER_INDEX.es.md)** | **[Portal DDD](../governance/construction/ddd-design/index.md)**
