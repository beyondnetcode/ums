# UMS Architecture Overview

**Document Status:** Production  
**Authoritative Scope:** Global Platform Architecture  
**Parent Framework Reference:** [arc32 Progressive Monolith Reference](https://github.com/beyondnetcode/arc32_progresive_monolith)

---

## 1. Global Architecture Vision

The User Management System (UMS) is designed as a **Modular Monolith** adhering to the principles of **Clean Architecture** (Hexagonal Architecture), strict **Domain-Driven Design (DDD)**, and strict **Tenant Isolation**.

UMS serves as an authorization and identity gateway that can either function as a standalone system or integrate with external corporate Identity Providers (IdPs) via OpenID Connect (OIDC), SAML 2.0, or WS-Federation protocols.

```
       ┌─────────────────────────────────────────────────────────┐
       │                   Presentation Layer                    │
       │     React 18 + Vite (SPA) / Web API / GraphQL / REST    │
       └────────────┬───────────────────────────────▲────────────┘
                    │ Commands                      │ Queries / DTOs
                    ▼                               │
       ┌────────────────────────────────────────────┴────────────┐
       │                    Application Layer                    │
       │       CQRS Handlers / Use Cases / Pipelines / DTOs      │
       └────────────┬───────────────────────────────▲────────────┘
                    │ Invokes                       │ Implements
                    ▼                               │
       ┌────────────────────────────────────────────┴────────────┐
       │                      Domain Layer                       │
       │     Pure POCO Aggregates / Value Objects / Events       │
       └────────────▲───────────────────────────────┬────────────┘
                    │ Implements                    │ Integrates
                    │                               ▼
       ┌────────────┴────────────────────────────────────────────┐
       │                   Infrastructure Layer                  │
       │      SQL Server (EF Core) / Dapr / Outbox / Outbound    │
       └─────────────────────────────────────────────────────────┘
```

### Shared Architectural Principles
1. **Domain Purity**: The Domain layer (`{BoundedContext}.Domain`) consists of pure C# objects (POCOs) with zero external library references, ensuring business logic remains unpolluted.
2. **Explicit Boundaries**: Cross-context interactions are strictly decoupled using event-driven communication (Transactional Outbox) or explicit Application-layer Anti-Corruption Layers (ACLs). Direct cross-context database joins are strictly prohibited.
3. **Tenant Isolation**: High-security multi-tenancy is enforced natively in the Application layer, with SQL Server Row-Level Security (RLS) serving as an infrastructure-level secondary failsafe (R-10).
4. **Command-Query Responsibility Segregation (CQRS)**: Read models are highly optimized and separated from write models. Writes are strictly transaction-safe, while reads leverage efficient flat projections or direct GraphQL execution.

---

## 2. System Context Map

UMS connects multiple actors and external suites to provide unified access management.

```mermaid
graph TD
    User["End User (Browser/Client)"] -->|HTTPS GraphQL/REST| Gate["API Gateway / BFF"]
    Admin["Tenant Admin"] -->|HTTPS Config| Gate
    SysAdmin["Platform System Admin"] -->|Global Maintenance| Gate

    Gate -->|UMS.API| Core["UMS Core Engine"]
    
    subgraph External Systems
        IDP["External IdPs (OIDC/SAML/ADFS)"]
        CRM["Enterprise ERP/CRM (CRM Reference)"]
    end
    
    Core -->|DNS & Metadata Verification| IDP
    Core -->|Tenant Reference Sync| CRM
    
    subgraph Persistence & Infrastructure
        DB[("SQL Server (Isolated Tenants)")]
        Dapr["Dapr Sidecar (Sagas/PubSub)"]
    end
    
    Core -->|EF Core / Tenant RLS| DB
    Core -->|Event Bus / Workflows| Dapr
```

---

## 3. High-Level Bounded Context Map

UMS is partitioned into six highly cohesive Bounded Contexts:
1. **Identity Bounded Context**: Governs tenant registration, organizational branches, white-label branding, identity providers, and physical user account lifecycles.
2. **Authorization Bounded Context**: Manages application suites, functional modules, granular options, profiles/roles, and dynamic permission matrices.
3. **Configuration Bounded Context**: Controls application behavior dynamically via feature flags and custom Identity Provider configuration details.
4. **Approvals Bounded Context**: Coordinates multi-step human-in-the-loop workflows for access elevation, document verification, and tenant transitions.
5. **IGA (Identity Governance & Administration) Bounded Context**: Evaluates role maturity, promotion request impacts, and separation of duties.
6. **Audit Bounded Context**: Collects immutable, non-repudiable logs of all critical platform transitions.

```mermaid
flowchart TD
    Identity["Identity BC"]
    Authorization["Authorization BC"]
    Configuration["Configuration BC"]
    Approvals["Approvals BC"]
    IGA["IGA BC"]
    Audit["Audit BC"]

    Identity -->|Publish: Tenant/User Events| Configuration
    Identity -->|Publish: Tenant/Branch Events| Authorization
    Identity -->|Scope & Actor Reference| Approvals
    Identity -->|Provide Identity Context| Audit
    
    Authorization -->|Enforce Access Context| Approvals
    Authorization -->|Consume for Elevation Analyis| IGA
    Authorization -->|Publish: Profile/Permission Events| Audit

    Configuration -->|Feature Flags & IdP parameters| Identity
    
    Approvals -->|Elevate Roles / Approve Changes| Authorization
    Approvals -->|Trigger Notifications| Audit
    
    IGA -->|Request Promotion / Role Recertification| Approvals
    
    style Audit fill:#f9f,stroke:#333,stroke-width:2px
```

### Relationship Map Summary

| Upstream Context | Downstream Context | Relationship Type | Integration Pattern |
|---|---|---|---|
| **Identity** | **Configuration** | Upstream-Downstream | Customer-Supplier (Tenant registration seeds Config) |
| **Identity** | **Authorization** | Upstream-Downstream | Customer-Supplier (Branch/User creation scopes profiles) |
| **Identity** | **Approvals** | Upstream-Downstream | Shared Kernel (User / Tenant identities) |
| **Configuration** | **Identity** | Upstream-Downstream | Conformist (Dynamic flags regulate features) |
| **Authorization** | **IGA** | Upstream-Downstream | Partnership (IGA evaluates role structures) |
| **IGA** | **Approvals** | Upstream-Downstream | Customer-Supplier (Promotion requests trigger approvals) |
| **All Contexts** | **Audit** | Downstream | Publish-Subscribe (Transactional Outbox events) |

---

## 4. Aggregate Catalog & Index

All domain rules, invariants, and architectural diagrams are consolidated strictly within each aggregate's dedicated documentation file.

```
/docs/domain/
├── identity/
│   ├── tenant.md               - Top-level organizational unit and its hierarchy
│   ├── branch.md               - Location units scoping users and profiles
│   ├── branding.md             - White-label visual profiles per tenant
│   ├── identity-provider.md    - External OIDC/SAML mappings
│   ├── user-account.md         - System login profiles and user life cycles
│   ├── password-credential.md  - Secure password hashes and policies
│   └── mfa-enrollment.md       - Multi-factor factors (SMS, Email, TOTP)
│
├── authorization/
│   ├── system-suite.md         - Top-level applications and platform options
│   ├── module.md               - Dynamic architectural functional zones
│   ├── menu.md                 - Nested navigational structure mappings
│   ├── sub-menu.md             - Deeper menu trees
│   ├── option.md               - User interfaces and entry paths
│   ├── action.md               - Granular operation tokens (Read/Write/Delete)
│   ├── permission-template.md  - Preset permission groups
│   ├── permission-template-item.md - Individual grants inside a template
│   ├── profile.md              - Dynamic security roles (Global, Tenant, Branch)
│   └── profile-permission.md   - Specific action grants for a profile
│
├── configuration/
│   ├── app-configuration.md    - Tenant environment features
│   ├── feature-flag.md         - Operational toggles
│   ├── flag-evaluation-log.md  - Audit trails for A/B and canary tests
│   └── idp-configuration.md    - Technical client secrets & keys
│
├── approvals/
│   ├── approval-workflow.md    - Human authorization pipeline definitions
│   ├── approval-required-document.md - Verification file lists
│   ├── approval-request.md     - Lifecycle of dynamic approval cases
│   ├── document-type.md        - Structured file class descriptors
│   ├── notification-rule.md    - Automated alerts and communications
│   ├── user-document.md        - Uploaded files requiring human validation
│   ├── access-notification.md  - Security notifications of elevation
│   └── access-enforcement-policy.md - Policy restrictions and limits
│
├── iga/
│   ├── promotion-request.md    - Access elevation business proposals
│   ├── promotion-impact-analysis.md - Separation of Duties collision risk report
│   └── role-maturity-status.md - Dynamic analysis of profile assignments
│
└── audit/
    └── audit-record.md         - Immutable, append-only security logs
```

---

## 5. Cross-Context Integration Principles

To preserve DDD bounded context purity while maintaining system cohesion, the following integration standards must be strictly followed:

### 1. The Transactional Outbox Pattern
Every state-changing operation within an aggregate must publish events to a local outbox table **in the same database transaction**. An asynchronous dispatcher reads these entries and forwards them to Dapr PubSub for cross-context delivery. This ensures reliable eventual consistency without distributed 2PC transactions.

### 2. Integration via Core Identifiers
Aggregates in downstream contexts must NEVER reference entities of upstream contexts directly. Instead, they reference only their global identifier (`TenantId`, `UserId`, `BranchId`, etc.) stored as strong Value Objects.

### 3. Anti-Corruption Layers (ACL)
When a context integrates with external directories or Legacy HR Systems, an explicit Anti-Corruption Layer (consisting of Adapters, Translators, and Facades) is built inside the infrastructure layer to prevent external concepts from bleeding into the pure Domain model.

---

## 6. Shared Architectural Rules

- **Zero NuGet References in Domain**: The Domain project is 100% pure C#. 
- **Result Pattern**: Application flows do not throw control exceptions. All business validations return a `Result<T>` structure mapping out failure cases clearly.
- **Single Source of Truth**: Business invariants and entity schemas belong strictly in their respective aggregate files listed in Section 4. Do not copy ER schemas or diagrams into any other architectural indexes.

---

**[Back to Master Index](../MASTER_INDEX.md)** | **[Go to Domain Index](../domain/index.md)**
