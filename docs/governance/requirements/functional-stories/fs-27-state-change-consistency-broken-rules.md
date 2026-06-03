# Functional Story 27: State-Change Consistency — Broken Rules on Active Dependencies

## 1. Business Purpose

Administrative operations that change entity state (suspend, block, deactivate, delete) can leave orphaned active data if executed while dependencies are still active. Administrators need clear, actionable feedback explaining what must be resolved before a state change is allowed, rather than receiving a generic error or silent failure.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **System Administrator** | Manages tenant and user lifecycle. |
| **Security Administrator** | Manages role, template, and domain resource lifecycle. |

## 3. Business Preconditions

- The actor is authenticated in the UMS portal with the appropriate administrative role.
- The target entity exists and is in a state that allows the attempted operation.

## 4. Main Functional Flow

1. The actor attempts a state-change operation (e.g., suspend tenant, block user).
2. The system checks for active blocking dependencies before executing the change.
3. If blocking dependencies exist, the system rejects the operation with HTTP 409 Conflict.
4. The system returns a structured response with the error code, a human-readable message, the broken business rule, and the list of blocking dependencies with entity type, status, and count.
5. The actor resolves each blocking dependency (e.g., deactivates users, removes profiles).
6. The actor retries the operation — it succeeds once all dependencies are resolved.

## 5. Alternative Flows and Exceptions

### A. No Blocking Dependencies

The state change proceeds normally and returns the expected success status (204 No Content).

### B. Multiple Blocking Dependency Types

The response lists all blocking dependency groups, each with its own `entityType`, `status`, and `count`.

## 6. Business Rules

1. A tenant cannot be suspended while it has active users, branches, or identity providers.
2. A user cannot be blocked or deleted while they have active profiles.
3. A role cannot be deactivated while it has active profiles or active child roles.
4. A permission template cannot be deprecated while active profiles reference it.
5. A domain resource cannot be removed while active permission template items reference it.
6. A module cannot be removed while active menus are configured within it.

## 7. Acceptance Criteria

1. Attempting a guarded state change with active dependencies returns HTTP 409.
2. The 409 response body contains `errorCode`, `message`, `brokenRule`, and `blockingDependencies[]`.
3. Each `blockingDependencies` entry contains `entityType`, `status`, and `count`.
4. After resolving all blocking dependencies, the same operation succeeds.
5. Operations with no blocking dependencies are not affected by the guard.

## 8. Technical Requirements

- Guard logic resides in application command handlers (not in the domain aggregate or UI).
- Error encoding: `BlockedOperationError.Encode(errorCode, deps)` with `|` separator.
- HTTP response: 409 Conflict with `BlockedOperationResponse` payload.
- Error codes defined in `DomainErrors`; messages mapped in `BlockedOperationMessages`.

## 9. Traceability

- Entities: `TENANT`, `USER_ACCOUNT`, `PROFILE`, `ROLE`, `PERMISSION_TEMPLATE`, `DOMAIN_RESOURCE`, `MODULE`
- ADRs: [ADR-0079](../../../architecture/adrs/0079-dependency-guard-policy.md)
