# PromotionRequest — Aggregate Architecture

**Bounded Context:** IGA  
**Aggregate Root:** Yes  
**Module:** `Ums.Domain.IGA.PromotionRequest`  
**Status:** Production

---

## 1. Aggregate Overview

### Purpose
The `PromotionRequest` aggregate coordinates access promotions, allowing users to safely request transitions from their current role to a more privileged target role. It enforces a strict, audited verification path including automatic risk scores, manager approval, security assessments, role execution, and post-execution verification. The aggregate also integrates the `PromotionImpactAnalysis` entity to log security risk assessments, tracking toxic combination indicators, separation of duties (SOD) violations, affected directory structures, and mitigation advice before access is authorized.

### Business Responsibility
- Record the intent of a user to acquire a more senior or privileged target role.
- Control the multi-step approval workflow.
- Quantify access promotion risks into a unified score (0 to 100).
- Identify permission conflicts and toxic combinations.
- List affected software systems and resources.
- Embed toxic-permission impact analysis results (`PromotionImpactAnalysis`) to provide security auditors with recommended mitigation policies.
- Coordinate execution and post-change confirmation states.

### Aggregate Root
`PromotionRequest` serves as the aggregate root, managing the lifecycle of the promotion process and housing the `PromotionImpactAnalysis` as an owned entity.

### Invariants and Consistency Rules
1. **INV-PR1 (Workflow State Transitions):** State transitions are strictly governed by the following FSM rules:
   - Creation puts the request into `Draft`.
   - `Draft` $\rightarrow$ `PendingManagerApproval` (via `Submit`).
   - `PendingManagerApproval` $\rightarrow$ `PendingSecurityReview` (via `ManagerApprove`) OR `Rejected` (via `ManagerReject`).
   - `PendingSecurityReview` $\rightarrow$ `ApprovedReadyToExecute` (via `SecurityReviewLowRisk` if the analyzed risk score is low) OR `PendingSecurityApproval` (via `SecurityReviewHighRisk` if risk score is high) OR `Rejected` (via `SecurityReject`).
   - `PendingSecurityApproval` $\rightarrow$ `ApprovedReadyToExecute` (via `SecurityApprove`) OR `Rejected` (via `SecurityReject`).
   - `ApprovedReadyToExecute` $\rightarrow$ `Executed` (via `Execute`).
   - `Executed` $\rightarrow$ `Verified` (via `Verify`) OR `VerificationFailed` (via `MarkVerificationFailed`).
2. **INV-PR2 (Impact Analysis Uniqueness):** Only one impact analysis can be recorded per promotion request to prevent history rewriting (`DomainErrors.IGA.ImpactAnalysisAlreadyExists`).
3. **INV-PR3 (Risk Score Limits):** The `RiskScore` within the impact analysis must be a decimal value strictly between `0` and `100` inclusive (`DomainErrors.IGA.InvalidPerformanceScore`).
4. **INV-PR4 (Scans Immutability):** Once calculated and saved, an impact analysis cannot be edited. If access scopes change, a brand new promotion cycle must begin.

### Related Entities / Value Objects
| Entity / VO | Type | Description |
|---|---|---|
| `PromotionRequestId` | Value Object | Unique aggregate identifier |
| `PromotionImpactAnalysisId` | Value Object | Unique identifier for the impact analysis entity |
| `TenantId` | Value Object | Partition identifier mapping to the tenant context |
| `UserId` | Value Object | Target user reference (Identity Context) |
| `RoleId` | Value Object | Reference to current and target roles (Authorization Context) |
| `PromotionStatus` | Enum | FSM status enum (`Draft`, `PendingManagerApproval`, etc.) |
| `ApprovalDecision` | Enum | `None` · `Approved` · `Rejected` |
| `PromotionImpactAnalysis` | Entity | Owned child entity containing risk metrics |
| `TextValueObject` | Value Object | General string properties (RequestReason, RiskLevel, Mitigations, ConflictingPermissions) |

---

## 2. Domain Model

### Classes / Entities / Value Objects
```text
PromotionRequest (Aggregate Root)
├── Props: PromotionRequestProps
│   ├── Id: PromotionRequestId
│   ├── TenantId: TenantId
│   ├── UserId: UserId (External Ref)
│   ├── CurrentRoleId: RoleId (External Ref)
│   ├── TargetRoleId: RoleId (External Ref)
│   ├── RequestedAt: DateTime
│   ├── RequestedBy: ActorId
│   ├── RequestReason: TextValueObject?
│   ├── ManagerId: UserId
│   ├── ManagerApprovalStatus: ApprovalDecision
│   ├── ManagerDecisionAt: DateTime?
│   ├── SecurityApprovalStatus: ApprovalDecision
│   ├── SecurityDecisionAt: DateTime?
│   ├── Status: PromotionStatus
│   ├── ExecutedAt: DateTime?
│   ├── ExecutedBy: ActorId?
│   ├── VerifiedAt: DateTime?
│   └── Audit: AuditValueObject
└── ImpactAnalyses: PromotionImpactAnalysis[] (Child Collection)
    └── Props: PromotionImpactAnalysisProps
        ├── Id: IdValueObject
        ├── PromotionRequestId: PromotionRequestId
        ├── RiskScore: decimal
        ├── RiskLevel: TextValueObject
        ├── NewPermissionsCount: int
        ├── RemovedPermissionsCount: int
        ├── AffectedSystemsCount: int
        ├── ConflictingPermissions: TextValueObject?
        ├── RiskFactors: TextValueObject?
        ├── SuggestedMitigations: TextValueObject?
        ├── AnalyzedAt: DateTime
        └── AnalyzedBy: TextValueObject?
```

---

## 3. Object Model Diagrams

```mermaid
classDiagram
    direction TB
    class PromotionRequest {
        +PromotionRequestProps Props
        +IReadOnlyCollection~PromotionImpactAnalysis~ ImpactAnalyses
        +Create(TenantId, UserId, RoleId, RoleId, UserId, TextValueObject?, ActorId) Result~PromotionRequest~
        +Submit(ActorId) Result
        +ManagerApprove(ActorId) Result
        +ManagerReject(ActorId) Result
        +SecurityReviewLowRisk(ActorId) Result
        +SecurityReviewHighRisk(ActorId) Result
        +SecurityApprove(ActorId) Result
        +SecurityReject(ActorId) Result
        +Execute(ActorId) Result
        +Verify(ActorId) Result
        +MarkVerificationFailed(ActorId) Result
        +AddImpactAnalysis(...) Result
    }
    class PromotionRequestProps {
        +IdValueObject Id
        +TenantId TenantId
        +UserId UserId
        +RoleId CurrentRoleId
        +RoleId TargetRoleId
        +DateTime RequestedAt
        +ActorId RequestedBy
        +TextValueObject RequestReason
        +UserId ManagerId
        +ApprovalDecision ManagerApprovalStatus
        +ApprovalDecision SecurityApprovalStatus
        +PromotionStatus Status
        +AuditValueObject Audit
    }
    class PromotionImpactAnalysis {
        +Guid Id
        +decimal RiskScore
        +TextValueObject RiskLevel
        +int NewPermissionsCount
        +int RemovedPermissionsCount
        +int AffectedSystemsCount
        +TextValueObject ConflictingPermissions
        +TextValueObject RiskFactors
        +TextValueObject SuggestedMitigations
        +DateTime AnalyzedAt
        +TextValueObject AnalyzedBy
        +Create() Result~PromotionImpactAnalysis~
    }
    class PromotionStatus {
        <<enumeration>>
        Draft
        PendingManagerApproval
        PendingSecurityReview
        PendingSecurityApproval
        ApprovedReadyToExecute
        Executed
        Verified
        Rejected
        VerificationFailed
    }

    PromotionRequest *-- PromotionRequestProps
    PromotionRequest "1" *-- "0..1" PromotionImpactAnalysis : owns
    PromotionRequestProps --> PromotionStatus
```

---

## 4. Sequence Diagrams

### High-Risk Promotion Process

```mermaid
sequenceDiagram
    autonumber
    actor Mgr as Manager
    actor Sec as Security Auditor
    participant App as Application Service
    participant PR as PromotionRequest [Aggregate]
    participant Repo as PromotionRequestRepository
    participant DB as SQL Server

    Note over Mgr, PR: Request is in PendingManagerApproval state
    Mgr->>App: ApprovePromotionRequest(RequestId)
    App->>Repo: GetByIdAsync(RequestId)
    Repo-->>App: PromotionRequest
    App->>PR: ManagerApprove(ManagerId)
    note over PR: Status = PendingSecurityReview
    PR-->>App: Success
    App->>Repo: SaveAsync(PR)
    
    Note over Sec, PR: Security Review discovers High-Risk Score (e.g. SOD conflict)
    Sec->>App: AddImpactAnalysis(RiskScore: 85)
    App->>PR: AddImpactAnalysis(...)
    PR-->>App: Success
    Sec->>App: ReviewHighRisk(SecurityId)
    App->>PR: SecurityReviewHighRisk(SecurityId)
    note over PR: Status = PendingSecurityApproval
    PR-->>App: Success
    
    Note over Sec, PR: Executive Security Approval
    Sec->>App: SecurityApprove(SecurityId)
    App->>PR: SecurityApprove(SecurityId)
    note over PR: Status = ApprovedReadyToExecute
    PR-->>App: Success
    App->>Repo: SaveAsync(PR)
    Repo->>DB: UPDATE PROMOTION_REQUEST Status = 'ApprovedReadyToExecute'
    DB-->>Repo: Acknowledge
```

---

## 5. ER Model

```mermaid
erDiagram
    PROMOTION_REQUEST ||--o| PROMOTION_IMPACT_ANALYSIS : "assessed by"

    PROMOTION_REQUEST {
        uniqueidentifier PromotionRequestId PK
        uniqueidentifier TenantId FK
        uniqueidentifier UserId FK "Identity Context"
        uniqueidentifier CurrentRoleId FK "Authorization Context"
        uniqueidentifier TargetRoleId FK "Authorization Context"
        datetime2 RequestedAt
        nvarchar RequestedBy
        nvarchar RequestReason
        uniqueidentifier ManagerId
        nvarchar ManagerApprovalStatus
        datetime2 ManagerDecisionAt
        nvarchar SecurityApprovalStatus
        datetime2 SecurityDecisionAt
        nvarchar Status
        datetime2 ExecutedAt
        nvarchar ExecutedBy
        datetime2 VerifiedAt
        nvarchar CreatedBy
        datetime2 CreatedAt
    }

    PROMOTION_IMPACT_ANALYSIS {
        uniqueidentifier AnalysisId PK
        uniqueidentifier PromotionRequestId FK
        decimal RiskScore
        nvarchar RiskLevel
        int NewPermissionsCount
        int RemovedPermissionsCount
        int AffectedSystemsCount
        nvarchar ConflictingPermissions
        nvarchar RiskFactors
        nvarchar SuggestedMitigations
        datetime2 AnalyzedAt
        nvarchar AnalyzedBy
    }
```

### Tenant Isolation Rules
- Partitioned by `TenantId`. Submissions are verified against tenant configuration properties to prevent cross-tenant request forgery. Inherits scoping rules to all child entities like `PromotionImpactAnalysis`, blocking access between tenants implicitly.

---

## 6. Bounded Context Integration

```mermaid
flowchart TD
    subgraph IdentityContext [Identity Context]
        U[UserAccount]
    end

    subgraph AuthContext [Authorization Context]
        R1[Current Role]
        R2[Target Role]
    end

    subgraph IgaContext [IGA Context]
        PR[PromotionRequest]
        PIA[PromotionImpactAnalysis]
    end

    PR -.->|references UserId| U
    PR -.->|references| R1
    PR -.->|references| R2
    PR *--|owns| PIA
```

---

## 7. Application Layer

### Commands & Queries
- **CreatePromotionRequestCommand:** Creates a request in `Draft`.
- **SubmitPromotionRequestCommand:** Submits a request to manager review.
- **ManagerApprovePromotionRequestCommand:** Records a manager's verification.
- **SecurityReviewPromotionRequestCommand:** Records dynamic performance analysis and triggers risk branching.
- **AddPromotionImpactAnalysisCommand:** Managed internally/asynchronously to coordinate adding the impact analysis via application handlers.
- **ExecutePromotionRequestCommand:** Executes the role change in target systems.
- **VerifyPromotionRequestCommand:** Final compliance sign-off validating successful promotion propagation.

---

## 8. Infrastructure/Persistence

### EF Core Mapping Configuration
```csharp
public class PromotionRequestConfiguration : IEntityTypeConfiguration<PromotionRequest>
{
    public void Configure(EntityTypeBuilder<PromotionRequest> builder)
    {
        builder.ToTable("PROMOTION_REQUEST");
        builder.HasKey(e => e.Id);
        
        builder.OwnsOne(e => e.Props, props =>
        {
            props.Property(p => p.Id).HasColumnName("PromotionRequestId");
            props.Property(p => p.TenantId).HasColumnName("TenantId");
            props.Property(p => p.UserId).HasColumnName("UserId");
            props.Property(p => p.CurrentRoleId).HasColumnName("CurrentRoleId");
            props.Property(p => p.TargetRoleId).HasColumnName("TargetRoleId");
            props.Property(p => p.RequestedAt).HasColumnName("RequestedAt");
            props.Property(p => p.RequestedBy).HasConversion(a => a.GetValue(), s => ActorId.Load(s)).HasColumnName("RequestedBy");
            props.Property(p => p.RequestReason).HasConversion(p => p.GetValue(), s => TextValueObject.Create(s).Value).HasColumnName("RequestReason");
            props.Property(p => p.ManagerId).HasColumnName("ManagerId");
            props.Property(p => p.ManagerApprovalStatus).HasConversion<string>().HasColumnName("ManagerApprovalStatus");
            props.Property(p => p.ManagerDecisionAt).HasColumnName("ManagerDecisionAt");
            props.Property(p => p.SecurityApprovalStatus).HasConversion<string>().HasColumnName("SecurityApprovalStatus");
            props.Property(p => p.SecurityDecisionAt).HasColumnName("SecurityDecisionAt");
            props.Property(p => p.Status).HasConversion<string>().HasColumnName("Status");
            props.Property(p => p.ExecutedAt).HasColumnName("ExecutedAt");
            props.Property(p => p.ExecutedBy).HasConversion(a => a == null ? (Guid?)null : a.GetValue(), s => s == null ? null : ActorId.Load(s.Value)).HasColumnName("ExecutedBy");
            props.Property(p => p.VerifiedAt).HasColumnName("VerifiedAt");
            props.OwnsOne(p => p.Audit);
        });

        builder.HasMany(e => e.ImpactAnalyses)
               .WithOne()
               .HasForeignKey("PromotionRequestId")
               .OnDelete(DeleteBehavior.Cascade); // Cascade delete on foreign keys guarantees database consistency
    }
}
```

---

## 9. Security & Compliance

- **Segregation of Duties (SOD):** The manager (`ManagerId`) authorized to approve a promotion request cannot be the target user (`UserId`) or the security auditor performing the security assessment.
- **Risk Branching:** Requests with high-risk impact analysis are routed to an extra step (`PendingSecurityApproval`), preventing automatic role additions without specialized sign-off.
- **Audit Immutability:** Impact analysis data is strictly read-only once saved. This prevents actors from downplaying toxic combos to sneak permission additions past auditors.

---

## 10. Technical Decisions

- **Asynchronous Risk Calculation:** Generating toxic permission scans requires complex graph analytics. Therefore, it is decoupled into a background analytical task that returns a `PromotionImpactAnalysis` entity rather than blocking application-layer write workflows synchronously. Decoupling permission calculation tasks from command processing routes via background queues prevents blocking user execution flows while heavy permission graph analysis runs.

---

**[Back to IGA Index](./index.md)**
