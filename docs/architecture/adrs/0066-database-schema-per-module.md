# ADR 0066: Database Boundary Strategy — Schema per Module for UMS

## Status

Accepted

## Parent Standard

This ADR specializes the Evolith parent decision:

- [Evolith ADR 0067: Modular Monolith Database Boundary — Schema per Domain](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/core/0067-modular-monolith-schema-per-domain.md)

## Context and Problem Statement

UMS is the official executable product reference for Evolith and is implemented in Phase 1 as a Modular Monolith using .NET 8 and SQL Server 2022.

Because UMS starts as a modular monolith, it must avoid premature operational complexity while also preventing future data-level coupling that would make later microservice extraction difficult.

The key decision is whether Phase 1 should use:

1. A single shared database without logical separation.
2. A single physical database with schema separation by module/domain.
3. A dedicated physical database per module from the beginning.

This decision directly impacts domain responsibility separation, module independence, future microservice migration, and Phase 1 ADR traceability.

## Decision

UMS will use:

```text
Single physical SQL Server database + dedicated schemas per UMS module/domain
```

UMS will not create multiple physical databases during Phase 1. However, it will also not place all module tables into a single unconstrained default schema.

Each UMS bounded context or module must own its schema.

Reference schema structure:

```text
ums_database
├── identity_schema
├── access_schema
├── tenant_schema
├── audit_schema
└── notification_schema
```

Final schema names must be aligned with the official UMS bounded context map and database design artifacts.

## Architectural Rules for UMS

1. Each UMS module owns its schema and internal tables.
2. A module must not directly modify tables owned by another module.
3. Cross-module communication must happen through application services, explicit contracts, ports, domain services, or integration events.
4. Cross-schema joins between different domains are discouraged and require documented architectural justification.
5. EF Core mappings and migrations must preserve schema ownership.
6. Exceptions for reporting, projections, or administrative read models must be explicitly documented.
7. The database must not become an informal integration layer between modules.

## Rationale

This approach keeps Phase 1 simple because UMS still operates with a single SQL Server database. At the same time, it creates visible persistence boundaries that match the modular monolith design.

Schema separation reduces the risk of hidden coupling through direct table access, unclear ownership, and cross-domain SQL dependencies.

It also gives UMS a cleaner evolution path:

```text
UMS Modular Monolith with schemas per module
        ↓
Identify a module requiring extraction
        ↓
Move the module schema to a dedicated database
        ↓
Expose the module through APIs, events, or integration contracts
```

## Alternatives Considered

### Alternative 1: Single database with no schema separation

```text
ums_database
└── dbo / public / default_schema
```

This is simple initially, but it increases the risk of database-level coupling and makes ownership ambiguous.

**Result:** Rejected.

### Alternative 2: Dedicated physical database per module from Phase 1

```text
ums_identity_db
ums_access_db
ums_audit_db
ums_notifications_db
```

This maximizes isolation but adds unnecessary deployment, transaction, backup, monitoring, and local-development complexity for Phase 1.

**Result:** Rejected for Phase 1.

### Alternative 3: Single physical database with schemas per module/domain

```text
ums_database
├── identity_schema
├── access_schema
├── tenant_schema
├── audit_schema
└── notification_schema
```

This balances simplicity and architectural discipline.

**Result:** Accepted.

## Consequences

### Positive

- Clear ownership of UMS persistence boundaries.
- Better alignment between bounded contexts and database design.
- Lower risk of hidden module coupling.
- Easier future extraction toward microservices when justified.
- Stronger compliance with Evolith inheritance rules.
- Clear Phase 1 ADR traceability.

### Negative / Trade-offs

- Requires schema naming and migration discipline.
- Requires code reviews to prevent accidental cross-schema coupling.
- Requires architectural governance for exceptional reporting/read-model cases.
- Does not prevent coupling by itself if modules bypass application-level contracts.

## Compliance

This ADR is mandatory for UMS Phase 1.

Any exception allowing direct cross-schema access, shared mutable tables, or direct modification of another module's schema must be documented and approved through a dedicated ADR amendment or exception record.
