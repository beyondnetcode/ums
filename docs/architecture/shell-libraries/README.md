# UMS Shell Libraries — Architecture Reference

> **Status:**Production-ready · Actively adopted
> **Last reviewed:**2026-05-24
> **Owner:**Architecture team

Shell libraries are internal .NET libraries shared across all UMS bounded contexts. They live in `src/libs/shell/` and are consumed as project references — never as NuGet packages.

---

## Library Catalogue

| Library | Projects | Purpose | Status in UMS API |
|---|---|---|---|
| [`BeyondNetCode.Shell.Ddd`](ddd.md) | `BeyondNetCode.Shell.Ddd` · `BeyondNetCode.Shell.Ddd.ValueObjects` | DDD base types: Entity, AggregateRoot, ValueObject, DomainEvent, BrokenRules | **Core foundation** — every aggregate extends it |
| [`BeyondNetCode.Shell.Factory`](factory.md) | `BeyondNetCode.Shell.Factory` · `BeyondNetCode.Shell.DI` | Selector-based abstract factory with fluent DSL | **Active** — transitive via BeyondNetCode.Shell.Ddd; available to all layers |
| [`BeyondNetCode.Shell.Aop`](aop.md) | `BeyondNetCode.Shell.Aop` · `BeyondNetCode.Shell.DispatchProxy` · `BeyondNetCode.Shell.Aspects` · `BeyondNetCode.Shell.Logger.Serilog` · `BeyondNetCode.Shell.DI` | DispatchProxy-based AOP: logging, retry, advice aspects | **Active** — wired to `CreateTenantCommandHandler`; expand to other handlers |
| [`BeyondNetCode.Shell.Bootstrapper`](bootstrapper.md) | `BeyondNetCode.Shell.Bootstrapper` · `BeyondNetCode.Shell.DI` · `BeyondNetCode.Shell.AutoMapper` · `BeyondNetCode.Shell.Observability` | Composable initialization pipeline (Composite pattern) | **Available** — use for complex multi-step startup chains | ---

## Physical Layout

```
src/libs/shell/
├── ddd/
│ └── src/
│ ├── BeyondNetCode.Shell.Ddd/ ← Entity, AggregateRoot, ValueObject, DomainEvent, DomainEnumeration
│ ├── BeyondNetCode.Shell.Ddd.ValueObjects/ ← AuditValueObject, StringValueObject, IntValueObject, BoolValueObject, DecimalValueObject
│ └── BeyondNetCode.Shell.Ddd.Test/
├── factory/
│ └── src/
│ ├── BeyondNetCode.Shell.Factory/ ← IFactory, AbstractFactorySetupSource, fluent DSL
│ ├── BeyondNetCode.Shell.DI/ ← AddFactory() DI extension
│ ├── BeyondNetCode.Shell.Factory.Test/
│ └── BeyondNetCode.Shell.Factory.Demo/
├── aop/
│ └── src/
│ ├── BeyondNetCode.Shell.Aop/ ← IAspect, IJoinPoint, IPointCut, OnMethodBoundaryAspect, OnRetryAspect
│ ├── BeyondNetCode.Shell.DispatchProxy/ ← AopProxy<TService,TImpl>, AopProxyCreator
│ ├── BeyondNetCode.Shell.Aspects/ ← LoggerAspect, AdviceAspect, RetryAspect + attributes
│ ├── BeyondNetCode.Shell.Aspects.Logger/ ← ISerializer, SensitiveDataResolver
│ ├── BeyondNetCode.Shell.Logger.Serilog/ ← SerilogLogger : ILogger (AOP)
│ ├── BeyondNetCode.Shell.DI/
│ └── BeyondNetCode.Shell.Aop.Tests/
└── bootstrapper/
 └── src/
 ├── BeyondNetCode.Shell.Bootstrapper/ ← IBootstrapper, IBootstrapperAsync, CompositeBootstrapper
 ├── BeyondNetCode.Shell.DI/ ← DependencyInjectionBootstrapper
 ├── BeyondNetCode.Shell.AutoMapper/ ← AutoMapperBootstrapper
 ├── BeyondNetCode.Shell.Observability/ ← ObservabilityBootstrapper + ObservabilityConfiguration
 └── BeyondNetCode.Shell.Bootstrapper.Tests/
```

---

## Dependency Graph

```
Ums.Domain
 └── BeyondNetCode.Shell.Ddd ← Entity, AggregateRoot
 └── BeyondNetCode.Shell.Factory ← IFactory (transitive)
 └── BeyondNetCode.Shell.Ddd.ValueObjects ← AuditValueObject

Ums.Application
 ├── Ums.Domain (above)
 └── BeyondNetCode.Shell.Aspects ← [LoggerAspect], [RetryAspect] attribute contract

Ums.Infrastructure
 ├── Ums.Application (above)
 ├── BeyondNetCode.Shell.DI ← AddAop(), AddAopProxy<>()
 └── BeyondNetCode.Shell.Logger.Serilog ← SerilogLogger
```

---

## How They Work Together — 30-Second View

```csharp
// 1. DDD: define your aggregate
public class Order : AggregateRoot<Order, OrderProps> { ... }

// 2. Factory: dispatch to correct handler based on runtime state
For<Order, IFulfillmentStrategy>()
 .Create<DigitalFulfillment>().When(o => o.Props.IsDigital);

// 3. AOP: add cross-cutting logging to the command handler
[LoggerAspect(Type = typeof(IMelLogger), LogDuration = true)]
public async Task<Result<OrderId>> Handle(PlaceOrderCommand cmd, CancellationToken ct) { ... }

// 4. Bootstrapper: compose multi-step startup
new CompositeBootstrapper()
 .Add(new DependencyInjectionBootstrapper(ConfigureServices))
 .Add(new ObservabilityBootstrapper(services, obsConfig))
 .Run();
```

A complete end-to-end example combining all four libraries is in**[combined-usage.md](combined-usage.md)**.

---

## Per-Library Guides

| Document | Contents |
|---|---|
| [ddd.md](ddd.md) | Entity · AggregateRoot · ValueObject · DomainEvent · DomainEnumeration · BrokenRules · TrackingState |
| [factory.md](factory.md) | AbstractFactorySetupSource · fluent DSL · IFactoryInterceptor · DI wiring · UMS patterns |
| [aop.md](aop.md) | IAspect chain · OnMethodBoundaryAspect · LoggerAspect · async proxy gap fix · MelLogger · DI wiring |
| [bootstrapper.md](bootstrapper.md) | IBootstrapper · CompositeBootstrapper · DI/AutoMapper/Observability bootstrappers |
| [combined-usage.md](combined-usage.md) | Full example: Fulfillment aggregate + factory routing + AOP logging + bootstrapped startup | ---

## Aspect Ordering Convention

When multiple aspects apply to the same method:

| Order | Aspect | Reason |
|---|---|---|
| 10 | Tracing | Must capture the full request span |
| 20 | Authorization | Reject early before any real work |
| 30 | Validation | Domain pre-conditions |
| 40 | Idempotency | Check dedup store before execution |
| 50 | Logging (`LoggerAspect`) | Observe the real execution |
| 60 | Metrics | Record duration/success after logging |
| 70 | Transaction | Outermost retry wrapper | Implement `IAspect.GetOrder(IJoinPoint)` in each aspect to return the appropriate constant.

---

## Cross-Cutting Policies

| Concern | Mechanism | File |
|---|---|---|
| PII in logs | `MelLogger`: log parameter names/types only, never values | `Ums.Infrastructure/Aop/MelLogger.cs` |
| PII in Serilog | `SensitiveDataResolver` + `[Sensitive]` attribute | `BeyondNetCode.Shell.Aspects.Logger/` |
| Domain purity | `Ums.Domain.csproj` has zero NuGet references | AGENTS.md |
| Factory ordering | `FactorySetupProvider` evaluates predicates in declaration order | `BeyondNetCode.Shell.Factory/Impl/FactorySetupProvider.cs` |
| Async aspects | `OnMethodBoundaryAspect` unwraps `Task`/`Task<T>` via continuation | `BeyondNetCode.Shell.Aop/Impl/OnMethodBoundaryAspect.cs` | ---

## Validation Checklist

Run these before any PR to the UMS API:

```bash
# From src/apps/ums.api/
dotnet build Ums.sln
dotnet test Ums.sln --verbosity minimal
```

Shell library tests run in their own repositories at `github.com/beyondnetcode/Shell.*`.

Expected: **0 build errors**, **0 test failures**across all suites.
