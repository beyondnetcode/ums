# ADR 0029: Delegated Administration and Approval Workflows

## Status
Approved

## Context
As the enterprise scales, Tenant Administrators cannot manually manage every user and profile assignment. Furthermore, the introduction of external identities (B2B, EXTRANET, PARTNER) poses a significant security risk if onboarded without oversight. The system requires a mechanism for administrators to securely delegate management tasks and enforce mandatory approvals for high-risk identity operations.

## Decision
We will implement an **Identity Governance & Approvals** framework integrated directly into the authorization model:

1.  **Hierarchical Delegated Administration**:
    *   The `USER` entity will include a self-referencing `ManagedByUserId` column.
    *   **Rule**: A delegated administrator can only manage users within their hierarchical subtree.
    *   **Principle of Least Privilege**: When an administrator assigns a `Profile` (Role) to a managed user, the system must verify that the administrator's own effective permissions (`ProfilePermission`) equal or exceed the permissions being granted. A manager cannot grant authority they do not possess.

2.  **User Categorization**:
    *   Users are strictly categorized (`INTERNAL`, `EXTERNAL`, `B2B`, `PARTNER`, etc.) via the `UserCategory` attribute.

3.  **Configurable Approval Workflows**:
    *   A dynamic approval engine is introduced (`APPROVAL_WORKFLOW`, `APPROVAL_REQUEST`, `APPROVAL_LOG`).
    *   Workflows can be configured per `TenantId`, `SuiteId`, or `TargetUserCategory`.
    *   **Dual-Scope Approvals**:
        *   *Onboarding Scope*: Activating a user (e.g., an `EXTERNAL` user requires HR approval before status becomes `ACTIVE`).
        *   *Assignment Scope*: Granting a sensitive `Profile` (e.g., granting "System Admin" requires dual approval).

4.  **Immutable Audit Trail**:
    *   All approval steps (APPROVE, REJECT) and delegation changes are logged immutably in `APPROVAL_LOG` using the standard corporate 10-column schema.

## Consequences
*   **Positive**: Dramatically reduces operational bottleneck on root Tenant Administrators. Ensures compliance with SOX/ISO27001 by mandating approval trails for B2B/External access. Enforces the Principle of Least Privilege dynamically.
*   **Negative**: Increases complexity in the onboarding API and User Management UI, as they must handle `PENDING` states and workflow triggers.
