# UMS Parameterization System Specification

> **Version:** 1.0.0
> **Status:** Proposed
> **Created:** 2026-05-30
> **Last Updated:** 2026-05-30

---

## 1. Overview

### 1.1 Purpose

Establish a mandatory pattern for all configurable behaviors in the UMS system. No business rules, validations, limits, durations, behaviors, permissions, or operational configurations may remain hardcoded in web, API, services, or internal components.

### 1.2 Scope

All configurable behaviors within UMS must be stored in the database, loaded into memory at system startup, and consumed through a centralized configuration provider service.

---

## 2. Parameterization Architecture

### 2.1 Parameter Storage Zones

```
┌─────────────────────────────────────────────────────────────┐
│                    UMS Configuration                         │
├─────────────────────────┬───────────────────────────────────┤
│   GLOBAL (System-wide)  │      TENANT-SPECIFIC              │
│   ScopeId = 1           │      ScopeId = 2                  │
├─────────────────────────┼───────────────────────────────────┤
│ • Visible to Internal   │ • Visible to Tenant Admin         │
│   Admin only            │   (with permissions)              │
│ • Applies to entire     │ • Applies only to own tenant      │
│   UMS system            │ • Internal Admin can view all     │
│ • Base configuration    │ • Tenant can override if global   │
│   for all behaviors     │   allows it                       │
└─────────────────────────┴───────────────────────────────────┘
```

### 2.2 Scope Identifiers

| ScopeId | Name | Description |
|---------|------|-------------|
| 1 | Global | System-wide parameters, UMS base configuration |
| 2 | Tenant | Tenant-specific parameters |
| 3 | Suite | System Suite level parameters (future) |
| 4 | Module | Module level parameters (future) |

---

## 3. Parameter Loader

### 3.1 Purpose

Load all parameters from the database into memory at system startup to avoid repeated database queries during runtime.

### 3.2 Behavior

```
System Startup
     │
     ▼
┌────────────────────┐
│ ConfigurationLoader│
│                    │
│ 1. Load Global     │ ← ScopeId = 1
│    Parameters      │
│                    │
│ 2. Load Tenant     │ ← For active tenants
│    Parameters      │
│                    │
│ 3. Build in-memory │
│    parameter store │
│                    │
│ 4. Register as     │
│    singleton       │
└────────────────────┘
         │
         ▼
┌────────────────────┐
│ConfigurationProvider│ ← Consumed by all system logic
│                    │
│ get(key)           │
│ get(key, tenantId) │
│ set(key, value)    │ ← Triggers audit log
└────────────────────┘
```

### 3.3 Loading Strategy

1. **Global Parameters**: Loaded synchronously at startup
2. **Tenant Parameters**: Loaded on-demand when tenant context is established, then cached
3. **Cache Refresh**: Manual trigger only (no automatic refresh during runtime)

### 3.4 Memory Structure

```typescript
interface ParameterStore {
  global: Map<string, AppConfiguration>;
  byTenant: Map<Guid, Map<string, AppConfiguration>>;
}
```

---

## 4. Configuration Provider Service

### 4.1 Interface

```typescript
interface IConfigurationProvider {
  // Get global parameter
  getGlobal(key: string): AppConfiguration | undefined;

  // Get tenant parameter (with precedence check)
  getForTenant(tenantId: Guid, key: string): AppConfiguration | undefined;

  // Get parameter value as typed value
  getValueAs<T>(key: string, tenantId?: Guid, defaultValue?: T): T;

  // Set parameter (triggers audit)
  set(key: string, value: string, scope: Scope, tenantId?: Guid): void;

  // Reload from database
  reload(): Promise<void>;

  // Reload for specific tenant
  reloadTenant(tenantId: Guid): Promise<void>;
}
```

### 4.2 Precedence Rules

1. **If tenant-specific parameter exists** → Use tenant value
2. **If only global parameter exists** → Use global value
3. **Global parameters act as maximum restriction** when applicable
4. **Tenant cannot override** when global parameter is marked as non-overridable

```typescript
function getWithPrecedence(key: string, tenantId?: Guid): AppConfiguration | undefined {
  if (tenantId) {
    const tenantParam = store.byTenant.get(tenantId)?.get(key);
    if (tenantParam) return tenantParam;
  }

  return store.global.get(key);
}
```

---

## 5. Parameter Structure

### 5.1 AppConfiguration Entity

| Field | Type | Description |
|-------|------|-------------|
| Id | Guid | Unique identifier |
| TenantId | Guid? | Tenant owner (null for Global) |
| SystemSuiteId | Guid? | System Suite (for Suite-level) |
| ModuleId | Guid? | Module (for Module-level) |
| Code | string | Parameter code (unique within scope) |
| Value | string | Parameter value |
| Description | string | Human-readable description |
| ScopeId | int | 1=Global, 2=Tenant, 3=Suite, 4=Module |
| IsInheritable | bool | Can be inherited by child scopes |
| IsEncrypted | bool | Value is encrypted |
| Version | string | Semantic version |
| StatusId | int | 1=Draft, 2=Published, 3=Archived |

### 5.2 Required Parameters (Initial Set)

#### Global Parameters (ScopeId = 1)

| Code | Type | Default | Description |
|------|------|---------|-------------|
| SESSION_TIMEOUT_MINUTES | int | 30 | Idle session timeout |
| MAX_LOGIN_ATTEMPTS | int | 5 | Max failed login attempts |
| ACCESS_TOKEN_DURATION_MS | int | 3600000 | Access token lifetime (1 hour) |
| REFRESH_TOKEN_DURATION_MS | int | 604800000 | Refresh token lifetime (7 days) |
| MIN_PASSWORD_LENGTH | int | 12 | Minimum password length |
| MAX_VALIDITY_PERIOD_DAYS | int | 365 | Account validity period |
| MFA_REQUIRED_FOR_ADMIN | bool | false | Require MFA for admins |
| PASSWORD_HISTORY_COUNT | int | 5 | Password history size |
| UI_LANGUAGE_DEFAULT | string | "es" | Default UI language |
| UI_TIMEZONE_DEFAULT | string | "America/Lima" | Default timezone |
| EMAIL_FROM_ADDRESS | string | "noreply@ums.local" | Email sender |
| NOTIFICATION_RETRY_ATTEMPTS | int | 3 | Notification retry count |

#### Tenant-Specific Parameters (ScopeId = 2)

| Code | Type | Description |
|------|------|-------------|
| MFA_ALLOWED_METHODS | string[] | Allowed MFA methods (SMS, TOTP, Email) |
| PASSWORD_MAX_AGE_DAYS | int | Password max age (overrides global if set) |
| ACCOUNT_LOCKOUT_DURATION_MINUTES | int | Account lockout duration |
| UI_CUSTOM_BRANDING_ENABLED | bool | Enable custom branding |
| SESSION_MAX_CONCURRENT | int | Max concurrent sessions per user |

---

## 6. Audit Logging

### 6.1 Audited Operations

- Create parameter
- Update parameter value
- Update parameter status (Draft → Published → Archived)
- Delete parameter
- Override global parameter at tenant level

### 6.2 Audit Record Structure

```typescript
interface ConfigurationAuditRecord {
  id: Guid;
  parameterId: Guid;
  parameterCode: string;
  tenantId: Guid | null;        // null for global parameters
  userId: Guid;
  userType: 'InternalAdmin' | 'TenantAdmin' | 'System';
  operation: 'Create' | 'Update' | 'Delete' | 'Override';
  previousValue: string | null;
  newValue: string | null;
  timestamp: DateTime;
  ipAddress: string;
  userAgent: string;
}
```

---

## 7. API Endpoints

### 7.1 Global Parameters (Internal Admin Only)

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/configurations/global` | List all global parameters |
| GET | `/api/v1/configurations/global/{code}` | Get global parameter by code |
| POST | `/api/v1/configurations/global` | Create global parameter |
| PUT | `/api/v1/configurations/global/{code}` | Update global parameter |
| DELETE | `/api/v1/configurations/global/{code}` | Archive global parameter |

### 7.2 Tenant Parameters

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/v1/tenants/{tenantId}/configurations` | List tenant parameters |
| GET | `/api/v1/tenants/{tenantId}/configurations/{code}` | Get tenant parameter |
| POST | `/api/v1/tenants/{tenantId}/configurations` | Create tenant parameter |
| PUT | `/api/v1/tenants/{tenantId}/configurations/{code}` | Update tenant parameter |
| DELETE | `/api/v1/tenants/{tenantId}/configurations/{code}` | Archive tenant parameter |

---

## 8. Frontend Integration

### 8.1 Configuration Screen Structure

```
┌─────────────────────────────────────────────────────────────┐
│ Parámetros del Sistema                                      │
├─────────────────────────┬───────────────────────────────────┤
│ [Global] [Por Tenant]   │                                   │
├─────────────────────────┤  ┌─────────────────────────────┐ │
│ 🔍 Search...            │  │ SESSION_TIMEOUT_MINUTES     │ │
│                         │  │ Valor: 30                   │ │
│ [Global]                │  │ Descripción: Idle session   │ │
│  • SESSION_TIMEOUT...   │  │ timeout in minutes          │ │
│  • MAX_LOGIN_ATTEMPTS   │  │ Scope: Global               │ │
│  • ACCESS_TOKEN_...     │  │ Status: Published           │ │
│                         │  │                             │ │
│ [Por Tenant]            │  │ [Edit] [Archive]            │ │
│  ▼ TENANT: Ransa        │  └─────────────────────────────┘ │
│    • MFA_ALLOWED_...    │                                   │
│    • PASSWORD_MAX_...   │                                   │
│  ▼ TENANT: APM          │                                   │
│    • MFA_ALLOWED_...    │                                   │
└─────────────────────────┴───────────────────────────────────┘
```

### 8.2 Tenant Configuration Tab

```
┌─────────────────────────────────────────────────────────────┐
│ Tenant: Ransa S.A.                        [Parámetros]      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  Configuraciones específicas para este tenant               │
│                                                             │
│  [+ Agregar Parámetro]                                     │
│                                                             │
│  ┌──────────────────────────────────────────────────────┐  │
│  │ MFA_ALLOWED_METHODS                                  │  │
│  │ Valor: ["TOTP", "Email"]                             │  │
│  │ Override del global: No establecido                  │  │
│  │ [Edit] [Remove Override]                             │  │
│  └──────────────────────────────────────────────────────┘  │
│                                                             │
└─────────────────────────────────────────────────────────────┘
```

---

## 9. Technical Implementation

### 9.1 Backend Services

1. **ConfigurationLoader** (Singleton)
   - Loads parameters at startup
   - Manages in-memory cache
   - Handles reload requests

2. **ConfigurationProvider** (Singleton)
   - Central access point for all configuration values
   - Implements precedence logic
   - Notifies audit service on changes

3. **ConfigurationAuditService**
   - Logs all configuration changes
   - Queries audit records

### 9.2 TODO: Future Redis Migration

```typescript
/**
 * TODO: [TD-003] Migrate ConfigurationProvider to Redis
 *
 * Current implementation stores parameters in-memory.
 * Future implementation should use Redis for distributed caching.
 *
 * Migration steps:
 * 1. Introduce IConfigurationCache abstraction
 * 2. Implement InMemoryConfigurationCache (current)
 * 3. Implement RedisConfigurationCache (future)
 * 4. Update ConfigurationProvider to use IConfigurationCache
 * 5. Add cache invalidation strategy
 * 6. Implement cache warming on startup
 *
 * Benefits:
 * - Shared configuration across API instances
 * - Automatic invalidation on updates
 * - Reduced database load
 * - Support for configuration hot-reload
 */
```

---

## 10. Acceptance Criteria

### 10.1 Functional Requirements

- [ ] All configurable behaviors use parameters from database
- [ ] Global parameters are visible only to Internal Admin
- [ ] Tenant parameters are visible to authorized tenant users
- [ ] Internal Admin can view/edit all tenant parameters
- [ ] Precedence rule: Tenant overrides Global when allowed
- [ ] All parameter changes are audited

### 10.2 Technical Requirements

- [ ] Parameters are loaded into memory at startup
- [ ] No direct database queries for parameters during request processing
- [ ] ConfigurationProvider is the single access point
- [ ] Cache can be manually refreshed
- [ ] Code is prepared for Redis migration (abstraction layer)

### 10.3 Security Requirements

- [ ] Authorization validates user type and tenant access
- [ ] Audit logs include user, timestamp, and changes
- [ ] Encrypted parameters are stored securely

---

## 11. Related Documents

- [Functional Story: System Parameter Management](./requirements/functional-stories/fs-20-system-parameter-management.md)
- [Technical Debt Registry](./technical-debt.md)
- [TODO: Configuration System Implementation](./project/TODO.md)

---

## 12. Changelog

| Version | Date | Changes |
|---------|------|---------|
| 1.0.0 | 2026-05-30 | Initial specification |