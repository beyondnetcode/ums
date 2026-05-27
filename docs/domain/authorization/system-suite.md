# SystemSuite — Aggregate Architecture

**Bounded Context:** Authorization  
**Aggregate Root:** `SystemSuite`  
**Module:** `Ums.Domain.Authorization.SystemSuite`  
**Status:** Production

---

## 1. Aggregate Overview

### Purpose
The `SystemSuite` aggregate represents a tenant-owned application surface registered in UMS. It defines the functional topology used by downstream authorization models and stores suite-level operational settings. In the current implementation, it owns `Module`, menu topology, `DomainResource` (Aggregates and Entities), `AppSetting`, and `Action` children. The independent `Role` aggregate is maintained in the selected suite context and references it through `SystemSuiteId`.

### Business Responsibility
- Register a tenant-scoped software suite.
- Maintain the suite identity: `Code`, `Name`, `Description`, `Status`.
- Own functional modules, domain resources (Entities/Aggregates), and suite-level application settings.
- Expose the action surface consumed by `PermissionTemplate` and effective authorization flows.
- Define the ownership boundary for the role catalog maintained by Authorization.
- Control activation state through `SystemStatus`.

### Aggregate Root
`SystemSuite` is the aggregate root. Changes to suite identity, modules, domain resources, app settings, and suite status must go through the aggregate root.

### Invariants and Consistency Rules
1. `TenantId`, `Code`, `Name`, and `Description` are mandatory.
2. `Code` must be unique within the owning tenant boundary.
3. `Module.Code` must be unique inside the suite.
4. App settings cannot duplicate the same `ConfigurationKey` for the same `ConfigurationScope`.
5. Module activation and deactivation are controlled through the parent aggregate.
6. Actions referenced by downstream permission templates must belong to the suite topology governed by this aggregate.

### Related Entities / Value Objects
| Entity / VO | Type | Ownership | Description |
|---|---|---|---|
| `Module` | Entity | Owned | Functional subsystem inside the suite |
| `AppSetting` | Entity | Owned | Suite-scoped configuration entry |
| `Action` | Entity | Owned / catalogued | Action tokens exposed for authorization targeting |
| `Role` | Aggregate Root | Related by `SystemSuiteId` | Responsibility catalog and hierarchy defined for the suite |
| `TenantId` | Value Object | - | Tenant ownership boundary |
| `Code` | Value Object | - | Technical identifier |
| `Name` | Value Object | - | Display label |
| `Description` | Value Object | - | Functional description |
| `SystemStatus` | Enumeration | - | `Active`, `Inactive`, `Beta`, etc. |

### Domain Events
| Event | Trigger |
|---|---|
| `SystemSuiteRegisteredEvent` | New suite created |
| `SystemSuiteStatusChangedEvent` | Suite status changed |
| `SystemSuiteModuleAddedEvent` | Module added |
| `SystemSuiteModuleRemovedEvent` | Module removed |
| `SystemSuiteModuleStatusChangedEvent` | Module activated or deactivated |

---

## 2. Domain Model

### Classes / Entities / Value Objects
```text
SystemSuite (Aggregate Root)
├── Props: SystemSuiteProps
│   ├── Id: IdValueObject
│   ├── TenantId: TenantId
│   ├── Code: Code
│   ├── Name: Name
│   ├── Description: Description
│   ├── Status: SystemStatus
│   └── Audit: AuditValueObject
├── Children
│   ├── IReadOnlyCollection<Module>
│   └── IReadOnlyCollection<AppSetting>
└── Catalog Surface
    └── IReadOnlyCollection<Action>
```

---

## 3. Object Model Diagrams

```mermaid
classDiagram
    direction TB
    class SystemSuite {
        +Guid Id
        +Guid TenantId
        +Code Code
        +Name Name
        +Description Description
        +SystemStatus Status
        +List~Module~ Modules
        +List~DomainResource~ DomainResources
        +List~AppSetting~ AppSettings
        +List~Action~ Actions
        +Create(tenantId, code, name, description, actor)
        +Update(name, description, actor)
        +SetStatus(status, actor)
        +AddModule(code, name, description, sortOrder, actor)
        +UpdateModule(moduleId, name, description, sortOrder, actor)
        +ActivateModule(moduleId, actor)
        +DeactivateModule(moduleId, actor)
        +RemoveModule(moduleId, actor)
        +AddDomainResource(moduleId, type, code, name, description, actor)
        +UpdateDomainResource(resourceId, moduleId, type, code, name, description, actor)
        +RemoveDomainResource(resourceId, actor)
        +AddAppSetting(key, value, scope, actor)
    }
    class Module {
        +Guid Id
        +Guid SuiteId
        +Code Code
        +Name Name
        +Description Description
        +int SortOrder
        +ModuleStatus Status
    }
    class DomainResource {
        +Guid Id
        +Guid? ModuleId
        +DomainResourceType Type
        +Code Code
        +Name Name
        +Description Description
    }
    class AppSetting {
        +Guid Id
        +ConfigurationKey Key
        +ConfigurationValue Value
        +ConfigurationScope Scope
    }
    class Action {
        +Guid Id
        +ActionCode Code
    }
    SystemSuite "1" *-- "0..*" Module : contiene
    SystemSuite "1" *-- "0..*" DomainResource : posee
    SystemSuite "1" *-- "0..*" AppSetting : configura
    SystemSuite "1" *-- "0..*" Action : expone
```

---

## 4. Sequence Diagrams

### Add Module Flow
```mermaid
sequenceDiagram
    participant C as Client
    participant H as Handler
    participant R as ISystemSuiteRepository
    participant S as SystemSuite (AR)

    C->>H: AddModuleCommand(systemSuiteId, code, name, description, sortOrder)
    H->>R: GetById(systemSuiteId)
    R-->>H: SystemSuite
    H->>S: AddModule(code, name, description, sortOrder, actor)
    S->>S: Validate module code uniqueness
    S->>S: Raise SystemSuiteModuleAddedEvent
    H->>R: Update(systemSuite)
    R-->>H: ok
    H-->>C: Success
```

---

## 5. ER Model

```mermaid
erDiagram
    TENANT ||--o{ SYSTEM_SUITE : "owns"
    SYSTEM_SUITE ||--o{ MODULE : "contiene"
    SYSTEM_SUITE ||--o{ DOMAIN_RESOURCE : "posee"
    SYSTEM_SUITE ||--o{ APP_SETTING : "configura"
    SYSTEM_SUITE ||--o{ ACTION : "expone"
    SYSTEM_SUITE ||--o{ ROLE : "defines"

    SYSTEM_SUITE {
        uniqueidentifier Id PK
        uniqueidentifier TenantId FK
        nvarchar Code
        nvarchar Name
        nvarchar Description
        int Status "Active(1) | Inactive(2)"
    }
    DOMAIN_RESOURCE {
        uniqueidentifier ResourceId PK
        uniqueidentifier SuiteId FK
        uniqueidentifier ModuleId FK "Nullable"
        int Type "Aggregate(1) | Entity(2)"
        nvarchar Code "Unique within Suite"
        nvarchar Name
        nvarchar Description
    }
    MODULE {
        nvarchar CreatedBy
        datetime2 CreatedAtUtc
        nvarchar UpdatedBy
        datetime2 UpdatedAtUtc
        nvarchar AuditTimeSpan
    }
```

### Tenant Isolation Rules
- `SystemSuite` is tenant-owned in the current implementation.
- Modules, app settings, and actions inherit suite ownership through the aggregate boundary.

---

## 6. Bounded Context Integration
- Upstream: tenant context from Identity.
- Downstream: consumed by `PermissionTemplate` and effective authorization resolution.
- Actions exposed by the suite are referenced by authorization templates and profiles.

---

## 7. Application Layer
- `CreateSystemSuiteCommand` -> Inputs: `TenantId, Code, Name, Description` -> Returns: `Guid`
- `UpdateSystemSuiteCommand` -> Inputs: `SystemSuiteId, Name, Description` -> Returns: `void`
- `SetSystemSuiteStatusCommand` -> Inputs: `SystemSuiteId, Status` -> Returns: `void`
- `CreateRoleCommand`, `UpdateRoleCommand`, and `SetRoleStatusCommand` operate on roles under the selected suite.
- GraphQL exposes `rolesBySystemSuite(systemSuiteId)` for the Roles detail tab.

---

## 8. Infrastructure/Persistence
- SQL Server and in-memory repository implementations are available for suite and role development/runtime modes.
- `[ums_authorization].[Roles]` references `[ums_authorization].[SystemSuites]` and supports a nullable parent-role FK.
- Application-layer tenant filtering is the primary isolation mechanism.

---

## 9. Security & Compliance
- Suite definition is an administrative capability.
- Module and status changes affect downstream authorization behavior and should be audited.

---

## 10. Technical Decisions
- `SystemSuite` is tenant-owned in the current domain model, even if some older documentation described it as a global catalog.
- The aggregate currently privileges module and setting management plus a flat action surface over the older deep menu tree narrative.

---

**[Back to Authorization Index](./index.md)**
