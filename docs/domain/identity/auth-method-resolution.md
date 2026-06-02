# Auth Method Resolution — How UMS Selects the Authentication Strategy

> **Language:** English | [Español](../../domain-es/identity/auth-method-resolution.md)

**Bounded Context:** Identity (`Ums.Domain.Identity`)
**Owner:** `AuthMethodResolverService` (Application layer)
**Status:** Production

---

## 1. Overview

UMS supports two authentication methods per tenant, but they are not applied uniformly to every entry point:

| Method | Description |
|---|---|
| **Local** | BCrypt credential validation. Password hash stored in `PasswordCredential` entity. Used primarily for portal management access. |
| **IDP** | Federated authentication delegated to an external Identity Provider (Azure AD, Okta, SAML2, etc.). Used primarily for external API authentication. |

The applicable method for a given tenant is **resolved at login-time from configuration**, never hardcoded. The resolver also receives an `AuthAccessScope` so the portal management flow can stay local while the external API flow can continue honoring the tenant's IDP configuration. This means the auth method can change without redeploying the application.

---

## 2. Controlling Parameter

The parameter `AUTH_USE_EXTERNAL_IDP` (Boolean, tenant-scoped) governs method selection only for the external API scope:

| Value | Result |
|---|---|
| `false` | `AuthMethod.Local()` — BCrypt validation |
| `true` + active IDP | `AuthMethod.Idp(activeProvider)` — federated auth |
| `true` + no active IDP | `Result.Failure("AUTH_011")` — configuration error |

For `AuthAccessScope.PortalManagement`, the resolver always returns `AuthMethod.Local()` and does not require the tenant IDP configuration.

This parameter lives in the `ParameterCatalog` and is loaded into `IConfigurationProvider` at application startup. Changes applied via the IdpPanel UI are persisted to the database and trigger a provider refresh — the new method takes effect on the **next login** without a service restart.

---

## 3. Resolution Flow

```
LoginCommand received
        │
        ▼
IAuthMethodResolver.ResolveAsync(tenantId, scope)
        │
        ├─ reads AUTH_USE_EXTERNAL_IDP from IConfigurationProvider (in-memory)
        │
        ├── false → AuthMethod.Local
        │            │
        │            └─► ILocalAuthStrategy.AuthenticateAsync(email, password)
        │                    └─ BCrypt.Verify(hash, password)
        │
        └── true  → reads active IdentityProvider from tenant aggregate
                     │
                     ├── none found → Result.Failure("AUTH_011")
                     │
                     └── found → AuthMethod.Idp(provider)
                                  │
                                  └─► IIdpAuthStrategy.AuthenticateAsync(provider, credentials)
                                           └─ Shell.Factory resolves IIdpAuthAdapter by strategy name
                                                └─ adapter.AuthenticateAsync(credentials)
```

---

## 4. Ports and Strategies

### `IAuthMethodResolver`

Pure application service. Reads `IConfigurationProvider` — **no DB query per request**.

```csharp
Task<Result<AuthMethod>> ResolveAsync(Guid tenantId, AuthAccessScope scope, CancellationToken ct);
```

### `ILocalAuthStrategy`

Validates BCrypt credentials against the active `PasswordCredential`.

```csharp
Task<Result<AuthenticatedPrincipal>> AuthenticateAsync(
    string email, string password, Guid tenantId, CancellationToken ct);
```

### `IIdpAuthStrategy`

Dispatches to the correct `IIdpAuthAdapter` by the provider's `Strategy` field (e.g., `AZURE_AD`, `OKTA`, `SAML2`, `GENERIC_OIDC`). Resolved via Shell.Factory.

```csharp
Task<Result<ExternalIdentity>> AuthenticateAsync(
    IdentityProvider provider, string credentials, CancellationToken ct);
```

### `IIdpAuthAdapter` (ACL Port — Infrastructure layer)

One implementation per IDP type. Registered in `IdpAuthAdapterFactorySetup`.

```csharp
string StrategyName { get; }  // matches IdpStrategyHint value
Task<Result<ExternalIdentity>> AuthenticateAsync(
    IdentityProvider provider, string credentials, CancellationToken ct);
```

---

## 5. Global vs. Tenant Parameterization

`IConfigurationProvider` merges parameters from two levels:

| Level | Scope | Precedence |
|---|---|---|
| Global default | `TenantId = NULL` (root) | Lower |
| Tenant override | `TenantId = <specific>` | Higher |

When a tenant has no explicit `AUTH_USE_EXTERNAL_IDP` override, the global default applies. This means a platform-wide default (e.g., Local auth) takes effect for all new tenants without manual setup, and individual tenants can override it independently.

---

## 6. In-Memory Cache

`IConfigurationProvider` is populated once at startup (or on explicit refresh) and serves subsequent lookups as O(1) dictionary reads. The resolver therefore adds **zero latency** per authentication request beyond the application-layer logic.

When the IdpPanel toggle fires `UpdateAuthModeCommand`, the command handler:
1. Updates the parameter in the database
2. Calls `IConfigurationProvider.RefreshAsync()` to re-populate the in-memory cache
3. The new method takes effect for the next login — no restart required

---

## 7. Development Stub

`StubIdpAuthAdapter` is registered in non-Production environments. It accepts any credential prefixed with `MOCK-` and returns a fabricated `ExternalIdentity` matching the UMS user by email suffix. This allows end-to-end IDP flow testing without a real external provider.

---

## 8. Relationship with the Authorization Graph

After the auth method resolves and the user is authenticated, `AuthorizationGraphBuilderService` constructs the full `AuthorizationGraph`. The `authentication` section of the graph records:
- The resolved method (`Local` or `IDP`)
- The provider name and strategy (when IDP)
- `mfaRequired`, `issuedAt`, `sessionExpiresAt`

See [Authorization Graph](./auth-graph.md) for the complete graph structure.

---

## 9. Error Codes

| Code | Trigger |
|---|---|
| AUTH_001 | Validation error — required fields missing |
| AUTH_002 | Tenant not found |
| AUTH_003 | Tenant not active |
| AUTH_004 | IDP user has no matching UMS `UserAccount` |
| AUTH_005 | `UserAccount.Status != ACTIVE` |
| AUTH_006 | Invalid credentials (Local BCrypt) |
| AUTH_011 | `AUTH_USE_EXTERNAL_IDP = true` but no active IDP configured |
| AUTH_012 | No `IIdpAuthAdapter` registered for the provider's strategy name |

---

## 10. References

- [ADR-0072: Dynamic Auth Method Resolution](../../architecture/adrs/0072-dynamic-auth-method-resolution.md)
- [ADR-0071: Auth Graph Engine](../../architecture/adrs/0071-auth-graph-engine.md)
- [Authorization Graph](./auth-graph.md)
- [Tenant Aggregate](./tenant.md)
- [`AuthMethodResolverService`](../../../src/apps/ums.api/Ums.Application/Identity/Auth/AuthMethodResolverService.cs)
- [`IdpAuthAdapterFactorySetup`](../../../src/apps/ums.api/Ums.Infrastructure/Identity/Auth/IdpAuthAdapterFactorySetup.cs)
