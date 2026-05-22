# PermissionTemplate — Aggregate Architecture

**Bounded Context:** Authorization  
**Aggregate Root:** `PermissionTemplate`  
**Module:** `Ums.Domain.Authorization.PermissionTemplate`  
**Status:** Production

---

## 1. Aggregate Overview

### Purpose
The `PermissionTemplate` aggregate defines reusable, standard packages of access rights (permissions) mapped to various system roles (e.g., "Standard Employee", "Branch Manager", "Tenant Financial Administrator"). It acts as a standardized blue-print that simplifies and automates dynamic profile (role) provisioning when new tenants are registered or new users are onboarded.

### Business Responsibility
- Author and maintain pre-packaged security templates.
- Link fine-grained suite Actions to a named template.
- Facilitate consistent and reproducible security setups across tenants.

### Aggregate Root
`PermissionTemplate` is the aggregate root. Child configuration details are managed inside the `PermissionTemplateItem` owned entity collection.

### Invariants and Consistency Rules
1. A Template `Code` must be unique across the system.
2. A Template must contain at least one `PermissionTemplateItem` to be active.
3. If an underlying `Action` in `SystemSuite` is deleted, the corresponding `PermissionTemplateItem` is automatically cascadingly pruned.

### Related Entities / Value Objects
| Entity / VO | Type | Ownership |
|---|---|---|
| `PermissionTemplateItem` | Entity | Owned (see [permission-template-item.md](./permission-template-item.md)) |
| `TemplateCode` | Value Object | Alpha-numeric template code |
| `TemplateName` | Value Object | Description and display label |

### Domain Events
- `PermissionTemplateCreatedEvent`
- `PermissionTemplateUpdatedEvent`
- `PermissionTemplateDeletedEvent`

---

## 2. Domain Model

### Classes / Entities / Value Objects
```
PermissionTemplate (Aggregate Root)
├── Props: PermissionTemplateProps
│   ├── Id: IdValueObject
│   ├── Code: TemplateCode
│   ├── Name: TemplateName
│   ├── Description: string
│   └── IsActive: bool
└── Children
    └── IReadOnlyList<PermissionTemplateItem>
```

---

## 3. Object Model Diagrams

```mermaid
classDiagram
    class PermissionTemplate {
        +Guid Id
        +TemplateCode Code
        +TemplateName Name
        +string Description
        +bool IsActive
        +List~PermissionTemplateItem~ Items
        +Create()
        +AddItem()
        +RemoveItem()
    }
    class PermissionTemplateItem {
        +Guid Id
        +Guid TemplateId
        +Guid ActionId
        +string PermissionKey
    }
    PermissionTemplate "1" *-- "0..*" PermissionTemplateItem
```

---

## 4. Sequence Diagrams

### Create Template Flow
```mermaid
sequenceDiagram
    participant C as Client
    participant H as CreateTemplateHandler
    participant R as IPermissionTemplateRepository
    participant T as PermissionTemplate (AR)

    C->H: CreatePermissionTemplateCommand(code, name, description)
    H->R: ExistsByCode(code)
    R-->>H: false
    H->T: PermissionTemplate.Create(id, code, name, description)
    T->T: Raise PermissionTemplateCreatedEvent
    H->R: Add(template)
    R-->>H: ok
    H-->>C: TemplateId
```

---

## 5. ER Model

```mermaid
erDiagram
    PERMISSION_TEMPLATE ||--o{ PERMISSION_TEMPLATE_ITEM : "contains"
    ACTION ||--o{ PERMISSION_TEMPLATE_ITEM : "references"

    PERMISSION_TEMPLATE {
        uniqueidentifier TemplateId PK
        nvarchar Code "Unique"
        nvarchar Name
        nvarchar Description
        bit IsActive
    }
    PERMISSION_TEMPLATE_ITEM {
        uniqueidentifier ItemId PK
        uniqueidentifier TemplateId FK
        uniqueidentifier ActionId FK
        nvarchar PermissionKey
    }
```

### Tenant Isolation Rules
- Templates can be configured as **Global** (available platform-wide to all tenants) or **Tenant-Scoped** (available only to the tenant that authored them). Tenant-scoped tables include a nullable `TenantId` column.

---

## 6. Bounded Context Integration
- Consumes `Action` metadata from `SystemSuite` aggregates.
- Downstream profiles consume these templates to seed default role permissions.

---

## 7. Application Layer
- `CreatePermissionTemplateCommand` -> Inputs: `Code, Name, Description` -> Returns: `Guid`

---

## 8. Infrastructure/Persistence
- Index: Unique index on `Code` and index on `TenantId`.

---

## 9. Security & Compliance
- Editing global templates is restricted to `Platform:Admin`.
- Tenant-scoped template creation is restricted to `Tenant:Admin`.

---

## 10. Technical Decisions
- Standardizing seed templates prevents manual setting fatigue during new organization registration.

---

**[Back to Authorization Index](./index.md)**
