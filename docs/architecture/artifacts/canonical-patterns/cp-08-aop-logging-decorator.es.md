# CP-08: Decorator de Logging AOP con Envelope de Observabilidad

**Tipo:** Patrón Canónico  
**Estado:** Aceptado  
**Disposición Evolith:** Propuesto para Evolith — depende solo de `Ums.Shell.Aop.Aspects.Logger.Serilog` (shell library portable)  
**ADRs relacionados:**
- [ADR-0060: Estrategia AOP Cross-Cutting](../../adrs/0060-aop-cross-cutting-concern-strategy.md)
- [ADR-0061: Execution Context Accessor](../../adrs/0061-execution-context-accessor.md)
- [ADR-0062: Serilog Seguro de PII](../../adrs/0062-pii-safe-serilog-configuration.md)

---

## Problema

Los command handlers necesitan logging de entrada/salida/excepción enriquecido con el envelope completo de observabilidad (TenantId, CorrelationId, SessionTrackingId, TraceId, SpanId, BoundedContext) sin:
- Acoplar el handler a `ILogger` o Serilog
- Duplicar la lógica de enriquecimiento en todos los handlers
- Filtrar valores de argumentos PII en los logs

---

## Patrón

Extender `StructuredAopLoggerBase` (shell library) para crear un adaptador de logger respaldado por Serilog. Registrarlo vía una interfaz marcador como servicio DI con clave. Los handlers declaran la intención con `[LoggerAspect(Type = typeof(IUmsLogger), ...)]` — sin acoplamiento en tiempo de ejecución.

```
[LoggerAspect(Type = typeof(IUmsLogger))]  ← Capa de Aplicación (solo atributo)
         │
         ▼ (DispatchProxy intercepta)
UmsSerilogLogger : StructuredAopLoggerBase ← Capa de Infraestructura
         │
         ├── ResolveExecutionContext()      lee snapshot de RequestContextAccessor
         ├── TenantId()                     lee IUserContext (scoped)
         ├── InferBoundedContext(Type)      parsea namespace (p.ej. Identity, Authorization)
         │
         ▼
ILogger<THandler> (MEL respaldado por Serilog)
         │
         ▼
PiiSanitizerEnricher → Sinks (Console / OTel / Loki)
```

---

## Clase Base de la Shell Library

```csharp
// Ums.Shell.Aop.Aspects.Logger.Serilog
public abstract class StructuredAopLoggerBase : ILogger
{
    private readonly IExecutionContextAccessor _accessor;

    protected StructuredAopLoggerBase(IExecutionContextAccessor accessor)
        => _accessor = accessor;

    /// <summary>
    /// Resuelve el envelope completo de observabilidad para el request actual.
    /// Prioridad: snapshot de RequestContextAccessor → baggage de Activity.Current → parámetro requestId → ""
    /// </summary>
    protected ExecutionContextSnapshot ResolveExecutionContext(string requestId) { ... }

    /// <summary>
    /// Infiere el bounded context desde el namespace del tipo del handler.
    /// Ums.Application.Identity.Tenant.Commands.* → "Identity"
    /// </summary>
    protected static string InferBoundedContext(Type targetType) { ... }

    // Contrato abstracto ILogger — implementar en subclase específica del satélite
    public abstract void OnEntry(IJoinPoint jp, Argument[] args, string requestId);
    public abstract void OnExit(IJoinPoint jp, Return ret, string requestId, long duration);
    // ... otras sobrecargas
    public abstract void OnException(IJoinPoint jp, string requestId, Exception ex);
}
```

---

## Implementación del Satélite (ejemplo UMS)

```csharp
// Ums.Infrastructure/Aop/UmsSerilogLogger.cs
public sealed class UmsSerilogLogger(
    ILoggerFactory loggerFactory,
    IUserContext userContext,
    IExecutionContextAccessor accessor) : StructuredAopLoggerBase(accessor), IUmsLogger
{
    public override void OnEntry(IJoinPoint jp, Argument[] args, string requestId)
    {
        var log = loggerFactory.CreateLogger(jp.TargetType);
        if (!log.IsEnabled(LogLevel.Information)) return;

        var ctx      = ResolveExecutionContext(requestId);
        var tenant   = userContext.TenantId ?? "system";
        var bc       = InferBoundedContext(jp.TargetType);

        // Seguro de PII: solo nombres + tipos CLR, nunca valores
        var argSummary = args is { Length: > 0 }
            ? string.Join(", ", args.Select(a => $"{a.Name}:{a.Type}"))
            : string.Empty;

        log.LogInformation(
            "→ {BoundedContext} {Handler}.{Method} params=[{Params}] | "
            + "tenant={TenantId} cid={CorrelationId} sid={SessionTrackingId} "
            + "trace={TraceId} span={SpanId}",
            bc, jp.TargetType.Name, jp.MethodInfo.Name, argSummary,
            tenant, ctx.CorrelationId, ctx.SessionTrackingId, ctx.TraceId, ctx.SpanId);
    }

    public override void OnException(IJoinPoint jp, string requestId, Exception ex)
    {
        var log    = loggerFactory.CreateLogger(jp.TargetType);
        var ctx    = ResolveExecutionContext(requestId);
        var tenant = userContext.TenantId ?? "system";

        log.LogError(ex,
            "✗ {BoundedContext} {Handler}.{Method} threw {ExType} | "
            + "tenant={TenantId} cid={CorrelationId} sid={SessionTrackingId}",
            InferBoundedContext(jp.TargetType),
            jp.TargetType.Name, jp.MethodInfo.Name, ex.GetType().Name,
            tenant, ctx.CorrelationId, ctx.SessionTrackingId);
    }

    // ... las sobrecargas de OnExit siguen el mismo patrón
}
```

---

## Interfaz Marcador (capa de Aplicación)

```csharp
// Ums.Application/Common/Aop/IUmsLogger.cs
// Marcador — cero código en tiempo de ejecución; selecciona el servicio DI con clave
public interface IUmsLogger : ILogger; // ILogger = Ums.Shell.Aop.Aspects.ILogger
```

---

## Registro en DI

```csharp
// Después de AddAop() — registra PointCut, AspectExecutor, aspectos incorporados
services.AddAop();

// Registrar adaptador de logger bajo clave de interfaz marcador
services.AddKeyedTransient<Ums.Shell.Aop.Aspects.ILogger, UmsSerilogLogger>(
    typeof(IUmsLogger));

// Envolver handler con DispatchProxy — debe ir DESPUÉS de AddMediatR()
services.AddAopProxy<
    IRequestHandler<CreateTenantCommand, Result<CreateTenantResponse>>,
    CreateTenantCommandHandler>();
```

---

## Decoración del Handler

```csharp
// Capa de Aplicación — sin import de Infraestructura
[LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
public async Task<Result<CreateTenantResponse>> Handle(
    CreateTenantCommand request, CancellationToken ct)
{
    // lógica de negocio pura — sin código de logging
}
```

---

## Salida de Log

```
→ Identity CreateTenantCommandHandler.Handle params=[request:CreateTenantCommand] |
  tenant=acme cid=a3f1b7c2 sid=f9d8e1a0 trace=4bf92f35... span=00f067aa...

← Identity CreateTenantCommandHandler.Handle in 42ms |
  tenant=acme cid=a3f1b7c2 sid=f9d8e1a0

✗ Identity CreateTenantCommandHandler.Handle threw ValidationException |
  tenant=acme cid=a3f1b7c2 sid=f9d8e1a0
```

---

## Dos Adaptadores de Logger Disponibles

| Adaptador | Clave de interfaz | Nivel | Enriquecimiento | Cuándo usar |
|-----------|------------------|-------|-----------------|-------------|
| `MelLogger` | `IMelLogger` | Debug | Ninguno más allá de los scopes MEL | Dev, trazado ligero |
| `UmsSerilogLogger` | `IUmsLogger` | Information | TenantId, CorrelationId, SessionTrackingId, TraceId, SpanId, BoundedContext | Todos los command handlers de producción |

---

## Patrones Relacionados

- [CP-05: Propagación del Contexto de Ejecución](./cp-05-execution-context-propagation.es.md)
- [CP-06: Logging Estructurado Seguro de PII](./cp-06-pii-safe-structured-logging.es.md)
- [Guía Shell Libraries — AOP](../../shell-libraries/aop.md)
