# Traceability Matrix — UMS

> **Purpose:** Forward and inverse trace from business requirements (FS) to architectural decisions (ADR) to implementation guides (TE).  
> **Scope:** UMS functional stories + UMS ADRs + arc32 core ADRs that back them.  
> **Update policy:** Update this matrix whenever a new FS, ADR, or TE is added or its status changes.

---

## 1. Forward Trace: FS → ADR → TE

> Legend: `[UMS]` = UMS-specific ADR · `[ARC]` = arc32 core ADR · ` TE pending` = pattern decided but no Technical Enabler yet

| FS | Title | Domain | ADRs | TE |
|:---|:---|:---|:---|:---|
| **FS-01** | User Authentication via External IdP | Authentication / Identity | [UMS] ADR-0020, ADR-0022, ADR-0026 | — |
| **FS-02** | Create and Instantiate Authorization Template | Authorization / Policy | [UMS] ADR-0039, ADR-0042, ADR-0021 · [ARC] ADR-0012 | TE-01 |
| **FS-03** | Register Organization and Configure IdP Strategy | Multi-Tenancy / Onboarding | [UMS] ADR-0031, ADR-0032, ADR-0034, ADR-0010 · [ARC] ADR-0010 | TE-03 |
| **FS-04** | Register System and Define Menu Topology | Administration / Topology | [UMS] ADR-0032, ADR-0034, ADR-0047 | — |
| **FS-05** | Create Profile and Manually Assign Authorization Template | Authorization / Profiles | [UMS] ADR-0039, ADR-0042, ADR-0043, ADR-0035 · [ARC] ADR-0012 | TE-01 |
| **FS-06** | Auto-Assign Authorization Template on Profile Creation | Authorization / Automation | [UMS] ADR-0042, ADR-0043, ADR-0035 | TE-01 |
| **FS-07** | Diagnose Permissions via Graph Visualizer | Authorization / Diagnostics | [UMS] ADR-0021, ADR-0039 | TE-01 |
| **FS-08** | Authenticate via Customizable Hosted Login Page | Authentication / Branding | [UMS] ADR-0020, ADR-0022 · [ARC] ADR-0008, ADR-0020 | — |
| **FS-09** | Adaptive MFA & Passwordless Authentication | Authentication / Security | [UMS] ADR-0026 | — |
| **FS-10** | B2B External Access Request and Approval Flow | External Access / B2B | [UMS] ADR-0031, ADR-0032, ADR-0038, ADR-0044 | — |
| **FS-11** | Upload and Validate User Document | Compliance / Documents | [UMS] ADR-0045, ADR-0016 · [ARC] ADR-0016 | — |
| **FS-12** | Execute Role Promotion Process | Governance / Role Lifecycle | [UMS] ADR-0046, ADR-0036 | — |
| **FS-13** | Configure Hierarchical System Parameters | Configuration / Inheritance | [UMS] ADR-0047, ADR-0024 · [ARC] ADR-0024 | TE-02 |
| **FS-14** | Delegate User Management Between Administrators | Governance / Delegation | [UMS] ADR-0038, ADR-0044 | — |
| **FS-15** | Configure Expiration Notification Rules | Compliance / Notifications | [UMS] ADR-0045, ADR-0016 · [ARC] ADR-0016 | — |
| **FS-16** | Define Access Policy on Expiration | Compliance / Enforcement | [UMS] ADR-0045, ADR-0035 | —
## 2. Technical Enablers Registry

| TE | Title | Pattern | Backing ADRs | Consumed by FS |
|:---|:---|:---|:---|:---|
| **TE-01** | Build User Authorization Graph | Permission graph compilation + Redis cache | [UMS] ADR-0014, ADR-0021, ADR-0039 | FS-02, FS-05, FS-06, FS-07 |
| **TE-02** | Resolve Hierarchical System Configuration | Config inheritance + deep merge | [UMS] ADR-0024, ADR-0047 · [ARC] ADR-0024 | FS-13 |
| **TE-03** | Enforce Row-Level Security by Organization | Multi-tenant data isolation (SQL Server RLS) | [UMS] ADR-0037, ADR-0041 · [ARC] ADR-0044 | FS-03 |
| **TE-04** | Transactional Outbox for Async Messaging | Outbox pattern — at-least-once delivery | [ARC] ADR-0033, ADR-0036 | FS-10, FS-11, FS-15 |
| **TE-05** | Distributed Saga with Dapr | Choreography/orchestration sagas | [ARC] ADR-0035, ADR-0006 | FS-10, FS-12 |
| **TE-06** | CQRS Projection Rebuild Strategy | Read model rebuild from event store | [ARC] ADR-0034, ADR-0015 | FS-07 | > **TE-04, TE-05, TE-06** are defined below but do not yet have implementation documents.  
> See [Coverage Gaps](#4-coverage-gaps) for creation tracking.

---

## 3. Inverse Index: ADR → FS

Navigates from an architectural decision back to which business requirements motivated it.

### UMS ADRs

| ADR | Title (short) | Motivating FS |
|:---|:---|:---|
| ADR-0010 | Multi-Tenancy Architecture Strategy | FS-03 |
| ADR-0014 | Distributed Caching (Redis) | FS-02, FS-05, FS-06, FS-07 |
| ADR-0016 | Immutable Audit Trail | FS-11, FS-15 |
| ADR-0020 | Identity Provider Abstraction | FS-01, FS-08 |
| ADR-0021 | High-Performance Auth Graph Compilation | FS-02, FS-05, FS-06, FS-07 |
| ADR-0022 | Contextual Auth & Pluggable Projections | FS-01, FS-08 |
| ADR-0024 | Centralized Configuration Platform | FS-13 |
| ADR-0026 | Adaptive MFA & Passwordless (WebAuthn) | FS-01, FS-09 |
| ADR-0031 | Identity Domain Abstraction | FS-03, FS-10 |
| ADR-0032 | Organization as Strategic Boundary | FS-03, FS-04, FS-10 |
| ADR-0034 | Hierarchical Multi-Tenancy (Closure Table) | FS-03, FS-04 |
| ADR-0035 | Hybrid Policy Inheritance Engine | FS-05, FS-06, FS-16 |
| ADR-0036 | Anti-Privilege Escalation Validation | FS-12 |
| ADR-0037 | Tenant-Aware Partitioning Strategy | FS-03 |
| ADR-0038 | Delegated Administration with Temporal Scopes | FS-10, FS-14 |
| ADR-0039 | Hybrid RBAC/ABAC Policy Compilation | FS-02, FS-05, FS-06, FS-07 |
| ADR-0042 | Authorization Template & Inheritance | FS-02, FS-05, FS-06 |
| ADR-0043 | Role-Scoped Master Templates & Governance | FS-05, FS-06 |
| ADR-0044 | Delegated Administration & Approval Workflows | FS-10, FS-14 |
| ADR-0045 | Automated Document-Based Access Enforcement | FS-11, FS-15, FS-16 |
| ADR-0046 | Role Evolution and Promotion Governance | FS-12 |
| ADR-0047 | Hierarchical Configuration Management | FS-04, FS-13 | ### arc32 Core ADRs referenced by UMS

| ADR | Title (short) | Motivating FS |
|:---|:---|:---|
| ADR-0006 | Future Microservices Transition (Dapr) | FS-10, FS-12 (via TE-05) |
| ADR-0008 | Progressive BFF Evolution / API Gateway | FS-08 |
| ADR-0010 | Multi-Tenancy SaaS Strategy | FS-03 |
| ADR-0012 | Advanced RBAC/ABAC & Security Auditing | FS-02, FS-05 |
| ADR-0015 | Event-Driven Architecture (intra-domain) | FS-07 (via TE-06) |
| ADR-0016 | Immutable Audit Trail | FS-11, FS-15 |
| ADR-0020 | IdP Abstraction Strategy | FS-01, FS-08 |
| ADR-0024 | Centralized Config & Feature Platform | FS-13 |
| ADR-0033 | Transactional Outbox Pattern | FS-10, FS-11, FS-15 (via TE-04) |
| ADR-0034 | CQRS Application Matrix | FS-07 (via TE-06) |
| ADR-0035 | Distributed Saga Strategy | FS-10, FS-12 (via TE-05) |
| ADR-0036 | Message Bus Delivery & DLQ Strategy | FS-10, FS-11, FS-15 (via TE-04) |
| ADR-0044 | Configurable Security Persistence (RLS) | FS-03 (via TE-03)
## 4. Coverage Gaps

### 4.1 Functional Stories without Technical Enabler

These FSs involve complex implementation patterns backed by ADRs but lack a TE document:

| FS | Missing TE | Pattern | Priority |
|:---|:---|:---|:---|
| FS-10, FS-11, FS-15 | TE-04 | Transactional Outbox — async at-least-once delivery | High |
| FS-10, FS-12 | TE-05 | Distributed Saga with Dapr choreography | High |
| FS-07 | TE-06 | CQRS Projection rebuild from event store | Medium | ### 4.2 ADRs with No Functional Story Coverage

These decisions are cross-cutting infrastructure concerns not tied to a specific user story:

| ADR | Title | Nature |
|:---|:---|:---|
| ADR-0001 | Monorepo Orchestáration (Nx) | Platform |
| ADR-0002 | Hexagonal Architecture / NestJS | Platform |
| ADR-0003 | Strict TypeScript + SonarJS | Platform |
| ADR-0005 | CI/CD Security with CodeQL | Platform |
| ADR-0007 | Observability (OTel + Loki) | Platform |
| ADR-0009 | Dependency Pinning | Platform |
| ADR-0011 | Fault Tolerance / Circuit Breakers | Platform |
| ADR-0013 | Cloud Topology & DR | Platform |
| ADR-0017 | Feature Flagging | Platform |
| ADR-0018 | Testing Pyramid & Quality Gates | Platform |
| ADR-0019 | Tactical DDD Patterns | Platform |
| ADR-0023 | Centralized UMS Core vs Decentralized Access | Platform |
| ADR-0025 | Feature Flag Provider Abstraction | Platform |
| ADR-0027 | Dual REST/gRPC Protocol + Kong | Platform |
| ADR-0028 | Self-Hosted OSS Infrastructure | Platform |
| ADR-0029 | @nestájslatam/ddd Adoption | Platform |
| ADR-0030 | Kong OSS vs NestJS Gateway | Platform |
| ADR-0033 | .NET Minimal APIs Strategy | Platform |
| ADR-0040 | Federated Token Strategy | Platform |
| ADR-0041 | Authoritative Database Engine | Platform | > Platform ADRs are foundational constraints, not feature-driven. Their absence from FS coverage is expected.

---

## 5. Quick Reference — Domain Clusters

| Domain | FSs | Key ADRs |
|:---|:---|:---|
| **Authentication** | FS-01, FS-08, FS-09 | ADR-0020, ADR-0022, ADR-0026 |
| **Authorization / Policy** | FS-02, FS-05, FS-06, FS-07 | ADR-0021, ADR-0035, ADR-0039, ADR-0042, ADR-0043 |
| **Multi-Tenancy** | FS-03, FS-04 | ADR-0010, ADR-0031, ADR-0032, ADR-0034, ADR-0037 |
| **Governance & Delegation** | FS-10, FS-12, FS-14 | ADR-0036, ADR-0038, ADR-0044, ADR-0046 |
| **Compliance & Documents** | FS-11, FS-15, FS-16 | ADR-0016, ADR-0045 |
| **Configuration** | FS-13 | ADR-0024, ADR-0047
## 6. Related Documents

- [Functional Stories Index](../governance/requirements/functional-stories/index.md)
- [ADR Registry](./adrs/index.md)
- [Technical Enablers Index](./blueprints/technical-enablers/index.md)
- [Architecture Blueprints](./blueprints/index.md)
- [arc32 ADR Registry](../../arc32_progresive_monolith/architecture/adrs/)
