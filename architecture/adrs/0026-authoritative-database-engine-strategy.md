# ADR 0026: Authoritative Unified Database Engine Strategy

## Status
Approved

## Context
As the UMS project evolves into an enterprise-grade monorepo, it is necessary to standardize the database engine to optimize for operational simplicity, cross-service data integrity, and corporate licensing alignment. 

Previously, a polyglot engine strategy was proposed (SQL Server for .NET and PostgreSQL/MongoDB for Node.js). However, managing multiple database engines in production—especially in on-premise or localized deployments—increases infrastructure overhead, security complexity (multiple RLS implementations), and total cost of ownership.

## Decision
We will adopt a **Unified Database Strategy** for all services within the UMS product ecosystem:

1.  **Unified Relational Engine:** **SQL Server 2022** (Standard or Enterprise).
    *   **All runtimes** (.NET 8 Core API and Node.js/NestJS Satellite services) must persist their data exclusively in SQL Server 2022.
    *   **Rationale:** Standardizing on a single engine allows for a unified Row-Level Security (RLS) implementation, simplified backup/restore procedures, and consistent execution plan analysis.

2.  **Schema-per-Context Isolation:**
    *   To maintain modularity, each Bounded Context will own a dedicated SQL Server Schema (e.g., `[ums_identity]`, `[ums_authz]`).
    *   Direct cross-schema joins are discouraged in favor of domain events, but permitted for optimized read-only reporting views under strict governance.

3.  **ORM / Driver Support:**
    *   **.NET 8**: Entity Framework Core with `Microsoft.EntityFrameworkCore.SqlServer`.
    *   **Node.js / NestJS**: TypeORM or Prisma using the **`mssql`** driver.

4.  **Security Implementation:**
    *   **Unified RLS**: All services will utilize SQL Server **Security Policies** and **SESSION_CONTEXT** for multi-tenant isolation.
    *   This eliminates the need to maintain parallel RLS logic for PostgreSQL.

## Consequences
*   **Correction**: All references to PostgreSQL or MongoDB for Node.js services in `stack.md` or earlier documentation are now deprecated.
*   **Infrastructure**: Local development environments (Docker Compose) will only require a single `mssql-server-linux` instance.
*   **Skill Set**: The team must ensure proficiency in T-SQL and SQL Server-specific performance tuning across all language stacks.
*   **Migration**: Any existing Node.js prototypes using PostgreSQL must be migrated to SQL Server.
