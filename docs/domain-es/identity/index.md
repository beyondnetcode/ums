# Identity BC — Arquitectura de Agregados

> **Idioma:** [English](../../domain/identity/index.md) | [Español](./index.md)

**Bounded Context:** Identity (`Ums.Domain.Identity`)  
**Aggregate Roots:** `Tenant`, `TenantSignupRequest`, `UserAccount`, `UserManagementDelegation`

---

| Agregado / Entidad | Tipo | Estado |
|---|---|---|
| [Tenant](./tenant.md) | Aggregate Root | Produccion |
| [TenantSignupRequest](./tenant-signup-request.md) | Aggregate Root | Implementado para onboarding de tenant |
| [Branch](./branch.md) | Entidad Propia (Tenant) | Produccion |
| [Branding](./branding.md) | Entidad Propia (Tenant) | Produccion |
| [IdentityProvider](./identity-provider.md) | Entidad Propia (Tenant) | Produccion |
| [UserAccount](./user-account.md) | Aggregate Root | Produccion |
| [PasswordCredential](./password-credential.md) | Entidad Propia (UserAccount) | Mantenimiento activo (FS-18) |
| [MfaEnrollment](./mfa-enrollment.md) | Entidad Propia (UserAccount) | Produccion |
| [UserManagementDelegation](./user-management-delegation.md) | Aggregate Root | Produccion |

---

## Documentacion Transversal de Identity

| Documento | Descripcion |
|---|---|
| [Grafo de Autorizacion](./auth-graph.md) | Estructura y semantica del `AuthorizationGraph` retornado en el login |
| [Resolucion del Metodo de Autenticacion](./auth-method-resolution.md) | Como UMS resuelve dinamicamente la estrategia de autenticacion (Local vs IDP) por tenant |

---

**[Volver al Indice de Dominio](../index.md)**
