# ADR 0012: Advanced Authorization (RBAC/ABAC) and Security Auditing Strategy

* **Status:** Accepted
* **Based on:** [arc32-12](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0012-advanced-authorization-rbac-abac.md)
* **Date:** 2026-05-08

## Adaptation Summary

The corporate standard is adopted with the following project-specific modifications:
1. UMS implements RBAC/ABAC via .NET middleware pipeline (ADR-0036, ADR-0039) instead of NestJS Guards + Decorators.
2. Policy compilation engine (ADR-0039) replaces NestJS @Roles()/@Permissions() decorators with a compiled policy graph.
3. Anti-privilege escalation (ADR-0036) is UMS-specific addition not covered by the corporate standard.

## Full Standard Reference

See the corporate source for the complete decision context and rationale.
