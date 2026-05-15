# ADR 0016: Traza de Auditoría de Negocio Inmutable y Seguimiento de Cambios

* **Status:** Accepted
* **Basado en:** [arc32-16](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/0016-immutable-business-audit-trail.md)
* **Date:** 2026-05-08

## Resumen de Adaptación

The corporate standard is adopted with the following modificaciones específicas del proyecto:
1. UMS añade contexto jerárquico: root_tenant_id, effective_tenant_id, actor_hierarchy_level, delegation_chain[] en cada registro.
2. Particionamiento por root_tenant_id (ADR-0037) para soberanía de datos por inquilino.

## Referencia Completa del Estándar

Ver la fuente corporativa para el contexto completo y la justificación de la decisión.
