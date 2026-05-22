# ApprovalWorkflow — Aggregate Architecture

**Bounded Context:** Approvals  
**Aggregate Root:** `ApprovalWorkflow`  
**Module:** `Ums.Domain.Approvals.ApprovalWorkflow`  
**Status:** Production

---

## 1. Aggregate Overview

### Purpose
The `ApprovalWorkflow` aggregate establishes dynamic routing rules and document checklists for operations requiring administrative oversight. It ensures that certain user actions—such as requesting profile promotions or submitting sensitive files—trigger the appropriate human authorization flows and define what supporting files are mandatory.

### Business Responsibility
- Register and coordinate tenant-scoped approval schemas.
- Target workflows to specific suites or user classifications.
- Declare a checklist of required supporting documents.
- Determine whether dynamic approval is active.

### Aggregate Root
`ApprovalWorkflow` is the aggregate root. Adding or removing required documents must flow through it to maintain integrity.

### Invariants and Consistency Rules
1. Every `ApprovalWorkflow` must follow the code-name-description corporate template.
2. The `Code` parameter must be unique within the active `TenantId`.
3. If `RequiresApproval` is true, the workflow must have at least one valid approver group or checklist criteria defined.
4. Each required document type mapping must be unique; a workflow cannot duplicate required document types.

### Related Entities / Value Objects
| Entity / VO | Type | Ownership |
|---|---|---|
| `ApprovalWorkflowId` | Value Object | Guid-based aggregate root identifier |
| `ApprovalRequiredDocument` | Entity | Owned (see [approval-required-document.md](./approval-required-document.md)) |
| `UserCategory` | Enum | INTERNAL · EXTERNAL · AUDITOR |
| `AuditValueObject` | Value Object | Tracks creation and modification metadata |

### Domain Events
| Event | Trigger |
|---|---|
| `ApprovalWorkflowCreatedEvent` | A new workflow definition is registered |
| `RequiredDocumentAddedEvent` | A document requirement mapping is added to the checklist |
| `RequiredDocumentRemovedEvent` | A document requirement mapping is removed |

### Commands / Use Cases
| Command | Description |
|---|---|
| `CreateApprovalWorkflowCommand` | Initialize a new approval workflow mapping |
| `AddRequiredDocumentToWorkflowCommand` | Bind a DocumentType as a mandate to complete the workflow |
| `RemoveRequiredDocumentFromWorkflowCommand` | Remove a DocumentType constraint from the checklist |

### Repository / Service Boundaries
- `IApprovalWorkflowRepository` — Persists and loads workflows.
- Queries are strictly isolated and filtered by the current `TenantId` session.

---

## 2. Domain Model

### Classes / Entities / Value Objects
```
ApprovalWorkflow (Aggregate Root)
├── Props: ApprovalWorkflowProps
│   ├── Id: ApprovalWorkflowId
│   ├── TenantId: TenantId
│   ├── SystemSuiteId?: SystemSuiteId
│   ├── Code: Code
│   ├── Name: Name
│   ├── Description: Description
│   ├── TargetUserCategory: UserCategory
│   ├── RequiresApproval: bool
│   └── Audit: AuditValueObject
└── Children
    └── IReadOnlyCollection<ApprovalRequiredDocument>
```

---

## 3. Object Model Diagrams

```mermaid
classDiagram
    direction LR
    class ApprovalWorkflow {
        +Guid Id
        +Guid TenantId
        +Guid? SystemSuiteId
        +Code Code
        +Name Name
        +Description Description
        +UserCategory TargetUserCategory
        +bool RequiresApproval
        +List~ApprovalRequiredDocument~ RequiredDocuments
        +Create()
        +AddRequiredDocument()
        +RemoveRequiredDocument()
    }
    class UserCategory {
        <<enumeration>>
        INTERNAL
        EXTERNAL
        AUDITOR
    }
    class ApprovalRequiredDocument {
        +Guid Id
        +Guid WorkflowId
        +Guid DocumentTypeId
        +bool IsMandatory
    }
    ApprovalWorkflow "1" *-- "0..*" ApprovalRequiredDocument
    ApprovalWorkflow "1" *-- "1" UserCategory
```

---

## 4. Sequence Diagrams

### Add Required Document Flow
```mermaid
sequenceDiagram
    participant C as TenantAdmin
    participant H as AddDocHandler
    participant R as IApprovalWorkflowRepository
    participant W as ApprovalWorkflow (AR)

    C->H: AddRequiredDocumentToWorkflowCommand(workflowId, docTypeId, isMandatory)
    H->R: GetById(workflowId)
    R-->>H: ApprovalWorkflow (AR)
    H->W: AddRequiredDocument(docTypeId, isMandatory, actorId)
    W->W: Validate document type uniqueness
    W->W: Raise RequiredDocumentAddedEvent
    H->R: Save(workflow)
    R-->>H: ok
    H-->>C: ok
```

---

## 5. ER Model

```mermaid
erDiagram
    TENANT ||--o{ APPROVAL_WORKFLOW : "governs"
    SYSTEM_SUITE ||--o{ APPROVAL_WORKFLOW : "scopes"
    APPROVAL_WORKFLOW ||--o{ APPROVAL_REQUIRED_DOCUMENT : "demands"

    APPROVAL_WORKFLOW {
        uniqueidentifier WorkflowId PK
        uniqueidentifier TenantId FK
        uniqueidentifier SystemSuiteId FK "Nullable"
        nvarchar Code "Unique per TenantId"
        nvarchar Name
        nvarchar Description
        nvarchar TargetUserCategory "INTERNAL-EXTERNAL-AUDITOR"
        bit RequiresApproval
        datetime2 UpdatedAt
        uniqueidentifier UpdatedBy
    }
    APPROVAL_REQUIRED_DOCUMENT {
        uniqueidentifier RequiredDocId PK
        uniqueidentifier WorkflowId FK
        uniqueidentifier DocumentTypeId FK
        bit IsMandatory
    }
```

### Tenant Isolation Rules
- All `APPROVAL_WORKFLOW` records are partitioned by `TenantId`. Direct database queries require application repository filtering (R-10).

---

## 6. Bounded Context Integration
- **Upstream**: Fetches optional `SystemSuiteId` from Authorization context.
- **Downstream**: Consulted by `ApprovalRequest` to verify submission checklists, and `PromotionRequest` in IGA context to check authorization mandates.

---

## 7. Application Layer
- `CreateApprovalWorkflowCommand` -> Inputs: `TenantId, Code, Name, Description, UserCategory, RequiresApproval, SystemSuiteId?` -> Returns: `Guid`
- `AddRequiredDocumentCommand` -> Inputs: `WorkflowId, DocumentTypeId, IsMandatory` -> Returns: `void`

---

## 8. Infrastructure/Persistence
- Index: Unique index on `TenantId, Code` to avoid overlapping codes.
- Transaction: Modifications to the workflow checklist are saved atomically in a single DbContext transaction.

---

## 9. Security & Compliance
- Designing workflows: Restricted to `Tenant:Admin` or higher roles.
- Compliance: Any change to an approval checklist triggers audit logs to secure procedural paths.

---

## 10. Technical Decisions
- Declaring separate required document junction tables ensures modular relationships between workflows and documents without locking the tables.

---

**[Back to Approvals Index](./index.md)**
