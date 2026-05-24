# CP-05: Execution Context Propagation

**Type:** Canonical Pattern  
**Status:** Accepted  
**Evolith disposition:** Proposed for Evolith — zero product-specific dependencies  
**Related ADR:** [ADR-0061: Execution Context Accessor](../../adrs/0061-execution-context-accessor.md)

---

## Problem

Command handlers, AOP aspects, and background services need access to request-scoped observability signals (CorrelationId, SessionTrackingId, TraceId, SpanId) without coupling to `IHttpContextAccessor` or `Activity.Current`.

---

## Pattern

A scoped `RequestContextAccessor` is written once by middleware and read by any component in the same request scope through a read-only `IRequestContext` port (Application) or a writable `IExecutionContextAccessor` port (Infrastructure/AOP).

```
HTTP Request
     │
     ▼
CorrelationIdMiddleware          writes Activity baggage + ILogger scope
     │
     ▼
SessionTrackingMiddleware        writes Activity baggage + calls RequestContextAccessor.Set()
     │
     ▼
RequestContextAccessor (scoped)  ← single source of truth for the request scope
     │
     ├── IRequestContext         read-only (Application layer)
     └── IExecutionContextAccessor  read + write (Infrastructure / AOP)
```

---

## Types

### Shell library (`Ums.Shell.Aop.Aspects.Logger.Serilog`)

```csharp
// Immutable snapshot — written once per request
public sealed record ExecutionContextSnapshot(
    string CorrelationId,
    string SessionTrackingId,
    string TraceId,
    string SpanId)
{
    public static readonly ExecutionContextSnapshot Empty = new("", "", "", "");
}

// Writable port (Infrastructure / middleware only)
public interface IExecutionContextAccessor
{
    ExecutionContextSnapshot Current { get; }
    void Set(ExecutionContextSnapshot snapshot);
}

// HTTP header name constants
public static class ObservabilityHeaders
{
    public const string CorrelationId     = "X-Correlation-Id";
    public const string SessionTrackingId = "X-Session-Tracking-Id";
}

// OTel baggage / tag key constants
public static class ObservabilityKeys
{
    public const string CorrelationId     = "correlation.id";
    public const string SessionTrackingId = "session.tracking_id";
}
```

### Application layer

```csharp
// Read-only port — no HTTP dependency
public interface IRequestContext
{
    string? CorrelationId     { get; }
    string? SessionTrackingId { get; }
    string? TraceId           { get; }
    string? SpanId            { get; }
}
```

### Infrastructure layer

```csharp
// Single class implements both ports — registered as both in DI
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

### DI registration

```csharp
services.AddScoped<RequestContextAccessor>();
services.AddScoped<IRequestContext>(sp =>
    sp.GetRequiredService<RequestContextAccessor>());
services.AddScoped<IExecutionContextAccessor>(sp =>
    sp.GetRequiredService<RequestContextAccessor>());
```

### Middleware writer (SessionTrackingMiddleware)

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

### Consumer — AOP logger (StructuredAopLoggerBase)

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

## Layer Reference Rules

| Layer | Interface | Operation |
|-------|-----------|-----------|
| `Domain` | — | No context needed |
| `Application` | `IRequestContext` | Read-only |
| `Infrastructure` / AOP | `IExecutionContextAccessor` | Read + fallback to Activity |
| `Presentation` / Middleware | `RequestContextAccessor` directly | Write (middleware), Read (endpoints) |

---

## Background Service Handoff

When a command spawns a background job or hands off to an outbox dispatcher, pass the snapshot explicitly:

```csharp
// In handler — capture before handing off
var snapshot = _requestContext.ToSnapshot(); // or just read fields

// In background service constructor / factory
public OutboxDispatcherJob(ExecutionContextSnapshot originatingContext) { ... }
```

---

## Related Patterns

- [CP-08: AOP Logging Decorator](./cp-08-aop-logging-decorator.md)
- [ADR-0061](../../adrs/0061-execution-context-accessor.md)
- [ADR-0053 OTel](../../adrs/0053-opentelemetry-observability.md)
