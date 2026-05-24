# CP-05: Propagación del Contexto de Ejecución

**Tipo:** Patrón Canónico  
**Estado:** Aceptado  
**Disposición Evolith:** Propuesto para Evolith — sin dependencias específicas del producto  
**ADR relacionado:** [ADR-0061: Execution Context Accessor](../../adrs/0061-execution-context-accessor.md)

---

## Problema

Los command handlers, aspectos AOP y servicios de background necesitan acceso a las señales de observabilidad con scope de request (CorrelationId, SessionTrackingId, TraceId, SpanId) sin acoplarse a `IHttpContextAccessor` ni a `Activity.Current`.

---

## Patrón

Un `RequestContextAccessor` con scope es escrito una vez por el middleware y leído por cualquier componente en el mismo scope de request a través de un puerto de solo lectura `IRequestContext` (Aplicación) o un puerto de escritura `IExecutionContextAccessor` (Infraestructura/AOP).

```
HTTP Request
     │
     ▼
CorrelationIdMiddleware          escribe baggage Activity + scope ILogger
     │
     ▼
SessionTrackingMiddleware        escribe baggage Activity + llama RequestContextAccessor.Set()
     │
     ▼
RequestContextAccessor (scoped)  ← fuente única de verdad para el scope del request
     │
     ├── IRequestContext         solo lectura (capa de Aplicación)
     └── IExecutionContextAccessor  lectura + escritura (Infraestructura / AOP)
```

---

## Tipos

### Shell library (`Ums.Shell.Aop.Aspects.Logger.Serilog`)

```csharp
// Snapshot inmutable — escrito una vez por request
public sealed record ExecutionContextSnapshot(
    string CorrelationId,
    string SessionTrackingId,
    string TraceId,
    string SpanId)
{
    public static readonly ExecutionContextSnapshot Empty = new("", "", "", "");
}

// Puerto de escritura (solo Infraestructura / middleware)
public interface IExecutionContextAccessor
{
    ExecutionContextSnapshot Current { get; }
    void Set(ExecutionContextSnapshot snapshot);
}

// Constantes de nombres de headers HTTP
public static class ObservabilityHeaders
{
    public const string CorrelationId     = "X-Correlation-Id";
    public const string SessionTrackingId = "X-Session-Tracking-Id";
}

// Constantes de claves baggage / tag OTel
public static class ObservabilityKeys
{
    public const string CorrelationId     = "correlation.id";
    public const string SessionTrackingId = "session.tracking_id";
}
```

### Capa de Aplicación

```csharp
// Puerto de solo lectura — sin dependencia HTTP
public interface IRequestContext
{
    string? CorrelationId     { get; }
    string? SessionTrackingId { get; }
    string? TraceId           { get; }
    string? SpanId            { get; }
}
```

### Capa de Infraestructura

```csharp
// Una clase implementa ambos puertos — registrada como ambos en DI
public sealed class RequestContextAccessor : IRequestContext, IExecutionContextAccessor
{
    private ExecutionContextSnapshot _current = ExecutionContextSnapshot.Empty;

    public string? CorrelationId     => _current.CorrelationId.NullIfEmpty();
    public string? SessionTrackingId => _current.SessionTrackingId.NullIfEmpty();
    public string? TraceId           => _current.TraceId.NullIfEmpty();
    public string? SpanId            => _current.SpanId.NullIfEmpty();
    public ExecutionContextSnapshot Current => _current;

    public void Set(ExecutionContextSnapshot snapshot) =>
        _current = snapshot ?? ExecutionContextSnapshot.Empty;
}
```

### Registro en DI

```csharp
services.AddScoped<RequestContextAccessor>();
services.AddScoped<IRequestContext>(sp =>
    sp.GetRequiredService<RequestContextAccessor>());
services.AddScoped<IExecutionContextAccessor>(sp =>
    sp.GetRequiredService<RequestContextAccessor>());
```

### Escritor en Middleware (SessionTrackingMiddleware)

```csharp
public async Task InvokeAsync(HttpContext context, RequestContextAccessor accessor)
{
    var sessionTrackingId = GetOrGenerate(context, ObservabilityHeaders.SessionTrackingId);

    Activity.Current?.SetBaggage(ObservabilityKeys.SessionTrackingId, sessionTrackingId);
    Activity.Current?.SetTag(ObservabilityKeys.SessionTrackingId, sessionTrackingId);

    accessor.Set(new ExecutionContextSnapshot(
        CorrelationId:     Activity.Current?.GetBaggageItem(ObservabilityKeys.CorrelationId)
                           ?? context.TraceIdentifier ?? string.Empty,
        SessionTrackingId: sessionTrackingId,
        TraceId:           Activity.Current?.TraceId.ToString() ?? string.Empty,
        SpanId:            Activity.Current?.SpanId.ToString() ?? string.Empty));

    context.Response.Headers[ObservabilityHeaders.SessionTrackingId] = sessionTrackingId;
    using (_logger.BeginScope(new Dictionary<string, object> { ["SessionTrackingId"] = sessionTrackingId }))
        await _next(context);
}
```

### Consumidor — Logger AOP (StructuredAopLoggerBase)

```csharp
protected ExecutionContextSnapshot ResolveExecutionContext(string requestId)
{
    var current  = _accessor.Current ?? ExecutionContextSnapshot.Empty;
    var activity = Activity.Current;

    return new ExecutionContextSnapshot(
        CorrelationId:     current.CorrelationId.FirstNonEmpty(
                               activity?.GetBaggageItem(ObservabilityKeys.CorrelationId),
                               requestId),
        SessionTrackingId: current.SessionTrackingId.FirstNonEmpty(
                               activity?.GetBaggageItem(ObservabilityKeys.SessionTrackingId)),
        TraceId:           current.TraceId.FirstNonEmpty(activity?.TraceId.ToString()),
        SpanId:            current.SpanId.FirstNonEmpty(activity?.SpanId.ToString()));
}
```

---

## Reglas de Referencia por Capa

| Capa | Interfaz | Operación |
|------|-----------|-----------|
| `Domain` | — | No necesita contexto |
| `Application` | `IRequestContext` | Solo lectura |
| `Infrastructure` / AOP | `IExecutionContextAccessor` | Lectura + fallback a Activity |
| `Presentation` / Middleware | `RequestContextAccessor` directamente | Escritura (middleware), Lectura (endpoints) |

---

## Handoff a Servicio de Background

Cuando un comando lanza un job de background o hace handoff a un dispatcher de outbox, pasar el snapshot explícitamente:

```csharp
// En el handler — capturar antes del handoff
var snapshot = _requestContext.ToSnapshot(); // o leer campos directamente

// En el constructor del servicio de background / factory
public OutboxDispatcherJob(ExecutionContextSnapshot originatingContext) { ... }
```

---

## Patrones Relacionados

- [CP-08: Decorator de Logging AOP](./cp-08-aop-logging-decorator.es.md)
- [ADR-0061](../../adrs/0061-execution-context-accessor.md)
- [ADR-0053 OTel](../../adrs/0053-opentelemetry-observability.md)
