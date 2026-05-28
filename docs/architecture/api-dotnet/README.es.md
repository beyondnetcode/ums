# Referencia Aplicada API Dotnet de UMS

> Idioma: [English](./README.md) | [Espanol](./README.es.md)

Esta seccion documenta como la implementacion API Dotnet de UMS aplica el Estandar API Dotnet de Evolith.

UMS es una referencia aplicada, no la fuente de estandares backend universales. Las reglas backend reutilizables pertenecen a Evolith. UMS conserva modulos concretos de producto, endpoints, headers, decisiones de persistencia, bounded contexts, seeds y decisiones locales de implementacion.

## Documentos

| Documento | Proposito |
|---|---|
| [Referencia Aplicada API Dotnet UMS](./ums-api-dotnet-applied-reference.es.md) | Mapeo basado en evidencia entre topicos API de Evolith y archivos fuente de UMS. |

## Limite de autoridad

| Aspecto | Evolith | UMS |
|---|---|---|
| Estandares API reutilizables | Posee principios, reglas boilerplate, quality gates y criterios de promocion | Consume estandares |
| Implementacion de producto | Referencia ejemplos solamente | Posee codigo fuente concreto y decisiones locales |
| Persistencia | Posee gobierno de providers y reglas de calidad de datos | Posee DbContext, repositorios, interceptores, migraciones y switches de provider concretos |
| Superficie API | Posee reglas de responsabilidad REST y GraphQL | Posee endpoints, route groups, schemas y modulos de bounded context concretos |
| Operaciones | Posee capacidades requeridas | Posee Serilog, OpenTelemetry, health checks, rate limits y valores runtime concretos |

## Alcance de evidencia actual

Esta referencia se basa en la API Dotnet actual ubicada en:

```text
src/apps/ums.api
```

Las fuentes observadas incluyen bootstrap de host, service bootstrappers, pipeline middleware, mapeo de superficie API, DI de aplicacion, DI de infraestructura, configuracion de providers de persistencia, health checks, AOP, servicios background y controles operacionales.

---
[Volver al Portal de Arquitectura](../index.es.md)
