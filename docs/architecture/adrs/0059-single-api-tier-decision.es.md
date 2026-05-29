# ADR-0059 — Decisión de Nivel Único de API

| Campo | Valor |
|---|---|
| **ID** | ADR-0059 |
| **Estado** | ACEPTADO |
| **Fecha** | 2026-05-15 |
| **Decisores** | Equipo de Arquitectura |
| **Relación Evolith** | Override — UMS diverge del baseline de API multi-nivel de Evolith |

---

## Contexto

Evolith permite dividir las superficies de query y command en niveles de API separados cuando la escala o la propiedad del equipo lo justifica (ej., un query-tier GraphQL dedicado y un command-tier REST separado desplegados independientemente). En la madurez actual de UMS esta división fue evaluada y explícitamente rechazada.

## Decisión

UMS co-localizará las superficies GraphQL (query) y REST (command) en una **única unidad de despliegue `ums.api`** usando HotChocolate Minimal APIs en .NET 8.

La separación CQRS se mantiene a nivel de **protocolo** (GraphQL para lecturas, REST para escrituras) pero no a nivel de despliegue/infraestructura.

## Justificación

| Factor | Análisis |
|---|---|
| **Escala** | La carga en MVP y producción temprana no justifica el overhead operacional de dos superficies de API desplegadas independientemente |
| **CQRS ya enforced** | CQRS a nivel de protocolo (GraphQL vs REST) proporciona el aislamiento de lectura/escritura que Evolith intenta, sin costo de infraestructura |
| **Tamaño de equipo** | Un nivel dividido requiere pipelines CI/CD separados, health checks separados, y políticas de scaling separadas — no costo-efectivo con la dotación actual |
| **Riesgo multi-tenant** | El riesgo de query compleja se mitiga por gateway rate limiting (TE-07), políticas de timeout, y límites de complejidad en el schema GraphQL — no por aislamiento de niveles |

## Consecuencias

- Todos los endpoints de API (GraphQL + REST) son servidos desde un único proceso y contenedor.
- YARP gateway (ADR-0058) rutea tráfico a este nivel único.
- El scaling horizontal aplica a todo el nivel, no a lectura vs. escritura separadamente.

## Trigger para Revisar

Esta decisión debe revisarse si **cualquiera** de las siguientes ocurre:

1. La latencia de queries de lectura excede SLA bajo carga multi-tenant de producción y el profiling confirma contención en el nivel de API (no contención de DB).
2. La propiedad del equipo crece hasta el punto donde equipos separados poseen las superficies de query vs. command.
3. Un tenant específico con complejidad GraphQL hambunea los commands REST bajo load testing a escala.

---

**[Índice ADR](./index.md)** | **[Portal de Arquitectura](../index.md)**