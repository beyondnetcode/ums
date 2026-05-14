# Service-Entity Map & Data Ownership

This document serves as the authoritative mapping between system entities, their Bounded Contexts, owning services, and database schemas within the UMS enterprise ecosystem.

---

## 🏛️ 1. Entity Ownership & Schema Mapping

| Entity | Bounded Context | Service Owner (Write) | Runtime | SQL Server Schema |
| :--- | :--- | :--- | :--- | :--- |
| **TENANT** | Identity | UMS Core API | .NET 8 | `[ums_identity]` |
| **BRANCH** | Identity | UMS Core API | .NET 8 | `[ums_identity]` |
| **USER_ACCOUNT** | Identity | UMS Core API | .NET 8 | `[ums_identity]` |
| **ROLE** | Authorization | UMS Core API | .NET 8 | `[ums_authz]` |
| **PROFILE** | Authorization | UMS Core API | .NET 8 | `[ums_authz]` |
| **PROFILE_PERMISSION** | Authorization | UMS Core API | .NET 8 | `[ums_authz]` |
| **PERMISSION_TEMPLATE** | Authorization | UMS Core API | .NET 8 | `[ums_authz]` |
| **FUNCTIONAL_MODULE** | Authorization | UMS Core API | .NET 8 | `[ums_authz]` |
| **FUNCTIONAL_SUBMODULE**| Authorization | UMS Core API | .NET 8 | `[ums_authz]` |
| **FUNCTIONAL_OPTION** | Authorization | UMS Core API | .NET 8 | `[ums_authz]` |
| **ACTION** | Authorization | UMS Core API | .NET 8 | `[ums_authz]` |
| **SYSTEM_SUITE** | Authorization | UMS Core API | .NET 8 | `[ums_authz]` |
| **ROLE_PROMOTION_CRITERIA**| IGA | IGA Satellite | NestJS | `[ums_iga]` |
| **USER_PROMOTION_PROCESS** | IGA | IGA Satellite | NestJS | `[ums_iga]` |
| **USER_MANAGEMENT_DELEGATION**| IGA | IGA Satellite | NestJS | `[ums_iga]` |
| **DOCUMENT_TYPE** | Compliance | Compliance Satellite | NestJS | `[ums_compliance]` |
| **USER_DOCUMENT** | Compliance | Compliance Satellite | NestJS | `[ums_compliance]` |
| **NOTIFICATION_RULE** | Compliance | Compliance Satellite | NestJS | `[ums_compliance]` |
| **ACCESS_ENFORCEMENT_POLICY**| Compliance | Compliance Satellite | NestJS | `[ums_compliance]` |
| **APPROVAL_WORKFLOW** | Approvals | UMS Core API | .NET 8 | `[ums_approval]` |
| **APPROVAL_REQUIRED_DOCUMENT**| Approvals | UMS Core API | .NET 8 | `[ums_approval]` |
| **APPROVAL_REQUEST** | Approvals | UMS Core API | .NET 8 | `[ums_approval]` |
| **APPROVAL_LOG** | Approvals | UMS Core API | .NET 8 | `[ums_approval]` |
| **APP_CONFIGURATION** | Configuration | UMS Core API | .NET 8 | `[ums_config]` |

---

## 🛠️ 2. Data Access & Governance Rules

1.  **Strict Write Ownership**: Only the **Service Owner** specified in the table above is allowed to perform `INSERT`, `UPDATE`, or `DELETE` operations on the corresponding entities.
2.  **Cross-Service Reads (Read-Only)**:
    *   Satellite services (NestJS) may perform `SELECT` operations on `ums_identity` and `ums_authz` schemas to resolve context, but must do so through optimized views or read-only database users.
    *   Cross-service data dependency should ideally be resolved via **Domain Events** (Asynchronous) rather than direct cross-schema joins to maintain decoupling.
3.  **Schema Isolation**: Each schema acts as a logical boundary. Permissions in SQL Server must be granted granularly per service account.

---

## ⚠️ 3. Explicit Correction: Database Engine Standardization

> [!IMPORTANT]
> **Unified Engine Strategy**: Although `architecture/blueprints/stack.md` and some early prototypes mention PostgreSQL for Node.js/NestJS components, the final authoritative decision for the UMS production product is **SQL Server 2022 for all services**, regardless of the runtime (.NET 8 or NestJS).
>
> **Action Required**: **ADR-0026** must be updated to reflect this unification, removing PostgreSQL and MongoDB from the relational/NoSQL requirements for this specific codebase.

---

## 🔗 4. References
*   For the complete E/R diagram and relationship cardinality, refer to **[er-export-formats.md](er-export-formats.md)**.
*   For technical implementation of Row-Level Security (RLS) in SQL Server, refer to the Identity Context documentation.
