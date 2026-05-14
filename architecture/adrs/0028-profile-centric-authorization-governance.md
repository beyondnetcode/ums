# ADR 0028: Enterprise Profile-Centric Authorization and Governance Model

## Status
Refactored (Enterprise Standards)

## Context
Standard RBAC models fail in multi-tenant, multi-suite enterprise environments where permissions are contextualized by organizational structure (Branches) and functional boundaries (Systems). Direct user-role assignment creates governance gaps. A robust model must isolate roles within systems and consolidate authority at a single contextual pivot: the **Profile**.

## Decision
We will implement an **Enterprise Profile-Centric Authorization Model** governed by the following strict rules:

1.  **Strict Hierarchical Ownership**:
    *   A **Tenant** owns its **Users**, **Systems (Suites)**, and **Branches**.
    *   A **System** belongs to a single Tenant.
    *   A **Role** belongs to a single System. Global roles are prohibited.
    *   A **Permission** belongs to the functional context of a System.

2.  **Profile as the Contextual Nexus**:
    *   The **Profile** is the unique intersection of: `Tenant` + `System` + `Branch` + `User` + `Role`.
    *   Authorizations are resolved and persisted at the **Profile** level as "Effective Permissions".

3.  **Governance & Audit Standards**:
    *   Every entity must implement the **Corporate Audit Schema** (10+ columns).
    *   Support for **Soft Delete**, **Optimistic Locking**, and **Audit Trails** is mandatory for all persistence operations.
    *   **Traceability**: Critical security operations must track `CorrelationId`, `AuditId`, and `TransactionId`.

4.  **Authorization Engine**:
    *   The engine resolves permissions by querying the **Effective Permissions** table for the current `ProfileId`.
    *   Supports **Overrides** (Grant/Deny) at the Profile level for maximum granularity.

## Technical Implementation
*   **Cardinality**: `Tenant (1:N) System (1:N) Role (1:N) Profile`.
*   **Persistence**: Effective permissions are projected from `Role -> ProfilePermission` upon profile creation/sync.

## Consequences
*   **Positive**: Perfect isolation, simplified contextual resolution, full auditability, and massive scalability for multi-org environments.
*   **Negative**: Increased metadata management and the requirement for a robust synchronization logic between Roles and Profiles.
