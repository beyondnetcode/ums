# ADR-0082: PostgreSQL Authoritative Persistence Baseline

**Status:** Accepted  
**Date:** 2026-06-08  
**Decision Owner:** Architecture  
**Supersedes:** SQL Server as the UMS default production persistence assumption in local UMS documentation  
**Related:** [ADR-0067](./0067-modular-monolith-schema-per-domain.md), [ADR-0070](./0070-database-schema-strategy-decision.md)

---

## Context

UMS already contains active PostgreSQL runtime configuration, Npgsql EF Core dependencies, PostgreSQL repositories, PostgreSQL schema bootstrapping, and PostgreSQL migrations. The previous documentation and project rules still described SQL Server as the authoritative production persistence target, which created a conflict between the implemented platform and the governance baseline.

The product owner has confirmed that PostgreSQL is the intended persistence target for UMS. This decision updates the UMS-specific baseline so future implementation, QA, documentation, and agent work evaluate persistence coherence against PostgreSQL instead of SQL Server.

Context7 resolution for this decision identified `Npgsql.EntityFrameworkCore.PostgreSQL` as the EF Core provider for PostgreSQL. The provider documentation shows `UseNpgsql(...)` as the DbContext configuration mechanism and supports PostgreSQL-specific migration operations such as `EnsurePostgresExtension(...)`. Microsoft EF Core documentation continues to define migrations and provider-based DbContext configuration as the expected schema-management path.

## Decision

UMS adopts PostgreSQL as its authoritative production persistence baseline.

The backend platform baseline is:

| Concern | Baseline |
|---|---|
| Runtime | .NET 10 |
| ORM | Entity Framework Core |
| Database | PostgreSQL |
| EF Core provider | Npgsql.EntityFrameworkCore.PostgreSQL |
| Module data ownership | Schema per bounded context |
| Tenancy primary control | Application-layer tenant filtering |
| Tenancy secondary failsafe | PostgreSQL row-level security, schema ownership, constraints, and database policies |
| Integration reliability | Outbox pattern with provider-specific EF Core persistence |

SQL Server references are now treated as legacy context, migration notes, or external comparison unless a future ADR explicitly reintroduces SQL Server as an active supported runtime target.

## Rules

1. Runtime configuration must default to PostgreSQL for non-test UMS execution paths.
2. New EF Core mappings, migrations, repositories, bootstrapping logic, read models, and integration tests must target PostgreSQL through Npgsql.
3. Application-layer tenant filtering remains the primary isolation mechanism.
4. PostgreSQL RLS and database policies are secondary safeguards only and must never replace application-layer authorization and query scoping.
5. Documentation must not describe SQL Server as the current authoritative UMS persistence target.
6. Any SQL Server mention must be explicitly framed as legacy context, migration history, external comparison, or corporate baseline contrast.
7. ADR-0070 remains valid for schema-per-module ownership, but its engine-specific wording is superseded by this ADR.

## Consequences

### Positive

- Aligns documentation, code, and runtime configuration around the implemented persistence provider.
- Removes ambiguity for QA gates and agent instructions.
- Allows PostgreSQL-specific capabilities, including advisory locks, extensions, schemas, and RLS policies, to be designed intentionally.
- Keeps the modular-monolith extraction path intact through explicit bounded-context schemas and application-layer tenant filtering.

### Trade-offs

- Existing SQL Server-oriented documents must be migrated or clearly marked as legacy.
- SQL Server-specific operational runbooks, performance plans, and RLS examples are no longer production-ready guidance until rewritten for PostgreSQL.
- Corporate Evolith examples that assume SQL Server require UMS-specific adaptation notes.

## Required Follow-Up

1. Update architecture, operations, requirements, and domain documents that still present SQL Server as current state.
2. Update test plans to use PostgreSQL containers and PostgreSQL-specific RLS/advisory-lock verification.
3. Update read-model and outbox documentation to cite Npgsql behavior explicitly.
4. Preserve bilingual parity for every changed document.

---

**[ADR Index](./index.md)** | **[ADR-0070](./0070-database-schema-strategy-decision.md)**
