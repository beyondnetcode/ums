# Database Provider Current State Assessment

## Current State

The current UMS monorepo implementation heavily relies on **SQL Server**.
- `Ums.Infrastructure` uses `Microsoft.EntityFrameworkCore.SqlServer`.
- SQL Scripts are specifically tailored for SQL Server (e.g., `sp_getapplock`, `sp_releaseapplock`, `NVARCHAR`, `UNIQUEIDENTIFIER`).
- Integration tests in `Ums.Presentation.IntegrationTest` rely on `Testcontainers.MsSql` and use SQL Server container fixtures.
- There are NO active references, packages, or configurations for **PostgreSQL** (`Npgsql`) anywhere in the source code.

## Gap Analysis vs Requirements

The requirement states that the solution must support both **SQL Server** and **PostgreSQL** without two independent functional implementations. 
- **Gap 1:** Migrations and schema bootstrapping are heavily coupled to SQL Server specifics (e.g. `SqlServerSchemaBootstrapper.cs`).
- **Gap 2:** Specific concurrency locks (`sp_getapplock`) do not exist in PostgreSQL (which uses `pg_advisory_lock`).
- **Gap 3:** No testing infrastructure for PostgreSQL integration tests.

## Conclusion

Currently, the architecture is portable in the Domain and Application layers (Clean Architecture), but the Infrastructure layer is heavily coupled to SQL Server. True provider-agnosticism requires abstracting the bootstrapper and lock mechanisms.
