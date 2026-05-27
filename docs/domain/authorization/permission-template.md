# PermissionTemplate — Aggregate Architecture

**Bounded Context:** Authorization  
**Aggregate Root:** `PermissionTemplate`  
**Module:** `Ums.Domain.Authorization.Template`  
**Status:** Production

---

## 1. Aggregate Overview

### Purpose

`PermissionTemplate` defines a reusable, versioned package of access rules for a specific Role within a SystemSuite. It acts as the authoritative blueprint for seeding `Profile` permissions when a new tenant is onboarded or a new role assignment is activated. Each item in the template targets a level of the SystemSuite hierarchy (suite → module → submodule → option) using an **Exclusive Arc** pattern, with a tri-state effect (`IsAllowed`, `IsDenied`, or neutral).

### Business Responsibility

- Create and version access-rule packages tied to a `(TenantId, RoleId, SystemSuiteId)` combination.
- Manage the template lifecycle: `Draft → Published → Deprecated`.
- Own a collection of `PermissionTemplateItem` entries, each targeting one hierarchy level of a SystemSuite with a specific permission effect.
- Enforce that items can only be added, mutated, or removed while the template is in `Draft` status.
- Provide the canonical rule set consumed by downstream `Profile` provisioning.

### Aggregate Root

`PermissionTemplate` is the sole aggregate root. `PermissionTemplateItem` is an owned child entity managed exclusively through the parent aggregate — never accessed or mutated directly.

### State Machine

```mermaid
stateDiagram-v2
    [*] --> Draft : Create
    Draft --> Published : Publish\n(requires ≥1 item)
    Published --> Deprecated : Deprecate
    Deprecated --> [*]
    note right of Draft : Items can be added,\nremoved, and mutated only\nwhen status = Draft
    note right of Published : Read-only — used for\nProfile seeding
    note right of Deprecated : Terminal — superseded\nby a newer template
```

### Invariants and Consistency Rules

| ID | Rule | Enforced by |
|---|---|---|
| INV-TPL1 | `(TenantId, RoleId, SystemSuiteId)` combination must be unique per active template | Repository uniqueness check |
| INV-TPL2 | Items can only be added, removed, or mutated when `Status = Draft` | `TemplateNotDraft` guard in every mutating method |
| INV-TPL3 | A `Published` template cannot be re-drafted; a new template must be created | `TemplateNotPublished` guard on `Deprecate` |
| INV-TPL4 | `(TargetType, TargetId, ActionId)` must be unique within a template — no duplicate item mappings | `TemplateItemTargetAlreadyExists` guard in `AddItem` |
| INV-TPL5 | An item cannot have `IsAllowed = true` AND `IsDenied = true` simultaneously | `PermissionEffectValidator` on every item |
| INV-TPL6 | `TargetId` must never be null — Exclusive Arc requires a concrete target reference | `ExclusiveArcValidator` on every item |

### Related Entities / Value Objects

| Entity / VO | Type | Ownership | Notes |
|---|---|---|---|
| `PermissionTemplateItem` | Entity | Owned | One entry per `(TargetType, TargetId, ActionId)` triplet |
| `TenantId` | Value Object | FK ref | Scopes the template to a tenant |
| `RoleId` | Value Object | FK ref | The role this template packages rules for |
| `SystemSuiteId` | Value Object | FK ref | The suite whose hierarchy the items reference |
| `TemplateVersion` | Value Object | — | Semver string (`"0.1.0"`) — starts at `Initial()` |
| `TemplateStatus` | Enumeration | — | `Draft(1) · Published(2) · Deprecated(3)` |
| `ExclusiveArcTarget` | Enumeration | — | `SystemSuite(1) · Module(2) · Submodule(3) · Option(4) · Aggregate(5) · Entity(6)` |
| `ActionId` | Value Object | FK ref | The specific action granted or denied |
| `AuditValueObject` | Value Object | — | `CreatedBy/At`, `UpdatedBy/At` on both AR and item |

### Domain Events

| Event | Trigger | Payload |
|---|---|---|
| `PermissionTemplateCreatedEvent` | Template drafted | `templateId, tenantId, roleId, systemSuiteId, version` |
| `PermissionTemplatePublishedEvent` | Template published | `templateId, version` |
| `PermissionTemplateMutatedEvent` | Item added, removed, or effect changed | `templateId, version` |
| `PermissionTemplateDeprecatedEvent` | Template deprecated | `templateId, version` |

> **Note:** A single `PermissionTemplateMutatedEvent` covers all item-level changes (add, remove, allow/deny/neutral toggle, activate/deactivate). Downstream consumers react to any mutation on the template version, not to individual item operations.

### Commands / Use Cases

| Command | Description | Pre-condition |
|---|---|---|
| `CreatePermissionTemplateCommand` | Draft a new template for `(tenantId, roleId, systemSuiteId)` | No active template for that triplet |
| `PublishPermissionTemplateCommand` | Transition `Draft → Published` | Status = Draft |
| `AddTemplateItemCommand` | Add an item `(targetType, targetId, actionId, isAllowed, isDenied)` | Status = Draft; no duplicate triplet |
| `SetItemAllowCommand` | Set `IsAllowed=true, IsDenied=false` | Status = Draft |
| `SetItemDenyCommand` | Set `IsAllowed=false, IsDenied=true` | Status = Draft |
| `SetItemNeutralCommand` | Set `IsAllowed=false, IsDenied=false` (inherit from parent scope) | Status = Draft |
| `ActivateItemCommand` | Set `IsActive=true` | Status = Draft |
| `DeactivateItemCommand` | Set `IsActive=false` | Status = Draft |
| `RemoveTemplateItemCommand` | Remove an item from the template | Status = Draft |
| `DeprecatePermissionTemplateCommand` | Transition `Published → Deprecated` | Status = Published |

### Repository / Service Boundaries

- `IPermissionTemplateRepository` — persists `PermissionTemplate` including owned items.
- `ITemplateUniquenessChecker` — validates INV-TPL1 before creation (no duplicate active template per triplet).

---

## 2. Object Model

```
PermissionTemplate (Aggregate Root)
├── Props: PermissionTemplateProps
│   ├── Id: IdValueObject
│   ├── TenantId: TenantId
│   ├── RoleId: RoleId
│   ├── SystemSuiteId: SystemSuiteId
│   ├── Version: TemplateVersion          -- semver, starts at "0.1.0"
│   ├── Status: TemplateStatus            -- Draft | Published | Deprecated
│   └── Audit: AuditValueObject
└── Children
    └── IReadOnlyCollection<PermissionTemplateItem>
        └── Props: PermissionTemplateItemProps
            ├── Id: IdValueObject
            ├── TemplateId: TemplateId
            ├── TargetType: ExclusiveArcTarget   -- SystemSuite | Module | Submodule | Option | Aggregate | Entity
            ├── TargetId: IdValueObject           -- FK to the target entity
            ├── ActionId: ActionId                -- FK to the action catalog
            ├── IsAllowed: bool                   -- tri-state: IsAllowed=true,IsDenied=false → Allow
            ├── IsDenied: bool                    -- tri-state: IsAllowed=false,IsDenied=true → Deny
            ├── IsActive: bool                    -- inactive items are skipped during provisioning
            └── Audit: AuditValueObject
```

### Main Attributes

| Attribute | Entity | Type | Notes |
|---|---|---|---|
| `Id` | PermissionTemplate | `Guid` | PK |
| `TenantId` | PermissionTemplate | `Guid` | FK — scopes template |
| `RoleId` | PermissionTemplate | `Guid` | FK — role being templated |
| `SystemSuiteId` | PermissionTemplate | `Guid` | FK — suite context |
| `Version` | PermissionTemplate | `string` | Semver `"major.minor.patch"` |
| `Status` | PermissionTemplate | `TemplateStatus` | `Draft(1)·Published(2)·Deprecated(3)` |
| `Id` | PermissionTemplateItem | `Guid` | PK |
| `TemplateId` | PermissionTemplateItem | `Guid` | FK → PermissionTemplate |
| `TargetType` | PermissionTemplateItem | `ExclusiveArcTarget` | `SystemSuite(1)·Module(2)·Submodule(3)·Option(4)·Aggregate(5)·Entity(6)` |
| `TargetId` | PermissionTemplateItem | `Guid` | FK to the target entity (arc target) |
| `ActionId` | PermissionTemplateItem | `Guid` | FK to Action catalog |
| `IsAllowed` | PermissionTemplateItem | `bool` | Explicit allow — mutually exclusive with `IsDenied` |
| `IsDenied` | PermissionTemplateItem | `bool` | Explicit deny — mutually exclusive with `IsAllowed` |
| `IsActive` | PermissionTemplateItem | `bool` | Inactive items skipped on profile seeding |

---

## 3. Class Diagram

```mermaid
classDiagram
    direction TB

    class PermissionTemplate {
        <<AggregateRoot>>
        +Guid Id
        +Guid TenantId
        +Guid RoleId
        +Guid SystemSuiteId
        +TemplateVersion Version
        +TemplateStatus Status
        +AuditValueObject Audit
        +IReadOnlyCollection~PermissionTemplateItem~ Items
        +Create(tenantId, roleId, systemSuiteId, createdBy) Result
        +Publish(updatedBy) Result
        +Deprecate(updatedBy) Result
        +AddItem(targetType, targetId, actionId, isAllowed, isDenied, createdBy) Result
        +SetItemAllow(itemId, updatedBy) Result
        +SetItemDeny(itemId, updatedBy) Result
        +SetItemNeutral(itemId, updatedBy) Result
        +ActivateItem(itemId, updatedBy) Result
        +DeactivateItem(itemId, updatedBy) Result
        +RemoveItem(itemId, updatedBy) Result
    }

    class PermissionTemplateItem {
        <<OwnedEntity>>
        +Guid Id
        +Guid TemplateId
        +ExclusiveArcTarget TargetType
        +Guid TargetId
        +Guid ActionId
        +bool IsAllowed
        +bool IsDenied
        +bool IsActive
        +AuditValueObject Audit
        +SetAllow(updatedBy) Result
        +SetDeny(updatedBy) Result
        +SetNeutral(updatedBy) Result
        +Activate(updatedBy) Result
        +Deactivate(updatedBy) Result
    }

    class TemplateStatus {
        <<Enumeration>>
        Draft = 1
        Published = 2
        Deprecated = 3
    }

    class ExclusiveArcTarget {
        <<Enumeration>>
        SystemSuite = 1
        Module = 2
        Submodule = 3
        Option = 4
    }

    PermissionTemplate "1" *-- "0..*" PermissionTemplateItem : owns
    PermissionTemplate --> TemplateStatus
    PermissionTemplateItem --> ExclusiveArcTarget
```

---

## 4. Sequence Diagrams

### Create Template & Publish Flow

```mermaid
sequenceDiagram
    participant A as Admin
    participant H as CreatePermissionTemplateHandler
    participant U as ITemplateUniquenessChecker
    participant T as PermissionTemplate (AR)
    participant R as IPermissionTemplateRepository

    A->>H: CreatePermissionTemplateCommand(tenantId, roleId, systemSuiteId)
    H->>U: IsUnique(tenantId, roleId, systemSuiteId)
    U-->>H: true
    H->>T: PermissionTemplate.Create(...)
    T->>T: Status = Draft · Version = "0.1.0"
    T->>T: Raise PermissionTemplateCreatedEvent
    H->>R: Add(template)
    H-->>A: templateId

    Note over A,R: Admin adds items...

    A->>H: PublishPermissionTemplateCommand(templateId)
    H->>R: GetById(templateId)
    R-->>H: PermissionTemplate (Draft)
    H->>T: template.Publish(actorId)
    T->>T: Guard: Status must be Draft
    T->>T: Status = Published
    T->>T: Raise PermissionTemplatePublishedEvent
    H->>R: Update(template)
    H-->>A: void
```

### Add Item Flow (Exclusive Arc)

```mermaid
sequenceDiagram
    participant A as Admin
    participant H as AddTemplateItemHandler
    participant T as PermissionTemplate (AR)
    participant R as IPermissionTemplateRepository

    A->>H: AddTemplateItemCommand(templateId, targetType, targetId, actionId, isAllowed, isDenied)
    H->>R: GetById(templateId)
    R-->>H: PermissionTemplate (Draft)
    H->>T: template.AddItem(targetType, targetId, actionId, isAllowed, isDenied, actorId)
    T->>T: Guard: Status = Draft
    T->>T: Guard: no duplicate (TargetType, TargetId, ActionId)
    T->>T: ExclusiveArcValidator — TargetId not null
    T->>T: PermissionEffectValidator — not (IsAllowed ∧ IsDenied)
    T->>T: Raise PermissionTemplateMutatedEvent
    H->>R: Update(template)
    H-->>A: itemId
```

### Permission Effect Override Flow

```mermaid
sequenceDiagram
    participant A as Admin
    participant H as SetItemDenyHandler
    participant T as PermissionTemplate (AR)

    A->>H: SetItemDenyCommand(templateId, itemId)
    H->>R: GetById(templateId)
    R-->>H: PermissionTemplate (Draft)
    H->>T: template.SetItemDeny(itemId, actorId)
    T->>T: Guard: Status = Draft
    T->>T: item.IsDenied = true · item.IsAllowed = false
    T->>T: Raise PermissionTemplateMutatedEvent
    H->>R: Update(template)
    H-->>A: void
```

---

## 5. Entity / Relationship Model

> **Exclusive Arc:** `PERMISSION_TEMPLATE_ITEM.TargetId` is a polymorphic FK — it can reference a `SystemSuite`, `Module`, `Submodule`, `Option`, `Aggregate`, or `Entity` depending on `TargetTypeId`. This is the Exclusive Arc pattern: only one of the possible references is valid per row, identified by `TargetTypeId`. No SQL foreign-key constraint spans all targets; referential integrity is enforced at the application layer.

```mermaid
erDiagram
    PERMISSION_TEMPLATE ||--o{ PERMISSION_TEMPLATE_ITEM : "contains"
    PERMISSION_TEMPLATE }o--|| TENANT                   : "scoped_to"
    PERMISSION_TEMPLATE }o--|| ROLE                     : "packages_for"
    PERMISSION_TEMPLATE }o--|| SYSTEM_SUITE             : "covers"
    PERMISSION_TEMPLATE_ITEM }o--|| ACTION              : "targets_action"

    PERMISSION_TEMPLATE {
        uniqueidentifier Id PK
        uniqueidentifier TenantId FK "RLS scope"
        uniqueidentifier RoleId FK "role being templated"
        uniqueidentifier SystemSuiteId FK "suite context"
        nvarchar Version "semver — 0.1.0"
        int StatusId "1=Draft 2=Published 3=Deprecated"
        nvarchar CreatedBy
        datetime2 CreatedAtUtc
        nvarchar UpdatedBy "Nullable"
        datetime2 UpdatedAtUtc "Nullable"
        nvarchar AuditTimeSpan
    }

    PERMISSION_TEMPLATE_ITEM {
        uniqueidentifier Id PK
        uniqueidentifier TemplateId FK
        int TargetTypeId "1=SystemSuite 2=Module 3=Submodule 4=Option 5=Aggregate 6=Entity"
        uniqueidentifier TargetId "Exclusive Arc — points to entity of TargetTypeId"
        uniqueidentifier ActionId FK "→ Action catalog"
        bit IsAllowed "true = explicit Allow"
        bit IsDenied "true = explicit Deny — CHECK NOT (IsAllowed AND IsDenied)"
        bit IsActive "false = skip during Profile seeding"
        nvarchar CreatedBy
        datetime2 CreatedAtUtc
        nvarchar UpdatedBy "Nullable"
        datetime2 UpdatedAtUtc "Nullable"
        nvarchar AuditTimeSpan
    }

    ACTION {
        uniqueidentifier Id PK
        nvarchar Code "unique action code"
        nvarchar Name
    }
```

---

## 6. Bounded Context Model

```mermaid
flowchart TD
    subgraph Authorization["Authorization BC"]
        PT[PermissionTemplate AR]
        PTI[PermissionTemplateItem]
        PROF[Profile AR]
        PP[ProfilePermission]
        PT --> PTI
        PROF --> PP
    end

    subgraph Identity["Identity BC"]
        TN[Tenant AR]
        UA[UserAccount AR]
    end

    subgraph SystemSuite_BC["SystemSuite BC"]
        SS[SystemSuite AR]
        ACT[Action]
    end

    PT -->|TenantId FK| TN
    PT -->|SystemSuiteId FK| SS
    PTI -->|ActionId FK| ACT

    PT -->|PermissionTemplateCreatedEvent| PROF
    PT -->|PermissionTemplatePublishedEvent\nseeds ProfilePermissions| PROF
    UA -->|UserRegisteredEvent → triggers\nProfile provisioning| PROF
```

---

## 7. API / Application Layer Contract

### Commands

| Command | Input | Output | Notes |
|---|---|---|---|
| `CreatePermissionTemplateCommand` | `tenantId, roleId, systemSuiteId` | `Guid templateId` | Starts at Draft · Version "0.1.0" |
| `PublishPermissionTemplateCommand` | `templateId` | `void` | Status must be Draft |
| `DeprecatePermissionTemplateCommand` | `templateId` | `void` | Status must be Published |
| `AddTemplateItemCommand` | `templateId, targetType, targetId, actionId, isAllowed, isDenied` | `Guid itemId` | Status must be Draft |
| `SetItemAllowCommand` | `templateId, itemId` | `void` | Status must be Draft |
| `SetItemDenyCommand` | `templateId, itemId` | `void` | Status must be Draft |
| `SetItemNeutralCommand` | `templateId, itemId` | `void` | Status must be Draft |
| `ActivateItemCommand` | `templateId, itemId` | `void` | Status must be Draft |
| `DeactivateItemCommand` | `templateId, itemId` | `void` | Status must be Draft |
| `RemoveTemplateItemCommand` | `templateId, itemId` | `void` | Status must be Draft |

### Queries

| Query | Returns |
|---|---|
| `GetAllPermissionTemplatesQuery` | `PagedResult<PermissionTemplateDto>` — filterable by `tenantId`, `status` |
| `GetPermissionTemplateByIdQuery` | `PermissionTemplateDetailDto` with items |
| `GetTemplatesByRoleQuery` | `List<PermissionTemplateSummaryDto>` |

### DTO Contract

```csharp
// Application layer read model
public sealed record PermissionTemplateDto(
    Guid TemplateId,
    Guid TenantId,
    Guid RoleId,
    Guid SystemSuiteId,
    string Version,
    string Status);
```

---

## 8. Persistence Notes

### Indexes

| Index | Columns | Type | Purpose |
|---|---|---|---|
| `UQ_PermissionTemplate_Tenant_Role_Suite` | `TenantId, RoleId, SystemSuiteId` | Unique (filtered `StatusId ≠ 3`) | INV-TPL1 — no duplicate active template |
| `IX_PermissionTemplate_TenantId` | `TenantId` | Non-unique | Tenant-scoped listing |
| `IX_PermissionTemplate_StatusId` | `StatusId` | Non-unique | Status filter |
| `UQ_PermissionTemplateItem_Target_Action` | `TemplateId, TargetTypeId, TargetId, ActionId` | Unique | INV-TPL4 — no duplicate item triplet |
| `IX_PermissionTemplateItem_TemplateId` | `TemplateId` | Non-unique | Load all items for a template |

### Transaction Boundary

`PermissionTemplate` and all its `PermissionTemplateItem` children are saved in a single `SaveChanges()` call. The `PermissionTemplatePublishedEvent` is dispatched via Transactional Outbox to trigger downstream Profile seeding.

### Security

| Operation | Required Role |
|---|---|
| Create / mutate / publish global template | `Platform:Admin` |
| Create / mutate / publish tenant-scoped template | `Tenant:Admin` |
| Deprecate any template | `Platform:Admin` |
| Read templates | `Tenant:Admin`, `Platform:Admin` |

---

**[← Authorization Index](./index.md)** | **[Domain Aggregate Index](../index.md)**
