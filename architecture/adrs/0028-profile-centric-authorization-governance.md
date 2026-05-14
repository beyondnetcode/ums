# ADR 0028: Master-Template Driven Authorization and Scoped Action Governance

## Status
Refactored (Hierarchical Action Ownership)

## Context
Previous iterations allowed for actions that could potentially exist outside a clear functional context. To ensure enterprise-grade governance, every authorized action must be strictly scoped to either a **System** (Global actions) or a **Functional Module** (Specific actions).

## Decision
We will implement **Scoped Action Governance** within the Master-Template Framework:

1.  **Strict Action Ownership**:
    *   Every `ACTION` must belong to a `SYSTEM_SUITE` (Global) or a `FUNCTIONAL_MODULE` (Specific).
    *   Orphaned actions (without a system or module parent) are strictly prohibited.

2.  **Resolution Path**:
    *   Authorization follows a mandatory hierarchical path:
        `Tenant -> System -> Module -> Resource -> Action -> Template -> ProfilePermission`.

3.  **Template Junction**:
    *   `PermissionTemplate` acts as the authorized junction between a **Resource** (System, Module, Menu, SubMenu, Option) and a **Scoped Action**.

4.  **No Effective Drift**:
    *   `ProfilePermission` is a materialized instance of a `PermissionTemplate`.
    *   Manual assignment of actions to profiles without a template is impossible by design.

## Technical Implementation
*   **Action Entity**: Implements `SystemId` and `ModuleId` (one must be non-null).
*   **Resource Mapping**: Templates map resources to scoped actions.
*   **Audit**: All ownership changes are fully audited with the 10-column corporate schema.

## Consequences
*   **Positive**: Eliminates functional ambiguity, ensures every permission has a clear owner, and simplifies auditing.
*   **Negative**: Slightly more complex schema due to the conditional ownership of actions.
