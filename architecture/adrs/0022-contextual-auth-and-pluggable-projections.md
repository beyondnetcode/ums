# ADR 0022: Contextual Authentication and Pluggable Output Projections

* **Status:** Accepted
* **Based on:** [arc32-22](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0022-contextual-auth-and-pluggable-projections.md)
* **Date:** 2026-05-08

## Adaptation Summary

The corporate standard is adopted with the following project-specific modifications:
1. Same pluggable projection concept, implemented via .NET Strategy Pattern with IServiceProvider resolution instead of NestJS module dynamic imports.
2. Output formats: JSON hierarchical graph, JWT compressed claims, .NET ClaimsPrincipal. Same formats, different serialization.

## Full Standard Reference

See the corporate source for the complete decision context and rationale.
