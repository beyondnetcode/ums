# ADR-0069: Authorization Graph Engine — Motor Central de Autenticación y Autorización

**Status:** Accepted
**Date:** 2026-05-31
**Decision Owner:** Architecture
**Related:**
- [ADR-0054: Shell Library Isolation](./0054-shell-library-isolation.md)
- [ADR-0061: Execution Context Accessor](./0061-execution-context-accessor.md)
- [ADR-0068: Feature Flag System Scope](./0068-feature-flag-system-scope.md)
- [ADR-0070: Dynamic Auth Method Resolution](./0070-dynamic-auth-method-resolution.md)

---

## Context

UMS's primary objective is to serve as the **central authentication and authorization engine** for multi-tenant client systems. Prior to this decision, the login endpoint (`POST /api/v1/auth/login`) returned an empty `Permissions: []` array and performed only BCrypt validation. Client systems received a JWT with no actionable authorization data, forcing them to implement their own permission logic or re-query UMS on every request.

The system needs to:
1. Authenticate users via Local (BCrypt) or external IDP depending on tenant configuration, while keeping portal management access local and the external API flow tenant-IDP aware
2. Build a complete, self-contained authorization graph from the user's profile
3. Return the graph to the client so it can operate autonomously without further UMS queries

---

## Decision

### 1. AuthorizationGraph — Immutable, Self-Contained Read Model

After authentication, UMS builds an `AuthorizationGraph` record containing:

```
AuthorizationGraph
├── context        — user, tenant, systemSuite, role, profile, branch
├── authentication — method (Local|IDP), provider, mfaRequired, expiry
├── actions[]      — all registered actions in the SystemSuite
├── menuAccess[]   — Module→Menu→SubMenu→Option tree with AccessEffect per option
├── domainPermissions[] — domain resources with effect per action (Aggregate/Entity)
├── featureFlags[] — flags evaluated against user context at auth-time
├── effectiveConfig — tenant-resolved parameters (session timeout, MFA, etc.)
├── scopes[]       — OAuth2-style "resourceCode.actionCode" from Allow permissions
├── generatedAt    — UTC timestamp
└── validUntil     — generatedAt + SESSION_TIMEOUT_MINUTES
```

The graph is **immutable** after construction. Client systems cache it for the session duration and make authorization decisions locally.

### 2. Permission Resolution — Deny-Wins, Override-Takes-Precedence

For each (TargetType, TargetId, ActionId) pair:
1. Find `ProfilePermission` where `IsActive = true`
2. If `IsOverride = true` → use PP values (Source = Override)
3. If `IsOverride = false` → use original TemplateItem values (Source = Template)
4. `IsDenied = true` → `AccessEffect.Deny` (always wins over Allow)
5. `IsAllowed = true` → `AccessEffect.Allow`
6. No entry → `AccessEffect.NotGranted` (implicit deny)

### 3. Two Dedicated Endpoints

- **`POST /api/v1/auth/login`** — existing endpoint, now returns the full graph. Sets session cookie for the web frontend (backward compatible). Returns `LoginSuccessResponse` enriched with `AuthorizationGraph`.
- **`POST /api/v1/client/authenticate`** — new, stateless endpoint for external client systems. No cookie. Returns `ClientAuthResponse` with JWT + serialized graph.

### 4. Dynamic Graph Serialization via Shell.Factory

The graph is serialized in the format configured by the tenant (`AUTH_GRAPH_DEFAULT_FORMAT` parameter, default: JSON). Supported formats: JSON, XML, YAML, CSV.

Clients can override via `?format=xml` query param or `Accept` header. New formats are added by registering an `IAuthorizationGraphSerializer` implementation in `AuthorizationGraphSerializerFactorySetup` — no application code changes needed.

---

## Rationale

### Why a self-contained graph instead of on-demand queries

Client systems need to check permissions multiple times per request (menu rendering, button visibility, API access guards). A self-contained graph eliminates N round-trips to UMS per session. The graph is valid for `SESSION_TIMEOUT_MINUTES` — the same duration as the session token.

### Why the graph is built from Profile, not from JWT claims

JWT claims have a size limit and are static after issuance. The graph is rich (hundreds of permissions), needs to include dynamic data (feature flags), and must reflect the state at auth-time. The graph is serialized separately and embedded in the response body, not in the JWT (the JWT only carries compact summary claims).

### Why AccessEffect not PermissionEffect

`Ums.Domain.Enums.PermissionEffect` already exists as a DomainEnumeration with `Allow` and `Deny`. The graph needs a third value `NotGranted` (implicit deny — no permission entry exists). A new `AccessEffect` enum in the Graph namespace avoids extending the existing enumeration.

---

## Consequences

### Positive

- Client systems receive everything they need in a single auth response
- Permission logic is centralized in UMS — client systems don't implement access rules
- Feature flag evaluations happen at auth-time with full user context
- Graph format is configurable per tenant and extensible without code changes
- Audit trail records every authentication event with method, result, and IP

### Trade-offs

- Graph size grows with the number of SystemSuite options and domain resources. For suites with >500 options, the response payload may be large. A read-model projection (future) can address this.
- The graph is a snapshot at auth-time. Permission changes take effect on next login. Clients with long-lived sessions may hold stale graphs.

---

## Implementation

| Component | Location |
|---|---|
| `AuthorizationGraph` record | `Ums.Domain/Authorization/Graph/` |
| `IAuthorizationGraphBuilder` port | `Ums.Domain/Authorization/Graph/IAuthorizationGraphBuilder.cs` |
| `AuthorizationGraphBuilderService` | `Ums.Application/Authorization/Graph/AuthorizationGraphBuilderService.cs` |
| `IAuthorizationGraphSerializer` | `Ums.Application/Authorization/Graph/Serializers/` |
| JSON/XML/YAML/CSV serializers | `Ums.Infrastructure/Authorization/Graph/` |
| `AuthorizationGraphSerializerFactorySetup` | `Ums.Infrastructure/Authorization/Graph/` |
| `AuthGraphFormatProvider` | `Ums.Application/Authorization/Graph/AuthGraphFormatProvider.cs` |
| `POST /api/v1/auth/login` | `Ums.Presentation/Endpoints/Identity/Auth/AuthEndpoints.cs` |
| `POST /api/v1/client/authenticate` | `Ums.Presentation/Endpoints/Identity/Auth/ClientAuthEndpoints.cs` |

---

**[ADR Registry](./index.md)**
