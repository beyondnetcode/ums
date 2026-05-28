# Referencia Aplicada API Dotnet UMS

> Idioma: [English](./ums-api-dotnet-applied-reference.md) | [Espanol](./ums-api-dotnet-applied-reference.es.md)

## 1. Proposito

Este documento mapea la implementacion API Dotnet de UMS contra el Estandar API Dotnet de Evolith. Es una referencia aplicada de producto, no un estandar universal.

Las reglas backend reutilizables pertenecen a Evolith. Los detalles especificos de UMS permanecen aqui salvo que se promuevan mediante un ADR de Evolith, estandar de gobierno o patron canonico.

## 2. Alcance de fuente

La referencia aplicada cubre:

```text
src/apps/ums.api
```

Perfil observado:

| Aspecto | Implementacion UMS |
|---|---|
| Host | ASP.NET Core minimal host |
| Composicion | Bootstrappers de Presentation |
| Frontera de aplicacion | MediatR y FluentValidation |
| Superficie API | Comandos REST y consultas GraphQL |
| Versionado | Versionado API por segmento URL |
| Persistencia | EF Core con baseline SQL Server y soporte SQLite local |
| Operaciones | Logs estructurados, registro de telemetria, health checks, rate limits, background workers |
| Politicas transversales | Aspectos para auditoria, transacciones, validacion de tenant y logging |

## 3. Mapeo

| Topico Evolith | Evidencia fuente UMS | Clasificacion |
|---|---|---|
| Bootstrap de host | `Ums.Presentation/Program.cs` | Evidencia aplicada |
| Composicion modular de servicios | `Ums.Presentation/Bootstrapping/UmsApiServiceBootstrappers.cs` | Patron reutilizable |
| Frontera de aplicacion | `Ums.Application/DependencyInjection.cs` | Patron reutilizable |
| Pipeline middleware | `UseUmsApiPipeline` | Evidencia aplicada |
| Mapeo de superficie API | `MapUmsApiSurface` y route groups por bounded context | Evidencia aplicada; modulos concretos locales |
| Superficie GraphQL de consultas | `MapGraphQlSurface` | Candidato a estandar de gobierno Evolith |
| Modelo de health | `MapHealthSurface` y health checks de infraestructura | Patron operacional reutilizable |
| Setup de provider de persistencia | `Ums.Infrastructure/DependencyInjection.cs` | Patron reutilizable; matriz de providers local |
| Perfil EF Core SQL Server | Setup DbContext, retries, interceptors y tabla de historial de migraciones | Candidato a perfil de persistencia Evolith |
| Background workers | Hosted services de persistencia de auditoria y outbox dispatcher | Patron de confiabilidad reutilizable |
| Aspectos transversales | Setup AOP y registro de proxies MediatR | Candidato a patron canonico |

## 4. Items que deben permanecer locales en UMS

| Item | Razon |
|---|---|
| Bounded contexts y modulos de agregados | Modelo de dominio del producto. |
| Grupos de endpoints y nombres de rutas concretas | Superficie API de UMS. |
| Metadata de documentacion del producto | Identidad del producto UMS. |
| Nombres concretos de headers y claims | Contrato API local. |
| Helpers solo de desarrollo | Soporte de desarrollo local. |
| Valores runtime de limites y retries | Politica operacional del producto. |
| Nombres de schemas y clases de repositorio | Implementacion de persistencia UMS. |
| Clases de estrategia especificas del producto | Implementacion de capacidades UMS. |
| Seed data de desarrollo | Comportamiento de entorno local. |

## 5. Items a promover a Evolith

| Candidato | Destino de promocion |
|---|---|
| Minimal host mas composicion con bootstrappers nombrados | Estandar de bootstrap API Evolith |
| Gobierno de comandos REST y consultas GraphQL | Estandar de superficie API Evolith o ADR |
| MediatR mas pipeline de validacion | Perfil de capa de aplicacion Evolith |
| Problem Details y contexto de error seguro para usuario | Estandar de errores API Evolith |
| Perfil EF Core SQL Server | Estandar de persistencia Evolith |
| Abstracciones de request context y execution context | Estandar de tenancy y observabilidad Evolith |
| Modelo de liveness, readiness y backlog health | Estandar operacional Evolith |
| Outbox y procesamiento background de auditoria | Estandar de confiabilidad Evolith |
| Politicas transversales basadas en aspectos o decoradores | Patron canonico Evolith o ADR |

## 6. Items que requieren ADR o promocion formal

| Item | Accion requerida |
|---|---|
| Dotnet 10 como baseline runtime backend | ADR Evolith o actualizacion de stack autoritativo |
| Comandos REST y consultas GraphQL en un solo API tier | ADR de gobierno API Evolith |
| MediatR como frontera de aplicacion por defecto | ADR Evolith o perfil backend opcional |
| FluentValidation como estandar de pipeline de validacion | Estandar de ingenieria Evolith |
| EF Core mas SQL Server como perfil de persistencia por defecto | ADR de persistencia Evolith o estandar de stack |
| Libreria shell AOP como mecanismo transversal | Patron canonico Evolith y ADR |
| Perfil de retry y circuit breaker para persistencia | Estandar de resiliencia Evolith |
| Requisito de idempotencia para comandos | Estandar de confiabilidad API Evolith |

## 7. Checklist de validacion

Antes de cambiar la arquitectura API de UMS, validar:

- Clasificar el cambio como estandar Evolith, decision local UMS o candidato de promocion.
- Mantener patrones backend compartidos independientes del lenguaje de producto UMS.
- Mantener rutas, headers, seed data, schemas y clases de estrategia de producto en UMS.
- Proponer primero en Evolith las reglas reutilizables.
- Actualizar documentacion en ingles y espanol en conjunto.
- Mantener Markdown UTF-8 limpio y sin iconos decorativos.
- No modificar workflows ni hooks sin autorizacion explicita.

---
[Volver al Portal API Dotnet](./README.es.md)
