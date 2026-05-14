# ADR 0027: Authorization Template and Inheritance Strategy

## Status
Proposed

## Context
The current User Management System (UMS) requires manual configuration of roles and permissions for each tenant. As the number of systems (Suites) and tenants grows, this approach becomes operationally expensive and error-prone. There is a need for a mechanism to define standardized, reusable "Permission Templates" that can be inherited or used as blueprints for creating tenant-specific roles and profiles.

## Decision
We will implement a **Template-Based Authorization Strategy** based on the following principles:

1.  **Decoupled Hierarchy**:
    *   **System/Suite**: The highest level of grouping for functional permissions.
    *   **Permission Templates**: Global or Tenant-specific sets of predefined authorizations.
    *   **Roles/Profiles**: Tenant-specific implementations that can inherit from a template.

2.  **Hybrid Inheritance Model**:
    *   **Static Initialization (Blueprint)**: Copying template permissions to a role at creation time.
    *   **Dynamic Linking (Linked)**: Maintaining a link to a template version where changes in the template can propagate to linked roles (governed by a "Sync" policy).

3.  **Scoped Templates**:
    *   **System Templates (Global)**: Provided by the platform (e.g., "Full Auditor", "Base Employee").
    *   **Tenant Templates (Local)**: Created by a tenant for its own internal structure.

4.  **Versioning**:
    *   Templates will support semantic versioning. Roles can be pinned to a specific version of a template to ensure stability.

## Technical Implementation
*   **Data Model**: Introduction of `PermissionTemplates` and `TemplatePermissions` tables.
*   **Decoupling**: Permissions are categorized by `SystemSuiteId`.
*   **Isolation**: Global templates will have a `TenantId = NULL` (or a system-reserved UUID), while local templates will be protected by RLS.

## Consequences
*   **Positive**: Significant reduction in manual configuration time, consistency across tenants, and easier auditing.
*   **Negative**: Slight increase in database complexity and the need for a background sync process for propagating template updates.
