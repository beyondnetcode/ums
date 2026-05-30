# ADR-0061: Execution Context Accessor Pattern

**Status:** Accepted  
**Date:** 2026-05-24  
**Decision Owner:** Architecture  
**Evolith disposition:** Proposed for Evolith adoption — zero UMS-specific dependencies; applicable to any .NET satellite  
**Related:**
- [ADR-0053: OpenTelemetry Observability](./0053-opentelemetry-observability.md)
- [ADR-0060: AOP Cross-Cutting Concern Strategy](./0060-aop-cross-cutting-concern-strategy.md)
- [CP-05: Execution Context Propagation](../artifacts/canonical-patterns/cp-05-execution-context-propagation.md)

---

## Context

Command handlers, AOP aspects, background services, and outbox dispatchers all need access to the same request-scoped observability signals: **CorrelationId**, **SessionTrackingId**, **TraceId**, and **SpanId**.

Before this ADR, each component resolved these independently:
- `MelLogger` read from `Activity.Current` directly
- `CorrelationIdMiddleware` wrote to `HttpContext.TraceIdentifier`
- `SerilogLogger` used the static `Log.ForContext()` without request state

This created three problems:

1. **Inconsistency** — different components used different resolution strategies; logs from the same request could carry different CorrelationId values depending on which code path ran.
2. **HTTP coupling** — any code needing CorrelationId had to depend on `IHttpContextAccessor`, leaking Presentation infrastructure into Application or Infrastructure layers.
3. **Background-service blind spot** — `Activity.Current` is null in outbox dispatchers and background services; there was no way to carry forward the originating request context.

### Alternatives considered

| Option | Problem |
|--------|---------|
| `IHttpContextAccessor` everywhere | Couples Application/Infrastructure to HTTP |
| `AsyncLocal<T>` flow | Breaks across `Task.Run` boundaries and `ConfigureAwait(false)` |
| Static `Activity.Current` only | Null in background services; no SessionTrackingId |
| **Scoped `RequestContextAccessor`** | Writable by middleware, readable by any scoped service — no HTTP dependency |

---

## Decision

**Introduce a scoped `RequestContextAccessor` that is written by middleware and read by any component in the request scope without coupling to HTTP.**

### Types

```
BeyondNetCode.Shell.Logger.Serilog   (shell library — generic, no UMS dependency)
├── ExecutionContextSnapshot            record(CorrelationId, SessionTrackingId, TraceId, SpanId)
├── IExecutionContextAccessor           interface { Current; Set(snapshot) }
├── ObservabilityHeaders                static class — HTTP header name constants
│     CorrelationId     = "X-Correlation-Id"
│     SessionTrackingId = "X-Session-Tracking-Id"
└── ObservabilityKeys                   static class — OTel baggage/tag key constants
      CorrelationId     = "correlation.id"
      SessionTrackingId = "session.tracking_id"

Ums.Application.Common.Interfaces
└── IRequestContext                     read-only port for Application layer
      SessionTrackingId, CorrelationId, TraceId, SpanId (all string?)

Ums.Infrastructure.Services
└── RequestContextAccessor              implements IRequestContext + IExecutionContextAccessor
      registered as IRequestContext (read-only) and IExecutionContextAccessor (writable)
```

### Propagation chain

```
[HTTP Request arrives]
      │
      ▼
CorrelationIdMiddleware
  – reads / generates X-Correlation-Id header
  – writes to Activity.Current baggage ("correlation.id")
  – writes to ILogger scope ("CorrelationId")
      │
      ▼
SessionTrackingMiddleware
  – reads / generates X-Session-Tracking-Id header
  – writes to Activity.Current baggage ("session.tracking_id")
  – calls RequestContextAccessor.Set(new ExecutionContextSnapshot(...))
  – writes to ILogger scope ("SessionTrackingId")
      │
      ▼
RequestContextAccessor (scoped)
  – holds snapshot for remainder of request
  – readable by AOP aspects, handlers, background handoffs
      │
      ▼
UmsSerilogLogger / StructuredAopLoggerBase
  – calls ResolveExecutionContext() → reads RequestContextAccessor.Current
  – falls back to Activity.Current if snapshot is empty (background service path)
```

### Resolution priority in `StructuredAopLoggerBase.ResolveExecutionContext()`

```
1. RequestContextAccessor.Current (set by SessionTrackingMiddleware)
2. Activity.Current baggage (fallback for non-HTTP contexts)
3. requestId parameter from [LoggerAspect] attribute
4. Empty string
```

### DI registration

```csharp
// Ums.Infrastructure/DependencyInjection.cs
services.AddScoped<RequestContextAccessor>();
services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<RequestContextAccessor>());
services.AddScoped<IExecutionContextAccessor>(sp => sp.GetRequiredService<RequestContextAccessor>());
```

### Layer rules

| Layer | May use | May NOT use |
|-------|---------|-------------|
| `Ums.Domain` | — (no context needed) | `IRequestContext`, `IExecutionContextAccessor` |
| `Ums.Application` | `IRequestContext` (read-only port) | `IExecutionContextAccessor`, `RequestContextAccessor` |
| `Ums.Infrastructure` | `IExecutionContextAccessor` (AOP adapters) | direct `RequestContextAccessor` (inject via interface) |
| `Ums.Presentation` | Both interfaces via DI; `RequestContextAccessor` in middleware | — |

---

## Consequences

### Positive

- Single source of truth for all observability signals — one snapshot per request scope
- Application layer has zero HTTP dependency for correlation context
- Background services and outbox dispatchers can carry forwarded context by receiving an `ExecutionContextSnapshot` at handoff time
- `StructuredAopLoggerBase` in the shell library uses this pattern without any UMS-specific import
- `ObservabilityHeaders` and `ObservabilityKeys` constants prevent string-literal proliferation across middleware

### Trade-offs

- `RequestContextAccessor` is writable by any code with `IExecutionContextAccessor` — the contract is by convention, not enforced. Middleware should be the only writer.
- Snapshot is captured once per request at `SessionTrackingMiddleware` position in the pipeline; spans that start later in the pipeline get a stale `SpanId` in the snapshot. AOP aspects compensate by reading `Activity.Current.SpanId` as fallback.

---

## Evolith Extraction Checklist

The following types are in `BeyondNetCode.Shell.Logger.Serilog` with no UMS-specific import:
- [ ] `ExecutionContextSnapshot` — generic record, no product references
- [ ] `IExecutionContextAccessor` — generic interface
- [ ] `ObservabilityHeaders` — constants, rename prefix as appropriate
- [ ] `ObservabilityKeys` — constants, rename prefix as appropriate
- [ ] `StructuredAopLoggerBase` — depends only on `IExecutionContextAccessor` and `IJoinPoint`

`IRequestContext` and `RequestContextAccessor` are UMS-namespaced but trivially portable to any satellite.

---

**[ADR Registry](./index.md)** | **[CP-05 Execution Context](../artifacts/canonical-patterns/cp-05-execution-context-propagation.md)** | **[ADR-0053 OTel](./0053-opentelemetry-observability.md)**
