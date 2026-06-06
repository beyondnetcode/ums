# Database Provider Strategy (ADR)

## Status
Proposed

## Context
We need to support SQL Server and PostgreSQL without creating two distinct functional implementations. The Domain and Application layers must remain 100% agnostic.

## Decision
1. **Primary Provider:** SQL Server will remain the default and primary provider for production environments in Fase 1.
2. **Alternative Certified Provider (100% Support):** PostgreSQL is an absolute 100% supported alternative. The architecture guarantees zero impact when PostgreSQL is activated.
3. **Single Codebase Strategy & Guarantee:** 
   - EF Core will be used as the ORM. The Bounded Contexts are 100% pure from infrastructure dependencies.
   - We will use multiple EF Core Migration Projects (`Ums.Infrastructure.SqlServer`, `Ums.Infrastructure.PostgreSql`) to handle motor-specific dialects (like schemas and constraints) once PostgreSQL is switched on.
   - All vendor-specific lock mechanisms (like `sp_getapplock`) are completely hidden behind an `IDistributedLockProvider` interface. By guaranteeing this abstraction, delaying the PostgreSQL migrations to a subsequent PR will *not* affect the core architecture or the Postgres implementation.

## Consequences
- Requires injecting the DB Provider at runtime via configuration (`Persistence__Provider = SqlServer | PostgreSQL`).
- Requires adding `Npgsql.EntityFrameworkCore.PostgreSQL`.
- The CI pipeline must run integration tests against both MsSql and Postgres testcontainers to certify both.
