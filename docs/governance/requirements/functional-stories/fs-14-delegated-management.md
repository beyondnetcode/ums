# Functional Story 14: Delegate User Management Between Administrators

## 1. Business Purpose

Organizations need to delegate user management without giving unrestricted administrative power. UMS must let an administrator assign limited management responsibility while preserving ownership, scope, and revocability.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Delegating Administrator** | Grants a limited management scope to another administrator. |
| **Receiving Administrator** | Manages only the delegated users and scope. |
| **Superior Administrator** | Can supervise or revoke delegated authority. | ## 3. Business Preconditions

- Both administrators belong to an authorized tenant context.
- The delegating administrator owns or controls the users and scope being delegated.
- The receiving administrator is eligible to manage users.

## 4. Main Functional Flow

1. The delegating administrator selects the users to delegate.
2. The delegating administrator selects the receiving administrator.
3. The delegating administrator defines the scope and optional time limit.
4. The system validates that the delegation does not exceed the delegator's own authority.
5. The system activates the delegation.
6. The receiving administrator can manage delegated users only within the approved scope.

## 5. Alternative Flows and Exceptions

### A. Scope Not Owned

If the delegating administrator attempts to delegate authority they do not own, the system blocks the operation.

### B. Circular or Unsafe Delegation

If the delegation creates a circular management chain or privilege escalation, the system rejects it.

## 6. Business Rules

1. No administrator can delegate authority they do not have.
2. Delegation must be limited by user pool, system/suite, tenant, or time when applicable.
3. Delegation must be revocable.
4. Multiple administrators may manage the same user only when explicitly delegated.

## 7. Acceptance Criteria

1. Delegated authority is limited to the approved users and scope.
2. Delegation can be revoked by the delegator or a superior administrator.
3. Circular delegation is prevented.
4. Every delegation change is auditable.

## 8. Technical Requirements

- Enforce recursive scope validation logic at application level.
- Support audit tracking on delegation lifecycle events.

## 9. Traceability

- Entities: `UserManagementDelegation` (Deferred AR), `UserAccount` (AR)
- ADRs: ADR-0038, ADR-0044
- Related Stories: FS-10, FS-12
