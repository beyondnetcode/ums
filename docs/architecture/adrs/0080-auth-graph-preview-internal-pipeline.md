# ADR-0080: Auth Graph Preview — Internal vs External Pipeline

**Status:** Accepted  
**Date:** 2026-06-03  
**Decision Owner:** Architecture  
**Related:**
- [ADR-0071: Auth Graph Engine](./0071-auth-graph-engine.md)
- [ADR-0072: Dynamic Auth Method Resolution](./0072-dynamic-auth-method-resolution.md)
- [ADR-0074: Auth Graph Schema Versioning](./0074-auth-graph-schema-versioning.md)

---

## Context

The UMS portal needs to let administrators preview the auth graph for a given profile without issuing a real authentication request. This is required for:

- Validating that a profile's permission template and role assignment produce the expected graph before assigning it to live users.
- Debugging access control issues during support sessions.

The naive approach would be a separate, simplified graph builder for the preview. However, building two independent graph builders means the preview can diverge from the real output, which defeats its diagnostic purpose.

---

## Decision

### Rule: preview uses the same `IAuthorizationGraphBuilder`

The internal preview endpoint `GET /api/v1/profiles/{id}/auth-graph/preview` calls `PreviewProfileAuthGraphCommandHandler`, which uses the same `IAuthorizationGraphBuilder` injection as the external `POST /api/v1/client/authenticate` flow.

There is **one** graph-building pipeline. The preview is distinguished from live authentication only by:

| Aspect | External auth (`POST /client/authenticate`) | Internal preview (`GET /profiles/{id}/auth-graph/preview`) |
|---|---|---|
| Credential validation | Yes — resolves and validates credentials | **Skipped** — profile is looked up directly by ID |
| Auth method resolution | Yes | Not needed — profile already exists |
| Audit event | `Auth.Success` / `Auth.Failure` | `Graph.Preview.Internal` |
| Response header | — | `X-Preview-Mode: internal-preview`, `X-Request-Id` |
| Access control | Public (token-based) | Requires authenticated portal session (`IUserContext.IsAuthenticated`) |

### What is preserved

- Graph format resolution (same `IAuthGraphFormatProvider` and `IFactory<..., IAuthorizationGraphSerializer>`).
- Format override via `?format=` query parameter — same logic as the external endpoint.
- Tenant parameters (`tenantId`, `tenantCode`, `authMethodUsed`) propagated from the profile.
- `requestId` field in the response for traceability.

### Endpoint

```
GET /api/v1/profiles/{profileId:guid}/auth-graph/preview[?format=JSON|CBOR|...]
Authorization: X-User-Id / X-Tenant-Id headers (portal session)
```

Response:
```json
{
  "format": "JSON",
  "graph": { ... },
  "requestId": "...",
  "previewMode": "internal-preview",
  "profileId": "...",
  "userId": "...",
  "tenantId": "...",
  "tenantCode": "...",
  "authMethodUsed": "..."
}
```

---

## Consequences

- **Positive:** Administrators see exactly what the external caller would see — no divergence risk.
- **Positive:** Single code path to maintain and test.
- **Negative:** Preview inherits the full graph-building cost; acceptable because it is an on-demand diagnostic tool, not a hot path.
- **Neutral:** Credential resolution is the only external-only step; removing it from the preview path does not affect graph correctness.
