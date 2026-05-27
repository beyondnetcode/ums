# UserAccount — Aggregate Architecture

**Bounded Context:** Identity  
**Aggregate Root:** `UserAccount`  
**Module:** `Ums.Domain.Identity.UserAccount`  
**Status:** Production

---

## 1. Aggregate Overview

### Purpose
The `UserAccount` aggregate represents a user's identity within a tenant. It governs registration, activation, blocking, external IdP linkage, password credential management, and MFA enrollment. It is the primary identity object referenced by all other bounded contexts. It fully owns the `PasswordCredential` and `MfaEnrollment` child entities.

> **Delegated Administration:** A `UserAccount` with an administrative role may receive a `UserManagementDelegation` from another administrator, granting them the authority to manage a restricted set of users within a defined scope (tenant, organization, department, system, or team). This authority is enforced at the application layer by checking for an `ACTIVE` delegation before processing restricted commands. See [`UserManagementDelegation`](./user-management-delegation.md) · [FS-14](../../governance/requirements/functional-stories/fs-14-delegated-management.md).

### Business Responsibility
- Register users within a tenant, assigning them a category and email.
- Track lifecycle: Pending → Active → Blocked → Active.
- Link users to an external identity reference from an authoritative source system (HR, vendor, government, partner), while authentication protocol selection remains governed by tenant/provider strategy.
- Own `PasswordCredential` and `MfaEnrollment` child entities.
- Emit authentication-related domain events (auth attempted, MFA enrolled/verified).

### Aggregate Root
`UserAccount` is the aggregate root. `PasswordCredential` and `MfaEnrollment` must be managed exclusively through `UserAccount` commands. External aggregates hold `UserId` references only.

### Invariants and Consistency Rules
1. `Email` must be unique per `TenantId`.
2. A `UserAccount` in status `Blocked` cannot authenticate.
3. A `UserAccount` in status `Pending` cannot authenticate; a local password may be provisioned before activation.
4. At most one `PasswordCredential` can be `IsActive = true` at any time.
5. Setting a new password automatically deactivates the previous active credential.
6. Historical credentials (IsActive = false) are retained for audit — never physically deleted.
7. `PasswordHash` must be a valid BCrypt hash (validated by domain service before assignment).
8. `IdentityReference` and `IdentityReferenceType` must be set together or both null.
9. A `FEDERATED` user (has `IdentityReference`) should not have an active `PasswordCredential`.
10. Multiple `MfaEnrollment` records may exist (one per method), but each method may only be enrolled once per user.
11. MFA enrollment status values follow the implemented model: `NotEnrolled`, `Enrolled`, `Verified`. New enrollments start in `Enrolled` and move to `Verified` once the challenge is confirmed.
12. `UserAccount.Status` must not be `Blocked` to enroll a new MFA method.
13. At least one enrolled MFA method must remain if the tenant requires MFA.
14. A `UserAccount` acting as a delegated admin may only execute management commands (`RegisterUserCommand`, `BlockUserCommand`, `AssignProfileCommand`) on users within their `ACTIVE` delegation scope — validated by `IDelegationScopeValidator` at application layer.
15. A `UserAccount` cannot delegate authority it does not itself possess (`no-elevation` rule — INV-DEL1).

### Related Entities / Value Objects
| Entity / VO | Type | Ownership |
|---|---|---|
| `PasswordCredential` | Entity | Owned — child of UserAccount |
| `MfaEnrollment` | Entity | Owned — child of UserAccount |
| `TenantId` | Value Object | FK reference to Tenant |
| `BranchId` | Value Object | FK reference to Branch (optional scope). Supported in props, persistence, application contracts, and aggregate creation flow. |
| `Email` | Value Object | Validated email |
| `UserCategory` | Enum | INTERNAL · EXTERNAL · B2B · PARTNER |
| `DelegationId` | Value Object | Reference to `UserManagementDelegation` — set in application context, not stored on aggregate |
| `UserStatus` | Enum | Pending · Active · Blocked |
| `IdentityReference` | Value Object | External master-data reference from the authoritative source system |
| `IdentityReferenceType` | Enum | HR_ID · VENDOR_CODE · GOVERNMENT_ID · PARTNER_REF |
| `PasswordHash` | Value Object | Validated BCrypt hash string |
| `MfaMethod` | Enum | TOTP · SMS · EMAIL · WEBAUTHN |
| `MfaEnrollmentStatus` | Enum | NotEnrolled · Enrolled · Verified |
| `AuditValueObject` | Value Object | CreatedAt/By, UpdatedAt/By |

### Domain Events
| Event | Trigger |
|---|---|
| `UserRegisteredEvent` | New user created in the system (by direct admin or delegated admin) |
| `UserActivatedEvent` | User moved from Pending or Blocked to Active |
| `UserBlockedEvent` | User blocked (compliance or manual action) |
| `UserRestoredEvent` | Blocked user restored to Active |
| `MfaEnrolledEvent` | New MFA method enrolled |
| `MfaVerifiedEvent` | MFA challenge successfully verified |
| `AuthenticationAttemptedEvent` | Login attempt recorded (success or failure) |

### Commands / Use Cases
| Command | Description |
|---|---|
| `RegisterUserCommand` | Register a new user within a tenant |
| `ActivateUserCommand` | Activate a pending or blocked user |
| `BlockUserCommand` | Block a user (compliance enforcement or manual) |
| `RestoreUserCommand` | Restore a blocked user to Active |
| `AddUserAccountPasswordCommand` | Create or rotate the active local password credential using server-side hashing |
| `LinkExternalIdentityCommand` | Associate an external authoritative-source identity reference |
| `EnrollMfaCommand` | Enroll a new MFA method |
| `VerifyMfaCommand` | Confirm MFA challenge (transitions Enrolled → Verified) |

### Repository / Service Boundaries
- `IUserAccountRepository` — persists `UserAccount` aggregate including owned credentials and enrollments.
- `IPasswordHashingService` — domain service for BCrypt hashing.
- `IEmailUniquenessChecker` — domain service to verify email uniqueness within a tenant.
- `IMfaChallengeService` — infrastructure service handling OTP generation/TOTP validation.
- `IDelegationScopeValidator` — application service; validates that the acting admin has an `ACTIVE` `UserManagementDelegation` covering the target user and requested action before processing delegation-gated commands.

---

## 2. Object Model

### Classes / Entities / Value Objects

```
UserAccount (Aggregate Root)
├── Props: UserAccountProps
│   ├── Id: IdValueObject
│   ├── TenantId: TenantId
│   ├── BranchId?: BranchId
│   ├── Email: Email
│   ├── Category: UserCategory
│   ├── Status: UserStatus
│   ├── IdentityReference?: IdentityReference
│   ├── IdentityReferenceType?: IdentityReferenceType
│   └── Audit: AuditValueObject
├── Children
│   ├── PasswordCredential? (0..N stored, 0..1 active)
│   │   └── Props: PasswordCredentialProps (Id, UserAccountId, PasswordHash, IsActive)
│   └── IReadOnlyList<MfaEnrollment>
│       └── Props: MfaEnrollmentProps (Id, UserAccountId, Method, Status)
└── DomainEvents: UserAccountDomainEventsManager
```

### Main Attributes
| Attribute | Entity | Type | Notes |
|---|---|---|---|
| `Id` | UserAccount | `Guid` | PK |
| `TenantId` | UserAccount | `Guid` | FK — RLS scope |
| `BranchId` | UserAccount | `Guid?` | Optional branch scope |
| `Email` | UserAccount | `string` | Unique per tenant |
| `Category` | UserAccount | `UserCategory` | Classification |
| `Status` | UserAccount | `UserStatus` | Lifecycle state |
| `IdentityReference` | UserAccount | `string?` | External master-data reference |
| `PasswordHash` | PasswordCredential | `string` | BCrypt hash — write-only |
| `IsActive` | PasswordCredential | `bool` | Only one `true` at a time |
| `Method` | MfaEnrollment | `MfaMethod` | TOTP / SMS / EMAIL / WEBAUTHN |
| `Status` | MfaEnrollment | `MfaEnrollmentStatus`| NotEnrolled / Enrolled / Verified |

---

## 3. Sequence Diagrams

*(Consolidated view covering core flows)*

### Register User Flow
```mermaid
sequenceDiagram
    participant C as Client
    participant H as RegisterUserHandler
    participant U as UserAccount (AR)
    participant R as IUserAccountRepository
    participant E as IEmailUniquenessChecker

    C->>H: RegisterUserCommand(...)
    H->>E: IsUnique(tenantId, email)
    E-->>H: true
    H->>U: UserAccount.Create(...)
    U->>U: Raise UserRegisteredEvent
    H->>R: Add(userAccount)
    H-->>C: UserId
```

### Set Password Flow
```mermaid
sequenceDiagram
    participant C as Client
    participant H as SetPasswordHandler
    participant R as IUserAccountRepository
    participant U as UserAccount (AR)
    participant P as IPasswordHashingService

    C->>H: AddUserAccountPasswordCommand(userId, password)
    H->>R: GetById(userId)
    R-->>H: UserAccount
    H->>P: Hash(plainPassword)
    P-->>H: passwordHash
    H->>U: userAccount.AddPassword(passwordHash, actorId)
    U->>U: Deactivate existing PasswordCredential
    U->>U: Create new PasswordCredential (IsActive = true)
    H->>R: Update(userAccount)
    H-->>C: void
```

### Enroll MFA Flow
```mermaid
sequenceDiagram
    participant C as Client
    participant H as EnrollMfaHandler
    participant R as IUserAccountRepository
    participant U as UserAccount (AR)
    participant MFA as IMfaChallengeService

    C->>H: EnrollMfaCommand(userId, method, actorId)
    H->>R: GetById(userId)
    R-->>H: UserAccount
    H->>U: userAccount.EnrollMfa(method, actorId)
    U->>U: Guard: method not already enrolled
    U->>U: Create MfaEnrollment (Status = Enrolled)
    U->>U: Raise MfaEnrolledEvent
    H->>R: Update(userAccount)
    H->>MFA: InitiateSetup(userId, method)
    MFA-->>H: setupToken
    H-->>C: enrollmentId, setupToken
```

---

## 4. Entity / Relationship Model

> **Relación con Delegación:** `USER_MANAGEMENT_DELEGATION` contiene **dos FK independientes hacia `USER_ACCOUNT`** — una para cada rol (`DelegatingAdminId` = grantor, `DelegatedAdminId` = grantee). Esto es un **dual self-join** (auto-relación de roles), no una relación M:M clásica. Un mismo `UserAccount` puede aparecer simultáneamente como grantor en N delegaciones que otorgó y como grantee en M delegaciones que recibe. La circularidad directa (A→B y B→A activos a la vez) se bloquea vía `IDelegationAuthorityChecker` (INV-DEL5); la auto-delegación se bloquea con `CHECK (DelegatingAdminId <> DelegatedAdminId)` en BD (INV-DEL2).

```mermaid
erDiagram
    USER_ACCOUNT ||--o{ PASSWORD_CREDENTIAL        : "authenticates_with"
    USER_ACCOUNT ||--o{ MFA_ENROLLMENT             : "enrolls_mfa"
    USER_ACCOUNT }o--|| TENANT                     : "belongs_to"
    USER_ACCOUNT }o--o| BRANCH                     : "scoped_to"
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "grants  (DelegatingAdminId)"
    USER_ACCOUNT ||--o{ USER_MANAGEMENT_DELEGATION : "receives (DelegatedAdminId)"

    USER_ACCOUNT {
        uniqueidentifier Id PK
        uniqueidentifier TenantId FK "RLS"
        uniqueidentifier BranchId FK "Nullable"
        nvarchar Email "Unique per TenantId"
        nvarchar Category "INTERNAL-EXTERNAL-B2B-PARTNER"
        nvarchar Status "PENDING-ACTIVE-BLOCKED"
        nvarchar IdentityReference "Nullable"
        nvarchar IdentityReferenceType "Nullable - HR_ID-VENDOR_CODE-GOVERNMENT_ID-PARTNER_REF"
        datetime2 CreatedAt
        uniqueidentifier CreatedBy
        datetime2 UpdatedAt
        uniqueidentifier UpdatedBy
    }

    PASSWORD_CREDENTIAL {
        uniqueidentifier Id PK
        uniqueidentifier UserAccountId FK
        nvarchar PasswordHash "BCrypt"
        bit IsActive "Only one active at a time"
        datetime2 CreatedAt
        uniqueidentifier CreatedBy
        datetime2 UpdatedAt
        uniqueidentifier UpdatedBy
    }

    MFA_ENROLLMENT {
        uniqueidentifier Id PK
        uniqueidentifier UserAccountId FK
        nvarchar Method "TOTP-SMS-EMAIL-WEBAUTHN"
        nvarchar Status "NOT_ENROLLED-ENROLLED-VERIFIED"
        datetime2 CreatedAt
        uniqueidentifier CreatedBy
        datetime2 UpdatedAt
        uniqueidentifier UpdatedBy
    }

    USER_MANAGEMENT_DELEGATION {
        uniqueidentifier Id PK
        uniqueidentifier TenantId FK "RLS"
        uniqueidentifier DelegatingAdminId FK "→ USER_ACCOUNT (grantor)"
        uniqueidentifier DelegatedAdminId FK "→ USER_ACCOUNT (grantee)"
        varchar ScopeTypeId "TENANT-ORGANIZATION-DEPARTMENT-SYSTEM-TEAM"
        uniqueidentifier ScopeId "Nullable — required when ScopeType ≠ TENANT"
        nvarchar AllowedActionsJson "JSON: CREATE_USER BLOCK_USER ASSIGN_PROFILE ..."
        datetimeoffset ValidFrom
        datetimeoffset ValidUntil
        int MaxDurationDays "Nullable"
        bit RequiresApproval
        uniqueidentifier ApprovalRequestId "Nullable FK"
        int StatusId "DRAFT-PENDING_APPROVAL-ACTIVE-REVOKED-EXPIRED-COMPLETED-REJECTED-ARCHIVED"
        datetimeoffset RevokedAt "Nullable"
        uniqueidentifier RevokedBy "Nullable FK → USER_ACCOUNT"
        nvarchar RevocationReason "Nullable"
        datetime2 CreatedAt
        uniqueidentifier CreatedBy
        datetime2 UpdatedAt
        uniqueidentifier UpdatedBy
    }
```

> **Nota INV-DEL2 / INV-DEL5:** La tabla incluye `CHECK (DelegatingAdminId <> DelegatedAdminId)` en BD. La anti-circularidad (A→B no puede coexistir con B→A `ACTIVE`) se detecta en aplicación porque requiere buscar filas — los `CHECK` constraints de SQL no pueden hacer lookups cruzados. Ver [`UserManagementDelegation`](./user-management-delegation.md) §7.

---

## 5. Bounded Context Model

```mermaid
flowchart TD
    subgraph Identity["Identity BC"]
        UA[UserAccount AR]
        PC[PasswordCredential]
        MFA[MfaEnrollment]
        UMD[UserManagementDelegation AR]
        UA --> PC
        UA --> MFA
        UMD -. "DelegatingAdminId\n(grantor FK)" .-> UA
        UMD -. "DelegatedAdminId\n(grantee FK)" .-> UA
    end

    subgraph Authorization["Authorization BC"]
        PROF[Profile]
    end

    subgraph Approvals["Approvals BC"]
        AR[ApprovalRequest]
        UD[UserDocument]
    end

    subgraph IGA["IGA BC"]
        PR[PromotionRequest]
        RMS[RoleMaturityStatus]
    end

    subgraph AppServices["Application Services"]
        HASH[IPasswordHashingService]
        TOTP[IMfaChallengeService]
        DSV[IDelegationScopeValidator]
        DAC[IDelegationAuthorityChecker]
    end

    UA  -->|UserRegisteredEvent| PROF
    UA  -->|UserId reference| AR
    UA  -->|UserId reference| UD
    UA  -->|UserId reference| PR

    DSV -->|"queries ACTIVE delegations"| UMD
    DSV -->|"gates RegisterUser / BlockUser\nAssignProfile commands"| UA
    DAC -->|"INV-DEL1: no elevation\nINV-DEL5: no circular"| UMD

    HASH -->|BCrypt hash| PC
    TOTP -->|OTP validation| MFA

    AR  -->|ApproveDelegationCommand| UMD
```

> **Dual self-join en Identity BC:** `UserManagementDelegation` vive dentro del mismo BC que `UserAccount` pero es un AR independiente. Las dos FK (`DelegatingAdminId`, `DelegatedAdminId`) apuntan ambas a `UserAccount` — líneas punteadas en el diagrama. La validación de autoridad se hace por `IDelegationScopeValidator` (application service), no dentro del propio AR de `UserAccount`.

---

## 6. API / Application Layer Contract

### Commands
| Command | Output |
|---|---|
| `RegisterUserCommand` | `Guid userId` |
| `ActivateUserCommand` | `void` |
| `BlockUserCommand` | `void` |
| `RestoreUserCommand` | `void` |
| `AddUserAccountPasswordCommand` | `Guid credentialId`; accepted input is a temporary password, never a precomputed client hash |
| `LinkExternalIdentityCommand` | `void` |
| `EnrollMfaCommand` | `Guid enrollmentId, string setupToken` |
| `VerifyMfaCommand` | `void` |

### Queries
| Query | Returns |
|---|---|
| `GetUserByIdQuery` | `UserAccountDetailDto` |
| `GetUserByEmailQuery` | `UserAccountDetailDto?` |
| `ListUsersQuery` | `PagedList<UserSummaryDto>` |
| `GetUserAccountByIdQuery` | Includes `hasActivePassword` and `passwordUpdatedAtUtc`; never returns `PasswordHash` |
| `GetUserMfaEnrollmentsQuery` | `List<MfaEnrollmentDto>` |

---

## 7. Persistence Notes

### Transaction Boundary
`UserAccount`, `PasswordCredential`, and `MfaEnrollment` are saved in a single `SaveChanges()` call.

### Indexes
| Index | Columns | Type |
|---|---|---|
| `IX_UserAccount_TenantId_Email` | `TenantId, Email` | Unique |
| `IX_UserAccount_TenantId_Status` | `TenantId, Status` | Non-unique |
| `IX_UserAccount_IdentityReference` | `IdentityReference, TenantId` | Non-unique |
| `IX_PasswordCredential_UserAccountId_IsActive` | `UserAccountId, IsActive` | Non-unique |
| `IX_MfaEnrollment_UserAccountId_Method` | `UserAccountId, Method` | Unique |

### Security
- `PasswordHash` column must never appear in query projections returned to clients.
- `PasswordHash` must never appear in `AuditRecord.WhatChanged` payloads.
- Column must be encrypted at rest (SQL Server Always Encrypted or TDE).
- The web client sends a temporary password over secured transport; BCrypt hashing is performed by the API before persistence.

---

## 8. Security and Audit

### Authorization Rules
| Operation | Required Role | Delegation Gate |
|---|---|---|
| Register User | `Tenant:Admin` or `Tenant:UserManager` | Delegated admin with `CREATE_USER` in scope |
| Block / Restore User | `Tenant:Admin` | Delegated admin with `BLOCK_USER` in scope |
| Set Password | User themselves or `Tenant:Admin` | — |
| Enroll / Revoke / Verify MFA | User themselves | — |
| Link External Identity | `Tenant:Admin` | — |
| Assign Profile | `Tenant:Admin` | Delegated admin with `ASSIGN_PROFILE` in scope |

> **Delegation Gate:** When present, the handler also accepts requests from a `UserAccount` holding an `ACTIVE` `UserManagementDelegation` that covers the target user and the listed action. Validated by `IDelegationScopeValidator` before the command reaches the aggregate. See [`UserManagementDelegation`](./user-management-delegation.md).

### Audit Events
- `USER_REGISTERED`, `USER_ACTIVATED`, `USER_BLOCKED`, `USER_RESTORED`
- `PASSWORD_SET`, `MFA_ENROLLED`, `MFA_VERIFIED`
- `AUTHENTICATION_ATTEMPTED` (with `AuditResult: SUCCESS/FAILURE`)
