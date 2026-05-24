# ADR-0051: Event Bus — Injectable Port Strategy (.NET / MassTransit)

## Status

Accepted

## Date

2026-05-15

## Context

Evolith ADR-0015 mandates an injectable messaging abstraction that decouples domain logic from any specific broker technology. UMS publishes domain events across 8 bounded contexts (identity, authorization, configuration, audit, approvals, IGA, compliance, console). Without a defined port, domain code would reference MassTransit or RabbitMQ APIs directly — violating the Hexagonal Architecture enforced by Evolith ADR-0011 and documented in UMS CP-01.

Additionally, ADR-0050 defines the CloudEvents type convention (`ums.{bounded-context}.{entity}.{past-participle}`). This ADR defines the runtime port and adapter that publishes events using that convention.

Key constraints:

- Domain layer **must not** reference any broker-specific type
- Integration tests must run without a broker (in-memory transport)
- CloudEvents 1.0 envelope is the wire format per Evolith ADR-0015 §4
- MassTransit is the preferred .NET integration bus per Evolith tech radar

---

## Decision

**Adopt `IEventBusPort` as the single injectable abstraction for all domain event publishing, implemented by a MassTransit adapter for production and an in-memory adapter for testing.**

### 1. Port Definition (Domain Layer)

```csharp
// src/UMS.Domain/Ports/IEventBusPort.cs
namespace UMS.Domain.Ports;

public interface IEventBusPort
{
    Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : IDomainEvent;
}
```

The port lives in the **Domain layer**. No reference to MassTransit, RabbitMQ, or any infrastructure type is allowed here.

### 2. CloudEvents Envelope (Infrastructure Layer)

All outbound messages are wrapped in a CloudEvents 1.0 envelope before dispatch:

```csharp
// src/UMS.Infrastructure/EventBus/CloudEventEnvelope.cs
public sealed record CloudEventEnvelope<TData>
{
    public string SpecVersion { get; } = "1.0";
    public string Id { get; } = Guid.NewGuid().ToString();
    public string Source { get; init; } = "ums";
    public string Type { get; init; }           // ums.{context}.{entity}.{past-participle}
    public string DataContentType { get; } = "application/json";
    public DateTimeOffset Time { get; } = DateTimeOffset.UtcNow;
    public TData Data { get; init; }
}
```

### 3. MassTransit Adapter (Production)

```csharp
// src/UMS.Infrastructure/EventBus/MassTransitEventBusAdapter.cs
internal sealed class MassTransitEventBusAdapter : IEventBusPort
{
    private readonly IPublishEndpoint _endpoint;
    private readonly ICloudEventTypeResolver _typeResolver;

    public MassTransitEventBusAdapter(
        IPublishEndpoint endpoint,
        ICloudEventTypeResolver typeResolver)
    {
        _endpoint = endpoint;
        _typeResolver = typeResolver;
    }

    public async Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : IDomainEvent
    {
        var cloudEventType = _typeResolver.Resolve<TEvent>();
        var envelope = new CloudEventEnvelope<TEvent>
        {
            Type = cloudEventType,
            Source = "ums",
            Data = domainEvent,
        };
        await _endpoint.Publish(envelope, ct);
    }
}
```

### 4. CloudEvent Type Resolver

```csharp
// src/UMS.Infrastructure/EventBus/CloudEventTypeResolver.cs
public interface ICloudEventTypeResolver
{
    string Resolve<TEvent>() where TEvent : IDomainEvent;
}

internal sealed class AttributeCloudEventTypeResolver : ICloudEventTypeResolver
{
    public string Resolve<TEvent>() where TEvent : IDomainEvent
    {
        var attr = typeof(TEvent).GetCustomAttribute<CloudEventTypeAttribute>()
            ?? throw new InvalidOperationException(
                $"{typeof(TEvent).Name} must declare [CloudEventType(\"ums.context.entity.verb\")]");
        return attr.Type;
    }
}

[AttributeUsage(AttributeTargets.Class)]
public sealed class CloudEventTypeAttribute : Attribute
{
    public string Type { get; }
    public CloudEventTypeAttribute(string type) => Type = type;
}
```

Usage on domain events:

```csharp
[CloudEventType("ums.identity.user.registered")]
public sealed record UserRegisteredEvent(Guid UserId, string Email, Guid TenantId) : IDomainEvent;
```

### 5. In-Memory Adapter (Tests)

```csharp
// src/UMS.Infrastructure/EventBus/InMemoryEventBusAdapter.cs
public sealed class InMemoryEventBusAdapter : IEventBusPort
{
    private readonly List<IDomainEvent> _published = new();
    public IReadOnlyList<IDomainEvent> Published => _published.AsReadOnly();

    public Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct = default)
        where TEvent : IDomainEvent
    {
        _published.Add(domainEvent);
        return Task.CompletedTask;
    }
}
```

### 6. DI Registration

```csharp
// src/UMS.Infrastructure/DependencyInjection.cs
public static IServiceCollection AddEventBus(
    this IServiceCollection services,
    IConfiguration config)
{
    services.AddSingleton<ICloudEventTypeResolver, AttributeCloudEventTypeResolver>();

    services.AddMassTransit(x =>
    {
        x.SetKebabCaseEndpointNameFormatter();
        x.UsingRabbitMq((ctx, cfg) =>
        {
            cfg.Host(config["EventBus:Host"], config["EventBus:VirtualHost"], h =>
            {
                h.Username(config["EventBus:Username"]);
                h.Password(config["EventBus:Password"]);
            });
            cfg.ConfigureEndpoints(ctx);
        });
    });

    services.AddScoped<IEventBusPort, MassTransitEventBusAdapter>();
    return services;
}
```

For test projects:

```csharp
services.AddSingleton<InMemoryEventBusAdapter>();
services.AddSingleton<IEventBusPort>(sp => sp.GetRequiredService<InMemoryEventBusAdapter>());
```

### 7. Transactional Outbox Integration

Per TE-04, `IEventBusPort` is **not** called directly from command handlers. The pattern is:

1. Command handler calls `aggregate.Raise(new UserRegisteredEvent(...))` — event stored in memory
2. `UnitOfWork.CommitAsync()` persists the entity AND the domain events to the `outbox_messages` table in the same SQL transaction
3. MassTransit Outbox processor reads `outbox_messages` and calls `IEventBusPort.PublishAsync` for each pending event
4. Marks message as processed once broker ACK is received

This guarantees exactly-once delivery under database failure.

### 8. CloudEvent Type Registry (per ADR-0050)

| Domain Event | CloudEvent type |
|---|---|
| `UserRegisteredEvent` | `ums.identity.user.registered` |
| `UserActivatedEvent` | `ums.identity.user.activated` |
| `UserBlockedEvent` | `ums.identity.user.blocked` |
| `DocumentExpiredEvent` | `ums.compliance.document.expired` |
| `PromotionRequestApprovedEvent` | `ums.iga.promotion-request.approved` |
| `ApprovalRequestApprovedEvent` | `ums.approvals.approval-request.approved` |
| `PermissionMutatedEvent` | `ums.authorization.permission.mutated` |
| `ProfileAssignedToUserEvent` | `ums.authorization.profile.assigned` |

---

## Consequences

### Positive

- Domain layer has zero dependency on MassTransit, RabbitMQ, or any broker type
- In-memory adapter makes all command handler tests broker-free (fast, deterministic)
- CloudEvents 1.0 envelope enables interoperability with any consumer regardless of broker
- `[CloudEventType]` attribute creates a compile-time registry that fails loudly on missing declarations
- Transactional Outbox integration (TE-04) guarantees no event loss under infrastructure failure

### Negative

- Every new domain event requires `[CloudEventType("...")]` attribute — must be enforced by code review or Roslyn analyzer
- MassTransit Outbox requires additional `outbox_messages` table migration (defined in TE-04)

---

**[ADR Registry](./index.md)** | **[Evolith ADR-0015](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/core/0015-event-driven-architecture-intra-domain.md)** | **[TE-04: Transactional Outbox](../blueprints/technical-enablers/te-04-transactional-outbox.md)**
