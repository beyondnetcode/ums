# API Audit Playbook

## Purpose

Use this playbook when reviewing or implementing backend queries, commands, contracts, endpoints, GraphQL schemas, repository behavior, or persistence concerns.

## Mandatory Checks

1. Commands remain REST-first.
2. Queries may be exposed through REST and GraphQL with equivalent semantics.
3. Pagination, filtering, sorting, and status/search normalization are centralized.
4. Error mapping is typed and consistent across REST and GraphQL.
5. Runtime validation exists at the boundary when contracts are untrusted.
6. Persistence is aligned to PostgreSQL through Npgsql and avoids SQL Server drift unless explicitly marked as legacy context or external comparison.
7. Tenant filtering is enforced at application level first, with PostgreSQL row-level security, schema ownership, constraints, and database policies as secondary failsafes.
8. Query handlers avoid unnecessary in-memory filtering when repository-level execution is feasible.

## Audit Focus Areas

- endpoint maturity
- query abstraction
- contract consistency
- persistence scalability
- cross-context leakage
- outbox and integration readiness

## Expected Outcome

The API should remain modular-monolith friendly today and extraction-friendly tomorrow.
