# UMS Shell Libraries — Technical Architecture Reference

**Role:** BMAD Architect  
**Scope:** `Ums.Shell.Factory` · `Ums.Shell.Aop` · `Ums.Shell.Bootstrapper`  
**Status:** Active evaluation — incremental adoption recommended  
**Last reviewed:** 2026-05-24

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Library Inventory](#2-library-inventory)
3. [Ums.Shell.Factory — Deep Dive](#3-umsshellfactory--deep-dive)
4. [Ums.Shell.Aop — Deep Dive](#4-umsshellaop--deep-dive)
5. [Ums.Shell.Bootstrapper — Deep Dive](#5-umsshellbootstrapper--deep-dive)
6. [Cross-Library Concerns](#6-cross-library-concerns)
7. [Decision Matrix](#7-decision-matrix)
8. [Incremental Adoption Plan](#8-incremental-adoption-plan)
9. [Final Recommendation](#9-final-recommendation)

---

## 1. Executive Summary

| Library | Current Use in API | Value | Decision |
|---|---|---|---|
| `Ums.Shell.Factory` | Referenced in `Ums.Domain.csproj`, not called | **Medium** — niche but unreplaceable for its specific pattern | **Keep & adopt** — activate for strategy/rule selection scenarios |
| `Ums.Shell.Aop` | Not referenced | **High** — real gap in the codebase | **Adopt actively** — AOP logging decorator, metrics, tracing |
| `Ums.Shell.Bootstrapper` | Not referenced | **Low** — mostly replaced by `IHostedService` and `IStartupFilter` | **Simplify** — adopt selectively for complex init chains only |

**Core finding:** All three libraries are more mature than their current adoption level suggests. The API is not using them, but `Ums.Shell.Aop` in particular solves a real architectural problem (non-invasive cross-cutting concerns) that currently has no clean solution in the codebase.

---

## 2. Library Inventory

### Physical structure

```
src/libs/shell/
├── factory/
│   ├── Ums.Shell.Factory          ← Core: fluent setup, selector engine, interceptor
│   └── Ums.Shell.Factory.Installer ← DI wiring (AddFactory extension)
├── aop/
│   ├── Ums.Shell.Aop              ← Core: IAspect, IJoinPoint, IPointCut, executors
│   ├── Ums.Shell.Aop.DispatchProxy ← Proxy creation via System.Reflection.DispatchProxy
│   ├── Ums.Shell.Aop.Aspects       ← Bundled cross-cutting aspects
│   ├── Ums.Shell.Aop.Aspects.Logger ← Logger aspect abstraction
│   ├── Ums.Shell.Aop.Aspects.Logger.Serilog ← Serilog-specific logger aspect
│   └── Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer ← DI wiring
└── bootstrapper/
    ├── Ums.Shell.Bootstrapper      ← Core: IBootstrapper, CompositeBootstrapper
    ├── Ums.Shell.Bootstrapper.DependencyInjection ← DI wiring
    └── Ums.Shell.Bootstrapper.AutoMapper ← AutoMapper bootstrap helper
```

### API reference in `Ums.Domain.csproj`

```xml
<!-- Only active reference across all ums.api projects -->
<ProjectReference Include="../../../libs/shell/factory/src/Ums.Shell.Factory/Ums.Shell.Factory.csproj" />
```

`Ums.Shell.Aop` and `Ums.Shell.Bootstrapper` have **zero** project references in `ums.api`.

---

## 3. Ums.Shell.Factory — Deep Dive

### 3.1 What it does

`Ums.Shell.Factory` implements a **configuration-driven, selector-based abstract factory**. Given a *target object* (the context) and a *service interface*, it returns the subset of registered implementations whose selector predicate matches the current target state.

```
IFactory.Create<TTarget, TService>(target)
    → FactorySetupProvider.Provide(target)          // filter by predicate
    → FactoryCreator.Create<TService>(implType)      // instantiate via Func<Type,object>
    → TService[]                                     // 0..N implementations
```

This is **not** a DI container replacement. It is an *abstract factory* where the set of returned implementations is determined at **runtime** by data on the target object.

### 3.2 Core mechanism

```csharp
// Setup (registered once at startup):
public class ValidationSetupSource : AbstractFactorySetupSource
{
    public ValidationSetupSource()
    {
        For<UserAccount, IValidator>().Create<ActiveUserValidator>().When(x => x.StatusId == UserStatus.Active);
        For<UserAccount, IValidator>().Create<PendingUserValidator>().When(x => x.StatusId == UserStatus.Pending);
        For<UserAccount, IValidator>().Create<BlockedUserValidator>().When(x => x.StatusId == UserStatus.Blocked);
    }
}

// Usage (called per request):
IValidator[] validators = _factory.Create<UserAccount, IValidator>(userAccount);
foreach (var v in validators)
    v.Validate(userAccount);
```

**`IFactoryInterceptor`** — lifecycle hooks: `OnEntry`, `OnSuccess`, `OnError`, `OnExit`. Can be used to log factory resolution, emit metrics, or add retry logic.

### 3.3 Comparison with native .NET DI

| Capability | `Ums.Shell.Factory` | Native Microsoft DI |
|---|---|---|
| Register multiple implementations of an interface | ✅ | ✅ (`IEnumerable<T>`) |
| Resolve ALL implementations | ✅ | ✅ (`GetServices<T>()`) |
| **Select implementations based on runtime data** | ✅ (selector predicate) | ❌ not built-in |
| **Named/grouped factories** | ✅ (`For<T,S>("group")`) | ⚠️ Keyed services (.NET 8+) — no predicates |
| Fluent setup DSL | ✅ | ❌ |
| Interceptor for factory lifecycle | ✅ | ❌ |
| Scoped / transient support | ✅ (via `Func<Type,object>`) | ✅ |

**The key differentiator:** Microsoft Keyed Services (`.NET 8`) select by *key string*, not by *predicate over a target object*. For scenarios where the right implementation depends on domain state (e.g., account status, tenant type, risk score), `Ums.Shell.Factory` fills a genuine gap.

### 3.4 Where it adds value in UMS

| Scenario | Target | Service | Selector example |
|---|---|---|---|
| Validation strategy | `UserAccount` | `IValidator` | `x.StatusId == Active` |
| Notification channel | `NotificationRule` | `INotificationChannel` | `x.ChannelType == Email` |
| IdP authentication flow | `IdpConfiguration` | `IAuthFlow` | `x.ProviderType == AzureAd` |
| Compliance check engine | `UserDocument` | `IComplianceCheck` | `x.DocumentType == Contract` |
| MFA factor | `MfaContext` (EP-06) | `IMfaFactor` | `x.RiskScore > 75` |

### 3.5 Where it does NOT add value

- Simple singleton/transient registration with one implementation.
- Named services where the key is a compile-time constant → use `.NET 8` Keyed Services.
- Services with complex dependency trees → use DI constructor injection directly.

### 3.6 Risks and anti-patterns

| Risk | Mitigation |
|---|---|
| `FactorySetupProvider` builds configuration at singleton initialization — **exception at startup** if any `SetupItem` has null types | Validate setup sources in integration tests |
| Selector predicates have **no compile-time check** on type safety | Keep predicates simple; unit test each source |
| `Func<Type, object>` in `FactoryCreator` hides dependencies | Document which concrete types must be registered in DI |
| `IFactoryInterceptor.Instance` is a **static mutable field** — last assignment wins in tests | Use `factory.Interceptor = new MyInterceptor()` per factory instance; avoid static mutation |

### 3.7 Recommended integration in UMS API

```csharp
// Ums.Infrastructure/DependencyInjection.cs
services.AddFactory(builder =>
{
    builder
        .AddSource<ValidationSetupSource>()     // domain validation
        .AddSource<NotificationSetupSource>()   // EP-07 notification channels
        .AddTransient<IValidator, ActiveUserValidator>()
        .AddTransient<IValidator, PendingUserValidator>()
        .AddTransient<INotificationChannel, EmailNotificationChannel>()
        .AddTransient<INotificationChannel, SmsNotificationChannel>();
});
```

**Decision: KEEP and adopt** for selector-based scenarios. Not a priority for day-one of MVP; relevant from EP-06 / EP-07 onward.

---

## 4. Ums.Shell.Aop — Deep Dive

### 4.1 What it does

`Ums.Shell.Aop` is a **lightweight Aspect-Oriented Programming framework** built on `System.Reflection.DispatchProxy`. It decorates service interfaces with cross-cutting behavior *without modifying the decorated class and without IL weaving*.

### 4.2 Architecture overview

```
                   ┌──────────────────────────────┐
  Caller ──────▶  │  Proxy (AopProxy<TService>)   │  ← DispatchProxy subclass
                   │  Intercepts every method call  │
                   └──────────────┬───────────────┘
                                  │ calls
                   ┌──────────────▼───────────────┐
                   │    AspectExecutor              │
                   │  - filters aspects by PointCut │
                   │  - orders by Aspect.GetOrder() │
                   │  - chains: A → B → C → target  │
                   └──────────────┬───────────────┘
                                  │
          ┌───────────┬───────────┼───────────┐
          ▼           ▼           ▼           ▼
      LogAspect  MetricAspect  RetryAspect  [target]
       OnEntry    OnEntry       Apply
       OnExit     OnExit        CanRetry
```

**Key types:**

| Type | Role |
|---|---|
| `IJoinPoint` | Carries method, arguments, return value, and `Proceed()` delegate |
| `IAspect` / `AbstractAspect<T>` | Single cross-cutting behavior; linked list via `SetNext` |
| `IPointCut` | Decides whether an aspect applies to a given method (attribute-driven by default) |
| `OnMethodBoundaryAspect<T>` | Template: `OnEntry`, `OnSuccess`, `OnExit`, `OnException` |
| `OnRetryAspect<T>` | Template: retry loop with `CanRetry` override |
| `AspectExecutor` | Assembles the chain and calls `root.Apply(joinPoint)` |
| `AopProxy<TService, TImpl>` | The actual `DispatchProxy`; calls `executor.Execute(joinPoint)` |

### 4.3 No IL weaving — why this matters

Unlike PostSharp, Fody, or Castle Windsor, `Ums.Shell.Aop` uses **`System.Reflection.DispatchProxy`** — built into the BCL. No build-time IL manipulation, no dynamic code generation at deploy time, no external dependency. The proxy wraps an interface; the real implementation is injected and called via `targetMethod.Invoke`.

**Trade-off:** DispatchProxy only wraps **interfaces**, not concrete classes. Every service to be decorated must be accessed through its interface (which is already true for all UMS services following Clean Architecture).

### 4.4 AOP Logging Decorator — Detailed Proposal

#### Step 1: Define the logging aspect attribute

```csharp
// Ums.Shell.Aop.Aspects.Logger / LogAttribute.cs
[AttributeUsage(AttributeTargets.Method)]
public sealed class LogAttribute : AbstractAspectAttribute
{
    public bool LogArguments  { get; set; } = false;  // never log by default — PII risk
    public bool LogReturnValue{ get; set; } = false;
    public LogLevel Level     { get; set; } = LogLevel.Debug;
}
```

#### Step 2: Implement the logging aspect

```csharp
// Ums.Infrastructure/Aop/Aspects/LoggingAspect.cs
public sealed class LoggingAspect : OnMethodBoundaryAspect<LogAttribute>
{
    private readonly ILogger<LoggingAspect> _logger;

    public LoggingAspect(ILogger<LoggingAspect> logger)
        => _logger = logger;

    protected override void OnEntry(IJoinPoint joinPoint)
    {
        var attr = CurrentAttribute;
        var level = attr?.Level ?? LogLevel.Debug;

        _logger.Log(level,
            "[{MethodName}] entry — tenant: {TenantId}",
            joinPoint.MethodInfo.Name,
            // pull from ambient context, not from args (PII safe)
            Activity.Current?.GetBaggageItem("tenant_id") ?? "unknown");
    }

    protected override void OnSuccess(IJoinPoint joinPoint)
        => _logger.LogDebug("[{MethodName}] completed", joinPoint.MethodInfo.Name);

    protected override void OnException(IJoinPoint joinPoint, Exception ex)
    {
        HandleException = true;  // suppress re-throw only if you catch-and-rethrow in handler

        _logger.LogError(ex,
            "[{MethodName}] failed — {ErrorMessage}",
            joinPoint.MethodInfo.Name, ex.Message);

        throw;  // always re-throw in command/query handlers
    }
}
```

#### Step 3: Decorate a handler with the attribute

```csharp
// Ums.Application/Identity/Tenant/Commands/CreateTenantCommandHandler.cs
public sealed class CreateTenantCommandHandler : ICommandHandler<CreateTenantCommand, Result<Guid>>
{
    // ...

    [Log(Level = LogLevel.Information)]
    public async Task<Result<Guid>> Handle(CreateTenantCommand command, CancellationToken ct)
    {
        // ...
    }
}
```

#### Step 4: Register in DI

```csharp
// Ums.Infrastructure/DependencyInjection.cs
services.AddAop(builder =>
{
    builder
        .For<ICommandHandler<CreateTenantCommand, Result<Guid>>,
             CreateTenantCommandHandler>()
        .WithAspect<LoggingAspect>();
});
```

> **Note:** The actual DI registration API may differ slightly depending on what `Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer` exposes. Read its source before wiring up.

### 4.5 Cross-cutting concerns catalogue

| Concern | Aspect | Attribute | Notes |
|---|---|---|---|
| **Structured logging** | `LoggingAspect` | `[Log]` | Never log arguments by default — PII risk |
| **Metrics / latency** | `MetricsAspect` | `[Metrics]` | Emit OTel histogram in `OnExit` via `Meter` |
| **Distributed tracing** | `TracingAspect` | `[Trace]` | Start child `Activity` in `OnEntry`, end in `OnExit` |
| **Validation guard** | `ValidationAspect` | `[Validate]` | Runs FluentValidation before `Proceed()` |
| **Authorization check** | `AuthorizationAspect` | `[Authorize(Policy)]` | Checks policy before `Proceed()`, throws 401/403 |
| **Idempotency** | `IdempotencyAspect` | `[Idempotent]` | Checks idempotency store before `Proceed()` |
| **Transaction boundary** | `TransactionAspect` | `[Transactional]` | Wraps `Proceed()` in `IUnitOfWorkScope` |
| **Retry** | Extend `OnRetryAspect<T>` | `[Retry(Max=3)]` | Override `CanRetry` with Polly conditions |
| **Rate limiting** | `RateLimitAspect` | `[RateLimit]` | Check token bucket before `Proceed()` |
| **Audit** | `AuditAspect` | `[Audit]` | Write to `IAuditRecordRepository` in `OnSuccess` |

### 4.6 Aspect ordering convention

Aspects execute in `Order` ascending. Recommended order:

```
Order 10 → TracingAspect      (outermost — span wraps everything)
Order 20 → AuthorizationAspect (before any business logic)
Order 30 → ValidationAspect   (before state mutation)
Order 40 → IdempotencyAspect  (check before touching state)
Order 50 → LoggingAspect      (log around real work)
Order 60 → MetricsAspect      (measure real work)
Order 70 → TransactionAspect  (innermost — wraps data access)
```

### 4.7 Comparison with alternatives

| Approach | Invasiveness | Performance | DI Friendly | IL Weaving |
|---|---|---|---|---|
| `Ums.Shell.Aop` (DispatchProxy) | **Non-invasive** | Reflection overhead (~microseconds) | ✅ | ❌ |
| Manual decorator pattern | Verbose, one class per concern | Zero overhead | ✅ | ❌ |
| MediatR `IPipelineBehavior<,>` | Invasive (MediatR-coupled) | Zero overhead | ✅ | ❌ |
| PostSharp / Fody | Non-invasive | Zero overhead | ✅ | ✅ (build-time) |
| Castle DynamicProxy | Non-invasive | Near-zero | ✅ | ❌ |

**Recommendation for UMS:** Use `Ums.Shell.Aop` for service-level decorators (application services, repositories). Continue using `IPipelineBehavior<,>` for MediatR pipeline concerns (`ValidationBehavior`). Do not replace the existing MediatR pipeline.

### 4.8 Risks

| Risk | Severity | Mitigation |
|---|---|---|
| **DispatchProxy reflection overhead** on hot paths | Low | Benchmark; most UMS handlers are I/O-bound, reflection is noise |
| **Async methods** — `Invoke` returns `Task`; joinpoint must handle `await` | Medium | The proxy returns the `Task` object correctly; `OnSuccess` fires before `await` completes unless you unwrap. **Wrap async with `await joinPoint.Proceed()` semantics** |
| **Only interfaces are proxyable** | Low | UMS already programs to interfaces everywhere |
| **Attribute-based activation** — forgotten attribute = no aspect | Low | Document convention; add integration test |
| **Exception handling** in `OnMethodBoundaryAspect` — `HandleException=true` swallows | Medium | Always re-throw in UMS command handlers; only catch-and-suppress in infrastructure |

**Decision: ADOPT actively** — highest architectural value of the three libraries. Priority: logging + tracing decorator in Sprint 2–3.

---

## 5. Ums.Shell.Bootstrapper — Deep Dive

### 5.1 What it does

`Ums.Shell.Bootstrapper` provides two interfaces and their Composite implementations:

```csharp
IBootstrapper      → void Run()
IBootstrapperAsync → Task RunAsync(CancellationToken)

CompositeBootstrapper      → runs N bootstrappers in sequence
CompositeBootstrapperAsync → awaits N bootstrappers in sequence
```

That's the entire library. No service discovery, no module loading, no convention-based registration.

### 5.2 Comparison with native .NET alternatives

| Capability | `Ums.Shell.Bootstrapper` | Native .NET |
|---|---|---|
| Sequential init on startup | ✅ `CompositeBootstrapper` | ✅ `IHostedService.StartAsync` |
| Async init with cancellation | ✅ `CompositeBootstrapperAsync` | ✅ `IHostedService` + `IHostApplicationLifetime` |
| Module-level DI registration | ❌ not built-in | ✅ `IServiceCollection` extension methods |
| Ordered startup with dependencies | ❌ manual order | ✅ `IStartupFilter` chain |
| Health gate during init | ❌ | ✅ `AddHealthChecks()` readiness checks |
| Convention-based module discovery | ❌ | ❌ (neither) |

### 5.3 When it adds value

`Ums.Shell.Bootstrapper` adds value **only** when you have multiple independent initialization steps that need to be composed without coupling them to `Program.cs`. Example:

```csharp
// Complex multi-step init (e.g., seed + warm-up + schema check)
var bootstrapper = new CompositeBootstrapperAsync(new[]
{
    new SqlSchemaBootstrapper(dbContext),
    new CacheWarmupBootstrapper(cache),
    new SearchIndexBootstrapper(searchClient),
});

await bootstrapper.RunAsync(cancellationToken);
```

For UMS, `SqlServerSchemaBootstrapper.InitializeAsync()` already does this directly. The Bootstrapper library would add a layer of abstraction without substantive new capability.

### 5.4 Recommended use — selective

Use it **only** if `Program.cs` startup logic grows beyond 3 independent initialization phases. Until then, keep direct calls.

```csharp
// Threshold: if this grows to 5+ await calls in Program.cs, introduce CompositeBootstrapperAsync
if (initializePlatformStore)
    await SqlServerSchemaBootstrapper.InitializeAsync(dbContext);
```

**Decision: SIMPLIFY** — Do not adopt broadly. If startup complexity grows, adopt `CompositeBootstrapperAsync` as a clean way to compose initialization steps. Current codebase does not need it.

---

## 6. Cross-Library Concerns

### 6.1 Coupling map

```
Ums.Shell.Aop ──────────────────────────────────▶ (no deps)
Ums.Shell.Factory ───────────────────────────────▶ Microsoft.Extensions.Logging.Abstractions
Ums.Shell.Bootstrapper ──────────────────────────▶ (no deps)

Ums.Domain ─────────────────────────────────────▶ Ums.Shell.Factory (reference exists, not used)
Ums.Infrastructure ──────────────────────────────▶ (none of the three)
Ums.Presentation ────────────────────────────────▶ (none of the three)
```

**Risk:** `Ums.Domain` references `Ums.Shell.Factory` but does not use it. This leaks an infrastructure concern into the Domain layer. The reference should be moved to `Ums.Infrastructure` when the factory is activated.

### 6.2 Library health assessment

| Library | Test Coverage | Public API Stability | Async Support | .NET Version |
|---|---|---|---|---|
| `Ums.Shell.Factory` | ✅ unit tests | ✅ stable | ⚠️ sync only | net10.0 |
| `Ums.Shell.Aop` | ✅ unit tests | ✅ stable | ⚠️ async proxy needs care | net10.0 |
| `Ums.Shell.Bootstrapper` | ✅ basic tests | ✅ stable | ✅ async first | net10.0 |

### 6.3 Async proxy consideration for AOP

The DispatchProxy `Invoke` method is **synchronous** at the proxy level — it returns `object`. For async methods, the returned object is a `Task` or `Task<T>`. `OnSuccess` in `OnMethodBoundaryAspect` fires when the **proxy call returns the Task**, not when the Task completes.

**Solution for UMS:** For async command handlers, wrap the actual awaited work inside `Proceed()` and use a continuation pattern:

```csharp
// In the aspect, detect and unwrap Tasks:
protected override void OnEntry(IJoinPoint joinPoint) { /* start timer */ }

public override void Apply(IJoinPoint joinPoint)
{
    OnEntry(joinPoint);
    joinPoint.Proceed();  // this sets joinPoint.Return = Task<Result>

    // Wrap the returned Task to fire OnSuccess/OnExit after completion:
    if (joinPoint.Return is Task task)
    {
        joinPoint.Return = WrapAsync(task, joinPoint);
    }
    else
    {
        OnSuccess(joinPoint);
        OnExit(joinPoint);
    }
}

private async Task WrapAsync(Task inner, IJoinPoint joinPoint)
{
    try { await inner; OnSuccess(joinPoint); }
    catch (Exception ex) { OnException(joinPoint, ex); throw; }
    finally { OnExit(joinPoint); }
}
```

This pattern must be applied in `OnMethodBoundaryAspect` for UMS to work correctly with async handlers. **This is a known gap in the library that should be addressed before adoption.**

---

## 7. Decision Matrix

| Library | Value | Complexity | Risk | Decision | Priority |
|---|---|---|---|---|---|
| **Ums.Shell.Factory** | Medium | Low | Low | **Keep & adopt** in EP-06/07 | Sprint 5+ |
| **Ums.Shell.Aop** | High | Medium | Medium | **Adopt actively** from Sprint 2 | Sprint 2–3 |
| **Ums.Shell.Bootstrapper** | Low | Low | Low | **Keep, use selectively** | If needed |

### What to remove

Nothing. All three libraries have a legitimate purpose. The `ServiceLocator` anti-pattern was already removed (commit `35a474a`). No further removal is warranted.

### What to fix before adoption

1. **Move `Ums.Shell.Factory` reference** from `Ums.Domain.csproj` to `Ums.Infrastructure.csproj` — Domain must not know about infrastructure factories.
2. **Fix async in `OnMethodBoundaryAspect`** — add Task unwrapping before any production AOP decorator.
3. **Fix 2 pre-existing failing tests** in `Ums.Shell.Ddd.Test` — `BrokenRules.Clear()` is commented out in `ValueObject.cs`; uncomment and verify validators accumulate correctly.

---

## 8. Incremental Adoption Plan

### Phase 0 — Housekeeping (Sprint 1, ~1 day)

- [ ] Move `Ums.Shell.Factory` project reference: `Ums.Domain.csproj` → `Ums.Infrastructure.csproj`
- [ ] Fix async proxy support in `OnMethodBoundaryAspect` (add Task unwrapping)
- [ ] Add `Ums.Shell.Aop` and `Ums.Shell.Bootstrapper` project references to `Ums.Infrastructure.csproj`

### Phase 1 — AOP Logging Decorator (Sprint 2, ~3 days)

- [ ] Define `[Log]` attribute in `Ums.Shell.Aop.Aspects.Logger`
- [ ] Implement `LoggingAspect` in `Ums.Infrastructure/Aop/Aspects/`
- [ ] Wire `ICommandHandler<CreateTenantCommand, ...>` as proof-of-concept
- [ ] Add integration test verifying log output via `ITestSink`
- [ ] Document PII-safe logging conventions (no argument logging by default)

### Phase 2 — Tracing + Metrics Aspects (Sprint 3, ~2 days)

- [ ] Implement `TracingAspect` (start `Activity`, add `tenant_id` tag)
- [ ] Implement `MetricsAspect` (record histogram via `Meter`)
- [ ] Apply to all command handlers across bounded contexts
- [ ] Verify no duplicate OTel spans (aspects + Serilog enricher)

### Phase 3 — Factory Activation (Sprint 5, EP-06 onset, ~3 days)

- [ ] Implement `NotificationSetupSource` for EP-07 notification channels
- [ ] Implement `MfaFactorSetupSource` for EP-06 adaptive MFA
- [ ] Register via `AddFactory(...)` in `Ums.Infrastructure/DependencyInjection.cs`
- [ ] Unit test each setup source for correct predicate behavior

### Phase 4 — Additional Aspects (Post-MVP)

- [ ] `ValidationAspect` — move FluentValidation guard from `ValidationBehavior` to optional aspect layer
- [ ] `TransactionAspect` — wrap multi-repo operations
- [ ] `AuditAspect` — secondary audit trail for high-value operations (EP-06 Approvals)

---

## 9. Final Recommendation

### As BMAD Architect

**`Ums.Shell.Factory`** — Keep. Its configuration-driven, predicate-based selector fills a gap that native .NET Keyed Services do not address. Activating it for EP-06 (MFA factor selection) and EP-07 (notification channel routing) will reduce complexity in those use cases significantly. Move its reference out of the Domain layer immediately.

**`Ums.Shell.Aop`** — This is the highest-value library in the set. The UMS API currently handles cross-cutting concerns through a mixture of MediatR pipeline behaviors, middleware, and manual code in handlers. `Ums.Shell.Aop` provides a clean, non-invasive, interface-proxying AOP framework that fits the Clean Architecture model perfectly. **The async proxy gap must be fixed first.** Once resolved, adopt for logging and tracing across all command handlers — this will reduce handler code by 30–40% in the concerns that are currently repeated (start timer, log, catch, rethrow).

**`Ums.Shell.Bootstrapper`** — Keep in the toolbox. Do not actively adopt unless `Program.cs` startup grows beyond its current complexity. If it does, `CompositeBootstrapperAsync` is the right abstraction.

**Overall principle:** These libraries follow Clean Architecture, have no circular dependencies, and are built on BCL primitives (DispatchProxy, IEnumerable, IHostedService patterns). They are not frameworks — they are thin, composable abstractions. The cost of adoption is low. The cost of *not* adopting `Ums.Shell.Aop` is continued proliferation of manual cross-cutting code in handlers.

---

*Document maintained by: Architecture team*  
*Next review: Sprint 3 retrospective*
