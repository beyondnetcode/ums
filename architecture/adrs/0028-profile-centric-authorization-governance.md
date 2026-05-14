# ADR 0028: Role-Scoped Master Templates and Deep Hierarchy Governance

## Status
Refactored (Role-Scoped Governance)

## Context
To ensure absolute functional integrity, permission templates must not exist as disconnected blueprints. They must be strictly scoped to a **Role** within a **System**, ensuring that authority is always defined within a valid organizational and functional context.

## Decision
We will implement **Role-Scoped Template Governance** with a deep functional hierarchy:

1.  **Strict Relationship Chain**:
    *   `System (1:N) Role (1:N) PermissionTemplate (1:N) ProfilePermission`.
    *   Templates are now an extension of the Role's authority definition.

2.  **Deep Functional Hierarchy**:
    *   The model explicitly supports authorization at 6 levels:
        1. **System/Suite**
        2. **FunctionalModule**
        3. **Menu/ItemMenu**
        4. **SubMenu**
        5. **FunctionalOption**
        6. **Action** (View, Create, etc.)

3.  **Scoped Action Ownership**:
    *   Actions MUST belong to a **System** or **Module**. No orphans.
    *   Actions are reused across templates within the Role's context.

4.  **Effective Materialization**:
    *   `ProfilePermission` is a materialized link to a **Role-Scoped Template**, supporting explicit `IsAllowed/IsDenied` overrides.

## Technical Implementation
*   **PermissionTemplate**: Foreign Key to `RoleId`.
*   **Hierarchy Mapping**: Resource identification supports recursive menu/option structures.
*   **Audit**: Mandatory 10-column corporate schema for all entities.

## Consequences
*   **Positive**: Eliminates disconnected authority sets, ensures roles are the primary governance pivot, and supports extremely granular enterprise requirements.
*   **Negative**: Stricter data entry requirements (Templates must be created per Role).
