# FeatureFlag — Aggregate Architecture

**Bounded Context:** Configuration  
**Aggregate Root:** `FeatureFlag`  
**Module:** `Ums.Domain.Configuration.FeatureFlag`  
**Status:** Production

---

## 1. Aggregate Overview

### Purpose
The `FeatureFlag` aggregate is the system's operational control switch. It defines platform-wide (global) or tenant-specific feature flags and rules. These flags control dynamic code paths, enabling features like dark launches, canary releases, percentage-based rollouts, and granular user/branch targeting without redeploying code.

### Business Responsibility
- Register and define feature flags with unique identifiers.
- Configure dynamic rollout strategies (boolean toggles, percentage allocations, targeting rules).
- Allow administrative control (enabling, disabling, pausing) over features at runtime.
- Maintain environment-specific toggle states (Development, Staging, Production).
- Enforce targeting based on user roles, branch locations, and system suites.

### Aggregate Root
`FeatureFlag` is the aggregate root. All updates to toggle states or dynamic evaluation rules must flow through the aggregate root commands to ensure consistency and validate invariants.

### Invariants and Consistency Rules
1. A Feature Flag key must be unique. Global flags (`TenantId IS NULL`) must be unique across the entire platform. Tenant-scoped flags must be unique within that `TenantId`.
2. The key must follow the strict kebab-case format (e.g. `billing.new-checkout-flow`).
3. For a `PERCENTAGE` rollout strategy, the rollout percentage must be an integer between 0 and 100 inclusive.
4. For a `TARGETED` strategy, there must be at least one active targeting rule (e.g. specific user list, branch list, or role list).
5. Active feature flags in a production environment cannot be permanently deleted; they must be archived or deactivated to maintain historic context for evaluation logs.

### Related Entities / Value Objects
| Entity / VO | Type | Ownership |
|---|---|---|
| `FeatureFlagId` | Value Object | Guid-based aggregate root identifier |
| `FeatureFlagKey` | Value Object | Unique, validated kebab-case identifier string |
| `RolloutStrategy` | Enum | BOOLEAN · PERCENTAGE · TARGETED |
| `TargetingRule` | Entity | Owned child entity containing rule match criteria |
| `AuditValueObject` | Value Object | Tracks creation and modification metadata |

### Domain Events
| Event | Trigger |
|---|---|
| `FeatureFlagCreatedEvent` | A new feature flag is registered in the system |
| `FeatureFlagToggledEvent` | A flag's active status is changed (Enabled/Disabled) |
| `RolloutStrategyChangedEvent` | The strategy or percentage rollout parameters are modified |
| `TargetingRulesUpdatedEvent` | The targeted rules list is added, updated, or cleared |
| `FeatureFlagArchivedEvent` | A flag is archived and rendered unavailable for new evaluations |

### Commands / Use Cases
| Command | Description |
|---|---|
| `CreateFeatureFlagCommand` | Register a new feature flag with default settings |
| `ToggleFeatureFlagCommand` | Enable or disable a feature flag instantly |
| `UpdateRolloutStrategyCommand` | Change strategy type or adjust percentage rollout |
| `UpdateTargetingRulesCommand` | Modify specific rules for user/branch segment targetings |
| `ArchiveFeatureFlagCommand` | Archive an active feature flag to prevent further evaluations |

### Repository / Service Boundaries
- `IFeatureFlagRepository` — Handles retrieval and persistence of flags.
- Query filters automatically append `TenantId` for tenant-scoped flags, while permitting read access to Global (`TenantId IS NULL`) flags.
- No write operations are allowed across tenant boundaries.

---

## 2. Domain Model

### Classes / Entities / Value Objects
```
FeatureFlag (Aggregate Root)
├── Props: FeatureFlagProps
│   ├── Id: FeatureFlagId
│   ├── TenantId?: TenantId
│   ├── Key: FeatureFlagKey
│   ├── Description: string
│   ├── Strategy: RolloutStrategy
│   ├── Percentage: int (0..100)
│   ├── IsActive: bool
│   ├── IsArchived: bool
│   └── Audit: AuditValueObject
└── Children
    └── IReadOnlyList<TargetingRule>
```

### Validation Rules
- `Key`: Regex matching `^[a-z0-9]+(?:-[a-z0-9]+)*(?:\.[a-z0-9]+(?:-[a-z0-9]+)*)*$` (standard kebab-case with optional dot nesting).
- `Percentage`: Required if strategy is `PERCENTAGE`, must be between 0 and 100.
- `TargetingRule`: Match rules must have valid operators (EQUALS, IN, NOT_IN) and non-empty criteria lists.

---

## 3. Object Model Diagrams

```mermaid
classDiagram
    direction LR
    class FeatureFlag {
        +Guid Id
        +Guid? TenantId
        +FeatureFlagKey Key
        +string Description
        +RolloutStrategy Strategy
        +int Percentage
        +bool IsActive
        +bool IsArchived
        +List~TargetingRule~ Rules
        +Create()
        +Toggle()
        +UpdateStrategy()
        +UpdateRules()
        +Archive()
    }
    class RolloutStrategy {
        <<enumeration>>
        BOOLEAN
        PERCENTAGE
        TARGETED
    }
    class TargetingRule {
        +Guid Id
        +string AttributeName
        +string Operator
        +List~string~ Values
        +Evaluate(context) bool
    }
    FeatureFlag "1" *-- "1" RolloutStrategy
    FeatureFlag "1" *-- "0..*" TargetingRule
```

---

## 4. Sequence Diagrams

### Update Targeting Rules Flow
```mermaid
sequenceDiagram
    participant C as Administrator
    participant H as UpdateRulesHandler
    participant R as IFeatureFlagRepository
    participant F as FeatureFlag (AR)

    C->>H: UpdateTargetingRulesCommand(flagId, tenantId, rulesDto)
    H->>R: GetById(flagId)
    R-->>H: FeatureFlag (AR)
    H->>F: UpdateRules(rulesDto, actorId)
    F->>F: Validate operators and non-empty values
    F->>F: Apply rules list replacements
    F->>F: Raise TargetingRulesUpdatedEvent
    H->>R: Save(featureFlag)
    R-->>H: ok
    H-->>C: ok
```

---

## 5. ER Model

```mermaid
erDiagram
    TENANT ||--o{ FEATURE_FLAG : "owns"
    FEATURE_FLAG ||--o{ TARGETING_RULE : "contains"

    FEATURE_FLAG {
        uniqueidentifier FlagId PK
        uniqueidentifier TenantId FK "Nullable (Null represents Global)"
        nvarchar Key "Unique per TenantId / Global"
        nvarchar Description
        nvarchar Strategy "BOOLEAN-PERCENTAGE-TARGETED"
        int Percentage
        bit IsActive
        bit IsArchived
        datetime2 UpdatedAt
        uniqueidentifier UpdatedBy
    }
    TARGETING_RULE {
        uniqueidentifier RuleId PK
        uniqueidentifier FlagId FK
        nvarchar AttributeName "e.g., UserId, Role, BranchId"
        nvarchar Operator "EQUALS-IN-NOT_IN"
        nvarchar ValuesJson "Serialized array of matching values"
    }
```

### Tenant Isolation Rules
- Global feature flags (`TenantId IS NULL`) are system-level definitions, read-only to tenant administrators.
- Tenant-scoped feature flags are isolated by `TenantId`. Any modification command checks tenant ownership.

---

## 6. Bounded Context Integration
- **Upstream**: Optionally fetches User, Branch, or Role identifiers from Identity and Authorization bounded contexts to evaluate targeting rules.
- **Downstream**: Consulted by routing layers, React components, and API controllers.
- Evaluation outcomes trigger `FlagEvaluationLog` entries in the same context.

---

## 7. Application Layer
- `CreateFeatureFlagCommand` -> Inputs: `TenantId?, Key, Description, Strategy, Percentage?` -> Returns: `Guid`
- `ToggleFeatureFlagCommand` -> Inputs: `FlagId, TenantId?, IsActive` -> Returns: `void`
- `UpdateTargetingRulesCommand` -> Inputs: `FlagId, TenantId?, List<RuleDto>` -> Returns: `void`
- `EvaluateFeatureFlagQuery` -> Inputs: `TenantId?, Key, UserContext` -> Returns: `EvaluationResultDto`

---

## 8. Infrastructure/Persistence
- Index: Unique index on `TenantId, Key` where `TenantId IS NOT NULL`. Unique index on `Key` where `TenantId IS NULL` (for Global flags).
- Transaction: Rules and toggle updates are atomic; updates save both `FEATURE_FLAG` and its associated `TARGETING_RULE` records in a single database transaction.

---

## 9. Security & Compliance
- Global flags (`TenantId IS NULL`): Restricted to `Platform:Admin` roles.
- Tenant-scoped flags: Modifiable by `Tenant:Admin` for their own tenant.
- Audit Compliance: All toggling actions or rule edits are permanently recorded with actor identifiers to satisfy regulatory auditing protocols.

---

## 10. Technical Decisions
- Storing targeting values as a serialized JSON array (`ValuesJson`) allows extensible, complex segment matchers without bloating the relational schema with excessive junction tables.

---

**[Back to Configuration Index](./index.md)**
