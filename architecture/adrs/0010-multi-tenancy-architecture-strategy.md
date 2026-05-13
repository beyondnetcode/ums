# ADR 0010: Multi-Tenancy Architecture Strategy for SaaS Evolution

* **Status:** Accepted
* **Based on:** [arc32-10](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/core/0010-multi-tenancy-architecture-strategy.md)
* **Date:** 2026-05-08

## Adaptation Summary

The corporate standard is adopted with the following project-specific modifications:
1. Corporate defines hybrid pooled + RLS. UMS extends with hierarchical multi-tenancy (ADR-0034): closure table + taxonomy types.
2. RLS implementation uses .NET DbConnectionInterceptor (SET LOCAL) instead of NestJS AsyncLocalStorage.
3. Partitioning strategy (ADR-0037) extends the corporate model with LIST partitioning by root_tenant_id.
4. The hierarchical model is UMS-specific; the base pooled+RLS strategy follows corporate verbatim.

## Full Standard Reference

See the corporate source for the complete decision context and rationale.
