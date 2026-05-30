# Functional Story 20: System Parameter Management

## 1. Business Purpose

UMS must provide a secure, configurable, and auditable mechanism for managing system-wide and tenant-specific parameters. Internal platform administrators can manage global parameters and tenant-specific parameters, while tenant administrators can only manage parameters within their own tenant scope. All parameter changes must be tracked for compliance and debugging.

## 2. Actors

| Actor | Responsibility |
| :--- | :--- |
| **Internal Platform Administrator** | Manages global system parameters and tenant-specific parameters for any tenant. Has full configuration visibility. |
| **Tenant Administrator** | Manages parameters within their own tenant scope only. Cannot access global configurations or other tenants' configurations. |
| **System** | Consumes configured parameters to determine behavior, rules, limits, and permissions. |

## 3. Business Preconditions

- The administrator is authenticated and holds a valid session.
- The administrator has the `CAN_MANAGE_GLOBAL_CONFIGURATION` permission (for global config) or `CAN_MANAGE_TENANT_CONFIGURATION` permission (for tenant-specific config).
- For internal admins, `isInternalAdmin=true` in the JWT claims.
- For tenant admins, `OrganizationId` matches the target tenant for tenant-specific operations.

## 4. Main Functional Flow

### 4.1 Access Global Configuration (Internal Admin Only)

1. The internal administrator navigates to the System Configuration section.
2. UMS validates that `IsInternalAdmin=true` and the user has `CAN_MANAGE_GLOBAL_CONFIGURATION`.
3. UMS displays all global parameters (where `Scope = Global`).
4. The administrator can create, modify, publish, or archive global parameters.
5. Changes are audited with full traceability.

### 4.2 Access Tenant Configuration

1. An administrator (internal or tenant) navigates to a tenant's Configuration section.
2. For internal admins: UMS allows viewing/managing any tenant's parameters.
3. For tenant admins: UMS validates that `OrganizationId == targetTenantId`.
4. UMS displays parameters where `Scope = Tenant` and `TenantId = targetTenant`.
5. The administrator can create, modify, publish, or archive tenant-specific parameters.

### 4.3 Create/Modify Parameter

1. The administrator selects "Add Parameter" or selects an existing parameter.
2. UMS displays a form with: Code, Value, Description, Scope, IsInheritable, IsEncrypted.
3. For global parameters: Scope is automatically set to Global.
4. For tenant parameters: Scope is set to Tenant and TenantId is auto-populated.
5. The administrator fills in the details and submits.
6. UMS validates the data and saves as Draft.
7. UMS records the audit entry.
8. UMS prompts the administrator to publish the parameter.

### 4.4 Publish Parameter

1. The administrator selects a Draft parameter and chooses "Publish".
2. UMS validates the parameter is valid.
3. UMS changes status from Draft to Published.
4. UMS records the audit entry.
5. The parameter becomes active and effective immediately.

## 5. Alternative Flows and Exceptions

### A. Unauthorized Access Attempt

If a tenant administrator attempts to access global configuration, UMS returns a 403 error with message "Global configuration is only accessible to internal administrators."

### B. Cross-Tenant Access Attempt

If a tenant administrator attempts to access another tenant's configuration, UMS returns a 403 error with message "You do not have permission to access this tenant's configuration."

### C. Parameter Not Found

If the administrator requests a non-existent parameter, UMS returns a 404 error.

### D. Concurrent Modification

If two administrators modify the same parameter simultaneously, UMS returns a 409 Conflict error with ETag mismatch information.

### E. Draft-Only Modifications

Parameters can only be modified when in Draft status. Published parameters require creating a new Draft version.

### F. Invalid Scope for User Type

If an internal admin tries to create a Tenant-scoped parameter without specifying TenantId, UMS returns a validation error.

## 6. Business Rules

1. **Global Configuration Scope**: Parameters with no `TenantId`, no `SystemSuiteId`, and no `ModuleId` are Global. Only internal admins can manage them.
2. **Tenant Configuration Scope**: Parameters with `TenantId` are tenant-specific. Both internal and tenant admins can manage them (admins limited to their tenant).
3. **Inheritance**: If `IsInheritable=true`, child configurations can override the parameter.
4. **Encryption**: If `IsEncrypted=true`, the value is stored encrypted and decrypted only when consumed.
5. **Versioning**: Parameters use semantic versioning (`Major.Minor.Patch`). Updates to Draft bump the minor version.
6. **Status Lifecycle**: Draft → Published → Archived. Only Draft parameters can be modified.
7. **Authorization Matrix**:
   | Configuration Scope | Internal Admin | Tenant Admin |
   |---------------------|----------------|--------------|
   | Global | Can manage | Cannot access |
   | Tenant (own) | Can manage | Can manage |
   | Tenant (other) | Can manage | Cannot access |
   | Suite | Can manage | Cannot access |
   | Module | Can manage | Cannot access |

## 7. Acceptance Criteria

1. Internal administrators can view and manage all global parameters.
2. Tenant administrators cannot see or access global configuration.
3. Tenant administrators can view and manage parameters within their own tenant.
4. Tenant administrators cannot access other tenants' configurations.
5. Internal administrators can access any tenant's configuration.
6. All parameter changes (create, update, publish, archive) generate audit records.
7. The system uses parameter values from AppConfiguration instead of hardcoded constants.
8. Parameters support versioning and status lifecycle (Draft → Published → Archived).
9. Encryption flag protects sensitive parameter values.
10. Inheritance flag allows child configurations to override parameters.
11. Optimistic concurrency prevents conflicting updates.

## 8. Technical Requirements

### 8.1 Authorization Checks

In all AppConfiguration endpoints, add `ITenantContext` injection and verify:

```csharp
// For global configs
if (scope == "Global" && !isInternalAdmin)
    return Results.Forbid();

// For tenant configs
if (targetTenantId != organizationId && !isInternalAdmin)
    return Results.Forbid();
```

### 8.2 Endpoints

| Method | Endpoint | Description | Authorization |
|--------|----------|-------------|---------------|
| GET | `/app-configurations` | List configurations (filtered by scope and user permissions) | Based on scope |
| GET | `/app-configurations/{id}` | Get single configuration | Based on scope and ownership |
| POST | `/app-configurations` | Create new configuration | Internal admin or tenant admin for own tenant |
| PUT | `/app-configurations/{id}` | Update draft configuration | Same as create |
| POST | `/app-configurations/{id}/publish` | Publish configuration | Same as create |
| POST | `/app-configurations/{id}/archive` | Archive configuration | Same as create |

### 8.3 Query Filters

| Parameter | Behavior |
|-----------|----------|
| `scope=Global` | Only internal admins can query; returns all global configs |
| `scope=Tenant&tenantId={id}` | Internal admins get all tenant configs; tenant admins get only their tenant |
| No scope filter | Returns configs based on user's permissions |

### 8.4 Key Hardcoded Values to Migrate

| Current Hardcode | Parameter Code | Default Value |
|-----------------|----------------|---------------|
| `ACCESS_TOKEN_DURATION` (frontend) | `ACCESS_TOKEN_DURATION_MS` | 3600000 (1 hour) |
| `REFRESH_TOKEN_DURATION` (frontend) | `REFRESH_TOKEN_DURATION_MS` | 604800000 (7 days) |
| `MIN_PASSWORD_LENGTH` | `MIN_PASSWORD_LENGTH` | 12 |
| `MAX_VALIDITY_PERIOD_DAYS` | `MAX_VALIDITY_PERIOD_DAYS` | 365 |

### 8.5 Audit Events

| Event | Fields |
|---|---|
| `APP_CONFIG_CREATED` | adminUserId, configId, code, scope, timestamp |
| `APP_CONFIG_UPDATED` | adminUserId, configId, code, oldVersion, newVersion, timestamp |
| `APP_CONFIG_PUBLISHED` | adminUserId, configId, code, version, timestamp |
| `APP_CONFIG_ARCHIVED` | adminUserId, configId, code, version, timestamp |

### 8.6 Feature Flags

| Flag | Description | Default |
|------|-------------|---------|
| `ALLOW_GLOBAL_CONFIG_MANAGEMENT` | Enable global configuration management | `true` |
| `ALLOW_TENANT_CONFIG_MANAGEMENT` | Enable tenant-specific configuration management | `true` |

## 9. Traceability

- Entities: `AppConfiguration`, `AuditRecord`
- Related stories: FS-01 (authentication), FS-17 (system roles), FS-19 (admin password reset)
- Related ADR: ADR-0012 (RBAC), ADR-0019 (audit trail)
- Diagram update: `docs/domain/identity/tenant.md` section 10 - add parameter management rules