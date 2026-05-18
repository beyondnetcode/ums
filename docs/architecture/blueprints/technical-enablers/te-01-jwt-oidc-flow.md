# TE-01: JWT / OIDC Authentication Flow

| Field | Value |
|-------|-------|
| **TE ID** | TE-01 |
| **Status** | Approved |
| **ADR Reference** | ADR-0020 (IdP Abstraction), ADR-0026 (MFA Adaptive) |
| **Satisfies** | FS-01 (User Authentication), FS-08 (Hosted Login Redirection), FS-09 (MFA / Passwordless Adaptive Auth) |
| **Owner** | Platform Team |
| **Date** | 2026-05-18 |

---

## Problem

UMS must authenticate users from multiple identity providers (internal bcrypt, Zitadel, Azure AD, Okta, SAML2, OIDC) without coupling the domain or application layer to any specific SDK. Additionally, MFA enforcement must adapt per tenant policy without changing the core authentication flow.

## Solution: Injectable IdP Port + Token Normalization

The `IAuthenticationPort` (Strategy Pattern) abstracts every IdP behind a single domain interface. The port contract returns a normalized `AuthenticationResult` carrying a standard JWT regardless of the upstream provider. The gateway handles protocol translation (SAML → OIDC, LDAP → internal token) before reaching application logic.

```
User Agent
    │
    ▼
┌──────────────────────────────────────────────────────┐
│  Auth Gateway (Reverse Proxy / BFF)                   │
│  - Routes by tenant domain hint (IdP routing table)   │
│  - Handles hosted login page (BC-C config)            │
│  - Calls selected IdP adapter                         │
└──────────────┬───────────────────────────────────────┘
               │  IAuthenticationPort.AuthenticateAsync()
               ▼
┌─────────────────────────────────────┐
│  IdP Adapter (selected at runtime)  │
│  Internal │ Zitadel │ Azure │ SAML  │
└──────────────────┬──────────────────┘
                   │  AuthenticationResult { UserId, TenantId, Claims }
                   ▼
┌──────────────────────────────────────┐
│  Token Service (UMS Core API)        │
│  - Validate result                   │
│  - Enforce MFA policy (FS-09)        │
│  - Issue signed JWT (RS256)          │
│  - Raise AuthenticationAttemptedEvent│
└──────────────────────────────────────┘
```

## JWT Structure

| Claim | Value | Notes |
|-------|-------|-------|
| `sub` | `UserAccountId` (Guid) | UMS principal ID |
| `tid` | `TenantId` (Guid) | Root tenant scope |
| `bid` | `BranchId` (Guid, nullable) | Branch scope for BRANCH_SCOPED profiles |
| `cat` | `UserCategory` | `INTERNAL / EXTERNAL / SERVICE_ACCOUNT` |
| `idp` | IdP strategy name | e.g., `ZITADEL`, `INTERNAL_BCRYPT` |
| `exp` | Unix timestamp | Configurable per tenant via BC-C |
| `jti` | UUID | For replay prevention |

## MFA Adaptive Enforcement (FS-09)

MFA is evaluated **after** primary authentication succeeds, before JWT issuance:

1. Load tenant MFA policy from BC-C (`IConfigCachePort`).
2. Check `UserAccount.MfaEnrollments` for an enrolled and verified method.
3. If policy = `REQUIRED` and no verified enrollment → return `MFA_ENROLLMENT_REQUIRED`.
4. If policy = `CONDITIONAL` and risk score ≥ threshold → challenge with enrolled method.
5. On MFA success → raise `MfaVerifiedEvent`.

## Port Contract

```csharp
public interface IAuthenticationPort
{
    Task<AuthenticationResult> AuthenticateAsync(
        AuthenticationRequest request,
        CancellationToken cancellationToken = default);
}

public record AuthenticationRequest(
    string IdpStrategy,
    string Identifier,
    string? Credential,
    Guid TenantId);

public record AuthenticationResult(
    bool Success,
    Guid? UserId,
    string? FailureReason,
    IReadOnlyDictionary<string, string> Claims);
```

## Implementation Sequence

1. Infrastructure registers adapters via DI: `services.AddIdpAdapter<ZitadelAdapter>("ZITADEL")`.
2. `IAuthenticationPort` implementation resolves the correct adapter from the tenant's active `IdentityProvider` strategy.
3. Token service validates result, checks MFA, and returns a signed JWT.
4. All outcomes (success and failure) are recorded via `AuthenticationAttemptedEvent` → Audit context.

## Coverage Gaps

| Gap | Status |
|-----|--------|
| `SERVICE_ACCOUNT` client_credentials flow (V6 in design-decisions.md) | Pending design decision |
| Passwordless magic-link fallback | Defined in `BrandingSettings.MagicLinkFallback`; adapter pending |

---

**[Technical Enablers Index](./index.md)** | **[Traceability Matrix](../../traceability-matrix.md)**
