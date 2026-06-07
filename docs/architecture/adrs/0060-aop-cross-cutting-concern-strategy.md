# ADR-0060: AOP Cross-Cutting Concern Strategy — DispatchProxy over MediatR Behaviors

> **Promoted to Evolith:** This ADR has been elevated to [Evolith ADR-0072 — .NET AOP Cross-Cutting Concern Strategy](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/dotnet/0072-dotnet-aop-cross-cutting-concern-strategy.md). UMS retains this document as implementation reference.

## Status

Accepted

## Date

2026-05-24

## Decision Owner

Architecture

## Related

- [ADR-0053: OpenTelemetry Observability](./0053-opentelemetry-observability.md) — defines the signals that AOP aspects must emit
- [ADR-0054: Shell Library Isolation](./0054-shell-library-isolation.md) — governs how `BeyondNetCode.Shell.Aop` is consumed
- [Shell Libraries — AOP Guide](../shell-libraries/aop.md) — implementation reference
- [Shell Libraries — Combined Usage](../shell-libraries/combined-usage.md) — end-to-end walkthrough

---

## Context

UMS command handlers need structured cross-cutting concerns: **entry/exit logging**with duration, **distributed tracing**with tenant tags, **metrics** (RED signals), and**exception capture**. These concerns must be:

1. **Selective** — applied per-handler or per-method, not uniformly to all requests.
2. **Non-invasive** — zero changes to the handler's business logic.
3. **Async-correct** — fire hooks *after* the awaited result, not when the `Task` object is returned.
4. **Testable in isolation** — handlers unit-tested without cross-cutting infrastructure.

UMS already uses MediatR `IPipelineBehavior<TRequest, TResponse>` for**uniform**pipeline concerns (`ValidationBehavior`). The question is whether to extend that mechanism for cross-cutting concerns or adopt a different model.

### Alternatives considered

| Option | Mechanism | Selective? | Async-correct? | External dep? | Decision |
|---|---|---|---|---|---|
| **A** | MediatR `IPipelineBehavior<,>` | all-or-nothing per type | | | Rejected for cross-cutting |
| **B** | Decorator classes per handler | manual | | | Rejected — O(n) boilerplate |
| **C** | Castle.DynamicProxy / Autofac interceptors | | | new NuGet required | Rejected — stack pollution |
| **D** | `BeyondNetCode.Shell.Aop` + `System.Reflection.DispatchProxy` | attribute-driven | (after fix) | owned shell lib | **Adopted** | #### Why MediatR `IPipelineBehavior` was not sufficient

`IPipelineBehavior<TRequest, TResponse>` applies to every command that matches its type constraint. This is the right model for**uniform**concerns (validation, idempotency) but creates unacceptable coupling for**selective**concerns:

- A logging behavior for `CreateTenantCommand` would need type-specific conditions or separate behavior registrations per request type.
- Conditional behavior logic (`if request is X then log, else skip`) is an anti-pattern that defeats the purpose of the pipeline abstraction.
- MediatR behaviors run inside a single request scope — they cannot distinguish between a handler that should emit Serilog structured logs versus one that should emit only MEL Debug logs.

**Resolution:**MediatR behaviors remain the canonical mechanism for uniform pipeline concerns. `BeyondNetCode.Shell.Aop` is the canonical mechanism for selective, per-method decoration.

#### Why `BeyondNetCode.Shell.Aop` was chosen over a new NuGet dependency

- `BeyondNetCode.Shell.Aop` is an**owned**shell library — no external package management, no upstream breaking changes, no additional CVE surface.
- The library already implements `DispatchProxy`, `AspectExecutor`, `PointCut`, `IAspect` chain, `OnMethodBoundaryAspect`, `LoggerAspect`, `RetryAspect`, and `AdviceAspect`.
- DI integration via `AddAop()` + `AddAopProxy<TService, TImpl>()` is already built and tested.
- The only missing capability was async-correctness (see below).

---

## Decision**Adopt `BeyondNetCode.Shell.Aop` with `System.Reflection.DispatchProxy` as the mechanism for selective, per-method cross-cutting concerns in UMS command handlers.**

### 1. Separation of responsibilities

| Concern | Mechanism | Applies to |
|---|---|---|
| Input validation | `ValidationBehavior` (MediatR) | All commands uniformly |
| Idempotency | `IdempotencyMiddleware` (HTTP) | All mutating endpoints |
| Logging (selective) | `LoggerAspect` via `BeyondNetCode.Shell.Aop` | Per-handler, opt-in via `[LoggerAspect]` |
| Tracing (Phase 2) | `TracingAspect` via `BeyondNetCode.Shell.Aop` | Per-handler, opt-in via `[Tracing]` |
| Metrics (Phase 2) | `MetricsAspect` via `BeyondNetCode.Shell.Aop` | Per-handler, opt-in via `[Metrics]` |
| Retry (selective) | `RetryAspect` via `BeyondNetCode.Shell.Aop` | Per-method, opt-in via `[RetryAspect]` | ### 2. Async proxy fix — mandatory prerequisite

`System.Reflection.DispatchProxy.Invoke` is synchronous. Prior to this ADR, `OnMethodBoundaryAspect.OnSuccess` and `OnExit` fired when a `Task` was *returned*, not when it *completed*. This caused hooks to observe incomplete state.

**Fix (implemented in `BeyondNetCode.Shell.Aop/Impl/OnMethodBoundaryAspect.cs`):**After `joinPoint.Proceed()`, detect `Task` / `Task<TResult>` return types and wrap them in continuation tasks via `ConfigureAwait(false)`. The original `Task<TResult>` path is handled via a cached generic `MethodInfo` (`WrapAsyncOfT<TResult>`) to preserve the result value. The synchronous `finally { OnExit() }` block is skipped for async paths to prevent double-firing.

### 3. Adoption scope

#### Phase 1 (implemented — 2026-05-24)

- `[LoggerAspect(Type = typeof(IMelLogger), LogDuration = true, LogException = true)]` on `CreateTenantCommandHandler.Handle`
- `MelLogger` registered as keyed `ILogger` (AOP) under `typeof(IMelLogger)` in Infrastructure DI
- `IMelLogger` marker interface in `Ums.Application.Common.Aop` — decouples Application layer from Infrastructure concrete type
- `AddAopProxy<IRequestHandler<CreateTenantCommand, Result<CreateTenantResponse>>, CreateTenantCommandHandler>()` in `Infrastructure.DependencyInjection`

#### Phase 2 (planned)

- `TracingAspect` implementing `ActivitySource.StartActivity()` with `tenant_id` tag (aligns with ADR-0053)
- `MetricsAspect` implementing `Histogram<long>` via `IMeterFactory`
- Expand `AddAopProxy<>` to all command handlers in Identity and Authorization bounded contexts
- Aspect ordering: Tracing(10) → Logging(50) → Metrics(60)

#### Phase 3 (future consideration)

- `RetryAspect` on Infrastructure services that call external IdP endpoints
- `AdviceAspect` for domain-specific audit hooks

### 4. PII policy for logging aspects

| Logger | Argument values logged | When to use |
|---|---|---|
| `MelLogger` | Never — names/types only | Default; all handlers |
| `SerilogLogger` | Destructured (opt-in) | Only after explicit PII review and approval | `[LoggerAspect(LogArguments = [])]` (empty array) is the PII-safe default and must be set on all handlers unless a specific argument is reviewed and cleared.

### 5. Layer references introduced

```
Ums.Application.csproj
 └── BeyondNetCode.Shell.Aspects ← attribute contract only ([LoggerAspect], etc.)

Ums.Infrastructure.csproj
 ├── BeyondNetCode.Shell.DI ← AddAop(), AddAopProxy<>()
 └── BeyondNetCode.Shell.Logger.Serilog ← SerilogLogger adapter
```

`Ums.Domain` does**not**reference any `BeyondNetCode.Shell.Aop` project. Domain purity is preserved.

---

## Consequences

### Positive

- Handlers remain pure business logic — no logging or telemetry imports in Application layer code.
- Cross-cutting concerns are applied selectively without modifying the MediatR pipeline for all handlers.
- `[LoggerAspect]` + `[Tracing]` attributes make the concern decoration visible and searchable in code review.
- Async-correct hooks fire after real completion, not after Task object creation — logs and metrics are accurate.
- `MelLogger` bridges AOP's custom `ILogger` interface to `Microsoft.Extensions.Logging` — no custom Serilog sink required in Application.
- The same proxy mechanism applies to any DI-registered interface, not just MediatR handlers — repositories, domain services, and IdP gateways can be decorated with identical patterns.

### Trade-offs

- `System.Reflection.DispatchProxy` requires the service to be an**interface** (or abstract class). Concrete class proxying is not supported — this is enforced by `AddAopProxy<TService, TImpl>()`.
- `DispatchProxy.Invoke` is intrinsically synchronous; the async continuation wrapper adds minor overhead (~1 allocation per async method call).
- `PointCut` caches `(MethodInfo, Type) → bool` per proxy type — cache grows proportionally with the number of proxied methods; negligible in practice.
- MediatR's `RegisterServicesFromAssembly` registers handlers before `AddAopProxy<>` — callers must ensure `AddAopProxy<>` is called**after**MediatR registration so the proxy wins the last-registration-wins resolution.
- Singleton proxies are explicitly prohibited (`AddAopProxy` throws `ArgumentException` for `ServiceLifetime.Singleton`) because aspects may resolve scoped services (e.g., `IUserContext`).

### Non-decisions

- **Compile-time weaving** (e.g., PostSharp, Fody) was not evaluated. UMS currently has no build-time weaving infrastructure; the added complexity is not justified at current scale.
- **Castle.DynamicProxy** / **Autofac interceptors**remain available as future alternatives if `DispatchProxy`'s interface constraint becomes limiting.

---

## Compliance

The following checks are mandatory after any change to an AOP aspect or AOP proxy registration:

```bash
# Build the full solution
dotnet build src/apps/ums.api/Ums.sln

# Run all test suites
dotnet test src/apps/ums.api/Ums.sln --verbosity minimal
dotnet test src/libs/shell/aop/src/BeyondNetCode.Shell.Aop.Tests/BeyondNetCode.Shell.Aop.Tests.csproj --verbosity minimal

# Verify no Domain purity violation
grep -r "BeyondNetCode.Shell.Aop" src/apps/ums.api/Ums.Domain/ --include="*.csproj"
# Expected: no output
```

---

**[ADR Registry](./index.md)** | **[AOP Developer Guide](../shell-libraries/aop.md)** | **[ADR-0053 OpenTelemetry](./0053-opentelemetry-observability.md)** | **[ADR-0054 Shell Isolation](./0054-shell-library-isolation.md)**
