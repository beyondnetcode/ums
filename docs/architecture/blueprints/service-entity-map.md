# Service-Entity Map & Data Ownership

This document serves as the authoritative mapping between system entities, their Bounded Contexts, owning services, and database schemas within the UMS enterprise ecosystem.

---

## 1. Entity Ownership & Schema Mapping

| Entity | Bounded Context | Service Owner (Write) | Runtime | SQL Server Schema |
| :--- | :--- | :--- | :--- | :--- |
| **TENANT** | Identity | UMS Core API | .NET 10 | `[ums_identity]` |
| **BRANCH** | Identity | UMS Core API | .NET 10 | `[ums_identity]` |
| **USER_ACCOUNT** | Identity | UMS Core API | .NET 10 | `[ums_identity]` |
| **ROLE** | Authorization | UMS Core API | .NET 10 | `[ums_authz]` |
| **PROFILE** | Authorization | UMS Core API | .NET 10 | `[ums_authz]` |
| **PROFILE_PERMISSION** | Authorization | UMS Core API | .NET 10 | `[ums_authz]` |
| **PERMISSION_TEMPLATE** | Authorization | UMS Core API | .NET 10 | `[ums_authz]` |
| **FUNCTIONAL_MODULE** | Authorization | UMS Core API | .NET 10 | `[ums_authz]` |
| **FUNCTIONAL_SUBMODULE**| Authorization | UMS Core API | .NET 10 | `[ums_authz]` |
| **FUNCTIONAL_OPTION** | Authorization | UMS Core API | .NET 10 | `[ums_authz]` |
| **ACTION** | Authorization | UMS Core API | .NET 10 | `[ums_authz]` |
| **SYSTEM_SUITE** | Authorization | UMS Core API | .NET 10 | `[ums_authz]` |
| **ROLE_MATURITY_STATUS**   | IGA            | UMS Core API           | .NET 10   | `[ums_iga]`        |
| **PROMOTION_REQUEST**      | IGA            | UMS Core API           | .NET 10   | `[ums_iga]`        |
| **PROMOTION_IMPACT_ANALYSIS**| IGA          | UMS Core API           | .NET 10   | `[ums_iga]`        |
| **DOCUMENT_TYPE**          | Compliance     | UMS Core API           | .NET 10   | `[ums_approval]`   |
| **USER_DOCUMENT**          | Compliance     | UMS Core API           | .NET 10   | `[ums_approval]`   |
| **NOTIFICATION_RULE**      | Compliance     | UMS Core API           | .NET 10   | `[ums_approval]`   |
| **ACCESS_ENFORCEMENT_POLICY**| Compliance   | UMS Core API           | .NET 10   | `[ums_approval]`   |
| **APPROVAL_WORKFLOW**      | Approvals      | UMS Core API           | .NET 10   | `[ums_approval]`   |
| **APPROVAL_REQUIRED_DOCUMENT**| Approvals   | UMS Core API           | .NET 10   | `[ums_approval]`   |
| **APPROVAL_REQUEST**       | Approvals      | UMS Core API           | .NET 10   | `[ums_approval]`   |
| **APPROVAL_LOG**           | Approvals      | UMS Core API           | .NET 10   | `[ums_approval]`   |
| **APP_CONFIGURATION**      | Configuration  | UMS Core API           | .NET 10   | `[ums_config]`     |
## 1.1 Mandatory Parametric Catalog Contract

For all parameter/configuration/catalog entities (including `APP_CONFIGURATION`, `NOTIFICATION_RULE`, `ACCESS_ENFORCEMENT_POLICY`, `APPROVAL_WORKFLOW`, and future `IDP_CONFIGURATION` / `SYSTEM_CONFIGURATION` / `FEATURE_FLAG` records), the write owner MUST enforce:

- mandatory fields: `Code`, `Value`, `Description`
- scope-aware unique constraints for `Code`
- explicit version lineage
- audit metadata and immutable change trail
- traceability events for cache invalidation and compliance
- forward-compatible schema evolution rules

`Description` MUST document purpose, functional impact, expected behavior, and applicable scope.

---

## 2. Data Access & Governance Rules

1.  **Strict Write Ownership**: Only the **Service Owner** specified in the table above is allowed to perform `INSERT`, `UPDATE`, or `DELETE` operations on the corresponding entities.
2.  **Cross-Service Reads (Read-Only)**:
    *   Satellite services (NestJS) may perform `SELECT` operations on `ums_identity` and `ums_authz` schemas to resolve context, but must do so through optimized views or read-only database users.
    *   Cross-service data dependency should ideally be resolved via **Domain Events** (Asynchronous) rather than direct cross-schema joins to maintain decoupling.
3.  **Schema Isolation**: Each schema acts as a logical boundary. Permissions in SQL Server must be granted granularly per service account.

---

## 3. Explicit Correction: Database Engine Standardization

> [!IMPORTANT]
> **Unified Engine Strategy**: The final authoritative decision for the UMS production product is **SQL Server 2022 for all services**, regardless of the runtime.
>
> **Action Required**: **ADR-0026** must be updated to reflect this unification and remove any legacy database alternatives from the requirements for this codebase.

---

## 4. References
*   For the complete E/R diagram and relationship cardinality, refer to **[er-export-formats.md](er-export-formats.md)**.
*   For technical implementation of **Progressive Isolation** (Application-level + SQL Server RLS hardening), refer to the Identity Context documentation and ADR-0041.
