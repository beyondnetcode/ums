# Audit BC — Aggregate Architecture

> **Language:** [English](./index.md) | [Español](../../domain-es/audit/index.md)

**Bounded Context:** Security & Compliance Audit (`Ums.Domain.Audit`)  
**Aggregate Roots:** `AuditRecord`

---

### System Activity Auditing
Coordinates immutable, append-only logs logging critical operations, authorization shifts, and multi-tenant security changes:
- [AuditRecord](./audit-record.md) (Aggregate Root) — Repositories tracking actor footprints, changed payloads, affected entities, and outcome successes. Strictly read-only once committed.

---

**[Back to Domain Index](../index.md)**
