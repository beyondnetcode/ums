# Functional Story 26: Preview Auth Graph from Profile Maintenance

## 1. Business Purpose

Authorization administrators need to verify that a profile's permission configuration will produce the correct auth graph before the profile is used in live authentication. Without this capability, misconfigured profiles can only be detected after a real user attempts to authenticate and receives incorrect access.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Security Administrator** | Verifies profile auth graph before activating or assigning a profile. |
| **SRE / Support Engineer** | Diagnoses permission issues for a specific profile. |

## 3. Business Preconditions

- The actor is authenticated in the UMS portal.
- The target profile exists.
- The actor has diagnostic access to profile data.

## 4. Main Functional Flow

1. The actor opens a profile in the Profile Maintenance section.
2. The actor triggers the auth graph preview.
3. The system calls `GET /api/v1/profiles/{profileId}/auth-graph/preview`.
4. The system builds the auth graph using the same pipeline as a real authentication.
5. The system returns the graph with metadata (format, requestId, tenantCode, authMethodUsed).
6. The actor inspects the graph to verify expected permissions.

## 5. Alternative Flows and Exceptions

### A. Unauthenticated Access

Requests without a valid portal session return HTTP 401 Unauthorized.

### B. Profile Not Found

If the profile ID does not exist, the system returns HTTP 400 with an error message.

## 6. Business Rules

1. The preview must use the same `IAuthorizationGraphBuilder` pipeline as live authentication — no separate or simplified builder.
2. Credential validation is skipped; the profile is resolved directly by ID.
3. The preview emits audit event `Graph.Preview.Internal`, not `Auth.Success`.
4. The preview does not modify any permissions or profile state.

## 7. Acceptance Criteria

1. Authenticated administrators can retrieve the auth graph preview for any accessible profile.
2. The response contains `previewMode: "internal-preview"`, `format`, `graph`, `requestId`, `profileId`, `tenantId`.
3. The response headers include `X-Preview-Mode: internal-preview` and `X-Request-Id`.
4. Unauthenticated requests return HTTP 401.
5. The preview graph is identical to what live authentication would produce for the same profile.

## 8. Technical Requirements

- Endpoint: `GET /api/v1/profiles/{profileId}/auth-graph/preview[?format=JSON|CBOR|...]`
- Handler: `PreviewProfileAuthGraphCommandHandler` using injected `IAuthorizationGraphBuilder`.
- Format override supported via `?format=` query parameter.
- Access guard: `IUserContext.IsAuthenticated` check at the endpoint.

## 9. Traceability

- Entities: `PROFILE`, `AUTH_GRAPH`
- ADRs: [ADR-0071](../../architecture/adrs/0071-auth-graph-engine.md), [ADR-0080](../../architecture/adrs/0080-auth-graph-preview-internal-pipeline.md)
- Related stories: FS-07
