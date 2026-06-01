# TenantSignupRequest - Aggregate Architecture

**Bounded Context:** Identity  
**Aggregate Root:** `TenantSignupRequest`  
**Module:** `Ums.Domain.Identity.TenantSignupRequest`  
**Status:** Implemented for Phase 4 tenant onboarding

---

## 1. Aggregate Overview

### Purpose
`TenantSignupRequest` represents a public company onboarding request submitted before a tenant exists. It is reviewed by a global administrator and, when approved, links to the newly created tenant.

### Business Responsibility
- Capture company name, company reference, contact name, and contact email from the public tenant signup form.
- Keep the request in `Pending` until a global administrator approves it.
- Link the request to the created tenant through `ApprovedTenantId`.
- Provide the source record for the global onboarding inbox.

### Implemented State Model
| State | Code Value | Meaning | Implemented Transition |
|---|---:|---|---|
| `Pending` | 1 | Request submitted and waiting for global review. | Created by `TenantSignupRequest.Create`. |
| `Approved` | 2 | Tenant was created and linked to the request. | `Approve(tenantId, updatedBy)`. |
| `Rejected` | 3 | Reserved in the enum for denied company requests. | Enum exists; aggregate command is not implemented yet. |

### Related Entities / Value Objects
| Entity / VO | Type | Ownership |
|---|---|---|
| `TenantSignupRequestStatus` | Enumeration | Pending, Approved, Rejected |
| `CompanyReference` | Value Object | Company RUC/code/reference |
| `Name` | Value Object | Company and contact names |
| `Email` | Value Object | Contact email |
| `TenantId` | Value Object | Nullable reference set after approval |
| `AuditValueObject` | Value Object | Created and updated metadata |

---

## 2. Object Model

```mermaid
classDiagram
    direction LR
    class TenantSignupRequest {
        +Guid Id
        +Name CompanyName
        +CompanyReference CompanyReference
        +Name ContactName
        +Email ContactEmail
        +TenantSignupRequestStatus Status
        +TenantId? ApprovedTenantId
        +Create()
        +Approve()
    }
    class TenantSignupRequestStatus {
        <<enumeration>>
        Pending
        Approved
        Rejected
    }
    TenantSignupRequest "1" *-- "1" TenantSignupRequestStatus
```

---

## 3. ER Model

```mermaid
erDiagram
    TENANT_SIGNUP_REQUEST }o--o| TENANT : "approved_as"

    TENANT_SIGNUP_REQUEST {
        uniqueidentifier Id PK
        nvarchar CompanyName
        nvarchar CompanyReference "Unique"
        nvarchar ContactName
        nvarchar ContactEmail
        int StatusId "1=Pending, 2=Approved, 3=Rejected"
        uniqueidentifier ApprovedTenantId FK "Nullable"
        nvarchar CreatedBy
        datetime2 CreatedAtUtc
        nvarchar UpdatedBy "Nullable"
        datetime2 UpdatedAtUtc "Nullable"
        nvarchar AuditTimeSpan
        rowversion RowVersion
    }
```

### Persistence Mapping
| Code Artifact | Mapping |
|---|---|
| EF record | `TenantSignupRequestRecord` |
| Table | `identity.TenantSignupRequests` |
| Unique index | `CompanyReference` |
| Query index | `StatusId`, `ContactEmail` |
| Repository | `ITenantSignupRequestRepository` |

---

## 4. Onboarding Alignment

This aggregate implements the company onboarding request from FS-21 and EP-09. It is global in scope because the tenant does not exist yet. Tenant isolation starts after approval when `ApprovedTenantId` references the created tenant.

---

**[Back to Identity Index](./index.md)**
