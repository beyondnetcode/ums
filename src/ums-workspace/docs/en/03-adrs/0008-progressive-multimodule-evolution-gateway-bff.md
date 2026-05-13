# ADR 0008: Progressive Multi-Module Evolution with API Gateway and BFF Patterns

* **Status:** Accepted
* **Based on:** [arc32-8](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0008-progressive-multimodule-evolution-gateway-bff.md)
* **Date:** 2026-05-08

## Adaptation Summary

The corporate standard is adopted with the following project-specific modifications:
1. BFF Gateway implemented in .NET 8 Minimal APIs instead of NestJS/Express.
2. Same architecture pattern (Web BFF, Mobile BFF, B2B Gateway), same Kong routing. Only the BFF runtime differs.
3. Deferred: Mobile BFF and gRPC internal communication until Phase 2.

## Full Standard Reference

See the corporate source for the complete decision context and rationale.
