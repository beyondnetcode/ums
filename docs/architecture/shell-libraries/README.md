# UMS Shell Libraries — Architecture Reference

> **Status:** Production-ready · Actively adopted  
> **Last reviewed:** 2026-05-24  
> **Owner:** Architecture team

Shell libraries are internal .NET libraries shared across all UMS bounded contexts. They live in `src/libs/shell/` and are consumed as project references — never as NuGet packages.

---

## Library Catalogue

| Library | Projects | Purpose | Status in UMS API |
|---|---|---|---|
| [`Ums.Shell.Ddd`](ddd.md) | `Ums.Shell.Ddd` · `Ums.Shell.Ddd.ValueObjects` | DDD base types: Entity, AggregateRoot, ValueObject, DomainEvent, BrokenRules | ✅ **Core foundation** — every aggregate extends it |
| [`Ums.Shell.Factory`](factory.md) | `Ums.Shell.Factory` · `Ums.Shell.Factory.Installer` | Selector-based abstract factory with fluent DSL | ✅ **Active** — transitive via Ums.Shell.Ddd; available to all layers |
| [`Ums.Shell.Aop`](aop.md) | `Ums.Shell.Aop` · `Ums.Shell.Aop.DispatchProxy` · `Ums.Shell.Aop.Aspects` · `Ums.Shell.Aop.Aspects.Logger.Serilog` · `Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer` | DispatchProxy-based AOP: logging, retry, advice aspects | ✅ **Active** — wired to `CreateTenantCommandHandler`; expand to other handlers |
| [`Ums.Shell.Bootstrapper`](bootstrapper.md) | `Ums.Shell.Bootstrapper` · `Ums.Shell.Bootstrapper.DependencyInjection` · `Ums.Shell.Bootstrapper.AutoMapper` · `Ums.Shell.Bootstrapper.Observability` | Composable initialization pipeline (Composite pattern) | 🔵 **Available** — use for complex multi-step startup chains |

---

## Physical Layout

```
src/libs/shell/
├── ddd/
│   └── src/
│       ├── Ums.Shell.Ddd/                   ← Entity, AggregateRoot, ValueObject, DomainEvent, DomainEnumeration
│       ├── Ums.Shell.Ddd.ValueObjects/      ← AuditValueObject, StringValueObject, IntValueObject, BoolValueObject, DecimalValueObject
│       └── Ums.Shell.Ddd.Test/
├── factory/
│   └── src/
│       ├── Ums.Shell.Factory/               ← IFactory, AbstractFactorySetupSource, fluent DSL
│       ├── Ums.Shell.Factory.Installer/     ← AddFactory() DI extension
│       ├── Ums.Shell.Factory.Test/
│       └── Ums.Shell.Factory.Demo/
├── aop/
│   └── src/
│       ├── Ums.Shell.Aop/                   ← IAspect, IJoinPoint, IPointCut, OnMethodBoundaryAspect, OnRetryAspect
│       ├── Ums.Shell.Aop.DispatchProxy/     ← AopProxy<TService,TImpl>, AopProxyCreator
│       ├── Ums.Shell.Aop.Aspects/           ← LoggerAspect, AdviceAspect, RetryAspect + attributes
│       ├── Ums.Shell.Aop.Aspects.Logger/    ← ISerializer, SensitiveDataResolver
│       ├── Ums.Shell.Aop.Aspects.Logger.Serilog/ ← SerilogLogger : ILogger (AOP)
│       ├── Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer/
│       └── Ums.Shell.Aop.Tests/
└── bootstrapper/
    └── src/
        ├── Ums.Shell.Bootstrapper/          ← IBootstrapper, IBootstrapperAsync, CompositeBootstrapper
        ├── Ums.Shell.Bootstrapper.DependencyInjection/  ← DependencyInjectionBootstrapper
        ├── Ums.Shell.Bootstrapper.AutoMapper/            ← AutoMapperBootstrapper
        ├── Ums.Shell.Bootstrapper.Observability/         ← ObservabilityBootstrapper + ObservabilityConfiguration
        └── Ums.Shell.Bootstrapper.Tests/
```

---

## Dependency Graph

```
Ums.Domain
  └── Ums.Shell.Ddd              ← Entity, AggregateRoot
        └── Ums.Shell.Factory    ← IFactory (transitive)
              └── Ums.Shell.Ddd.ValueObjects  ← AuditValueObject

Ums.Application
  ├── Ums.Domain (above)
  └── Ums.Shell.Aop.Aspects      ← [LoggerAspect], [RetryAspect] attribute contract

Ums.Infrastructure
  ├── Ums.Application (above)
  ├── Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer ← AddAop(), AddAopProxy<>()
  └── Ums.Shell.Aop.Aspects.Logger.Serilog ← SerilogLogger
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

A complete end-to-end example combining all four libraries is in **[combined-usage.md](combined-usage.md)**.

---

## Per-Library Guides

| Document | Contents |
|---|---|
| [ddd.md](ddd.md) | Entity · AggregateRoot · ValueObject · DomainEvent · DomainEnumeration · BrokenRules · TrackingState |
| [factory.md](factory.md) | AbstractFactorySetupSource · fluent DSL · IFactoryInterceptor · DI wiring · UMS patterns |
| [aop.md](aop.md) | IAspect chain · OnMethodBoundaryAspect · LoggerAspect · async proxy gap fix · MelLogger · DI wiring |
| [bootstrapper.md](bootstrapper.md) | IBootstrapper · CompositeBootstrapper · DI/AutoMapper/Observability bootstrappers |
| [combined-usage.md](combined-usage.md) | Full example: Fulfillment aggregate + factory routing + AOP logging + bootstrapped startup |

---

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
| 70 | Transaction | Outermost retry wrapper |

Implement `IAspect.GetOrder(IJoinPoint)` in each aspect to return the appropriate constant.

---

## Cross-Cutting Policies

| Concern | Mechanism | File |
|---|---|---|
| PII in logs | `MelLogger`: log parameter names/types only, never values | `Ums.Infrastructure/Aop/MelLogger.cs` |
| PII in Serilog | `SensitiveDataResolver` + `[Sensitive]` attribute | `Ums.Shell.Aop.Aspects.Logger/` |
| Domain purity | `Ums.Domain.csproj` has zero NuGet references | AGENTS.md |
| Factory ordering | `FactorySetupProvider` evaluates predicates in declaration order | `Ums.Shell.Factory/Impl/FactorySetupProvider.cs` |
| Async aspects | `OnMethodBoundaryAspect` unwraps `Task`/`Task<T>` via continuation | `Ums.Shell.Aop/Impl/OnMethodBoundaryAspect.cs` |

---

## Validation Checklist

Run these before any PR that touches a shell library:

```bash
# From repo root
dotnet build src/apps/ums.api/Ums.sln
dotnet test  src/apps/ums.api/Ums.sln --verbosity minimal

dotnet test src/libs/shell/aop/src/Ums.Shell.Aop.Tests/Ums.Shell.Aop.Tests.csproj --verbosity minimal
dotnet test src/libs/shell/factory/src/Ums.Shell.Factory.Test/Ums.Shell.Factory.Test.csproj --verbosity minimal
dotnet test src/libs/shell/ddd/src/Ums.Shell.Ddd.Test/Ums.Shell.Ddd.Test.csproj --verbosity minimal
dotnet test src/libs/shell/bootstrapper/src/Ums.Shell.Bootstrapper.Tests/Ums.Shell.Bootstrapper.Tests.csproj --verbosity minimal
```

Expected: **0 build errors**, **0 test failures** across all suites.
