# Ums.Shell.Aop — Developer Guide

> **Part of:** [Shell Libraries](README.md)  
> **Projects:** `Ums.Shell.Aop` · `Ums.Shell.Aop.DispatchProxy` · `Ums.Shell.Aop.Aspects` · `Ums.Shell.Aop.Aspects.Logger.Serilog` · `Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer`  
> **Dependencies:** `Microsoft.Extensions.DependencyInjection` · `Serilog` (optional) · `System.Linq.Dynamic.Core`

`Ums.Shell.Aop` provides **non-invasive aspect-oriented programming** via `System.Reflection.DispatchProxy`. Cross-cutting concerns (logging, retry, advice) are applied as an ordered chain of `IAspect` objects around any interface-backed service — with no modification to the service implementation.

---

## Table of Contents

1. [Architecture Overview](#1-architecture-overview)
2. [Project Structure](#2-project-structure)
3. [Standalone Usage — no DI](#3-standalone-usage--no-di)
4. [DI Usage with `AddAop()` + `AddAopProxy()`](#4-di-usage-with-addaop--addaopproxy)
5. [Built-in Aspects](#5-built-in-aspects)
6. [Writing a Custom Aspect](#6-writing-a-custom-aspect)
7. [Async Support](#7-async-support)
8. [PII-Safe Logging — MelLogger](#8-pii-safe-logging--mellogger)
9. [API Reference](#9-api-reference)
10. [UMS Integration](#10-ums-integration)
11. [Aspect Ordering Convention](#11-aspect-ordering-convention)
12. [Troubleshooting](#12-troubleshooting)

---

## 1. Architecture Overview

```
Caller
  │
  ▼
AopProxy<TService, TImpl>          ← DispatchProxy subclass
  │ Invoke(MethodInfo, args[])
  ▼
AspectExecutor
  │ for each matching aspect (ordered by GetOrder)
  ▼
IAspect chain  →  OnMethodBoundaryAspect<TAttribute>
  │                 OnEntry()
  │                 Proceed()  ──────────────────────────► real TImpl.Method()
  │                 OnSuccess() (after Task completes)
  │                 OnExit()
  │                 OnException() (if throws)
  ▼
return value (Task or sync)
```

Key design decisions:
- **Attribute-driven selection**: `PointCut.CanApply` checks for the attribute type in `aspect.BaseType.GetGenericArguments()`. An aspect only fires if the target method carries its corresponding attribute.
- **Ordered chain**: aspects are sorted by `GetOrder(joinPoint)` — earlier order numbers run first, outer-most in the call stack.
- **Async-aware** (since Phase 0-C fix): `OnMethodBoundaryAspect.Apply` detects `Task`/`Task<T>` returns and defers `OnSuccess`/`OnExit`/`OnException` to a continuation task.

---

## 2. Project Structure

```
Ums.Shell.Aop/
├── Interface/
│   ├── IAspect.cs           ← void Apply(IJoinPoint), SetNext/GetNext, GetOrder
│   ├── IAspectExecutor.cs   ← void Execute(IJoinPoint)
│   ├── IJoinPoint.cs        ← MethodInfo, Arguments, Return, TargetType, Proceed()
│   └── IPointCut.cs         ← bool CanApply(IJoinPoint, Type aspectType)
└── Impl/
    ├── AbstractAspect.cs              ← chain linkage + GetAttribute<TAttr>()
    ├── AbstractAspectAttribute.cs     ← marker base for aspect attributes
    ├── AspectExecutor.cs              ← filter + order + chain execution
    ├── OnMethodBoundaryAspect.cs      ← template: OnEntry/OnSuccess/OnExit/OnException + async support
    ├── OnRetryAspect.cs               ← retry-aware boundary
    ├── JoinPoint.cs                   ← IJoinPoint implementation
    └── PointCut.cs                    ← attribute-based CanApply with cache

Ums.Shell.Aop.DispatchProxy/
├── AopProxy.cs              ← System.Reflection.DispatchProxy subclass
└── AopProxyCreator.cs       ← static Create<TService,TImpl>(target, executor)

Ums.Shell.Aop.Aspects/
├── Impl/
│   ├── LoggerAspect.cs          ← OnMethodBoundaryAspect<LoggerAspectAttribute>
│   ├── LoggerAspectAttribute.cs ← Type, LogArguments[], LogReturn, LogDuration, LogException, Expression
│   ├── AdviceAspect.cs          ← OnMethodBoundaryAspect<AdviceAspectAttribute>
│   ├── AdviceAspectAttribute.cs ← Type (IAdvice implementation)
│   ├── RetryAspect.cs           ← OnRetryAspect<RetryAspectAttribute>
│   ├── RetryAspectAttribute.cs  ← MaxRetries, ExceptionType
│   ├── Advice.cs                ← IAdvice; called by AdviceAspect
│   ├── Evaluator.cs             ← System.Linq.Dynamic expression evaluator
│   └── Factory.cs               ← IFactory<T> wrapping Func<Type,T>
└── Interface/
    ├── IAdvice.cs               ← void OnEntry/OnSuccess/OnException/OnExit(IJoinPoint)
    ├── IEvaluator.cs            ← string Evaluate(IJoinPoint, expression, default)
    ├── IFactory.cs              ← T Create(Type)
    └── ILogger.cs               ← AOP logger contract (not MEL ILogger)

Ums.Shell.Aop.Aspects.Logger.Serilog/
└── SerilogLogger.cs         ← ILogger (AOP) backed by Serilog static Log.*

Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer/
├── AopAspectsBuilder.cs         ← AddAspect<T>(), AddAdvice<T>(), AddLogger<T>()
└── ServiceCollectionExtension.cs ← AddAop(configure?), AddAopProxy<TService,TImpl>()
```

---

## 3. Standalone Usage — no DI

Use when writing unit tests or console tools without a full DI container.

```csharp
using Ums.Shell.Aop;
using Ums.Shell.Aop.Aspects;
using Ums.Shell.Aop.DispatchProxy;
using Ums.Shell.Aop.Impl;

// ──── 1. Define service
public interface ICalculator { int Add(int a, int b); }

public class Calculator : ICalculator
{
    public int Add(int a, int b) => a + b;
}

// ──── 2. Write a custom aspect (extend OnMethodBoundaryAspect)
public class TimingAttribute : AbstractAspectAttribute { }

public class TimingAspect : OnMethodBoundaryAspect<TimingAttribute>
{
    private readonly Stopwatch _sw = new();

    protected override void OnEntry(IJoinPoint jp)
        => _sw.Restart();

    protected override void OnExit(IJoinPoint jp)
        => Console.WriteLine($"{jp.MethodInfo.Name} took {_sw.ElapsedMilliseconds}ms");
}

// ──── 3. Decorate the target method
public class TimedCalculator : ICalculator
{
    [Timing]
    public int Add(int a, int b) => a + b;
}

// ──── 4. Build the proxy manually
var target = new TimedCalculator();

var pointCut = new PointCut();
var executor = new AspectExecutor(
    types: [typeof(TimingAspect)],
    aspectFactory: type => type == typeof(TimingAspect)
        ? new TimingAspect()
        : throw new InvalidOperationException(),
    pointCut: pointCut);

ICalculator proxy = AopProxyCreator.Create<ICalculator, TimedCalculator>(target, executor);

// ──── 5. Call through the proxy
int result = proxy.Add(3, 4);  // Console: "Add took 0ms"
// result == 7
```

---

## 4. DI Usage with `AddAop()` + `AddAopProxy()`

`AddAop()` wires the built-in aspects (`LoggerAspect`, `AdviceAspect`, `RetryAspect`), the `PointCut`, `AspectExecutor`, and the keyed-service factories for `ILogger` (AOP) and `IAdvice`.

`AddAopProxy<TService, TImpl>()` registers:
1. `TImpl` as itself (concrete handler).
2. `TService` as a factory that creates a `DispatchProxy` wrapping `TImpl`.

Because DI returns the last registration, MediatR (or any caller) transparently resolves the proxy.

```csharp
// In Ums.Infrastructure/DependencyInjection.cs
services.AddAop(builder =>
{
    // Register additional loggers beyond the defaults
    builder.AddLogger<SerilogLogger>();    // key = typeof(SerilogLogger)
    builder.AddLogger<MelLogger>();        // key = typeof(MelLogger) / typeof(IMelLogger)

    // Register additional advice implementations
    builder.AddAdvice<AuditAdvice>();
});

// Register the keyed MelLogger under the IMelLogger key so handlers can use it
services.AddKeyedTransient<ILogger, MelLogger>(typeof(IMelLogger));

// Wrap a MediatR handler
services.AddAopProxy<
    IRequestHandler<CreateTenantCommand, Result<CreateTenantResponse>>,
    CreateTenantCommandHandler>();
```

### Decorating the handler

```csharp
// In Ums.Application (references Ums.Shell.Aop.Aspects only — no Infrastructure dep)
public sealed class CreateTenantCommandHandler
    : ICommandHandler<CreateTenantCommand, CreateTenantResponse>
{
    [LoggerAspect(
        Type         = typeof(IMelLogger),  // resolved from DI as keyed service
        LogDuration  = true,
        LogException = true,
        LogArguments = [])]                 // PII-safe: no arg values
    public async Task<Result<CreateTenantResponse>> Handle(
        CreateTenantCommand request,
        CancellationToken cancellationToken)
    {
        // ... handler logic
    }
}
```

---

## 5. Built-in Aspects

### 5.1 LoggerAspect

`OnMethodBoundaryAspect<LoggerAspectAttribute>` — fires log statements before and after the method.

#### LoggerAspectAttribute properties

| Property | Type | Description |
|---|---|---|
| `Type` | `Type` | `ILogger` implementation to resolve (must be registered as keyed service) |
| `LogArguments` | `string[]` | Parameter names whose values to log (PII-safe: leave empty to log names/types only) |
| `LogReturn` | `bool` | Include the return value in the exit log |
| `LogDuration` | `bool` | Include elapsed milliseconds in the exit log |
| `LogException` | `bool` | Catch exceptions, log them, then re-throw |
| `Expression` | `string` | Dynamic expression (System.Linq.Dynamic) to extract a request-ID from the JoinPoint arguments |

```csharp
// Log entry, exit with duration, and exceptions; use request.TenantId as request-ID
[LoggerAspect(
    Type         = typeof(SerilogLogger),
    LogDuration  = true,
    LogException = true,
    Expression   = "request.TenantId")]
public async Task<Result> Handle(ActivateTenantCommand request, CancellationToken ct) { ... }
```

### 5.2 AdviceAspect

`OnMethodBoundaryAspect<AdviceAspectAttribute>` — delegates to a registered `IAdvice` for flexible cross-cutting logic.

```csharp
public class AuditAdvice : IAdvice
{
    public void OnEntry(IJoinPoint jp)   => /* pre-call action */ ;
    public void OnSuccess(IJoinPoint jp) => /* post-success action */;
    public void OnException(IJoinPoint jp, Exception ex) => /* error handling */;
    public void OnExit(IJoinPoint jp)    => /* always-runs action */;
}

// Register
services.AddAop(b => b.AddAdvice<AuditAdvice>());

// Use on method
[AdviceAspect(Type = typeof(AuditAdvice))]
public async Task<Result> Handle(SomeCommand cmd, CancellationToken ct) { ... }
```

### 5.3 RetryAspect

`OnRetryAspect<RetryAspectAttribute>` — retries the method on transient failure.

```csharp
[RetryAspect(MaxRetries = 3, ExceptionType = typeof(HttpRequestException))]
public async Task<Result> CallExternalServiceAsync(Request req, CancellationToken ct) { ... }
```

---

## 6. Writing a Custom Aspect

```csharp
// 1. Define the attribute
public class MetricsAttribute : AbstractAspectAttribute
{
    public string MetricName { get; set; } = string.Empty;
}

// 2. Implement the aspect
public class MetricsAspect(IMeterFactory meterFactory)
    : OnMethodBoundaryAspect<MetricsAttribute>
{
    private readonly Histogram<long> _duration =
        meterFactory.Create("ums").CreateHistogram<long>("handler.duration.ms");

    private Stopwatch _sw = new();

    protected override void OnEntry(IJoinPoint jp)
        => _sw.Restart();

    protected override void OnSuccess(IJoinPoint jp)
    {
        _sw.Stop();
        _duration.Record(_sw.ElapsedMilliseconds,
            new TagList { { "method", jp.MethodInfo.Name } });
    }

    // Return custom order so this aspect runs after Logging (50) but before Transaction (70)
    public override int GetOrder(IJoinPoint jp) => 60;
}

// 3. Register
services.AddAop(b => b.AddAspect<MetricsAspect>());

// 4. Apply
[MetricsAspect(MetricName = "create_tenant")]
public async Task<Result<CreateTenantResponse>> Handle(...) { ... }
```

---

## 7. Async Support

`OnMethodBoundaryAspect.Apply` is async-aware since the Phase 0-C fix. It:

1. Calls `joinPoint.Proceed()` which stores the raw return value in `joinPoint.Return`.
2. If `joinPoint.Return` is a `Task`: wraps it in a new continuation task (`WrapAsync` / `WrapAsyncOfT<T>`).
3. Stores the wrapper task back in `joinPoint.Return`; `OnSuccess`/`OnException`/`OnExit` fire inside the continuation after `ConfigureAwait(false)`.
4. `AopProxy.Invoke` returns `joinPoint.Return` (the wrapper task) — the caller awaits it normally.

**Effect:** `OnSuccess` fires when the `Task` actually completes, not when it is returned.

For `Task<TResult>` methods, the result value is preserved through the `WrapAsyncOfT<TResult>` path via reflection + cached `MethodInfo`.

```
Caller awaits proxy.Handle(cmd, ct)
  → AopProxy.Invoke returns Task<Result<...>> (wrapper)
       → wrapper awaits real Handle() task
            → OnSuccess fires
            → return result to caller
```

---

## 8. PII-Safe Logging — MelLogger

`MelLogger` (`Ums.Infrastructure/Aop/MelLogger.cs`) is the Microsoft.Extensions.Logging adapter:

- Resolves a per-call `ILogger` from `ILoggerFactory` using `jp.TargetType` as the category name.
- **Never logs argument values** — only parameter names and CLR types.
- Registered as a keyed service under `typeof(IMelLogger)`.
- Use `[LoggerAspect(LogArguments = [])]` (empty array) to log only entry/exit metadata.
- Use `[LoggerAspect(LogArguments = ["request"])]` to include parameter name + type (not value) of `request`.

For richer structured logging with value capture (after PII review), use `SerilogLogger` instead — it uses `Log.ForContext("Arguments", arguments, true)` with Serilog's destructuring.

| Logger | Arg values | Category | Structured |
|---|---|---|---|
| `MelLogger` | ❌ Never | `jp.TargetType` | ✅ via MEL templates |
| `SerilogLogger` | ✅ Destructured | `[ClassName, MethodName]` | ✅ Serilog |

---

## 9. API Reference

### `AddAop(Action<IAopAspectsBuilder>?)`

Registers into DI:
- `LoggerAspect`, `AdviceAspect`, `RetryAspect` as keyed transient `IAspect` services.
- `Advice` as keyed transient `IAdvice`.
- `IPointCut` (singleton `PointCut`).
- `IAspectExecutor` (transient `AspectExecutor`).
- `IFactory<IAdvice>` and `IFactory<ILogger>` (transient factories backed by keyed-service resolution).
- `IEvaluator` (singleton `Evaluator` using System.Linq.Dynamic).

### `AddAopProxy<TService, TImpl>(ServiceLifetime = Scoped)`

| Restriction | Detail |
|---|---|
| Singleton not supported | Aspects may depend on scoped services; `Singleton` throws `ArgumentException` |
| `TImpl` must implement `TService` | Compile-time constraint |
| Last-wins registration | Call after `AddMediatR` / any other registration of `TService` |

### `IAopAspectsBuilder`

| Method | Registers |
|---|---|
| `AddAspect<T>()` | Keyed `IAspect` with key `typeof(T)` + adds `T` to the aspect type list |
| `AddAdvice<T>()` | Keyed `IAdvice` with key `typeof(T)` |
| `AddLogger<T>()` | Keyed `ILogger` (AOP) with key `typeof(T)` |

### `OnMethodBoundaryAspect<TAttribute>`

| Virtual method | When called |
|---|---|
| `OnEntry(IJoinPoint)` | Before the method (always synchronous) |
| `OnSuccess(IJoinPoint)` | After method succeeds (after Task completes for async) |
| `OnExit(IJoinPoint)` | Always, after success or exception (after Task for async) |
| `OnException(IJoinPoint, Exception)` | When an exception is thrown; only if `HandleException = true` |
| `Continue(IJoinPoint) → bool` | If `false`, skips method invocation entirely |

---

## 10. UMS Integration

### Currently wired

| Handler | Aspect | Config |
|---|---|---|
| `CreateTenantCommandHandler.Handle` | `LoggerAspect` via `MelLogger` | `LogDuration=true, LogException=true` |

### Expanding to other handlers

```csharp
// In Ums.Infrastructure/DependencyInjection.cs — add more AddAopProxy calls

services.AddAopProxy<
    IRequestHandler<CreateUserAccountCommand, Result<Guid>>,
    CreateUserAccountCommandHandler>();

services.AddAopProxy<
    IRequestHandler<ActivateTenantCommand, Result>,
    ActivateTenantCommandHandler>();
```

Decorate each handler with `[LoggerAspect(Type = typeof(IMelLogger), LogDuration = true, LogException = true, LogArguments = [])]`.

### Adding Tracing aspect (Phase 2 — planned)

```csharp
public class TracingAttribute : AbstractAspectAttribute { }

public class TracingAspect(ActivitySource source) : OnMethodBoundaryAspect<TracingAttribute>
{
    private Activity? _activity;

    protected override void OnEntry(IJoinPoint jp)
        => _activity = source.StartActivity(jp.MethodInfo.Name);

    protected override void OnSuccess(IJoinPoint jp)
        => _activity?.SetStatus(ActivityStatusCode.Ok);

    protected override void OnException(IJoinPoint jp, Exception ex)
        => _activity?.SetStatus(ActivityStatusCode.Error, ex.Message);

    protected override void OnExit(IJoinPoint jp)
        => _activity?.Dispose();

    public override int GetOrder(IJoinPoint jp) => 10; // first in chain
}
```

---

## 11. Aspect Ordering Convention

| Order | Aspect | Role |
|---|---|---|
| 10 | `TracingAspect` | Outer span — captures full latency |
| 20 | `AuthorizationAspect` | Reject early |
| 30 | `ValidationAspect` | Domain pre-conditions |
| 40 | `IdempotencyAspect` | Dedup before any side-effects |
| 50 | `LoggerAspect` | Observe the real execution window |
| 60 | `MetricsAspect` | Record duration/throughput |
| 70 | `RetryAspect` | Outermost retry loop |

Implement `GetOrder(IJoinPoint)` in your aspect class to return the appropriate constant.

---

## 12. Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| Aspect never fires | Method doesn't have the attribute | Add `[YourAttribute]` to the concrete method (not the interface) |
| `InvalidCastException` in proxy | `TService` must be an interface or abstract class | DispatchProxy requires interface/abstract target |
| `OnSuccess` fires before task completes | Old `OnMethodBoundaryAspect` (pre Phase 0-C) | Update to latest `Ums.Shell.Aop` — fix already applied |
| Keyed service not found | Logger type not registered | Add `builder.AddLogger<MyLogger>()` in `AddAop()` callback |
| `ArgumentException: Singleton not supported` | Called `AddAopProxy<,>(ServiceLifetime.Singleton)` | Use `Scoped` (default) or `Transient` |
| `LoggerAspect.Init`: `Type should not be null` | Missing `Type` property on attribute | Always set `Type = typeof(IMyLogger)` in the attribute |

---

## Related Docs

- [DDD](ddd.md) — aggregates whose command handlers are wrapped with AOP
- [Factory](factory.md) — factory-resolved services can also be wrapped
- [Bootstrapper](bootstrapper.md) — `ObservabilityBootstrapper` provides OpenTelemetry tracing infrastructure
- [Combined Usage](combined-usage.md) — all four libraries working together
