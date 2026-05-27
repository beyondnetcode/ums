# Functional Story 18: Manage a User Local Password

## 1. Business Purpose

UMS must allow an authorized administrator to establish or rotate a local password for a user whose authentication strategy is internal, so the user can access the platform securely when external identity federation is not used.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Identity Administrator** | Establishes or rotates a local password for an eligible user account. |
| **User** | Uses the local credential after receiving it through an approved secure channel. |

## 3. Business Preconditions

- The user account exists in the selected tenant.
- The account uses internal authentication and is not linked to an external identity provider.
- The administrator is allowed to manage user credentials.

## 4. Main Functional Flow

1. The administrator opens a user account and selects its Credentials view.
2. UMS indicates whether a local password is configured and the date of its latest rotation.
3. The administrator chooses to set or rotate the password.
4. The administrator enters and confirms a temporary password following the stated rule.
5. UMS saves the new active local credential and marks any previous credential as historical.
6. UMS confirms that the password was updated without displaying secret values.

## 5. Alternative Flows and Exceptions

### A. Federated Account

If the user authenticates through an external identity provider, UMS does not offer local password maintenance and directs the administrator to the configured provider.

### B. Invalid Temporary Password

If the password does not meet the stated minimum rule or confirmation differs, UMS explains the required correction before saving.

### C. Failure During Rotation

If the operation cannot be completed, UMS shows a clear reason when available and a support error identifier, without exposing technical details.

## 6. Business Rules

1. A federated user must not have an active local password.
2. A local user can have at most one active password.
3. Setting a new password deactivates the prior active credential and retains its history for audit.
4. Password secret values and protected representations are never displayed or included in operational messages.
5. A temporary password must contain at least 12 characters.

## 7. Acceptance Criteria

1. The administrator can see password management within the selected user's Credentials view.
2. For an internal account, the administrator can set or rotate a local password.
3. For a federated account, the UI explains that password management belongs to the external provider and does not allow local entry.
4. The UI displays only credential status and latest rotation date, never a password or protected value.
5. Validation and operation failures are human-readable and include a support error ID when returned by the API.

## 8. Technical Requirements

- `PasswordCredential` remains an owned entity of the `UserAccount` aggregate in the Identity bounded context.
- Commands are REST-first through `POST /user-accounts/{userAccountId}/passwords`; the request carries a plaintext temporary password over the secured transport and the API hashes it using BCrypt before persistence.
- Queries may expose `hasActivePassword` and `passwordUpdatedAtUtc`; `PasswordHash` and historical credential identifiers must not be exposed to the web client.
- SQL Server with EF Core persists credentials within the `UserAccount` transaction boundary.
- Application-layer tenant filtering remains primary; SQL Server RLS remains a secondary safeguard.
- Operation errors follow the safe error response standard and correlate Serilog/Loki diagnostics by `ErrorId`.
- The React view is localized in Spanish and English and validates the minimum password rule before issuing the command.

## 9. Traceability

- Entities: `UserAccount`, `PasswordCredential`, `IdentityProvider`
- Related stories: FS-01, FS-03, FS-08, FS-09
- Related ADR: ADR-0066 actionable user error contract
- Diagram update: `docs/domain/identity/password-credential.md`, entity lifecycle and UserAccount ownership, triggered by FS-18
