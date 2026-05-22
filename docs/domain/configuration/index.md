# Configuration BC — Aggregate Architecture

> **Language:** [English](./index.md) | [Español](../../domain-es/configuration/index.md)

**Bounded Context:** Configuration (`Ums.Domain.Configuration`)  
**Aggregate Roots:** `AppConfiguration`, `FeatureFlag`, `IdpConfiguration`

---

### App Configurations & Toggles
- [AppConfiguration](./app-configuration.md) (Aggregate Root) — Controls tenant-level application configuration parameters, session timeouts, MFA policies, and environment variables.
- [FeatureFlag](./feature-flag.md) (Aggregate Root) — Defines platform-wide or tenant-scoped operational flags and release rules.
- [FlagEvaluationLog](./flag-evaluation-log.md) (Owned Entity) — Tracks flag evaluation contexts and outcomes at runtime for troubleshooting and audit.

### Integration Configurations
- [IdpConfiguration](./idp-configuration.md) (Aggregate Root) — Mappings of technical secrets, private keys, client IDs, and endpoints for federated Identity Providers (OIDC, SAML, WS-Fed).

---

**[Back to Domain Index](../index.md)**
