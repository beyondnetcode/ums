# Lenguaje Ubicuo

> **Idioma:** Español | *Versión en inglés no disponible*

**Tipo:** DDD — Ubiquitous Language  
**Version:** 2.0 | **Fecha:** 2026-05-15 | **Estado:** Propuesto  
**Alcance:** Producto completo — FS-01 a FS-16  

Términos canónicos que todos los artefactos de código, documentación y conversación deben respetar. Cualquier término fuera de esta tabla debe ser propuesto y aprobado antes de usarse en el código.

> **Ver también:** [Glosario de Requisitos](../../../requirements/glossary.md) — versión orientada a negocio del lenguaje ubicuo, organizada por término con definiciones y ejemplos de uso para stakeholders no técnicos.

---

## Glosario de Dominio

| Termino de Negocio | Clase de Dominio | Contexto | FS |
|-------------------|-----------------|----------|----|
| Organizacion / Tenant | `Tenant` | Identity | FS-03 |
| Sede | `Branch` | Identity | FS-03 |
| Usuario | `UserAccount` | Identity | FS-01, FS-03, FS-09, FS-10 |
| Sistema / Aplicacion | `SystemSuite` | Authorization | FS-04 |
| Modulo | `FunctionalModule` | Authorization | FS-04 |
| Menu (submodulo) | `FunctionalSubmodule` | Authorization | FS-04 |
| Opcion | `FunctionalOption` | Authorization | FS-04 |
| Accion | `Action` | Authorization | FS-02, FS-04 |
| Rol | `Role` | Authorization | FS-02, FS-05 |
| Plantilla de Autorizacion | `PermissionTemplate` | Authorization | FS-02, FS-05, FS-06 |
| Perfil | `Profile` | Authorization | FS-05, FS-06, FS-07 |
| Permiso Efectivo | `ProfilePermission` | Authorization | FS-05, FS-07 |
| Regla de Asignacion Auto | `TemplateAssignmentRule` | Authorization | FS-06 |
| Grafo de Autorizacion | `CompiledPolicyGraph` (runtime VO) | Authorization | FS-07 |
| Config de Proveedor de Identidad | `IdpConfiguration` | Configuration | FS-01, FS-03, FS-08 |
| Parametro de Configuracion | `AppConfiguration` | Configuration | FS-08, FS-13 |
| Indicador de Funcionalidad | `FeatureFlag` | Configuration | FS-08, FS-13 |
| Solicitud de Aprobacion | `ApprovalRequest` | Approvals | FS-10, FS-12 |
| Flujo de Aprobacion | `ApprovalWorkflow` | Approvals | FS-10, FS-12 |
| Proceso de Promocion | `UserPromotionProcess` | IGA | FS-12 |
| Criterio de Promocion | `RolePromotionCriteria` | IGA | FS-12 |
| Delegacion de Administracion | `UserManagementDelegation` | IGA | FS-14 |
| Documento del Usuario | `UserDocument` | Compliance | FS-11, FS-15, FS-16 |
| Tipo de Documento | `DocumentType` | Compliance | FS-11, FS-15, FS-16 |
| Regla de Notificacion | `NotificationRule` | Compliance | FS-15 |
| Politica de Enforcement | `AccessEnforcementPolicy` | Compliance | FS-16 |

---

## Estados Canonicos por Agregado

| Agregado | Estados |
|---------|---------|
| `UserAccount` | `PENDING` / `ACTIVE` / `BLOCKED` |
| `PermissionTemplate` | `DRAFT` / `PUBLISHED` / `DEPRECATED` |
| `AppConfiguration` | `DRAFT` / `PUBLISHED` / `ARCHIVED` |
| `ApprovalRequest` | `PENDING` / `APPROVED` / `REJECTED` / `CANCELLED` / `EXPIRED` |
| `UserPromotionProcess` | `EVALUATING` / `CRITERIA_MET` / `PENDING_APPROVAL` / `PROMOTED` |
| `UserDocument` | `PENDING_REVIEW` / `VALID` / `EXPIRED` / `REJECTED` |
| `Tenant` | `ACTIVE` / `SUSPENDED` / `ARCHIVED` |
| `SystemSuite` | `DRAFT` / `PUBLISHED` / `RETIRED` |
| `IdpConfiguration` | `DRAFT` / `ACTIVE` / `INACTIVE` |
| `UserManagementDelegation` | `ACTIVE` / `REVOKED` / `EXPIRED` |

---

**[Anterior: Bounded Context Map](./01-bounded-context-map.md)** | **[Indice DDD](./index.md)** | **[Siguiente: Identity Context](./03-identity-context.md)**
