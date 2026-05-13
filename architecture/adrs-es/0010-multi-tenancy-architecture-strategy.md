# ADR 0010: Estrategia de Arquitectura Multi-Inquilino para Evolución SaaS

* **Status:** Accepted
* **Basado en:** [arc32-10](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/core/0010-multi-tenancy-architecture-strategy.md)
* **Date:** 2026-05-08

## Resumen de Adaptación

The corporate standard is adopted with the following modificaciones específicas del proyecto:
1. UMS extiende el modelo corporativo con multi-inquilino jerárquico (ADR-0034): closure table + taxonomy.
2. RLS implementado con .NET DbConnectionInterceptor. Particionamiento LIST por root_tenant_id (ADR-0037).

## Referencia Completa del Estándar

Ver la fuente corporativa para el contexto completo y la justificación de la decisión.
