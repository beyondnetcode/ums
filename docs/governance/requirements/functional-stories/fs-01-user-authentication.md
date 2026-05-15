# Functional Story 1: Corporate User Authentication via External IdP

## 1. Business Purpose

Corporate users need to access client systems using their organization's trusted identity provider. UMS must validate that the user is recognized, active, and allowed to start a secure session without requiring a separate password managed by each application.

---

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Corporate User** | Attempts to sign in to a client system. |
| **External Identity Provider** | Confirms the user's corporate identity. |
| **UMS** | Validates the identity against active UMS records and establishes the application session. |
| **IT Administrator** | May use emergency access when the external provider is unavailable.
## 3. Business Preconditions

- The user exists in UMS and is linked to a valid corporate identity reference.
- The user's account is active.
- The organization has an identity provider configured.

---

## 4. Main Functional Flow

1. The user opens the client portal and selects corporate sign-in.
2. The user is redirected to the organization's trusted identity provider.
3. The user authenticates with corporate credentials.
4. The identity provider confirms the user's identity to UMS.
5. UMS verifies that the identity belongs to an active registered user.
6. UMS establishes the user's session and applies the correct tenant context.
7. The user enters the client application with the permissions assigned to their profiles.

---

## 5. Alternative Flows and Exceptions

### A. External Identity Provider Unavailable

If the external identity provider is unavailable, standard users are asked to retry later. Authorized IT administrators may use an emergency local access path when enabled by policy.

### B. Corporate Identity Not Linked or Inactive

If the external identity is valid but not linked to an active UMS user, the sign-in is rejected and a security warning is recorded.

### C. User Account Suspended

If the user exists but is suspended or terminated, the system blocks access and shows a clear account status message.

---

## 6. Business Rules

1. A corporate identity alone is not enough; the user must also be active in UMS.
2. Emergency access is limited to explicitly authorized IT administrators.
3. Every failed or blocked sign-in must be auditable.
4. The tenant context must be established before permissions are resolved.

---

## 7. Acceptance Criteria

1. An active linked user can sign in through the organization's identity provider.
2. A user without an active UMS record cannot access the application.
3. A suspended user cannot start a session.
4. Provider unavailability does not silently grant access.
5. Sign-in outcomes are visible for audit and support investigation.

---

## 8. Technical Requirements

- Support OAuth 2.0 Authorization Code with PKCE for external IdP authentication.
- Validate signed tokens and required identity claims.
- Match the external identity claim to the UMS identity reference.
- Establish a secure application session using HTTP-only, SameSite cookies or the approved session mechanism.
- Return an authorization failure when the identity is not linked or active.
- Emit immutable access audit events for failures and warnings.

---

## 9. Traceability

- Entities: `USER_ACCOUNT`, `IDP_CONFIGURATION`, `PROFILE`
- ADRs: ADR-0020, ADR-0022, ADR-0026
- Related Stories: FS-08, FS-09
