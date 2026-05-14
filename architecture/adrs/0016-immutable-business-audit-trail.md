# ADR 0016: Immutable Business Audit Trail and Change Tracking

* **Status:** Accepted
* **Based on:** [arc32-16](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/0016-immutable-business-audit-trail.md)
* **Date:** 2026-05-08

## Adaptation Summary

The corporate standard is adopted with the following project-specific modifications:
1. UMS adds hierarchical audit context: every audit record includes root_tenant_id, effective_tenant_id, actor_hierarchy_level, and delegation_chain[] (ADR-0038).
2. Corporate defines row-level metadata + immutable ledger. UMS implementation is identical for the base pattern.
3. Audit log is partitioned by root_tenant_id (ADR-0037) for tenant-level data sovereignty.

## Full Standard Reference

See the corporate source for the complete decision context and rationale.
