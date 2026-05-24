# Shell Libraries — Combined Usage Guide

> **Part of:** [Shell Libraries](README.md)  
> **Demonstrates:** `Ums.Shell.Ddd` + `Ums.Shell.Factory` + `Ums.Shell.Aop` + `Ums.Shell.Bootstrapper` in a single coherent example

This guide walks through a self-contained vertical slice of a **Fulfillment** bounded context. The example is deliberately separate from the existing UMS Identity/Authorization contexts so it is easy to follow without prior domain knowledge.

---

## Scenario

> A fulfillment service receives orders and dispatches them via the correct channel (digital download, physical shipment, or courier pickup) based on order properties. Every command handler is logged via AOP. The entire startup is composed with bootstrappers.

---

## Table of Contents

1. [Domain Layer — DDD](#1-domain-layer--ddd)
2. [Factory Layer — Strategy Routing](#2-factory-layer--strategy-routing)
3. [Application Layer — Command Handler + AOP](#3-application-layer--command-handler--aop)
4. [Infrastructure Layer — DI Wiring + MelLogger](#4-infrastructure-layer--di-wiring--mellogger)
5. [Startup Layer — Bootstrapper](#5-startup-layer--bootstrapper)
6. [Running It](#6-running-it)
7. [How Each Library Contributes](#7-how-each-library-contributes)

---

## 1. Domain Layer — DDD

### 1.1 Value Objects

```csharp
// OrderId — wraps Guid
public class OrderId : ValueObject<Guid>
{
    public static OrderId Create() => new(Guid.NewGuid());
    public static OrderId Load(Guid id) => new(id);
    private OrderId(Guid value) : base(value) { }
    protected override IEnumerable<object> GetEqualityComponents() { yield return GetValue(); }
}

// ChannelType — domain enumeration (type-safe enum)
public abstract class ChannelType : DomainEnumeration
{
    public static readonly ChannelType Digital  = new DigitalChannel();
    public static readonly ChannelType Physical = new PhysicalChannel();
    public static readonly ChannelType Courier  = new CourierChannel();

    private ChannelType(int id, string name) : base(id, name) { }
    private class DigitalChannel  : ChannelType { public DigitalChannel()  : base(1, "DIGITAL")  { } }
    private class PhysicalChannel : ChannelType { public PhysicalChannel() : base(2, "PHYSICAL") { } }
    private class CourierChannel  : ChannelType { public CourierChannel()  : base(3, "COURIER")  { } }
}
```

### 1.2 Props + Aggregate

```csharp
// Props — immutable state snapshot
public record OrderProps(
    Guid CustomerId,
    ChannelType Channel,
    bool IsDigital,
    bool RequiresCourier,
    OrderStatus Status) : IProps
{
    public object Clone() => MemberwiseClone();
}

// Domain events
public record OrderDispatched(Guid OrderId, string Channel) : DomainEvent;
public record OrderFailed(Guid OrderId, string Reason) : DomainEvent;

// Aggregate root
public class Order : AggregateRoot<Order, OrderProps>
{
    private Order(OrderProps props) : base(props) { }

    // Factory method — the ONLY way to create an Order
    public static Result<Order> Place(Guid customerId, ChannelType channel)
    {
        if (customerId == Guid.Empty)
            return Result<Order>.Failure("CustomerId cannot be empty.");

        var order = new Order(new OrderProps(
            CustomerId:       customerId,
            Channel:          channel,
            IsDigital:        channel == ChannelType.Digital,
            RequiresCourier:  channel == ChannelType.Courier,
            Status:           OrderStatus.Pending));

        return Result<Order>.Success(order);
    }

    public Result Dispatch()
    {
        if (Props.Status != OrderStatus.Pending)
            return Result.Failure("Only pending orders can be dispatched.");

        SetProps(Props with { Status = OrderStatus.Dispatched });
        TrackingState.MarkAsDirty();
        DomainEvents.RaiseEvent(new OrderDispatched(Id.GetValue(), Props.Channel.Name));

        return Result.Success();
    }

    public override void AddValidators()
    {
        // Add domain invariant validators here if needed
    }
}
```

---

## 2. Factory Layer — Strategy Routing

The factory dynamically selects the correct `IFulfillmentStrategy` based on the runtime `Order` state.

```csharp
// Service contract
public interface IFulfillmentStrategy
{
    Task<Result> ExecuteAsync(Order order, CancellationToken ct);
}

// Implementations
public class DigitalFulfillment : IFulfillmentStrategy
{
    public async Task<Result> ExecuteAsync(Order order, CancellationToken ct)
    {
        // Generate download link, send email
        await Task.Delay(10, ct);
        return Result.Success();
    }
}

public class PhysicalFulfillment : IFulfillmentStrategy
{
    public async Task<Result> ExecuteAsync(Order order, CancellationToken ct)
    {
        // Create warehouse pick list
        await Task.Delay(10, ct);
        return Result.Success();
    }
}

public class CourierFulfillment : IFulfillmentStrategy
{
    public async Task<Result> ExecuteAsync(Order order, CancellationToken ct)
    {
        // Book courier slot
        await Task.Delay(10, ct);
        return Result.Success();
    }
}

// Setup source — rules declared once, evaluated at runtime
public class FulfillmentFactorySetup : AbstractFactorySetupSource
{
    public FulfillmentFactorySetup()
    {
        For<Order, IFulfillmentStrategy>()
            .Create<DigitalFulfillment>()
            .When(o => o.Props.IsDigital);

        For<Order, IFulfillmentStrategy>()
            .Create<PhysicalFulfillment>()
            .When(o => !o.Props.IsDigital && !o.Props.RequiresCourier);

        For<Order, IFulfillmentStrategy>()
            .Create<CourierFulfillment>()
            .When(o => o.Props.RequiresCourier);
    }
}
```

---

## 3. Application Layer — Command Handler + AOP

### 3.1 IMelLogger marker (Application — no Infrastructure coupling)

```csharp
// Application/Common/Aop/IMelLogger.cs
using Ums.Shell.Aop.Aspects;

public interface IMelLogger : ILogger;  // marker for keyed-service resolution
```

### 3.2 Command + Handler

```csharp
public record DispatchOrderCommand(Guid CustomerId, string Channel) : IRequest<Result<Guid>>;

public class DispatchOrderCommandHandler(
    IOrderRepository repo,
    IFactory          factory)
    : IRequestHandler<DispatchOrderCommand, Result<Guid>>
{
    // AOP: log entry, duration, and exceptions — via IMelLogger (no Infrastructure ref needed)
    [LoggerAspect(
        Type         = typeof(IMelLogger),
        LogDuration  = true,
        LogException = true,
        LogArguments = [])]          // PII-safe: no arg values
    public async Task<Result<Guid>> Handle(DispatchOrderCommand cmd, CancellationToken ct)
    {
        // 1. Build domain object
        var channel = DomainEnumeration.FromDisplayName<ChannelType>(cmd.Channel);
        if (channel is null)
            return Result<Guid>.Failure($"Unknown channel: {cmd.Channel}");

        var orderResult = Order.Place(cmd.CustomerId, channel);
        if (orderResult.IsFailure)
            return Result<Guid>.Failure(orderResult.Error);

        var order = orderResult.Value;

        // 2. Factory selects correct strategy at runtime
        var strategies = factory.Create<Order, IFulfillmentStrategy>(order);
        if (strategies.Length == 0)
            return Result<Guid>.Failure("No fulfillment strategy matched.");

        // 3. Execute strategy
        foreach (var strategy in strategies)
        {
            var execResult = await strategy.ExecuteAsync(order, ct);
            if (execResult.IsFailure)
                return Result<Guid>.Failure(execResult.Error);
        }

        // 4. Transition aggregate state
        var dispatchResult = order.Dispatch();
        if (dispatchResult.IsFailure)
            return Result<Guid>.Failure(dispatchResult.Error);

        // 5. Persist
        await repo.AddAsync(order, ct);
        await repo.UnitOfWork.SaveEntitiesAsync(ct);

        return Result<Guid>.Success(order.Id.GetValue());
    }
}
```

---

## 4. Infrastructure Layer — DI Wiring + MelLogger

### 4.1 MelLogger adapter

```csharp
// Infrastructure/Aop/MelLogger.cs
using AopILogger = Ums.Shell.Aop.Aspects.ILogger;

public sealed class MelLogger(ILoggerFactory loggerFactory) : IMelLogger
{
    private ILogger Logger(IJoinPoint jp) => loggerFactory.CreateLogger(jp.TargetType);

    public void OnEntry(IJoinPoint jp, Argument[] args, string requestId)
    {
        var log = Logger(jp);
        if (!log.IsEnabled(LogLevel.Debug)) return;
        log.LogDebug("Enter {Class}.{Method} [{RequestId}]",
            jp.TargetType.Name, jp.MethodInfo.Name, requestId);
    }

    public void OnExit(IJoinPoint jp, Return @return, string requestId, long duration)
        => Logger(jp).LogDebug("Exit {Class}.{Method} [{RequestId}] in {Duration}ms",
            jp.TargetType.Name, jp.MethodInfo.Name, requestId, duration);

    public void OnExit(IJoinPoint jp, string requestId, long duration)
        => Logger(jp).LogDebug("Exit {Class}.{Method} [{RequestId}] in {Duration}ms",
            jp.TargetType.Name, jp.MethodInfo.Name, requestId, duration);

    public void OnExit(IJoinPoint jp, Return @return, string requestId)
        => Logger(jp).LogDebug("Exit {Class}.{Method} [{RequestId}]",
            jp.TargetType.Name, jp.MethodInfo.Name, requestId);

    public void OnExit(IJoinPoint jp, string requestId)
        => Logger(jp).LogDebug("Exit {Class}.{Method} [{RequestId}]",
            jp.TargetType.Name, jp.MethodInfo.Name, requestId);

    public void OnException(IJoinPoint jp, string requestId, Exception ex)
        => Logger(jp).LogError(ex, "Exception in {Class}.{Method} [{RequestId}]",
            jp.TargetType.Name, jp.MethodInfo.Name, requestId);
}
```

### 4.2 Dependency Injection

```csharp
// Infrastructure/DependencyInjection.cs
public static IServiceCollection AddFulfillmentInfrastructure(
    this IServiceCollection services)
{
    // ── Repository ────────────────────────────────────────────────────
    services.AddScoped<IOrderRepository, InMemoryOrderRepository>();

    // ── Factory: strategy routing ─────────────────────────────────────
    services.AddFactory(builder =>
    {
        builder.AddSource<FulfillmentFactorySetup>();
        builder.AddTransient<IFulfillmentStrategy, DigitalFulfillment>();
        builder.AddTransient<IFulfillmentStrategy, PhysicalFulfillment>();
        builder.AddTransient<IFulfillmentStrategy, CourierFulfillment>();
    });

    // ── AOP: logging aspect ───────────────────────────────────────────
    // AddAop() registers LoggerAspect, AdviceAspect, RetryAspect, PointCut, AspectExecutor
    services.AddAop();

    // MelLogger registered under typeof(IMelLogger) — matches [LoggerAspect(Type=typeof(IMelLogger))]
    services.AddKeyedTransient<Ums.Shell.Aop.Aspects.ILogger, MelLogger>(typeof(IMelLogger));

    // Wrap the command handler: MediatR resolves the proxy (last registration wins)
    services.AddAopProxy<
        IRequestHandler<DispatchOrderCommand, Result<Guid>>,
        DispatchOrderCommandHandler>();

    return services;
}
```

---

## 5. Startup Layer — Bootstrapper

```csharp
// Program.cs (or a dedicated startup orchestrator)

// ── Phase 1: Core DI ──────────────────────────────────────────────────
var coreBootstrapper = new DependencyInjectionBootstrapper(services =>
{
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(DispatchOrderCommandHandler).Assembly));

    services.AddFulfillmentInfrastructure();
});

// ── Phase 2: Observability (Serilog + OpenTelemetry) ─────────────────
var obsConfig = new ObservabilityConfiguration
{
    OTLPEndpoint   = configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317",
    ServiceName    = "fulfillment-api",
    ServiceVersion = "1.0.0",
    ResourceAttributes = new Dictionary<string, object>
    {
        { "deployment.environment", environment.EnvironmentName }
    }
};

// ObservabilityBootstrapper needs the IServiceCollection (from phase 1's Result)
// so run phase 1 first, then feed its Result into phase 2.
coreBootstrapper.Run();
IServiceCollection populatedServices = coreBootstrapper.Result!;

new ObservabilityBootstrapper(populatedServices, obsConfig).Run();
// Serilog + OTLP tracing now configured

var app = builder.Build();
```

### Alternative: fully composable (if phases do not depend on each other's Result)

```csharp
new CompositeBootstrapper()
    .Add(new DependencyInjectionBootstrapper(builder.Services, services =>
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(...));
        services.AddFulfillmentInfrastructure();
    }))
    .Add(new ObservabilityBootstrapper(builder.Services, obsConfig))
    .Run();
```

---

## 6. Running It

```csharp
// After all bootstrappers have run and the host is built:
var mediator = app.Services.GetRequiredService<IMediator>();

var result = await mediator.Send(new DispatchOrderCommand(
    CustomerId: Guid.NewGuid(),
    Channel:    "DIGITAL"));

if (result.IsSuccess)
    Console.WriteLine($"Order {result.Value} dispatched.");
else
    Console.WriteLine($"Failed: {result.Error}");
```

**Execution trace** for `Channel = "DIGITAL"`:

```
[LoggerAspect] Enter DispatchOrderCommandHandler.Handle
  ↓ Handler.Handle
    ↓ ChannelType = DigitalChannel
    ↓ Order.Place(customerId, DigitalChannel)  ← DDD aggregate created
    ↓ factory.Create<Order, IFulfillmentStrategy>(order)
        ↓ FactorySetupProvider evaluates predicates
        ↓ o.Props.IsDigital == true → DigitalFulfillment selected
    ↓ DigitalFulfillment.ExecuteAsync(order)   ← strategy resolved by factory
    ↓ order.Dispatch()                          ← domain state transition + event raised
    ↓ repo.AddAsync(order)
    ↓ SaveEntitiesAsync()                       ← outbox pattern stores OrderDispatched
  ↑ return Result<Guid>.Success(orderId)
[LoggerAspect] Exit DispatchOrderCommandHandler.Handle in 12ms
```

---

## 7. How Each Library Contributes

| Library | Role in This Example |
|---|---|
| **Ums.Shell.Ddd** | `Order` (aggregate root), `ChannelType` (domain enumeration), `OrderDispatched` (domain event), `Result<T>` (discriminated union) |
| **Ums.Shell.Factory** | `FulfillmentFactorySetup` (predicate rules), `IFactory.Create<Order, IFulfillmentStrategy>()` (runtime dispatch), `FactoryCreator` backed by DI |
| **Ums.Shell.Aop** | `[LoggerAspect]` on `Handle`, `AopProxy<IRequestHandler<...>, DispatchOrderCommandHandler>` (DispatchProxy), `MelLogger` (MEL adapter), `OnMethodBoundaryAspect` async-aware hooks |
| **Ums.Shell.Bootstrapper** | `DependencyInjectionBootstrapper` (DI phase), `ObservabilityBootstrapper` (Serilog + OTLP), `CompositeBootstrapper` (pipeline orchestration) |

### Dependency chain

```
Program.cs
  └── CompositeBootstrapper
        ├── DependencyInjectionBootstrapper
        │     └── services.AddFulfillmentInfrastructure()
        │           ├── AddFactory(FulfillmentFactorySetup)      → IFactory
        │           └── AddAop() + AddAopProxy<..., DispatchOrderCommandHandler>
        └── ObservabilityBootstrapper (Serilog + OTLP)
              
MediatR.Send(DispatchOrderCommand)
  └── AopProxy<IRequestHandler<...>, DispatchOrderCommandHandler>.Invoke()
        └── AspectExecutor → LoggerAspect → DispatchOrderCommandHandler.Handle()
              └── Order.Place()                    [Ums.Shell.Ddd]
              └── factory.Create<Order, IFulfillmentStrategy>()  [Ums.Shell.Factory]
              └── DigitalFulfillment.ExecuteAsync() [Factory-resolved]
              └── order.Dispatch()                 [Ums.Shell.Ddd]
```

---

## Individual Library Guides

- [ddd.md](ddd.md) — Entity, AggregateRoot, ValueObject, DomainEvent, DomainEnumeration
- [factory.md](factory.md) — AbstractFactorySetupSource, fluent DSL, DI wiring
- [aop.md](aop.md) — OnMethodBoundaryAspect, LoggerAspect, async fix, MelLogger
- [bootstrapper.md](bootstrapper.md) — IBootstrapper, CompositeBootstrapper, ObservabilityBootstrapper
