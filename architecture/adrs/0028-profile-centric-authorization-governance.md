# ADR 0028: Master-Template Driven Authorization and Governance Model

## Status
Refactored (Master-Template Governance)

## Context
Standard authorization models often allow for "ad-hoc" permissions at the user or profile level, leading to governance drift and auditing nightmares. Enterprise environments require a single, immutable source of truth for all possible authority: the **Permission Template**.

## Decision
We will implement a **Master-Template Driven Authorization Framework** governed by the following immutable rules:

1.  **PermissionTemplate as the Master Source**:
    *   No permission can exist in a Profile if it hasn't been defined in the `PERMISSION_TEMPLATE` master catalog.
    *   Templates define the granular intersection of a **Resource** (Module/Menu/Option) and an **Action** (View/Create/Export/etc.).

2.  **Materialization via ProfilePermission**:
    *   Authority is "materialized" from a Template into a **ProfilePermission** entry.
    *   `ProfilePermission` stores the **Effective State** using a triple-state logic: `IsAllowed`, `IsDenied`, `IsActive`.
    *   This supports both RBAC (inheritance) and ABAC-lite (overrides/denies).

3.  **Strict Hierarchy**:
    *   Every permission belongs to a **System/Suite**.
    *   Templates are grouped by **Functional Modules**.

4.  **Governance & Audit**:
    *   Every entity implements the **Corporate Audit Schema** (10+ columns).
    *   The `Authorization Engine` resolves effective authority by querying the materialized `ProfilePermission` table, ensuring single-hop resolution performance.

## Technical Implementation
*   **Cardinality**: `System (1:N) Module (1:N) PermissionTemplate (1:N) ProfilePermission`.
*   **Persistence**: `ProfilePermission` is a physical copy of the template's authority for a specific Profile, allowing for non-destructive overrides.

## Consequences
*   **Positive**: Absolute control over the permission catalog, 100% auditable history, support for explicit denies, and high-performance resolution.
*   **Negative**: Requires a synchronization process when the Master Template is updated.
