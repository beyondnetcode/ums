# ADR 0066: Database Schema per Domain for UMS Phase 1

## Status

Accepted

## Parent Standard

This ADR specializes the Evolith parent decision:

- [Evolith ADR 0067: Modular Monolith Database Boundary — Schema per Domain](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/core/0067-modular-monolith-schema-per-domain.md)

## Context and Problem Statement

UMS is implemented as both the official applied reference for Evolith and the Phase 1 product solution for enterprise user management.

The solution starts as a Modular Monolith, but it must avoid decisions that create unnecessary coupling or block a future migration toward distributed modules or microservices.

The key architectural question is whether UMS should use:

1. A single shared database without logical schema separation.
2. A single physical database with schemas separated by module/domain.
3. Multiple physical databases from Phase 1.

This decision is critical because it directly impacts:

- Domain responsibility separation.
- Future migration to microservices.
- Module independence.
- Prevention of unnecessary coupling.
- Formal Phase 1 ADR traceability.

## Decision

UMS Phase 1 will use:

```text
Single physical SQL Server database + logical schemas per module/domain
```

UMS will not start with multiple physical databases. However, it will also not use an unconstrained shared default schema for all modules.

Each bounded context or module must own its database schema.

Reference structure:

```text
ums_database
├── identity_schema
├── access_schema
├── tenant_schema
├── audit_schema
└── notification_schema
```

The final schema names must align with the official UMS bounded context map and domain model.

## Architectural Rules for UMS

1. Each UMS module owns its schema and internal tables.
2. A module must not directly mutate tables owned by another module.
3. Cross-module interaction must happen through application services, explicit contracts, ports, domain services, or integration events.
4. Cross-schema joins between modules are discouraged and require explicit architectural justification.
5. EF Core mappings and migrations must preserve module ownership by schema.
6. Any exception to schema ownership must be documented and reviewed architecturally.

## Rationale

This strategy preserves Phase 1 simplicity while enforcing architectural boundaries at the persistence layer.

A fully shared database schema would allow hidden coupling through direct table access, implicit joins, and unclear ownership. That would make the code look modular while the data model remains tightly coupled.

Using schemas per module/domain makes the Modular Monolith more honest and prepares UMS for future extraction when needed.

The intended evolution path is:

```text
UMS Modular Monolith with schema-per-domain boundaries
        ↓
Identify a bounded context that needs extraction
        ↓
Move that schema to a dedicated database
        ↓
Replace direct access with APIs, events, or integration contracts
```

## Alternatives Considered

### Alternative 1: Single SQL Server database with one shared default schema

Example:

```text
ums_database
└── dbo
```

**Advantages:**

- Simplest setup for the first implementation.
- Fewer initial conventions and migration rules.

**Disadvantages:**

- High risk of hidden coupling.
- Unclear table ownership.
- Harder future service extraction.
- Easier introduction of cross-domain joins and direct table dependencies.

**Result:** Rejected.

### Alternative 2: Physical database per module from Phase 1

Example:

```text
ums_identity_db
ums_access_db
ums_audit_db
ums_notification_db
```

**Advantages:**

- Strongest physical isolation.
- Clear deployment and ownership boundaries.

**Disadvantages:**

- Premature operational complexity for Phase 1.
- More complex local development, backups, observability, transactions, migrations, and deployment.
- Higher coordination cost before the product requires independent scaling or deployment.

**Result:** Rejected for Phase 1.

### Alternative 3: Single SQL Server database with schemas per module/domain

Example:

```text
ums_database
├── identity
├── access
├── tenant
├── audit
└── notification
```

**Advantages:**

- Balances simplicity and architectural discipline.
- Makes ownership explicit.
- Reduces coupling risk.
- Supports future microservice extraction.

**Disadvantages:**

- Requires schema naming and migration discipline.
- Requires governance to avoid unauthorized cross-schema access.

**Result:** Accepted.

## Consequences

### Positive

- UMS has clear persistence ownership boundaries from Phase 1.
- Module independence is reinforced beyond source-code structure.
- Future migration to microservices becomes less disruptive.
- Database coupling risks become visible and governable.
- The decision remains aligned with Evolith parent architecture.

### Negative / Trade-offs

- Requires schema conventions from the beginning.
- Requires careful EF Core configuration and migrations.
- Teams must avoid using SQL Server as an informal integration mechanism.
- Reporting or read-model scenarios may need explicit architectural exceptions.

## Implementation Guidance

- Define one EF Core schema mapping per bounded context/module.
- Keep migrations traceable to the owning module/schema.
- Avoid direct writes across schema boundaries.
- Prefer application-layer contracts, integration events, or dedicated read models for cross-module scenarios.
- Document any approved cross-schema dependency as an explicit exception.

## Final Rule

Each UMS module owns its schema. No module may directly access or modify another module's internal tables except through explicit, documented, and architecturally approved contracts.
