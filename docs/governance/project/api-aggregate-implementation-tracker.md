# API Aggregate Implementation Tracker

This document captures the current implementation status of the UMS API by aggregate so execution can resume without rebuilding context.

## 1. Current Summary

| Aggregate | Domain | Application | REST | GraphQL Queries | SQL Server Persistence | Status |
| :--- | :--- | :--- | :--- | :--- | :--- | :--- |
| Tenant | Yes | Yes | Yes | Yes | Yes | Partial |
| UserAccount | Yes | Yes | Yes | Yes | Yes | Partial |
| Profile | Yes | Yes | Yes | Yes | Yes | Partial |
| SystemSuite | Yes | Yes | Yes | Yes | No | Partial |
| PermissionTemplate | Yes | Yes | Yes | Yes | No | Partial |
| ApprovalWorkflow | Yes | Yes | Yes | Yes | No | Partial |
| ApprovalRequest | Yes | Yes | Yes | Yes | No | Partial |
| DocumentType | Yes | Yes | Yes | Yes | No | Partial |
| UserDocument | Yes | Yes | Yes | Yes | No | Partial |
| AccessEnforcementPolicy | Yes | Yes | Yes | Yes | No | Partial |
| NotificationRule | Yes | Yes | Yes | Yes | No | Partial |
| PromotionRequest | Yes | Yes | Yes | Yes | No | Partial |
| RoleMaturityStatus | Yes | Yes | Yes | Yes | No | Partial |
| AuditRecord | Yes | Yes | Yes | Yes | No | Partial |
| AppConfiguration | Yes | No | No | No | No | Missing |
| FeatureFlag | Yes | No | No | No | No | Missing |
| IdpConfiguration | Yes | No | No | No | No | Missing |

## 2. Highest-Priority Gaps

1. Complete the Configuration context in the API:
   - `AppConfiguration`
   - `FeatureFlag`
   - `IdpConfiguration`
2. Finish domain behavior exposure for aggregates already present in REST and GraphQL:
   - `Profile`
   - `SystemSuite`
   - `PermissionTemplate`
   - `PromotionRequest`
   - `RoleMaturityStatus`
   - `DocumentType`
   - `UserDocument`
3. Move remaining in-memory repositories to SQL Server.
4. Extend transactional outbox coverage beyond the currently migrated SQL-backed aggregates.

## 3. Aggregate Follow-Up Detail

### 3.1 Identity

#### Tenant
- Missing API capabilities:
  - update tenant base data
  - edit branch data
  - edit identity provider data
- Persistence:
  - SQL Server implemented
- Recommended next step:
  - add update commands for tenant, branch, and identity provider maintenance

#### UserAccount
- Implemented API capabilities:
  - set or rotate a local password through server-side BCrypt hashing
  - expose active password status and latest rotation date without returning hashes
  - record authentication attempt
  - enroll MFA method (`POST /user-accounts/{id}/mfa-enrollments`)
  - verify MFA enrollment (`POST /user-accounts/{id}/mfa-enrollments/{id}/verify`)
  - list MFA enrollments (`GET /user-accounts/{id}/mfa-enrollments`)
  - revoke MFA enrollment (`DELETE /user-accounts/{id}/mfa-enrollments/{id}`)
  - adaptive MFA policy enforcement at login (`MfaRequiredForAdmin` config → `AUTH_011` if no verified enrollment)
- Persistence:
  - SQL Server implemented
- Implemented API capabilities (FS-19 additions):
  - modify validity period (`PATCH /user-accounts/{id}/validity`) — validates against `MAX_VALIDITY_PERIOD_DAYS`, tenant scope enforced, raises `ValidityPeriodModifiedEvent`
  - force password reset now sends notification to affected user
  - `UserAccountDto` exposes `ExpiresAt`
- Persistence:
  - `ExpiresAtUtc` column added via migration `Fs19UserAccountValidityPeriod`
- Recommended next step:
  - no further gaps for this aggregate

### 3.2 Authorization

#### Profile
- Missing API capabilities:
  - assign template
  - override permission allow/deny/neutral
  - activate or deactivate individual permissions
- Persistence:
  - SQL Server implemented

#### SystemSuite
- Missing API capabilities:
  - module lifecycle
  - menu lifecycle
  - submenu lifecycle
  - option lifecycle
  - action lifecycle
  - app setting lifecycle
- Persistence:
  - still in-memory

#### PermissionTemplate
- Missing API capabilities:
  - deprecate template
  - add item
  - update item decision state
  - activate or deactivate item
  - remove item
- Persistence:
  - still in-memory

### 3.3 Approvals

#### ApprovalWorkflow
- Missing API capabilities:
  - add required document
  - remove required document
- Persistence:
  - still in-memory

#### ApprovalRequest
- Missing API capabilities:
  - advanced lifecycle review if business requires expiration or cancellation
- Persistence:
  - still in-memory

#### DocumentType
- Missing API capabilities:
  - update document type
  - configure notification rules
  - remove notification rules
  - define enforcement policy
  - update enforcement policy
- Persistence:
  - still in-memory

#### UserDocument
- Missing API capabilities:
  - reject document
  - expire document
  - re-upload document
  - record notification sent
  - record enforcement executed
- Persistence:
  - still in-memory

#### AccessEnforcementPolicy
- Missing API capabilities:
  - update action
- Persistence:
  - still in-memory

#### NotificationRule
- Missing API capabilities:
  - update recipient or rule configuration
- Persistence:
  - still in-memory

### 3.4 IGA

#### PromotionRequest
- Missing API capabilities:
  - manager approve or reject
  - security low-risk or high-risk review
  - security approve or reject
  - execute
  - verify
  - mark verification failed
  - add impact analysis
- Persistence:
  - still in-memory

#### RoleMaturityStatus
- Missing API capabilities:
  - record certification completion
  - record training completion
  - update performance score
  - mark compliance issue
  - resolve compliance issue
  - review eligibility
- Persistence:
  - still in-memory

### 3.5 Audit

#### AuditRecord
- Missing implementation:
  - SQL Server repository
  - full outbox and immutable ledger alignment review

### 3.6 Configuration

#### AppConfiguration
- Missing implementation:
  - repositories
  - application commands and queries
  - REST endpoints
  - GraphQL queries
  - SQL Server persistence

#### FeatureFlag
- Missing implementation:
  - repositories
  - application commands and queries
  - REST endpoints
  - GraphQL queries
  - SQL Server persistence

#### IdpConfiguration
- Missing implementation:
  - repositories
  - application commands and queries
  - REST endpoints
  - GraphQL queries
  - SQL Server persistence

## 4. Recommended Next Order

1. Complete Configuration context in API
2. Finish `UserAccount` and `Profile` domain behavior exposure
3. Finish `SystemSuite` and `PermissionTemplate`
4. Finish `PromotionRequest` and `RoleMaturityStatus`
5. Finish remaining Approvals behaviors
6. Migrate remaining aggregates from in-memory to SQL Server

---
**Last update:** 2026-06-04 (FS-19)
