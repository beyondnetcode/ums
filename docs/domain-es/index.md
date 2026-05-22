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
| `SystemSuite` | Aggregate Root | *(pendiente)* |
| `Module` | Entidad Propia (SystemSuite) | *(pendiente)* |
| `Menu` | Entidad Propia (Module) | *(pendiente)* |
| `SubMenu` | Entidad Propia (Menu) | *(pendiente)* |
| `Option` | Entidad Propia (SubMenu) | *(pendiente)* |
| `Action` | Entidad Propia (Option) | *(pendiente)* |
| `PermissionTemplate` | Aggregate Root | *(pendiente)* |
| `PermissionTemplateItem` | Entidad Propia (PermissionTemplate) | *(pendiente)* |
| `Profile` | Aggregate Root | *(pendiente)* |
| `ProfilePermission` | Entidad Propia (Profile) | *(pendiente)* |

## Configuration BC — `Ums.Domain.Configuration`

| Agregado / Entidad | Tipo | Documento |
|---|---|---|
| `AppConfiguration` | Aggregate Root | *(pendiente)* |
| `FeatureFlag` | Aggregate Root | *(pendiente)* |
| `FlagEvaluationLog` | Entidad Propia (FeatureFlag) | *(pendiente)* |
| `IdpConfiguration` | Aggregate Root | *(pendiente)* |

## Approvals BC — `Ums.Domain.Approvals`

| Agregado / Entidad | Tipo | Documento |
|---|---|---|
| `ApprovalWorkflow` | Aggregate Root | *(pendiente)* |
| `ApprovalRequiredDocument` | Entidad Propia (ApprovalWorkflow) | *(pendiente)* |
| `ApprovalRequest` | Aggregate Root | *(pendiente)* |
| `DocumentType` | Aggregate Root | *(pendiente)* |
| `NotificationRule` | Aggregate Root | *(pendiente)* |
| `UserDocument` | Aggregate Root | *(pendiente)* |
| `AccessNotification` | Aggregate Root | *(pendiente)* |
| `AccessEnforcementPolicy` | Aggregate Root | *(pendiente)* |

## IGA BC — `Ums.Domain.IGA`

| Agregado / Entidad | Tipo | Documento |
|---|---|---|
| `PromotionRequest` | Aggregate Root | *(pendiente)* |
| `PromotionImpactAnalysis` | Entidad Propia (PromotionRequest) | *(pendiente)* |
| `RoleMaturityStatus` | Aggregate Root | *(pendiente)* |

## Audit BC — `Ums.Domain.Audit`

| Agregado / Entidad | Tipo | Documento |
|---|---|---|
| `AuditRecord` | Aggregate Root | *(pendiente)* |

---

**[Volver al Indice Maestro](../MASTER_INDEX.es.md)** | **[Portal DDD](../governance/construction/ddd-design/index.md)**
