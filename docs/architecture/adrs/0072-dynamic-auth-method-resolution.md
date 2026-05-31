# ADR-0070: Dynamic Auth Method Resolution — From Configuration, Not Code

**Status:** Accepted
**Date:** 2026-05-31
**Decision Owner:** Architecture
**Related:**
- [ADR-0069: Auth Graph Engine](./0069-auth-graph-engine.md)
- [ADR-0054: Shell Library Isolation](./0054-shell-library-isolation.md)

---

## Context

UMS supports both Local (BCrypt) and external IDP authentication per tenant. Previously, the login endpoint used only BCrypt unconditionally — a hardcoded strategy that did not reflect the tenant's configured authentication method.

The tenant's auth method is controlled by `AUTH_USE_EXTERNAL_IDP` (a parameter in the ParameterCatalog, tenant-scoped). When `true`, the tenant uses external Identity Providers (Azure AD, Okta, etc.); when `false`, local BCrypt is used. This configuration change must take effect without deploying new code.

---

## Decision

### 1. IAuthMethodResolver Port

`IAuthMethodResolver.ResolveAsync(tenantId)` reads `AUTH_USE_EXTERNAL_IDP` from `IConfigurationProvider` (already in memory) — **zero DB queries per request**. Returns:
- `AuthMethod.Local()` when false
- `AuthMethod.Idp(activeProvider)` when true and an active IDP exists
- `Result.Failure("AUTH_011")` when true but no active IDP is configured

The resolver is a pure application service — no infrastructure dependencies.

### 2. Strategy Pattern via Domain Ports

```
ILocalAuthStrategy  — validates BCrypt credentials (Application layer)
IIdpAuthStrategy    — dispatches to correct IIdpAuthAdapter by strategy name
IIdpAuthAdapter     — ACL port per provider type (Infrastructure layer)
```

The `IIdpAuthStrategy` uses Shell.Factory to resolve the correct `IIdpAuthAdapter` (same pattern as `IdpResolutionStrategyFactorySetup`). New IDP adapters are added by registering a new implementation — no changes to the auth flow.

### 3. Development Stub IDP Adapter

`StubIdpAuthAdapter` accepts credentials starting with `MOCK-` prefix and returns a valid `ExternalIdentity`. It is registered in non-Production environments only, allowing end-to-end testing of the IDP flow without a real external provider.

### 4. Auth Error Codes

| Code | Meaning |
|---|---|
| AUTH_001 | Validation error (missing fields) |
| AUTH_002 | Tenant not found |
| AUTH_003 | Tenant not active |
| AUTH_004 | IDP user has no UMS account |
| AUTH_005 | User account not active |
| AUTH_006 | Invalid credentials (Local) |
| AUTH_011 | IDP mode configured but no active provider |
| AUTH_012 | No IDP adapter registered for strategy |

---

## Rationale

### Why read from IConfigurationProvider, not the DB

`IConfigurationProvider` loads all configuration values at startup and caches them in memory. Reading from it is a dictionary lookup — no DB roundtrip. When a tenant's auth mode is changed (e.g., via the IdpPanel toggle), the configuration is updated in the DB and the provider is refreshed. The change takes effect on the next login without restarting the service.

### Why not hardcode BCrypt as the default

Hardcoding creates implicit coupling between the auth flow and a specific implementation. Any new auth strategy (SAML2, OAuth2, custom) would require modifying the command handler. The strategy pattern allows new methods to be added by registering a new adapter.

---

## Consequences

### Positive

- Adding a new IDP adapter requires only implementing `IIdpAuthAdapter` and registering it in `IdpAuthAdapterFactorySetup`
- Auth method changes take effect immediately without code deployment
- The stub adapter enables complete IDP flow testing in development
- Auth method is recorded in the audit trail for traceability

### Trade-offs

- The IDP flow requires a valid `UserAccount` in UMS matching the IDP-asserted email. Just-in-time provisioning is out of scope.

---

## Implementation

| Component | Location |
|---|---|
| `IAuthMethodResolver` port | `Ums.Domain/Identity/Auth/IAuthMethodResolver.cs` |
| `AuthMethodResolverService` | `Ums.Application/Identity/Auth/AuthMethodResolverService.cs` |
| `ILocalAuthStrategy` port | `Ums.Domain/Identity/Auth/ILocalAuthStrategy.cs` |
| `LocalAuthStrategyService` | `Ums.Application/Identity/Auth/LocalAuthStrategyService.cs` |
| `IIdpAuthStrategy` port | `Ums.Domain/Identity/Auth/IIdpAuthStrategy.cs` |
| `IIdpAuthAdapter` ACL port | `Ums.Domain/Identity/Auth/IIdpAuthAdapter.cs` |
| `IdpAuthStrategyDispatcher` | `Ums.Infrastructure/Identity/Auth/IdpAuthStrategyDispatcher.cs` |
| `StubIdpAuthAdapter` | `Ums.Infrastructure/Identity/Auth/StubIdpAuthAdapter.cs` |
| `IdpAuthAdapterFactorySetup` | `Ums.Infrastructure/Identity/Auth/IdpAuthAdapterFactorySetup.cs` |

---

**[ADR Registry](./index.md)**
