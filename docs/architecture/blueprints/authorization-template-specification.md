# Authorization Template Specification

## 1. Overview
The Authorization Template module provides a mechanism to define reusable sets of permissions (Blueprints) that can be inherited by Roles and Profiles across the UMS ecosystem. This reduces operational overhead and ensures consistency in permission assignment.

## 2. Domain Model & Decoupling
The system enforces a strict decoupling between functional areas and implementation roles:

1.  **System/Suite**: Defines the functional boundary owned by a Tenant.
2.  **Permission Template**: A versioned set of permissions strictly scoped to a Suite and linked to a **System Role**.
3.  **Role**: A System-specific blueprint representing a functional set of permissions.
4.  **Profile**: The contextual nexus where effective authorizations (derived from Template/Role) are persisted.

## 3. Business Rules

### 3.1 Inheritance Types
*   **Static (Clone)**: Permissions are copied from the Template to the Role at creation time. Subsequent changes to the Template do not affect the Role.
*   **Dynamic (Linked)**: The Role maintains a reference to a specific Template version. The system can trigger a "Sync" to update the Role's effective permissions when the Template version changes.

### 3.2 Overrides
Tenants can add or remove specific permissions to a Role that was created from a Template, creating a "Delta" from the base blueprint.

### 3.3 Scoping & Multi-tenancy
*   **System Scoping**: Every template is bound to a specific System/Suite.
*   **Tenant Isolation**: Templates are isolated by `TenantId`. Global templates are managed as System-wide defaults but always within the context of a System.

## 4. Lifecycle & Versioning
*   Templates support **Draft**, **Published**, and **Deprecated** states.
*   Versioning follows SemVer (e.g., v1.2.0).
*   Automatic traceability is maintained for every change to a template.
*   **Editable Metadata**: The `Name` and `Description` of a template are editable throughout its lifecycle (subject to audit).

## 5. Creation Flow
1.  **Select Suite**: User selects the target functional area.
2.  **Select Template**: User selects a base template (Global or Local). This template is already linked to a **System Role**.
3.  **Initialize Profile**: The system pre-fills permissions based on the template, regardless of the target **Branch**.
4.  **Customize**: User adds specific overrides if necessary.
5.  **Audit**: The system logs the `SourceTemplateId` and `Version` for compliance.
