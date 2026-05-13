# ADR 0012: Autorización Avanzada (RBAC/ABAC) y Estrategia de Auditoría de Seguridad

* **Status:** Accepted
* **Basado en:** [arc32-12](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/nodejs/0012-advanced-authorization-rbac-abac.md)
* **Date:** 2026-05-08

## Resumen de Adaptación

The corporate standard is adopted with the following modificaciones específicas del proyecto:
1. UMS implementa RBAC/ABAC via pipeline de middlewares .NET (ADR-0036, 0039) en lugar de Guards + Decorators NestJS.
2. Motor de compilación de políticas (ADR-0039) reemplaza decoradores @Roles()/@Permissions().

## Referencia Completa del Estándar

Ver la fuente corporativa para el contexto completo y la justificación de la decisión.
