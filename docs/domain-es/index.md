# Arquitectura de Agregados de Dominio

> **Idioma:** [English](../domain/index.md) | [Español](./index.md)

Documentos de arquitectura detallados para cada Aggregate Root y Entidad Propia del modelo de dominio UMS, organizados por Bounded Context. Cada documento cubre la estructura de 8 secciones: Descripcion del Agregado · Modelo de Objetos · Diagramas de Secuencia · Modelo ER · Modelo de Bounded Context · Contrato de API · Notas de Persistencia · Seguridad y Auditoria.

---

## Identity BC — `Ums.Domain.Identity`

| Agregado / Entidad | Tipo | Documento |
|---|---|---|
| `Tenant` | Aggregate Root | [tenant.md](./identity/tenant.md) |
| `Branch` | Entidad Propia (Tenant) | [branch.md](./identity/branch.md) |
| `Branding` | Entidad Propia (Tenant) | [branding.md](./identity/branding.md) |
| `IdentityProvider` | Entidad Propia (Tenant) | [identity-provider.md](./identity/identity-provider.md) |
| `UserAccount` | Aggregate Root | [user-account.md](./identity/user-account.md) |
| `PasswordCredential` | Entidad Propia (UserAccount) | [password-credential.md](./identity/password-credential.md) |
| `MfaEnrollment` | Entidad Propia (UserAccount) | [mfa-enrollment.md](./identity/mfa-enrollment.md) |

## Authorization BC — `Ums.Domain.Authorization`

| Agregado / Entidad | Tipo | Documento |
|---|---|---|
| `SystemSuite` | Aggregate Root | [system-suite.md](./authorization/system-suite.md) |
| `Module` | Entidad Propia (SystemSuite) | [module.md](./authorization/module.md) |
| `Menu` | Entidad Propia (Module) | [menu.md](./authorization/menu.md) |
| `SubMenu` | Entidad Propia (Menu) | [sub-menu.md](./authorization/sub-menu.md) |
| `Option` | Entidad Propia (SubMenu) | [option.md](./authorization/option.md) |
| `Action` | Entidad Propia (Option) | [action.md](./authorization/action.md) |
| `PermissionTemplate` | Aggregate Root | [permission-template.md](./authorization/permission-template.md) |
| `PermissionTemplateItem` | Entidad Propia (PermissionTemplate) | [permission-template-item.md](./authorization/permission-template-item.md) |
| `Profile` | Aggregate Root | [profile.md](./authorization/profile.md) |
| `ProfilePermission` | Entidad Propia (Profile) | [profile-permission.md](./authorization/profile-permission.md) |

## Configuration BC — `Ums.Domain.Configuration`

| `AppConfiguration` | Aggregate Root | [app-configuration.md](./configuration/app-configuration.md) |
| `FeatureFlag` | Aggregate Root | [feature-flag.md](./configuration/feature-flag.md) |
| `FlagEvaluationLog` | Entidad Propia (FeatureFlag) | [flag-evaluation-log.md](./configuration/flag-evaluation-log.md) |
| `IdpConfiguration` | Aggregate Root | [idp-configuration.md](./configuration/idp-configuration.md) |

## Approvals BC — `Ums.Domain.Approvals`

| Agregado / Entidad | Tipo | Documento |
|---|---|---|
| `ApprovalWorkflow` | Aggregate Root | [approval-workflow.md](./approvals/approval-workflow.md) |
| `ApprovalRequiredDocument` | Entidad Propia (ApprovalWorkflow) | [approval-required-document.md](./approvals/approval-required-document.md) |
| `ApprovalRequest` | Aggregate Root | [approval-request.md](./approvals/approval-request.md) |
| `DocumentType` | Aggregate Root | [document-type.md](./approvals/document-type.md) |
| `NotificationRule` | Entidad Propia (DocumentType) | [notification-rule.md](./approvals/notification-rule.md) |
| `UserDocument` | Aggregate Root | [user-document.md](./approvals/user-document.md) |
| `AccessNotification` | Entidad Propia (UserDocument) | [access-notification.md](./approvals/access-notification.md) |
| `AccessEnforcementPolicy` | Aggregate Root | [access-enforcement-policy.md](./approvals/access-enforcement-policy.md) |

## IGA BC — `Ums.Domain.IGA`

| Agregado / Entidad | Tipo | Documento |
|---|---|---|
| `PromotionRequest` | Aggregate Root | [promotion-request.md](./iga/promotion-request.md) |
| `PromotionImpactAnalysis` | Entidad Propia (PromotionRequest) | [promotion-impact-analysis.md](./iga/promotion-impact-analysis.md) |
| `RoleMaturityStatus` | Aggregate Root | [role-maturity-status.md](./iga/role-maturity-status.md) |

## Audit BC — `Ums.Domain.Audit`

| Agregado / Entidad | Tipo | Documento |
|---|---|---|
| `AuditRecord` | Aggregate Root | [audit-record.md](./audit/audit-record.md) |

---

**[Volver al Indice Maestro](../MASTER_INDEX.es.md)** | **[Portal DDD](../governance/construction/ddd-design/index.md)**
