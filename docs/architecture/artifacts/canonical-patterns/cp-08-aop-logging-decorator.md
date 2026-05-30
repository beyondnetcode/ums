# CP-08: AOP Logging Decorator with Observability Envelope

**Type:** Canonical Pattern  
**Status:** Accepted  
**Evolith disposition:** Proposed for Evolith — depends only on `BeyondNetCode.Shell.Logger.Serilog` (portable shell library)  
**Related ADRs:**
- [ADR-0060: AOP Cross-Cutting Concern Strategy](../../adrs/0060-aop-cross-cutting-concern-strategy.md)
- [ADR-0061: Execution Context Accessor](../../adrs/0061-execution-context-accessor.md)
- [ADR-0062: PII-Safe Serilog](../../adrs/0062-pii-safe-serilog-configuration.md)

---

## Problem

Command handlers need entry/exit/exception logging enriched with the full observability envelope (TenantId, CorrelationId, SessionTrackingId, TraceId, SpanId, BoundedContext) without:
- Coupling the handler to `ILogger` or Serilog
- Duplicating the enrichment logic across handlers
- Leaking PII argument values into logs

---

## Pattern

Extend `StructuredAopLoggerBase` (shell library) to create a Serilog-backed logger adapter. Register it via a marker interface as a keyed DI service. Handlers declare intent with `[LoggerAspect(Type = typeof(IUmsLogger), ...)]` — no runtime coupling.

```
[LoggerAspect(Type = typeof(IUmsLogger))]  ← Application layer (attribute only)
         │
         ▼ (DispatchProxy intercepts)
UmsSerilogLogger : StructuredAopLoggerBase ← Infrastructure layer
         │
         ├── ResolveExecutionContext()      reads RequestContextAccessor snapshot
         ├── TenantId()                     reads IUserContext (scoped)
         ├── InferBoundedContext(Type)      parses namespace  (e.g. Identity, Authorization)
         │
         ▼
ILogger<THandler> (MEL backed by Serilog)
         │
         ▼
PiiSanitizerEnricher → Sinks (Console / OTel / Loki)
```

---

## Shell Library Base Class

```csharp
// BeyondNetCode.Shell.Logger.Serilog
public abstract class StructuredAopLoggerBase : ILogger
{
    private readonly IExecutionContextAccessor _accessor;

    protected StructuredAopLoggerBase(IExecutionContextAccessor accessor)
        => _accessor = accessor;

    /// <summary>
    /// Resolves the full observability envelope for the current request.
    /// Priority: RequestContextAccessor snapshot → Activity.Current baggage → requestId param → ""
    /// </summary>
    protected ExecutionContextSnapshot ResolveExecutionContext(string requestId) { ... }

    /// <summary>
    /// Infers the bounded context from the handler type's namespace.
    /// Ums.Application.Identity.Tenant.Commands.* → "Identity"
    /// </summary>
    protected static string InferBoundedContext(Type targetType) { ... }

    // Abstract ILogger contract — implement in satellite-specific subclass
    public abstract void OnEntry(IJoinPoint jp, Argument[] args, string requestId);
    public abstract void OnExit(IJoinPoint jp, Return ret, string requestId, long duration);
    // ... other overloads
    public abstract void OnException(IJoinPoint jp, string requestId, Exception ex);
}
```

---

## Satellite Implementation (UMS example)

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

        // PII-safe: only names + CLR types, never values
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

    // ... OnExit overloads follow the same pattern
}
```

---

## Marker Interface (Application layer)

```csharp
// Ums.Application/Common/Aop/IUmsLogger.cs
// Marker — zero runtime code; selects the keyed DI service
public interface IUmsLogger : ILogger; // ILogger = BeyondNetCode.Shell.Aspects.ILogger
```

---

## DI Registration

```csharp
// After AddAop() — registers PointCut, AspectExecutor, built-in aspects
services.AddAop();

// Register logger adapter under marker interface key
services.AddKeyedTransient<BeyondNetCode.Shell.Aspects.ILogger, UmsSerilogLogger>(
    typeof(IUmsLogger));

// Wrap handler with DispatchProxy — must be AFTER AddMediatR()
services.AddAopProxy<
    IRequestHandler<CreateTenantCommand, Result<CreateTenantResponse>>,
    CreateTenantCommandHandler>();
```

---

## Handler Decoration

```csharp
// Application layer — no Infrastructure import
[LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
public async Task<Result<CreateTenantResponse>> Handle(
    CreateTenantCommand request, CancellationToken ct)
{
    // pure business logic — no logging code
}
```

---

## Log Output

```
→ Identity CreateTenantCommandHandler.Handle params=[request:CreateTenantCommand] |
  tenant=acme cid=a3f1b7c2 sid=f9d8e1a0 trace=4bf92f35... span=00f067aa...

← Identity CreateTenantCommandHandler.Handle in 42ms |
  tenant=acme cid=a3f1b7c2 sid=f9d8e1a0

✗ Identity CreateTenantCommandHandler.Handle threw ValidationException |
  tenant=acme cid=a3f1b7c2 sid=f9d8e1a0
```

---

## Two Logger Adapters Available

| Adapter | Interface key | Level | Enrichment | When to use |
|---------|--------------|-------|------------|-------------|
| `MelLogger` | `IMelLogger` | Debug | None beyond MEL scopes | Dev-time, lightweight tracing |
| `UmsSerilogLogger` | `IUmsLogger` | Information | TenantId, CorrelationId, SessionTrackingId, TraceId, SpanId, BoundedContext | All production command handlers |

---

## Related Patterns

- [CP-05: Execution Context Propagation](./cp-05-execution-context-propagation.md)
- [CP-06: PII-Safe Structured Logging](./cp-06-pii-safe-structured-logging.md)
- [Shell Libraries — AOP Guide](../../shell-libraries/aop.md)
