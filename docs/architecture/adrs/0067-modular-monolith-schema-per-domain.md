# ADR-0067: Modular Monolith Schema Per Domain

**Status:** Accepted  
**Date:** 2026-05-27  
**Decision Owner:** Evolith Architecture Board  
**Scope:** All Evolith satellite repositories (modular monoliths using SQL Server)

---

## Context

Evolith satellite repositories implement modular monolith architectures where multiple bounded contexts coexist within a single deployable unit. When using SQL Server as the persistence engine, a decision must be made about how to isolate data between bounded contexts at the database level.

The options considered were:

1. **Single schema, all tables in `dbo`.** Simple but provides no isolation. Any module can query or modify any table. No ownership boundaries.
2. **Separate databases per module.** Maximum isolation but adds operational complexity (connection strings, migrations, backups, cross-database transactions).
3. **Single database, schema per module.** Logical isolation within a single physical database. Each module owns its schema. Cross-module access is explicit and controlled.

Option 3 provides the best balance of isolation, operational simplicity, and architectural clarity for a modular monolith.

## Decision

**All Evolith modular monoliths using SQL Server must use a single physical database with dedicated schemas per bounded context. Each module owns its schema and its tables.**

### Mandatory Rules

1. **One database per deployable unit.** A modular monolith connects to exactly one SQL Server database.
2. **One schema per bounded context.** Each bounded context declares and owns exactly one schema.
3. **Explicit schema declaration.** All entity mappings (EF Core `ToTable`, Dapper queries, raw SQL) must explicitly reference the owning module's schema. No entity may rely on the default schema.
4. **No cross-schema joins in repositories.** A module's repository layer must not JOIN tables from another module's schema. Cross-module data access must use:
   - Application service calls
   - Domain services
   - Read models / materialized views
   - Integration events (Outbox pattern)
5. **No `dbo` tables.** Application tables must never be created in the `dbo` schema.
6. **Migration ownership.** Each migration file targets a single module's schema. Cross-schema migrations are prohibited.
7. **Schema naming convention.** Schemas follow the pattern `ums_<context>` for core business contexts. Infrastructure schemas may use shorter names (e.g., `audit`, `outbox`) with justification.
8. **Centralized schema constants.** Each module declares its schema name in a constants file. All entity configurations reference this constant.

### Cross-Module Communication

Modules communicate through explicit contracts:

| Pattern | Direction | Mechanism |
|---------|-----------|-----------|
| Customer-Supplier | Upstream → Downstream | Integration events via Outbox |
| Conformist | Any → Audit | Append-only event ledger |
| Read-Aside | Domain → Cache | Cache port with invalidation |
| ACL | External → Domain | Strategy pattern port |

Direct database access between modules is prohibited.

### DbContext Strategy

For .NET modular monoliths, a single `DbContext` may serve all bounded contexts, but:

- Each entity configuration must explicitly declare its schema via `ToTable(tableName, schema)`.
- `HasDefaultSchema()` may be set for infrastructure purposes but must not be relied upon for entity mappings.
- Global query filters (e.g., tenant isolation) are applied per-entity within the DbContext.

## Consequences

### Positive

- **Logical isolation.** Modules cannot accidentally modify another module's data.
- **Clear ownership.** Schema boundaries make it obvious which team/module owns which data.
- **Operational simplicity.** Single database means one connection string, one backup strategy, one migration pipeline.
- **Auditability.** Schema boundaries simplify compliance audits and data lineage tracing.
- **Evolution path.** If a module needs to be extracted to a separate service, its schema can be migrated to a separate database with minimal changes.

### Trade-offs

- **Compile-time coupling.** A single `DbContext` means all modules are compiled together. This is accepted for modular monoliths.
- **Single point of failure.** One database means all modules share the same availability risk. Mitigated by SQL Server HA/DR features.
- **Schema naming discipline.** Teams must agree on and enforce schema naming conventions.

## Compliance

Satellite repositories must:

1. Document their schema registry in a product-level ADR that references this parent ADR.
2. Ensure all EF Core entity configurations use `ToTable(tableName, schema)` with explicit schema.
3. Ensure all SQL migrations create schemas explicitly and respect module ownership.
4. Add architecture guard tests to enforce schema boundaries.
5. Keep documentation (bounded context maps, domain context docs) aligned with actual schema names.

---

**[Evolith ADR Registry](./README.md)** | **[Child: UMS ADR-0066](../../../../docs/architecture/adrs/0066-database-schema-per-module.md)**
