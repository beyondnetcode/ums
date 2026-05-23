# Identity BC — Aggregate Architecture

> **Language:** [English](./index.md) | [Español](../../domain-es/identity/index.md)

**Bounded Context:** Identity (`Ums.Domain.Identity`)  
**Aggregate Roots:** `Tenant`, `UserAccount`, `UserManagementDelegation`

---

| Aggregate / Entity | Type | Status |
|---|---|---|
| [Tenant](./tenant.md) | Aggregate Root | Production |
| [Branch](./branch.md) | Owned Entity (Tenant) | Production |
| [Branding](./branding.md) | Owned Entity (Tenant) | Production |
| [IdentityProvider](./identity-provider.md) | Owned Entity (Tenant) | Production |
| [UserAccount](./user-account.md) | Aggregate Root | Production |
| [PasswordCredential](./password-credential.md) | Owned Entity (UserAccount) | Production |
| [MfaEnrollment](./mfa-enrollment.md) | Owned Entity (UserAccount) | Production |
| [UserManagementDelegation](./user-management-delegation.md) | Aggregate Root | Production |

---

**[Back to Domain Index](../index.md)**
