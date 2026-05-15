# Technical Enablers Index

**[Back to Main README](../../../../README.md)**

**[Back to Blueprints](../index.md)**

Technical Enablers bridge the gap between architectural decisions (ADRs) and implementation. Each document specifies transaction flows, actors, failure handling, and observability for a complex cross-cutting pattern.

## Domain Enablers

| ID | Title | Pattern | Consumed by |
| :--- | :--- | :--- | :--- |
| [TE-01](./te-01-build-authorization-graph.md) | Build Authorization Graph | Permission graph compilation + Redis cache | FS-02, FS-05, FS-06, FS-07 |
| [TE-02](./te-02-resolve-hierarchical-config.md) | Resolve Hierarchical Config | Config inheritance + deep merge | FS-13 |
| [TE-03](./te-03-enforce-organization-rls-sql-server.md) | Enforce Organization RLS | Multi-tenant data isolation (SQL Server 2022) | FS-03 | ## Messaging & Consistency Enablers

| ID | Title | Pattern | Consumed by |
| :--- | :--- | :--- | :--- |
| [TE-04](./te-04-transactional-outbox.md) | Transactional Outbox | At-least-once domain event delivery | FS-10, FS-11, FS-15 |
| [TE-05](./te-05-distributed-saga-dapr.md) | Distributed Saga with Dapr | Choreography saga + compensation flows | FS-10, FS-12 |
| [TE-06](./te-06-cqrs-projection-rebuild.md) | CQRS Projection Rebuild | Read model rebuild from event store | FS-07
See the [Traceability Matrix](../../traceability-matrix.md) for full FS → ADR → TE coverage.
