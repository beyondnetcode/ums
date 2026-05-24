# Ums.Shell.Factory — Developer Guide

> **Part of:** [Shell Libraries](README.md)  
> **Projects:** `Ums.Shell.Factory` · `Ums.Shell.Factory.Installer`  
> **Dependencies:** None (pure BCL)

`Ums.Shell.Factory` is a **selector-based abstract factory** with a fluent DSL. Given a context object (the "target") and a service interface, it evaluates a set of registered predicate rules and instantiates the subset of implementations whose `When(...)` condition matches.

---

## Table of Contents

1. [When to Use](#1-when-to-use)
2. [Project Structure](#2-project-structure)
3. [Core Concepts](#3-core-concepts)
4. [Standalone Usage](#4-standalone-usage)
5. [DI Usage with `AddFactory()`](#5-di-usage-with-addfactory)
6. [Named Groups](#6-named-groups)
7. [Interceptor Hooks](#7-interceptor-hooks)
8. [API Reference](#8-api-reference)
9. [UMS Integration Examples](#9-ums-integration-examples)
10. [FAQ](#10-faq)

---

## 1. When to Use

Use `Ums.Shell.Factory` when:

- You need to pick a different **strategy / handler / loader** based on the runtime state of a domain object.
- The set of implementations is **open-closed** — new ones can be added without modifying the factory.
- The selection logic is a **simple predicate** on the context (no complex graphs, no scope management).

Do **not** use it when:
- You need only one fixed implementation (just use DI directly).
- Selection requires complex orchestration (prefer MediatR pipeline or the `IStrategyPattern` from Application layer).
- You need parent–child scoping (use `IServiceScopeFactory` from DI instead).

---

## 2. Project Structure

```
Ums.Shell.Factory/
├── Interface/
│   ├── IFactory.cs                ← main entry point
│   ├── IFactoryCreator.cs         ← instantiation delegate
│   ├── IFactoryInterceptor.cs     ← lifecycle hooks
│   ├── IFactorySetupProvider.cs   ← aggregates all sources
│   ├── IFactorySetupSource.cs     ← one configuration unit
│   └── IFactoryBuilder.cs         ← DI builder (Installer only)
├── Fluent/
│   └── Interface/
│       ├── IFactoryRecordSetupCreateBuilder.cs   ← .Create<TImpl>()
│       ├── IFactoryRecordSetupWhenBuilder.cs     ← .When(predicate)
│       └── IFactoryRecordSetupGroupCreateBuilder.cs ← named groups
├── Impl/
│   ├── AbstractFactorySetupSource.cs  ← base class for your setup sources
│   ├── AbstractFactoryInterceptor.cs  ← base for custom interceptors
│   ├── Factory.cs                     ← IFactory implementation
│   ├── FactoryCreator.cs              ← Func<Type,object> wrapper
│   └── FactorySetupProvider.cs        ← predicate filter engine
└── Model/
    ├── Setup.cs                        ← List<SetupItem>
    └── SetupItem.cs                    ← (TargetType, ImplType, ServiceType, Name, Selector)

Ums.Shell.Factory.Installer/
├── Extensions/
│   ├── ServiceCollectionExtensions.cs ← AddFactory(Action<IFactoryBuilder>?)
│   └── ServiceProviderExtensions.cs   ← GetFactory()
└── Impl/
    └── FactoryBuilder.cs              ← IFactoryBuilder implementation
```

---

## 3. Core Concepts

### Setup Source — the rule book

Extend `AbstractFactorySetupSource` and declare rules in its constructor using the fluent DSL:

```
For<TTarget, TService>()
    .Create<TImplementation>()
    .When(target => <predicate>);
```

### Factory — the dispatcher

`IFactory.Create<TTarget, TService>(target)` evaluates all registered rules against `target` and returns a `TService[]` of instantiated implementations whose predicate returned `true`.

### FactoryCreator — the instantiator

`IFactoryCreator` wraps a `Func<Type, object>`. In production this is `sp.GetRequiredService`; in tests it is `Activator.CreateInstance`.

### Execution flow

```
IFactory.Create<TTarget, TService>(target, name?)
    ↓
IFactoryInterceptor.OnEntry(target, name)
    ↓
IFactorySetupProvider.Provide<TTarget, TService>(target, name)  ← filter by predicate
    ↓ [per matching SetupItem]
IFactoryCreator.Create<TService>(implType)                       ← instantiate
    ↓
IFactoryInterceptor.OnSuccess(target, name, services)
    ↓
return TService[]
```

---

## 4. Standalone Usage

No DI required. Wire everything manually (perfect for unit tests or console tools).

### Step 1 — Define a setup source

```csharp
using Ums.Shell.Factory.Impl;

// Domain context
public record Discount(int CustomerAge, bool IsPremium);

// Service interface
public interface IDiscountStrategy { decimal Apply(decimal price); }

// Implementations
public class SeniorDiscount   : IDiscountStrategy { public decimal Apply(decimal p) => p * 0.80m; }
public class PremiumDiscount  : IDiscountStrategy { public decimal Apply(decimal p) => p * 0.85m; }
public class StandardDiscount : IDiscountStrategy { public decimal Apply(decimal p) => p * 0.95m; }

// Setup source — rules declared in constructor
public class DiscountFactorySetup : AbstractFactorySetupSource
{
    public DiscountFactorySetup()
    {
        For<Discount, IDiscountStrategy>()
            .Create<SeniorDiscount>()
            .When(d => d.CustomerAge >= 65);

        For<Discount, IDiscountStrategy>()
            .Create<PremiumDiscount>()
            .When(d => d.IsPremium);

        For<Discount, IDiscountStrategy>()
            .Create<StandardDiscount>()
            .When(d => d.CustomerAge < 65 && !d.IsPremium);
    }
}
```

### Step 2 — Build the factory

```csharp
using Ums.Shell.Factory.Impl;
using Ums.Shell.Factory.Interfaces;

// FactoryCreator uses Activator for zero-DI scenarios
var creator = new FactoryCreator(
    type => Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Cannot create {type.Name}"));

var provider = new FactorySetupProvider(
    new IFactorySetupSource[] { new DiscountFactorySetup() });

IFactory factory = new Factory(provider, creator);
```

### Step 3 — Use the factory

```csharp
var senior  = new Discount(CustomerAge: 70, IsPremium: false);
var premium = new Discount(CustomerAge: 30, IsPremium: true);
var regular = new Discount(CustomerAge: 40, IsPremium: false);

// Senior customer: SeniorDiscount fires
var strategies = factory.Create<Discount, IDiscountStrategy>(senior);
// strategies.Length == 1  →  SeniorDiscount
var finalPrice = strategies[0].Apply(100m);  // 80m

// Premium customer: PremiumDiscount fires
var ps = factory.Create<Discount, IDiscountStrategy>(premium);
// ps.Length == 1  →  PremiumDiscount
ps[0].Apply(100m); // 85m

// Regular customer: StandardDiscount fires
var rs = factory.Create<Discount, IDiscountStrategy>(regular);
// rs.Length == 1  →  StandardDiscount
```

> **Multiple matches are intentional.** If a senior is also premium, both `SeniorDiscount` AND `PremiumDiscount` are returned. `Create(...)` returns `TService[]` — the caller decides which to apply.

---

## 5. DI Usage with `AddFactory()`

```csharp
// Program.cs / DependencyInjection.cs
services.AddFactory(builder =>
{
    // Register your setup sources (where the rules live)
    builder.AddSource<DiscountFactorySetup>();

    // Register all concrete implementations that the factory may create
    builder.AddTransient<IDiscountStrategy, SeniorDiscount>();
    builder.AddTransient<IDiscountStrategy, PremiumDiscount>();
    builder.AddTransient<IDiscountStrategy, StandardDiscount>();
});

// Inject and use
public class PriceService(IFactory factory)
{
    public decimal CalculateFinalPrice(Discount discount, decimal basePrice)
    {
        var strategies = factory.Create<Discount, IDiscountStrategy>(discount);

        return strategies.Length == 0
            ? basePrice
            : strategies.Aggregate(basePrice, (price, s) => s.Apply(price));
    }
}
```

`IFactory`, `IFactorySetupProvider`, and `IFactoryCreator` are all registered as singletons by `AddFactory()`.

---

## 6. Named Groups

When you need multiple independent factory groups for the same `(TTarget, TService)` pair, use named groups:

```csharp
public class OrderFulfillmentSetup : AbstractFactorySetupSource
{
    public OrderFulfillmentSetup()
    {
        // Group "primary"
        For<Order, IFulfillmentChannel>("primary", group =>
        {
            group.Create<EmailFulfillment>().When(o => o.Props.HasEmail);
            group.Create<SmsFulfillment>().When(o => o.Props.HasPhone);
        });

        // Group "backup"
        For<Order, IFulfillmentChannel>("backup", group =>
        {
            group.Create<PostalFulfillment>().When(o => o.Props.HasAddress);
        });
    }
}

// Resolve by name
var primary = factory.Create<Order, IFulfillmentChannel>(order, "primary");
var backup  = factory.Create<Order, IFulfillmentChannel>(order, "backup");
```

---

## 7. Interceptor Hooks

`IFactoryInterceptor` provides lifecycle hooks useful for logging, metrics, or tracing.

```csharp
public class FactoryLoggingInterceptor(ILogger<FactoryLoggingInterceptor> logger)
    : AbstractFactoryInterceptor
{
    public override void OnEntry<TTarget>(TTarget target, string name)
        => logger.LogDebug("Factory.Create {Name} for {Target}", name, typeof(TTarget).Name);

    public override void OnSuccess<TTarget, TService>(TTarget target, string name, IList<TService> services)
        => logger.LogDebug("Factory resolved {Count} {Service}", services.Count, typeof(TService).Name);

    public override void OnError<TTarget, TService>(TTarget target, string name, IList<TService> services, Exception ex)
        => logger.LogError(ex, "Factory.Create failed for {Target}/{Name}", typeof(TTarget).Name, name);

    public override void OnExit<TTarget, TService>(TTarget target, string name, IList<TService> services)
        => logger.LogDebug("Factory.Create completed");
}

// Wire via DI
services.AddFactory();
services.AddSingleton<IFactoryInterceptor, FactoryLoggingInterceptor>();

// Or set directly on the factory (standalone mode)
factory.Interceptor = new FactoryLoggingInterceptor(logger);
```

---

## 8. API Reference

### `IFactory`

| Method | Description |
|---|---|
| `Create<TTarget, TService>(target)` | Evaluate all rules and instantiate matching implementations |
| `Create<TTarget, TService>(target, name)` | Same, filtered to the named group |
| `ConfigurationFor<TTarget, TService>(target)` | Return matching `SetupItem[]` without instantiating |
| `ConfigurationFor<TTarget, TService>(target, name)` | Same, named group |
| `IFactoryInterceptor Interceptor { get; set; }` | Lifecycle hook (default: no-op) |

### `AbstractFactorySetupSource`

| Method | Description |
|---|---|
| `For<TTarget, TService>()` | Start a single rule chain → `.Create<TImpl>().When(pred)` |
| `For<TTarget, TService>(name, action)` | Start a named-group rule chain |

### `IFactoryCreator`

| Method | Description |
|---|---|
| `T Create<T>(Type type)` | Instantiate `type` and cast to `T`; throws on null or wrong type |

### `ServiceCollectionExtensions.AddFactory()`

Registers: `IFactoryCreator` (backed by `sp.GetRequiredService`), `IFactory`, `IFactorySetupProvider`.

```csharp
services.AddFactory(builder =>
{
    builder.AddSource<MySetupSource>();           // registers IFactorySetupSource
    builder.AddSingleton<IMyService, ImplA>();    // registers ImplA
    builder.AddTransient<IMyService, ImplB>();    // registers ImplB
});
```

---

## 9. UMS Integration Examples

### Fulfillment strategy (Domain → Infrastructure)

```csharp
// In a setup source registered in Ums.Infrastructure/DependencyInjection.cs
public class FulfillmentFactorySetup : AbstractFactorySetupSource
{
    public FulfillmentFactorySetup()
    {
        For<Tenant, IProvisioningStrategy>()
            .Create<InternalProvisioningStrategy>()
            .When(t => t.Props.OrganizationType == OrganizationType.INTERNAL);

        For<Tenant, IProvisioningStrategy>()
            .Create<ExternalProvisioningStrategy>()
            .When(t => t.Props.OrganizationType == OrganizationType.EXTERNAL);
    }
}

// In a command handler
public class ProvisionTenantCommandHandler(IFactory factory, ITenantRepository repo)
{
    public async Task<Result> Handle(ProvisionTenantCommand cmd, CancellationToken ct)
    {
        var tenant = await repo.GetByIdAsync(cmd.TenantId, ct);
        var strategies = factory.Create<Tenant, IProvisioningStrategy>(tenant!);

        foreach (var strategy in strategies)
            await strategy.ProvisionAsync(tenant!, ct);

        return Result.Success();
    }
}
```

### Approval routing

```csharp
public class ApprovalRouteSetup : AbstractFactorySetupSource
{
    public ApprovalRouteSetup()
    {
        For<ApprovalRequest, IApprovalRouter>()
            .Create<ManagerApprovalRouter>()
            .When(r => r.Props.RiskScore < 50);

        For<ApprovalRequest, IApprovalRouter>()
            .Create<CommitteeApprovalRouter>()
            .When(r => r.Props.RiskScore >= 50 && r.Props.RiskScore < 80);

        For<ApprovalRequest, IApprovalRouter>()
            .Create<BoardApprovalRouter>()
            .When(r => r.Props.RiskScore >= 80);
    }
}
```

---

## 10. FAQ

**Q: What happens if no predicate matches?**  
`Create(...)` returns an empty array `TService[0]`. Always check `.Length > 0` before using the result.

**Q: Are implementations shared or created fresh each time?**  
Controlled entirely by DI lifetime. `AddSingleton` → shared; `AddTransient` → fresh per call.

**Q: Can predicates be async?**  
No. Predicates are synchronous `Func<TTarget, bool>`. For async conditions, resolve the result before calling the factory.

**Q: Can I have multiple setup sources?**  
Yes. All sources registered via `AddSource<>()` are merged into a single `FactorySetupProvider`.

---

## Related Docs

- [DDD](ddd.md) — domain objects used as `TTarget`
- [AOP](aop.md) — add logging to factory-resolved services with `[LoggerAspect]`
- [Combined Usage](combined-usage.md) — Factory + DDD + AOP + Bootstrapper in one example
