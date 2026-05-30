# Functional Story 19: Admin Password Reset and User Validity Period Management

## 1. Business Purpose

UMS must allow authorized administrators to reset user passwords and modify user account validity periods, with scope restrictions enforced by the administrator's type (internal vs. tenant-specific) and their operational scope. All such actions must be recorded in the audit trail to ensure accountability and compliance with security policies.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Internal Platform Administrator** | Manages passwords and validity periods for users across any tenant in the system. Has cross-tenant operational scope. |
| **Tenant Administrator** | Manages passwords and validity periods for users within their own tenant only. Has tenant-scoped operational scope. |
| **Affected User** | Subject of the password reset or validity period modification. Receives notification of the action taken. |

## 3. Business Preconditions

- The administrator performing the action is authenticated and holds an ADMIN role in their operational scope.
- The target user account exists and belongs to a tenant within the administrator's operational scope.
- The administrator has the required permission (`CAN_RESET_PASSWORD` and/or `CAN_MODIFY_VALIDITY_PERIOD`) assigned to their role.
- The feature flags controlling these capabilities are enabled for the system or tenant.

## 4. Main Functional Flow

### 4.1 Password Reset Flow

1. The administrator navigates to the user management section and selects the target user.
2. UMS displays the user's account details including current credential status.
3. The administrator selects the "Reset Password" action.
4. UMS validates that the administrator has permission and that the target user is within their scope.
5. The administrator confirms the action, optionally specifying a temporary password or requesting automatic generation.
6. UMS generates a new temporary password, hashes it, and stores it as the active credential.
7. UMS marks any previous active credential as historical (retained for audit).
8. UMS records the audit entry with all required details.
9. UMS sends a notification to the affected user (via configured channel) with instructions to change the temporary password.
10. UMS confirms to the administrator that the password was reset successfully.

### 4.2 Validity Period Modification Flow

1. The administrator navigates to the user management section and selects the target user.
2. UMS displays the user's account details including current validity period (created, expires, last activity).
3. The administrator selects the "Modify Validity Period" action.
4. UMS validates that the administrator has permission and that the target user is within their scope.
5. The administrator specifies the new validity parameters (extension, reduction, or explicit expiration date).
6. UMS validates the new period against system rules (e.g., cannot exceed maximum allowed duration).
7. UMS updates the user account's validity period.
8. UMS records the audit entry with all required details including the previous and new values.
9. UMS notifies the affected user if the validity period was reduced or the account was deactivated.
10. UMS confirms to the administrator that the validity period was updated.

## 5. Alternative Flows and Exceptions

### A. Cross-Tenant Operation Attempt (Tenant Admin)

If a tenant-specific administrator attempts to reset a password or modify validity for a user belonging to a different tenant, UMS rejects the operation and returns an access denied message explaining that the action is outside their operational scope.

### B. User Not in Scope

If the target user is not within the administrator's scope (e.g., user belongs to a tenant the admin cannot access), UMS does not reveal the user's existence and returns a generic "user not found or access denied" message.

### C. Permission Not Held

If the administrator lacks the required permission (`CAN_RESET_PASSWORD` or `CAN_MODIFY_VALIDITY_PERIOD`), UMS does not display the respective action button and returns authorization error if the API is called directly.

### D. Feature Disabled

If the feature flag controlling the action is disabled system-wide or for the specific tenant, UMS hides the action and returns a "feature not available" message.

### E. User is Federated

If the target user authenticates via an external identity provider, the password reset action is not applicable and UMS redirects the administrator to the external provider's management interface.

### F. Maximum Validity Exceeded

If the requested validity period exceeds the maximum allowed duration configured in the system, UMS rejects the change and explains the maximum allowable duration.

### G. Operation Failure

If the operation cannot be completed due to a system error, UMS shows a clear reason when available and a support error identifier for traceability.

## 6. Business Rules

1. **Internal ADMIN scope**: Users with the internal ADMIN role (belonging to `INTERNAL_ADMIN` tenant) can perform password resets and validity period modifications on any user account in the system.
2. **Tenant ADMIN scope**: Users with tenant-specific ADMIN roles can only perform these actions on users within their own tenant.
3. **Permission requirements**: The administrator must have `CAN_RESET_PASSWORD` permission to reset passwords, and `CAN_MODIFY_VALIDITY_PERIOD` permission to modify validity periods. These permissions are assigned via role-based authorization.
4. **Audit requirement**: Every password reset and validity period modification MUST generate an immutable audit record.
5. **Cross-tenant denial**: Tenant-specific administrators MUST NOT be able to view, modify, or administer users from other tenants.
6. **No secret exposure**: Password values (temporary or permanent) MUST NEVER be displayed in the UI or included in operational messages.
7. **Notification requirement**: Affected users MUST be notified when their password is reset or validity period is modified.
8. **Configurable rules**: Maximum validity period, minimum password length, and other configurable parameters MUST be read from system configuration, not hardcoded.

## 7. Acceptance Criteria

1. An internal ADMIN can reset passwords for users in any tenant.
2. A tenant ADMIN can reset passwords only for users in their own tenant.
3. An internal ADMIN can modify validity periods for users in any tenant.
4. A tenant ADMIN can modify validity periods only for users in their own tenant.
5. Password reset operations generate an audit record with: administrator ID, affected user ID, tenant ID, timestamp, and operation type (`PASSWORD_RESET`).
6. Validity period modifications generate an audit record with: administrator ID, affected user ID, tenant ID, previous validity values, new validity values, timestamp, and operation type (`VALIDITY_PERIOD_MODIFIED`).
7. The UI does not display action buttons for operations the administrator lacks permission to perform.
8. The system rejects cross-tenant operations attempted by tenant-specific administrators with an appropriate error message.
9. Federated users cannot have their passwords reset via UMS (local password not applicable).
10. All configuration parameters (max validity period, password rules, etc.) are read from system configuration.
11. The system hides or disables actions when the corresponding feature flag is disabled.

## 8. Technical Requirements

### 8.1 Authorization

- The `ITenantContext.IsInternalAdmin` flag determines if an admin has cross-tenant scope.
- The `ITenantContext.OrganizationId` determines the tenant scope for non-internal admins.
- Authorization checks in handlers must validate:
  - `IsInternalAdmin == true` OR `targetUser.TenantId == OrganizationId`
  - Admin role has required permissions (`CAN_RESET_PASSWORD`, `CAN_MODIFY_VALIDITY_PERIOD`)
- Feature flags for these capabilities are stored in `FeatureFlags` table with keys `ALLOW_PASSWORD_RESET_BY_ADMIN` and `ALLOW_VALIDITY_PERIOD_MODIFICATION`.

### 8.2 Commands

| Command | Endpoint | Description |
|---|---|---|
| `ResetUserPasswordCommand` | `POST /user-accounts/{userAccountId}/passwords/reset` | Reset password for target user |
| `ModifyUserValidityPeriodCommand` | `PATCH /user-accounts/{userAccountId}/validity` | Modify validity period |

### 8.3 Audit Events

| Event | Fields |
|---|---|
| `PASSWORD_RESET` | `adminUserId`, `targetUserId`, `targetTenantId`, `timestamp`, `reason`, `effectiveImmediately` |
| `VALIDITY_PERIOD_MODIFIED` | `adminUserId`, `targetUserId`, `targetTenantId`, `timestamp`, `previousExpiresAt`, `newExpiresAt`, `reason` |

### 8.4 Configuration (Configurable Parameters)

| Parameter | Config Location | Default |
|---|---|---|
| `MAX_VALIDITY_PERIOD_DAYS` | `AppConfiguration` | 365 |
| `MIN_PASSWORD_LENGTH` | `AppConfiguration` | 12 |
| `PASSWORD_RESET_NOTIFICATION_CHANNEL` | `AppConfiguration` | email |
| `ALLOW_PASSWORD_RESET_BY_ADMIN` | `FeatureFlag` | true |
| `ALLOW_VALIDITY_PERIOD_MODIFICATION` | `FeatureFlag` | true |

### 8.5 Data Model

- `UserAccount` aggregate includes `ValidityPeriod` value object with `CreatedAt`, `ExpiresAt`, `LastActivityAt`, `IsActive`.
- `PasswordCredential` is an owned entity with `IsActive`, `CreatedAt`, `Historical` flag.
- Audit records stored in `AuditRecords` table with `OperationType`, `ActorId`, `TargetId`, `TargetType`, `TenantId`, `Timestamp`, `Details` (JSON).

### 8.6 Error Codes

| Code | Description |
|---|---|
| `AUTH_009` | Administrator lacks required permission |
| `AUTH_010` | Target user outside administrator's scope |
| `USER_015` | Federated user cannot have local password reset |
| `CONFIG_003` | Requested validity period exceeds maximum allowed |

## 9. Traceability

- Entities: `UserAccount`, `PasswordCredential`, `AuditRecord`, `FeatureFlag`, `AppConfiguration`
- Related stories: FS-01 (user authentication), FS-03 (register organization), FS-18 (manage local password)
- Related ADR: ADR-0012 role-based access control, ADR-0019 audit trail requirements
- Diagram update: `docs/domain/identity/user-account.md` - add validity period management, `docs/governance/audit/audit-events.md` - add new event types