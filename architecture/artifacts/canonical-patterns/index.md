# Canonical Code Patterns

**[Back to Artifacts](../index.md)**

Reference implementations for the critical architectural patterns mandated by the UMS engineering standards. Each pattern document includes the problem statement, the canonical C# / .NET 8 code, and the anti-patterns to reject in code review.

> **Runtime:** C# / .NET 8 · ASP.NET Core Minimal APIs · EF Core 8  
> **Layer mandate:** `Ums.Domain` has zero NuGet references. All patterns respect this invariant.

---

## Pattern Catalog

| ID | Pattern | ADR | When to apply |
| :--- | :--- | :--- | :--- |
| [CP-01](./cp-01-hexagonal-port-adapter.md) | Hexagonal Port / Adapter | ADR-0002 | Every external dependency (DB, cache, HTTP, messaging) |
| [CP-02](./cp-02-aggregate-root-domain-event.md) | Aggregate Root + Domain Event | ADR-0019, ADR-0015 | Any entity with business invariants or state transitions |
| [CP-03](./cp-03-result-pattern.md) | Result Pattern | dotnet migration plan §3.1 | Every use case handler and domain factory method |
| [CP-04](./cp-04-multitenant-repository-rls.md) | Multi-tenant Repository + RLS | ADR-0010, ADR-0037, ADR-0041 | Every repository that touches tenant-scoped data
## Decision Guide

```
Need to call a database / cache / external API?
  → CP-01 (define a Port, write an Adapter)

Entity has business rules or state transitions?
  → CP-02 (Aggregate Root + factory method + domain events)

Use case or factory method can fail for a business reason?
  → CP-03 (return Result<T>, never throw)

Query touches data owned by a specific organization?
  → CP-04 (inject SESSION_CONTEXT via interceptor, never filter manually)
```

---

## Related Documents

- [Traceability Matrix](../../traceability-matrix.md)
- [Engineering Standards](../engineering-standards.md)
- [dotnet Migration & Tech Stack Plan](../../blueprints/dotnet-migration-and-tech-stack-plan.md)
- [ADR-0002 — Clean Architecture & Hexagonal Boundaries](../../adrs/0002-clean-architecture-nestjs.md)
- [ADR-0019 — Tactical Design Patterns](../../adrs/0019-tactical-design-patterns-future-proofing.md)
