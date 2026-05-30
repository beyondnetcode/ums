# ADR-0061: Patrón Execution Context Accessor

**Estado:** Aceptado  
**Fecha:** 2026-05-24  
**Responsable:** Arquitectura  
**Disposición Evolith:** Propuesto para adopción en Evolith — sin dependencias específicas de UMS; aplicable a cualquier satélite .NET  
**Relacionados:**
- [ADR-0053: Estrategia de Observabilidad con OpenTelemetry](./0053-opentelemetry-observability.md)
- [ADR-0060: Estrategia AOP Cross-Cutting](./0060-aop-cross-cutting-concern-strategy.md)
- [CP-05: Propagación del Contexto de Ejecución](../artifacts/canonical-patterns/cp-05-execution-context-propagation.es.md)

---

## Contexto

Los command handlers, aspectos AOP, servicios de background y dispatchers de outbox necesitan acceso a las mismas señales de observabilidad con scope de request: **CorrelationId**, **SessionTrackingId**, **TraceId** y **SpanId**.

Antes de este ADR, cada componente las resolvía de forma independiente:
- `MelLogger` leía directamente desde `Activity.Current`
- `CorrelationIdMiddleware` escribía en `HttpContext.TraceIdentifier`
- `SerilogLogger` usaba el estático `Log.ForContext()` sin estado de request

Esto generaba tres problemas:

1. **Inconsistencia** — distintos componentes usaban distintas estrategias de resolución; logs del mismo request podían llevar distintos valores de CorrelationId según la ruta de código ejecutada.
2. **Acoplamiento HTTP** — cualquier código que necesitara CorrelationId debía depender de `IHttpContextAccessor`, filtrando infraestructura de Presentación hacia las capas de Aplicación o Infraestructura.
3. **Punto ciego en servicios de background** — `Activity.Current` es null en dispatchers de outbox y servicios de background; no había forma de propagar el contexto del request originador.

### Alternativas consideradas

| Opción | Problema |
|--------|---------|
| `IHttpContextAccessor` en todas partes | Acopla Aplicación/Infraestructura a HTTP |
| `AsyncLocal<T>` flow | Se rompe en límites de `Task.Run` y `ConfigureAwait(false)` |
| Solo `Activity.Current` estático | Null en servicios de background; sin SessionTrackingId |
| **`RequestContextAccessor` con scope** | Escribible por middleware, legible por cualquier servicio con scope — sin dependencia HTTP |

---

## Decisión

**Introducir un `RequestContextAccessor` con scope que el middleware escribe una vez y cualquier componente del scope del request puede leer a través de un puerto de solo lectura `IRequestContext` (Aplicación) o un puerto de escritura `IExecutionContextAccessor` (Infraestructura/AOP), sin acoplarse a HTTP.**

### Tipos

```
BeyondNetCode.Shell.Logger.Serilog   (shell library — genérica, sin dependencia de UMS)
├── ExecutionContextSnapshot            record(CorrelationId, SessionTrackingId, TraceId, SpanId)
├── IExecutionContextAccessor           interface { Current; Set(snapshot) }
├── ObservabilityHeaders                clase estática — constantes de nombres de headers HTTP
│     CorrelationId     = "X-Correlation-Id"
│     SessionTrackingId = "X-Session-Tracking-Id"
└── ObservabilityKeys                   clase estática — constantes de claves baggage/tag OTel
      CorrelationId     = "correlation.id"
      SessionTrackingId = "session.tracking_id"

Ums.Application.Common.Interfaces
└── IRequestContext                     puerto de solo lectura para la capa de Aplicación
      SessionTrackingId, CorrelationId, TraceId, SpanId (todos string?)

Ums.Infrastructure.Services
└── RequestContextAccessor              implementa IRequestContext + IExecutionContextAccessor
      registrado como IRequestContext (solo lectura) e IExecutionContextAccessor (escritura)
```

### Cadena de propagación

```
[Llega HTTP Request]
      │
      ▼
CorrelationIdMiddleware
  – lee / genera header X-Correlation-Id
  – escribe en baggage Activity.Current ("correlation.id")
  – escribe en scope ILogger ("CorrelationId")
      │
      ▼
SessionTrackingMiddleware
  – lee / genera header X-Session-Tracking-Id
  – escribe en baggage Activity.Current ("session.tracking_id")
  – llama RequestContextAccessor.Set(new ExecutionContextSnapshot(...))
  – escribe en scope ILogger ("SessionTrackingId")
      │
      ▼
RequestContextAccessor (scoped)
  – mantiene el snapshot durante el resto del request
  – legible por aspectos AOP, handlers, handoffs a background
      │
      ▼
UmsSerilogLogger / StructuredAopLoggerBase
  – llama ResolveExecutionContext() → lee RequestContextAccessor.Current
  – cae a Activity.Current si el snapshot está vacío (ruta de servicio background)
```

### Prioridad de resolución en `StructuredAopLoggerBase.ResolveExecutionContext()`

```
1. RequestContextAccessor.Current (establecido por SessionTrackingMiddleware)
2. Baggage de Activity.Current (fallback para contextos no-HTTP)
3. Parámetro requestId del atributo [LoggerAspect]
4. Cadena vacía
```

### Registro en DI

```csharp
// Ums.Infrastructure/DependencyInjection.cs
services.AddScoped<RequestContextAccessor>();
services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<RequestContextAccessor>());
services.AddScoped<IExecutionContextAccessor>(sp => sp.GetRequiredService<RequestContextAccessor>());
```

### Reglas por capa

| Capa | Puede usar | NO puede usar |
|------|---------|-------------|
| `Ums.Domain` | — (no necesita contexto) | `IRequestContext`, `IExecutionContextAccessor` |
| `Ums.Application` | `IRequestContext` (puerto solo lectura) | `IExecutionContextAccessor`, `RequestContextAccessor` |
| `Ums.Infrastructure` | `IExecutionContextAccessor` (adaptadores AOP) | `RequestContextAccessor` directo (inyectar por interfaz) |
| `Ums.Presentation` | Ambas interfaces vía DI; `RequestContextAccessor` en middleware | — |

---

## Consecuencias

### Positivas

- Fuente única de verdad para todas las señales de observabilidad — un snapshot por scope de request
- La capa de Aplicación tiene cero dependencia HTTP para el contexto de correlación
- Los servicios de background y dispatchers de outbox pueden propagar contexto recibiendo un `ExecutionContextSnapshot` en el momento del handoff
- `StructuredAopLoggerBase` en la shell library usa este patrón sin ningún import específico de UMS
- Las constantes `ObservabilityHeaders` y `ObservabilityKeys` evitan la proliferación de literales de cadena en los middlewares

### Compromisos

- `RequestContextAccessor` es escribible por cualquier código con `IExecutionContextAccessor` — el contrato es por convención, no forzado. El middleware debe ser el único escritor.
- El snapshot se captura una vez por request en la posición de `SessionTrackingMiddleware`; los spans que se inician después en el pipeline tienen un `SpanId` obsoleto en el snapshot. Los aspectos AOP compensan leyendo `Activity.Current.SpanId` como fallback.

---

## Checklist de Extracción Evolith

Los siguientes tipos están en `BeyondNetCode.Shell.Logger.Serilog` sin ningún import específico de UMS:
- [ ] `ExecutionContextSnapshot` — record genérico, sin referencias de producto
- [ ] `IExecutionContextAccessor` — interfaz genérica
- [ ] `ObservabilityHeaders` — constantes, renombrar prefijo según corresponda
- [ ] `ObservabilityKeys` — constantes, renombrar prefijo según corresponda
- [ ] `StructuredAopLoggerBase` — depende solo de `IExecutionContextAccessor` e `IJoinPoint`

`IRequestContext` y `RequestContextAccessor` tienen namespace de UMS pero son trivialmente portables a cualquier satélite.

---

**[Registro ADR](./index.md)** | **[CP-05 Contexto de Ejecución](../artifacts/canonical-patterns/cp-05-execution-context-propagation.es.md)** | **[ADR-0053 OTel](./0053-opentelemetry-observability.md)**
