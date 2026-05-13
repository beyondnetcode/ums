# ADR 0021: High-Performance Authentication and Authorization Graph Compilation

* **Status:** Accepted
* **Based on:** [arc32-21](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0021-high-performance-auth-and-graph-compilation.md)
* **Date:** 2026-05-08

## Adaptation Summary

The corporate standard is adopted with the following project-specific modifications:
1. Conceptually integrated into ADR-0039 (Policy Compilation Engine). The corporate concept of Redis-cached auth graph is adopted.
2. .NET implementation uses IDistributedCache instead of NestJS CacheManager. Key schema uses `compiled_policy:v2:` prefix.
3. Deferred: gRPC sub-calls for internal graph resolution until Phase 2.

## Full Standard Reference

See the corporate source for the complete decision context and rationale.
