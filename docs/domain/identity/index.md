# Identity BC — Aggregate Architecture

> **Language:** [English](./index.md) | [Español](../../domain-es/identity/index.md)

**Bounded Context:** Identity (`Ums.Domain.Identity`)  
**Aggregate Roots:** `Tenant`, `TenantSignupRequest`, `UserAccount`, `UserManagementDelegation`

---

| Aggregate / Entity | Type | Status |
|---|---|---|
| [Tenant](./tenant.md) | Aggregate Root | Production |
| [TenantSignupRequest](./tenant-signup-request.md) | Aggregate Root | Implemented for tenant onboarding |
| [Branch](./branch.md) | Owned Entity (Tenant) | Production |
| [Branding](./branding.md) | Owned Entity (Tenant) | Production |
| [IdentityProvider](./identity-provider.md) | Owned Entity (Tenant) | Production |
| [UserAccount](./user-account.md) | Aggregate Root | Production |
| [PasswordCredential](./password-credential.md) | Owned Entity (UserAccount) | Active maintenance (FS-18) |
| [MfaEnrollment](./mfa-enrollment.md) | Owned Entity (UserAccount) | Production |
| [UserManagementDelegation](./user-management-delegation.md) | Aggregate Root | Production |

---

## Cross-Cutting Identity Documentation

| Document | Description |
|---|---|
| [Authorization Graph](./auth-graph.md) | Structure and semantics of the `AuthorizationGraph` returned at login |
| [Auth Method Resolution](./auth-method-resolution.md) | How UMS dynamically resolves the authentication strategy (Local vs IDP) per tenant |

---

**[Back to Domain Index](../index.md)**
