# ADR-0029: Adoption of @nestjslatam/ddd for Optional Tactical DDD Primitives

* **Status:** Accepted
* **Based on:** [arc32-29](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0029-tactical-ddd-primitives-library.md)
* **Date:** 2026-05-08

## Adaptation Summary

The corporate standard is adopted with the following project-specific modifications:
1. UMS uses C# native DDD primitives (records for ValueObjects, Entity base class, Result pattern) instead of @nestjslatam/ddd TypeScript package.
2. The DDD approach is identical: zero-dependency Domain, ValueObject with structural equality, AggregateRoot with domain events.
3. C# primitives are defined in Ums.Domain/Common/ (Entity.cs, Result.cs). No external library dependency.

## Full Standard Reference

See the corporate source for the complete decision context and rationale.
