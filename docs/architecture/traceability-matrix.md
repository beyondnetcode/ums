# Traceability Matrix — UMS

> **Language:** [English](./traceability-matrix.md) | [Español](./traceability-matrix.es.md)

This matrix links every **Functional Story (FS)** to its governing **Architectural Decision Records (ADRs)** — both from the UMS product context and the Evolith framework — and to the **Technical Enablers (TEs)** that implement those decisions.

---

## 1. Forward Matrix — FS → ADR → TE

| FS ID | Story Title | UMS ADR | Evolith ADR | Technical Enabler |
|-------|-------------|---------|-----------|-------------------|
| FS-01 | User Authentication | — | ADR-0020 (IdP Abstraction), ADR-0026 (MFA Adaptive) | TE-01 (JWT / OIDC flow) |
| FS-02 | Create Authorization Template | — | ADR-0012 (RBAC/ABAC Guards), ADR-0021 (Auth Graph) | TE-02 (Permission Graph Compiler) |
| FS-03 | Register Organization | ADR-0077 | ADR-0010 (Multi-Tenancy RLS), ADR-0031 (Schema per Context) | TE-03 (Tenant Provisioning), TE-04 (Transactional Outbox) |
| FS-04 | Register System Topology | — | ADR-0031 (Schema per Context), ADR-0034 (CQRS) | TE-06 (CQRS Projection Rebuild) |
| FS-05 | Create Profile / Manual Template | ADR-0054 (Shell Library Isolation) | ADR-0012 (RBAC/ABAC), ADR-0029 (DDD Primitives) | TE-02 (Permission Graph) |
| FS-06 | Auto-Assign Template | — | ADR-0015 (Event Bus), ADR-0033 (Transactional Outbox) | TE-04 (Transactional Outbox) |
| FS-07 | Visual Graph Resolver | — | ADR-0021 (Auth Graph Compilation), ADR-0022 (Contextual Projections) | TE-06 (CQRS Projection Rebuild) |
| FS-08 | Hosted Login Redirection | — | ADR-0020 (IdP Abstraction), ADR-0027 (Dual-Protocol Node) | TE-01 (JWT / OIDC flow) |
| FS-09 | MFA / Passwordless Adaptive Auth | — | ADR-0026 (MFA Adaptive), ADR-0020 (IdP Abstraction) | TE-01 (JWT / OIDC flow) |
| FS-10 | External B2B Access Request / Approval | — | ADR-0035 (Distributed Sagas), ADR-0015 (Event Bus) | TE-05 (Distributed Saga with Dapr) |
| FS-11 | User Document Upload | — | ADR-0016 (Immutable Audit Trail), ADR-0033 (Transactional Outbox) | TE-04 (Transactional Outbox) |
| FS-12 | Role Promotion Process | — | ADR-0035 (Distributed Sagas), ADR-0012 (RBAC/ABAC) | TE-05 (Distributed Saga with Dapr) |
| FS-13 | Hierarchical Configuration | — | ADR-0024 (Config Platform), ADR-0034 (CQRS) | TE-06 (CQRS Projection Rebuild) |
| FS-14 | Delegated Management | ADR-0077 | ADR-0023 (Centralized Kernel), ADR-0012 (RBAC/ABAC) | TE-02 (Permission Graph), TE-03 (Tenant Provisioning) |
| FS-15 | Notification Rules | — | ADR-0015 (Event Bus), ADR-0036 (Message Bus FIFO/DLQ) | TE-04 (Transactional Outbox) |
| FS-16 | Access Enforcement Policy | — | ADR-0012 (RBAC/ABAC), ADR-0016 (Immutable Audit Trail) | TE-02 (Permission Graph), TE-03 (Tenant Provisioning) |
| FS-17 | Maintain Roles for a System Suite | — | ADR-0012 (RBAC/ABAC Guards), ADR-0021 (Auth Graph) | TE-02 (Permission Graph Compiler) |
| FS-18 | Manage Local User Password | — | ADR-0020 (IdP Abstraction), ADR-0016 (Immutable Audit Trail) | TE-01 (JWT / OIDC flow) |
| FS-19 | Admin Password Reset and User Validity Period Management | ADR-0077 | ADR-0012 (RBAC/ABAC Guards), ADR-0016 (Immutable Audit Trail) | TE-01 (JWT / OIDC flow), TE-04 (Transactional Outbox) |
| FS-20 | System Parameter Management | ADR-0077 | ADR-0024 (Config & Feature Platform), ADR-0034 (CQRS), ADR-0012 (RBAC/ABAC) | TE-06 (CQRS Projection Rebuild) |
| FS-21 | Tenant Signup Request / Approval | ADR-0075 | ADR-0035 (Distributed Sagas), ADR-0015 (Event Bus) | TE-05 (Distributed Saga with Dapr), TE-04 (Transactional Outbox) |
| FS-22 | User Signup Request / Approval | ADR-0075 | ADR-0035 (Distributed Sagas), ADR-0015 (Event Bus) | TE-05 (Distributed Saga with Dapr), TE-04 (Transactional Outbox) |
| FS-23 | Profile Access Request | ADR-0075, ADR-0071 | ADR-0012 (RBAC/ABAC), ADR-0016 (Immutable Audit Trail) | TE-02 (Permission Graph Compiler), TE-04 (Transactional Outbox) |
| FS-24 | Profile Request Approval / Manual Assignment | ADR-0075, ADR-0071 | ADR-0012 (RBAC/ABAC), ADR-0016 (Immutable Audit Trail) | TE-02 (Permission Graph Compiler), TE-04 (Transactional Outbox) |

---

## 2. Inverse Matrix — ADR → FS

| ADR | Title | Satisfies FS |
|-----|-------|-------------|
| ADR-0010 | Multi-Tenancy RLS Strategy | FS-03, FS-14, FS-16 |
| ADR-0012 | Auth RBAC/ABAC Guards | FS-02, FS-05, FS-12, FS-14, FS-16 |
| ADR-0015 | Injectable Event Bus | FS-06, FS-10, FS-15 |
| ADR-0016 | Immutable Audit Trail | FS-11, FS-16 |
| ADR-0020 | IdP Abstraction | FS-01, FS-08, FS-09 |
| ADR-0021 | Auth Graph Compilation | FS-02, FS-07 |
| ADR-0022 | Contextual Projections | FS-07 |
| ADR-0023 | Centralized Kernel Boundary | FS-14 |
| ADR-0024 | Config & Feature Platform | FS-13 |
| ADR-0026 | MFA Adaptive Implementation | FS-01, FS-09 |
| ADR-0027 | Dual-Protocol Node Setup | FS-08 |
| ADR-0029 | Tactical DDD Primitives | FS-05 |
| ADR-0054 | Shell Library Isolation for DDD and Factory Patterns | FS-05 |
| ADR-0031 | Isolated Schema Per Context | FS-03, FS-04 |
| ADR-0033 | Transactional Outbox | FS-06, FS-11, FS-15 |
| ADR-0034 | CQRS Applicability | FS-04, FS-13 |
| ADR-0035 | Distributed Sagas | FS-10, FS-12 |
| ADR-0036 | Message Bus Delivery Strategy | FS-15 |
| ADR-0071 | Auth Graph Engine | FS-01, FS-05, FS-07, FS-08, FS-09, FS-23, FS-24 |
| ADR-0072 | Dynamic Auth Method Resolution | FS-01, FS-08, FS-09, FS-18, FS-19 |
| ADR-0075 | Tenant and Signup Request Approval | FS-21, FS-22, FS-23, FS-24 |
| ADR-0077 | Tenant Portal Management Authorization Boundary | FS-03, FS-14, FS-19, FS-20 |

---

## 3. Technical Enabler Registry

| TE ID | Title | Status | Implements ADR | Satisfies FS |
|-------|-------|--------|----------------|-------------|
| TE-01 | JWT / OIDC Authentication Flow | **[Approved](./blueprints/technical-enablers/te-01-jwt-oidc-flow.md)** | ADR-0020, ADR-0026 | FS-01, FS-08, FS-09 |
| TE-02 | Permission Graph Compiler | **[Approved](./blueprints/technical-enablers/te-02-permission-graph-compiler.md)** | ADR-0012, ADR-0021 | FS-02, FS-05, FS-07, FS-14, FS-16 |
| TE-03 | Tenant Provisioning Pipeline | **[Approved](./blueprints/technical-enablers/te-03-tenant-provisioning.md)** | ADR-0010 | FS-03, FS-14, FS-16 |
| TE-04 | Transactional Outbox Pattern | **[Defined](./blueprints/technical-enablers/te-04-transactional-outbox.md)** | ADR-0033, ADR-0015 | FS-06, FS-11, FS-15 |
| TE-05 | Distributed Saga with Dapr | **[Defined](./blueprints/technical-enablers/te-05-distributed-saga-dapr.md)** | ADR-0035 | FS-10, FS-12 |
| TE-06 | CQRS Projection Rebuild | **[Defined](./blueprints/technical-enablers/te-06-cqrs-projection-rebuild.md)** | ADR-0034 | FS-04, FS-07, FS-13 |

---

## 4. Domain Cluster Summary

| Bounded Context | Primary FS | Primary ADRs | Primary TEs |
|-----------------|-----------|-------------|------------|
| Identity | FS-01, FS-08, FS-09, FS-18, FS-19 | ADR-0020, ADR-0026, ADR-0072, ADR-0077 | TE-01 |
| Authorization | FS-02, FS-05, FS-07, FS-16, FS-17, FS-23, FS-24 | ADR-0012, ADR-0021, ADR-0022, ADR-0071, ADR-0077 | TE-02 |
| Tenant / Org | FS-03, FS-04, FS-14, FS-21, FS-22 | ADR-0010, ADR-0031, ADR-0075, ADR-0077 | TE-03, TE-05, TE-06 |
| Approvals / Workflow | FS-10, FS-12 | ADR-0035 | TE-05 |
| Configuration | FS-13, FS-20 | ADR-0024, ADR-0034, ADR-0077 | TE-06 |
| Audit / Compliance | FS-11, FS-15 | ADR-0016, ADR-0033 | TE-04 |

---

## 5. Coverage Gaps

| Gap | Description | Recommended Action |
|-----|-------------|-------------------|
| No FS-level acceptance tests linked | FSes do not expose a standardized acceptance-test pointer | Add an "Acceptance Tests" section to each FS |
| BC-C / BC-D / BC-I coverage still incomplete | Configuration, Audit, and Compliance contexts still need the same level of completeness as the core bounded contexts | Continue closing the remaining domain and application gaps in those contexts |
| Design decisions V1, V3–V6 open | See [12-design-decisions.md](../governance/construction/ddd-design/12-design-decisions.md) | Resolve in technical workshop before each dependent change |

---

**[Back to Architecture Portal](./index.md)** | **[Back to Master Index](../MASTER_INDEX.md)**
