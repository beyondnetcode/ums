# ADR 0028: Profile-Centric Authorization and Governance Model

## Status
Proposed

## Context
The previous authorization model relied on a direct association between Users, Roles, and Permissions. This led to conceptual inconsistencies where permission overrides for a specific user required creating a new role or managing ad-hoc assignments, complicating governance and audit. There is a need for a central entity that encapsulates the entire context of an authorization (User, Role, System, Branch, and Tenant) and serves as the container for "Effective Permissions".

## Decision
We will transition to a **Profile-Centric Authorization Model**.

1.  **Profile as the Governance Axis**:
    *   A **Profile** is the atomic unit of authorization.
    *   It is a contextual composition of: `UserId` + `RoleId` + `SystemId` + `BranchId` + `TenantId`.
    *   A user can have multiple profiles (e.g., "Manager in Branch A" and "Auditor in Branch B").

2.  **Effective Permission Persistence**:
    *   Instead of resolving permissions on-the-fly from roles, the final set of authorizations will be persisted at the **Profile** level in a `ProfilePermissions` table.
    *   This allows for granular **Overrides** (adding or removing permissions) for a specific profile without affecting other users sharing the same Role.

3.  **Traceability and Audit Standards**:
    *   All authorization changes must be tracked with a `TransactionId` or `AuditId`.
    *   Every entity in the system will follow the **Standard Corporate Audit Schema** (Created, Updated, Deleted, Version, Status).

4.  **Decoupling**:
    *   **Role**: A blueprint of permissions.
    *   **Profile**: A contextual implementation of a Role for a specific User and Branch.

## Technical Implementation
*   The `Users` table will no longer be directly linked to `Roles`.
*   The `Profiles` table becomes the central junction point.
*   The `Authorization Engine` will resolve permissions by querying the **Active Profile ID** in the execution context.

## Consequences
*   **Positive**: Perfect granularity for overrides, simplified audit (one place to see what a user can do in a specific context), and better alignment with complex organizational structures.
*   **Negative**: Higher number of rows in the permission tables (`ProfilePermissions`) and the need for logic to keep profiles in sync with their source roles/templates when required.
