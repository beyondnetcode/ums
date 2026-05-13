# ADR 0003: Strict TypeScript Standards and SonarJS Static Analysis

* **Status:** Accepted
* **Based on:** [arc32-3](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0003-strict-typescript-standards.md)
* **Date:** 2026-05-08

## Adaptation Summary

The corporate standard is adopted with the following project-specific modifications:
1. Scope limited to frontend (React/TS) only. Backend is C# with Roslyn/SonarAnalyzer.CSharp equivalents.
2. Frontend follows verbatim: no `any`, `interface` over `type`, SonarJS in ESLint. C# follows corporate ADR-0041 standards.

## Full Standard Reference

See the corporate source for the complete decision context and rationale.
