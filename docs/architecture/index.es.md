# Portal de Arquitectura

Bienvenido al **Portal de Arquitectura** del User Management System (UMS).

## Modelo de Gobernanza — Arquitectura Satelite arc32

UMS es un **producto satelite** del [arc32 Progressive Monolith Architecture Reference](https://github.com/beyondnetcode/arc32_progresive_monolith). Esta relacion define como se toman las decisiones arquitecturales en este repositorio.

### Como funciona la herencia

```
arc32 (padre)                         UMS (satelite)
─────────────────────────────         ──────────────────────────────────
Define politicas base          ──►    Hereda por referencia (ADR-0050)
Provee patrones canonicos      ──►    Adopta o adapta segun contexto UMS
Establece convenciones         ──►    Conforma + documenta excepciones
```

Un ADR de UMS puede hacer una de tres cosas respecto a arc32:

| Modo | Cuando usarlo | Ejemplo |
|---|---|---|
| **Adoptar** | La politica de arc32 aplica tal cual | ADR-0050: Taxonomia de nombres adoptada literalmente |
| **Especializar** | La politica aplica pero UMS agrega restricciones | ADR-0052: Audit trail inmutable con especificaciones SQL Server |
| **Anular** | UMS se desvía de arc32 con justificacion explicita | ADR-0059: Decision de tier API unico (co-localizacion sobre tiers separados) |

### Por que esto importa en el onboarding

Cuando encuentres una decision que parece contradecir arc32, revisa primero el ADR de UMS relevante. La desviacion puede ser intencional y justificada. Si no existe un ADR, aplica la linea base de arc32. **Nunca asumas que el silencio es permiso para desviarse.**

### Ejemplo practico — decision de API Tier

arc32 permite separar superficies de consulta y comando en tiers de API independientes cuando la escala o la propiedad de equipos lo justifica. UMS decidio explicitamente **no hacerlo** en la madurez actual:

- La separacion CQRS ya existe a nivel de protocolo (GraphQL para queries / REST para comandos).
- Separar tiers agrega costo operacional sin beneficio medible a escala MVP.
- El riesgo de carga multi-tenant se mitiga con limites de complejidad, timeouts y rate limiting en el gateway (TE-07).

Esta decision esta registrada en [ADR-0059](./adrs/0059-single-api-tier-decision.md) con disparadores explicitos para cuando la decision debe revisarse.

> Este es el patron esperado: **heredar la linea base, anular con evidencia, documentar el disparador para revertir.**

---

## Activos Core de Arquitectura

Esta sección se centra en el diseño estructural y los modelos de base de datos del sistema.

### [Blueprints (Planos técnicos)](./blueprints-es/index.md)
Planos detallados de ingeniería centrados en:
- **[Diseño de Base de Datos ER](./blueprints-es/database-design-er.md)**: El modelo entidad-relación de referencia.
- **[Formatos de Exportación ER](./blueprints-es/er-export-formats.md)**: Exportaciones SQL, Mermaid e imágenes del esquema.
- **[Arquitectura de Librerías Shell](./blueprints-es/shell-library-architecture.md)**: Capa shell propia de UMS para patrones DDD y Factory heredados.

### Relacionado — Diseño de Capa de Dominio
- **[Portal DDD](../governance/construction/ddd-design/index.md)**: Bounded contexts, agregados, value objects, comandos, eventos y maquinas de estado para el producto completo.
- **[Primitivas DDD](../governance/construction/ddd-design/11-ddd-primitives.md)**: Primitivas de dominio implementadas mediante la capa de librerías shell de UMS.
- **[Viewer Interactivo DDD](../governance/construction/ddd-design/interactive-ddd-viewer.html)**: Herramienta en el navegador para explorar el mapa de contextos, maquinas de estado y flujos cross-contexto.

---

**[Volver al Índice Maestro](../MASTER_INDEX.es.md)** | **[Volver al README Principal](../README.es.md)**
