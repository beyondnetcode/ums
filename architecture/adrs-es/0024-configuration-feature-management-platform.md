# ADR-0024: Plataforma Centralizada de Configuración y Gestión de Funcionalidades

* **Estado:** Aceptado (Incorporado por Referencia)
* **Fuente Corporativa:** [arc32-24](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/arc-corporate-ws/corporate-standards/02-adrs/core/0024-configuration-feature-management-platform.md)

## Decisión

Este proyecto adopta el estándar corporativo textualmente según lo definido en la fuente anterior. No se requiere adaptación específica del proyecto.

## Notas Específicas del Proyecto

- Detalles de implementación: ver `docs/es/04-artifacts/corporate-standards-baseline.md`

## Extensión Obligatoria UMS — Estándar de Catálogos Paramétricos

Desde el **2026-05-14**, todas las entidades/tablas de parámetros, configuración y catálogos en UMS DEBEN cumplir la siguiente estructura mínima y controles de gobernanza.

### Campos Mínimos Obligatorios

- `code`: identificador técnico único (clave estable para runtime, APIs y llaves de caché)
- `value`: valor operativo/configurable consumido por el sistema
- `description`: explicación funcional clara del propósito y uso

### Semántica Obligatoria de `description`

El campo `description` DEBE documentar:

1. para qué se usa el parámetro,
2. impacto funcional en el comportamiento,
3. comportamiento esperado en runtime,
4. alcance/contexto de configuración aplicable (Global/Tenant/System/Suite/etc).

### Alcance de Aplicación Obligatorio

Este estándar aplica a:

- parámetros globales,
- parámetros por tenant,
- parámetros por system/suite,
- feature flags,
- políticas,
- configuraciones de seguridad,
- workflows,
- reglas de negocio,
- configuraciones de notificación y aprobación.

### Requisitos de Validación y Gobernanza

Toda tabla/entidad conforme DEBE definir y validar:

- constraints únicos (unicidad de `code` por alcance),
- versionado (histórico inmutable o linaje semántico),
- auditoría (quién/cuándo/qué cambió),
- trazabilidad (vinculación a ADR/FS/TE y eventos),
- cacheabilidad (llaves determinísticas e invalidación por eventos),
- extensibilidad futura (sin bloqueos de esquema para nuevos alcances/proveedores/tipos de regla).
