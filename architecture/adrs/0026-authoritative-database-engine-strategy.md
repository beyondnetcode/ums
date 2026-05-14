# ADR 0026: Authoritative Database Engine Strategy by Language Stack

## Status
Proposed

## Context
As the UMS project evolves into an enterprise-grade monorepo and migrates core components to .NET 8, it is necessary to standardize the database engine preferences to optimize for ecosystem alignment, performance, and maintainability across different language stacks.

Previously, PostgreSQL was used as the default relational engine for both Node.js and .NET prototypes. However, corporate standards for .NET excellence prioritize SQL Server for mission-critical .NET applications.

## Decision
We will adopt a polyglot-aware database strategy based on the primary language of the application service:

1.  **For .NET 8+ Applications (e.g., UMS Core API):**
    *   **Relational Engine:** **SQL Server 2022** (Latest stable version).
    *   **ORM:** Entity Framework Core (EF Core) with `Microsoft.EntityFrameworkCore.SqlServer`.
    *   **Rationale:** Seamless integration with .NET ecosystem, superior tooling (SSMS/Azure Data Studio), and optimized execution plans for MediatR-based workloads.

2.  **For Node.js / NestJS Applications:**
    *   **Relational Engine:** **PostgreSQL 16**.
    *   **NoSQL Engine:** **MongoDB**.
    *   **Rationale:** Community maturity, native JSONB support in PG, and high developer velocity in the Node.js ecosystem.

### Security Implementation
*   **Row-Level Security (RLS):** For .NET/SQL Server services, RLS will be implemented using SQL Server **Security Policies** and **Inline Table-Valued Functions (iTVF)** for filtering, instead of PostgreSQL policies.
*   **Multi-Tenancy:** The "Shared Database, Shared Schema" model remains mandatory, enforced via SQL Server RLS.

## Consequences
*   The UMS migration plan from NestJS (Node) to .NET 8 must be updated to replace Npgsql with the SQL Server provider.
*   Local development environments (Docker Compose) will include a `mssql-server-linux` container for the .NET API.
*   Architectural blueprints and technical inventories must be updated to reflect SQL Server as the authoritative engine for the .NET stack.
*   Satellite systems inheriting from this reference architecture must follow these same engine preferences based on their language stack.
