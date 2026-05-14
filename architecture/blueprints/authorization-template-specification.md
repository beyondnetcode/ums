# Authorization Template Specification

## 1. Overview
The Authorization Template module provides a mechanism to define reusable sets of permissions (Blueprints) that can be inherited by Roles and Profiles across the UMS ecosystem. This reduces operational overhead and ensures consistency in permission assignment.

## 2. Domain Model & Decoupling
The system enforces a strict decoupling between functional areas and implementation roles:

1.  **System/Suite**: Defines the functional boundary (e.g., "Logistics Suite", "Finance Suite").
2.  **Permission Template**: A versioned set of permissions within a Suite.
3.  **Role**: A tenant-specific object that implements a Template.

## 3. Business Rules

### 3.1 Inheritance Types
*   **Static (Clone)**: Permissions are copied from the Template to the Role at creation time. Subsequent changes to the Template do not affect the Role.
*   **Dynamic (Linked)**: The Role maintains a reference to a specific Template version. The system can trigger a "Sync" to update the Role's effective permissions when the Template version changes.

### 3.2 Overrides
Tenants can add or remove specific permissions to a Role that was created from a Template, creating a "Delta" from the base blueprint.

### 3.3 Scoping & Multi-tenancy
*   **Global Templates**: Created by the System Admin (`TenantId = NULL`). Available to all tenants.
*   **Local Templates**: Created by a Tenant Admin. Available only within that specific tenant.

## 4. Lifecycle & Versioning
*   Templates support **Draft**, **Published**, and **Deprecated** states.
*   Versioning follows SemVer (e.g., v1.2.0).
*   Automatic traceability is maintained for every change to a template.

## 5. Creation Flow
1.  **Select Suite**: User selects the target functional area.
2.  **Select Template**: User selects a base template (Global or Local).
3.  **Configure Role**: The system pre-fills permissions based on the template.
4.  **Customize**: User adds specific overrides if necessary.
5.  **Audit**: The system logs the `SourceTemplateId` and `Version` for compliance.
