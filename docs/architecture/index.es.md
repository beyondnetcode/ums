# Portal de Arquitectura

Bienvenido al **Portal de Arquitectura** del User Management System (UMS).

## Modelo de Gobernanza -- Arquitectura Satelite Evolith

UMS es un **producto satelite** del [Evolith Architecture Reference](https://github.com/beyondnetcode/evolith_arch32). Esta relacion define como se toman las decisiones arquitecturales en este repositorio.

### Como funciona la herencia

```
Evolith (padre)                         UMS (satelite)
─────────────────────────────         ──────────────────────────────────
Define politicas base          ──►    Hereda por referencia (ADR-0050)
Provee patrones canonicos      ──►    Adopta o adapta segun contexto UMS
Establece convenciones         ──►    Conforma + documenta excepciones
```

Un ADR de UMS puede hacer una de tres cosas respecto a Evolith:

| Modo | Cuando usarlo | Ejemplo |
|---|---|---|
| **Adoptar** | La politica de Evolith aplica tal cual | ADR-0050: Taxonomia de nombres adoptada literalmente |
| **Especializar** | La politica aplica pero UMS agrega restricciones | ADR-0052: Audit trail inmutable con especificaciones SQL Server |
| **Anular** | UMS se desvía de Evolith con justificacion explicita | ADR-0059: Decision de tier API unico (co-localizacion sobre tiers separados) |

### Por que esto importa en el onboarding

Cuando encuentres una decision que parece contradecir Evolith, revisa primero el ADR de UMS relevante. La desviacion puede ser intencional y justificada. Si no existe un ADR, aplica la linea base de Evolith. **Nunca asumas que el silencio es permiso para desviarse.**

### Ejemplo practico -- decision de API Tier

Evolith permite separar superficies de consulta y comando en tiers de API independientes cuando la escala o la propiedad de equipos lo justifica. UMS decidio explicitamente **no hacerlo** en la madurez actual:

- La separacion CQRS ya existe a nivel de protocolo (GraphQL para queries / REST para comandos).
- Separar tiers agrega costo operacional sin beneficio medible a escala MVP.
- El riesgo de carga multi-tenant se mitiga con limites de complejidad, timeouts y rate limiting en el gateway (TE-07).

Esta decision esta registrada en [ADR-0059](./adrs/0059-single-api-tier-decision.md) con disparadores explicitos para cuando la decision debe revisarse.

> Este es el patron esperado: **heredar la linea base, anular con evidencia, documentar el disparador para revertir.**

---

## Vision General de Arquitectura

- **[Vision General de Arquitectura](./overview.es.md)**: Vision global de arquitectura, mapa de bounded contexts, catalogo de agregados y principios de integracion cross-contexto.

---

## Activos Core de Arquitectura

Esta seccion se centra en el diseno estructural y los modelos de base de datos del sistema.

### [Blueprints](./blueprints-es/index.md)

Planos detallados de ingenieria centrados en:
- **[Diseno de Base de Datos ER](./blueprints-es/database-design-er.md)**: El modelo entidad-relacion de referencia.
- **[Formatos de Exportacion ER](./blueprints-es/er-export-formats.md)**: Exportaciones SQL, Mermaid e imagenes del esquema.
- **[Visor ER Interactivo](./blueprints/interactive-er-viewer.html)**: Herramienta basada en navegador para explorar la estructura de la base de datos.
- **[Mapa de Entidades de Servicio](./blueprints/service-entity-map.md)**: Mapeo logico entre servicios del sistema y entidades de base de datos.
- **[Arquitectura de Librerias Shell](./blueprints-es/shell-library-architecture.md)**: Capa shell propia de UMS para patrones DDD y Factory heredados.
- **[Guias de Desarrollo de Librerias Shell](./shell-libraries/README.es.md)**: Guías completas para las cuatro librerias shell con ejemplos de uso independiente y combinado.
  - [BeyondNetCode.Shell.Ddd](./shell-libraries/ddd.es.md) · [BeyondNetCode.Shell.Factory](./shell-libraries/factory.es.md) · [BeyondNetCode.Shell.Aop](./shell-libraries/aop.es.md) · [BeyondNetCode.Shell.Bootstrapper](./shell-libraries/bootstrapper.es.md) · [Uso Combinado](./shell-libraries/combined-usage.md)

### [API Dotnet](./api-dotnet/README.es.md)

Referencia aplicada backend API para UMS. Esta seccion mapea evidencia actual de codigo fuente Dotnet contra el Estandar API Dotnet de Evolith manteniendo los detalles especificos de producto dentro de UMS.

### [Web Frontend](./web-frontend/README.es.md)

Referencia aplicada React Web para UMS. Esta seccion mapea evidencia actual de codigo fuente contra el Estandar Web Frontend React de Evolith manteniendo los detalles especificos de producto dentro de UMS.

### [Matriz de Trazabilidad](./traceability-matrix.md)

Referencia cruzada de todas las Historias Funcionales (FS-01..16) hacia ADRs y Habilitadores Tecnicos.

### [Habilitadores Tecnicos](./blueprints/technical-enablers/index.md)

Planos de ingenieria que especifican como los ADRs se implementan en el contexto UMS:
- **[TE-04: Outbox Transaccional](./blueprints/technical-enablers/te-04-transactional-outbox.md)**
- **[TE-05: Saga Distribuida con Dapr](./blueprints/technical-enablers/te-05-distributed-saga-dapr.md)**
- **[TE-06: Reconstruccion de Proyecciones CQRS](./blueprints/technical-enablers/te-06-cqrs-projection-rebuild.md)**

### [Patrones Canonicos](./artifacts/canonical-patterns/index.md)

Implementaciones de referencia para patrones architectonicos core:
- **[CP-01: Arquitectura Hexagonal -- Puerto y Adaptador](./artifacts/canonical-patterns/cp-01-hexagonal-port-adapter.md)**
- **[CP-02: Aggregate Root y Dominio Evento](./artifacts/canonical-patterns/cp-02-aggregate-root-domain-event.md)**
- **[CP-03: Patrón Resultado](./artifacts/canonical-patterns/cp-03-result-pattern.md)**
- **[CP-04: Repositorio Multi-Tenant con RLS](./artifacts/canonical-patterns/cp-04-multitenant-repository-rls.md)**

### Relacionado -- Diseno de Capa de Dominio

- **[Portal DDD](../governance/construction/ddd-design/index.md)**: Bounded contexts, agregados, value objects, comandos, eventos y maquinas de estado para el producto completo.
- **[Primitivas DDD](../governance/construction/ddd-design/11-ddd-primitives.md)**: Primitivas de dominio implementadas mediante la capa de librerias shell de UMS.
- **[Viewer Interactivo DDD](../governance/construction/ddd-design/interactive-ddd-viewer.html)**: Herramienta en el navegador para explorar el mapa de contextos, maquinas de estado y flujos cross-contexto.

---

## Metricas de Ingenieria

- **[Dashboard de Metricas de Solucion](../operations/metrics/index.md)**: Metricas de ingenieria consolidadas atraves de todas las soluciones (API, Web, Librerias, Tests) organizadas por categoria: codificacion, seguridad, calidad, tests y uso de IA.

---

**[Volver al Indice Maestro](../MASTER_INDEX.es.md)** | **[Volver al README Principal](../README.es.md)**