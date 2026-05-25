# Operations Portal

Operational procedures and runbooks for the User Management System (UMS) in production.

## Runbooks

Step-by-step recovery and operational procedures:

- **[RB-01: Incident Response Procedure](./runbooks/rb-01-incident-response.md)** — Severity classification, triage, war room, post-mortem template
- **[RB-02: Rollback Procedure](./runbooks/rb-02-rollback-procedure.md)** — Application and DB rollback steps
- **[RB-03: Cache Failure Recovery](./runbooks/rb-03-cache-failure-recovery.md)** — Redis failure diagnosis and recovery
- **[RB-04: Database Failover](./runbooks/rb-04-database-failover.md)** — Primary failover, PITR, multi-tenant RLS verification

## Engineering Metrics

- **[Solution Metrics Dashboard](./metrics/index.md)** — Consolidated metrics across all solutions (API, Web, Libs, Tests) by category: coding, security, quality, tests, and AI usage. Auto-updated after each commit.

---

**[Back to Master Index](../MASTER_INDEX.md)**
