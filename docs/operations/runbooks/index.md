# Runbooks Index

**[Back to Operations](../index.md)**

Operational procedures for diagnosing and recovering from infrastructure and application failures in the UMS environment. Each runbook is self-contained and executable by an on-call engineer without prior context.

---

## Runbook Catalog

| ID | Title | Trigger | Severity |
| :--- | :--- | :--- | :--- |
| [RB-01](./rb-01-incident-response.md) | Incident Response | Any production alert | P1–P4 |
| [RB-02](./rb-02-rollback-procedure.md) | Rollback Procedure | Deployment regression detected | P1–P2 |
| [RB-03](./rb-03-cache-failure-recovery.md) | Cache Failure Recovery (Redis) | Redis unavailable or hit rate degraded | P2 |
| [RB-04](./rb-04-database-failover.md) | Database Failover (SQL Server) | SQL Server unreachable or data integrity risk | P1
## Decision Tree — Which Runbook?

```
Alert fires

 Auth 401/403 spike or latency > 2s?
    Redis PING fails → RB-03 (Cache Recovery)
    SQL Server unreachable → RB-04 (DB Failover)
    After recent deployment → RB-02 (Rollback)

 Full service outage?
    Start with RB-01 (Incident Response) → triage leads to RB-03 or RB-04

 Deployment just released and metrics degraded?
    RB-02 (Rollback Procedure)

 Unsure of root cause?
     Always start with RB-01 (Incident Response)
```

---

## Related Technical Enablers

Runbooks reference these TEs for implementation context:

- [TE-01 — Build Authorization Graph](../../architecture/blueprints/technical-enablers/te-01-build-authorization-graph.md)
- [TE-03 — Enforce Organization RLS](../../architecture/blueprints/technical-enablers/te-03-enforce-organization-rls-sql-server.md)
- [TE-06 — CQRS Projection Rebuild](../../architecture/blueprints/technical-enablers/te-06-cqrs-projection-rebuild.md)
