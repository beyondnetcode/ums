# ADR 0002: Clean Architecture and Hexagonal Boundaries on NestJS

* **Status:** Accepted
* **Based on:** [arc32-2](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/core/0002-clean-architecture-hexagonal.md)
* **Date:** 2026-05-08

## Adaptation Summary

The corporate standard is adopted with the following project-specific modifications:
1. .NET 8 runtime instead of NestJS/Node.js — Domain layer uses C# POCOs, Application layer uses MediatR-free service interfaces.
2. Port/Adapter pattern is identical; Imports are enforced via .editorconfig + Roslyn analyzers instead of eslint-plugin-boundaries.
3. Dependency injection uses native Microsoft.Extensions.DependencyInjection instead of NestJS DI.
4. Validation uses FluentValidation instead of NestJS class-validator.

## Full Standard Reference

See the corporate source for the complete decision context and rationale.
