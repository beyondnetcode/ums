# ADR-0066: Database Schema Per Module

**Status:** Accepted  
**Date:** 2026-05-27  
**Decision Owner:** Architecture  
**Parent:** [ADR-0067 — Modular Monolith Schema Per Domain](../../../reference/architecture/adrs/core/0067-modular-monolith-schema-per-domain.md)

---

## Context

UMS is a modular monolith built on .NET 10 and SQL Server, implementing multiple bounded contexts (Identity, Authorization, Configuration, Audit, Approvals, IGA). The Evolith architecture baseline (ADR-0067) mandates that each bounded context must own its own database schema within a single physical database, enforcing logical separation and module ownership.

Prior to this decision, schema naming was inconsistent between documentation and code, and some entity configurations relied on the DbContext default schema fallback rather than explicitly declaring their schema. This created risks of:

- Silent misplacement of tables into the wrong schema if the default changes.
- Developer confusion when documentation schema names did not match the actual database.
- Unclear module ownership at the database level.

## Decision

**UMS uses a single physical SQL Server database with dedicated schemas per bounded context. Each module owns its schema and its tables. Cross-module database access is prohibited at the repository level.**

### Schema Registry

| Bounded Context | Code | Schema Name | Owner Module |
|-----------------|------|-------------|--------------|
| Identity | BC-A | `ums_identity` | `Ums.Infrastructure/Persistence/Identity/` |
| Authorization | BC-B | `ums_authorization` | `Ums.Infrastructure/Persistence/Authorization/` |
| Configuration | BC-C | `ums_configuration` | `Ums.Infrastructure/Persistence/Configuration/` |
| Audit | BC-D | `audit` | `Ums.Infrastructure/Persistence/Audit/` |
| Approvals | BC-F | `approvals` | `Ums.Infrastructure/Persistence/Approvals/` |
| IGA | BC-H | `iga` | `Ums.Infrastructure/Persistence/IGA/` |
| Platform/Outbox | Infra | `ums_platform` | `Ums.Infrastructure/Persistence/Outbox/` |

### Rules

1. **Single physical database.** All schemas coexist in one SQL Server instance.
2. **Schema per module.** Each bounded context has exactly one schema. Tables belonging to a context are created in its schema only.
3. **Explicit schema in EF Core.** Every `IEntityTypeConfiguration` must call `ToTable(tableName, schema)` with the module's schema constant. No entity may rely on `HasDefaultSchema()` fallback.
4. **Centralized schema constants.** Each module declares its schema name in a `*PersistenceConstants.cs` file under its persistence folder.
5. **Module ownership.** A module's repositories may only query DbSets belonging to its own bounded context. Cross-module access must use application services, domain services, read models, or integration events (Outbox pattern).
6. **No `dbo` usage.** No table may be created in the default `dbo` schema.
7. **Migration ownership.** Each SQL migration file creates or modifies tables within a single module's schema. Cross-schema migrations are prohibited.
8. **Cross-module communication.** Modules communicate via the Outbox pattern (integration events), not via direct database joins across schemas.

### DbContext Configuration

`UmsPlatformDbContext` serves all bounded contexts but respects schema boundaries through explicit `ToTable(tableName, schema)` in each entity configuration:

```csharp
// Example: Authorization entity configuration
builder.ToTable("SystemSuites", AuthorizationPersistenceConstants.Schema);
```

The default schema (`ums_platform`) is set as a fallback for infrastructure entities only (Outbox), but even these must explicitly declare their schema.

## Consequences

### Positive

- **Clear module ownership.** Each schema is owned by exactly one bounded context.
- **Isolation at database level.** Modules cannot accidentally modify another module's tables.
- **Auditability.** Schema boundaries make it easy to trace which module owns which data.
- **Migration safety.** Migrations are scoped to a single schema, reducing conflict risk.
- **Documentation alignment.** Schema names in code and docs are identical.

### Trade-offs

- **Single DbContext coupling.** All bounded contexts share one `UmsPlatformDbContext`, creating compile-time coupling. This is accepted for a modular monolith but would need refactoring if UMS evolves to a distributed architecture.
- **Schema naming convention.** Not all schemas use the `ums_` prefix (`audit`, `approvals`, `iga`). This is a pragmatic choice to keep names concise; the `ums_` prefix is reserved for core business contexts.

## Compliance

- EF Core entity configurations must use `ToTable(tableName, schema)` with a constant from `*PersistenceConstants.cs`.
- SQL migrations must create schemas explicitly (`CREATE SCHEMA`) and all tables within that schema.
- The ADR index and bounded context map must reflect the canonical schema names listed in this document.
- Architecture guard tests (NetArchTest or equivalent) enforce that repositories only access their own module's DbSets.

---

**[ADR Index](./index.md)** | **[Parent ADR-0067](../../../reference/architecture/adrs/core/0067-modular-monolith-schema-per-domain.md)**
