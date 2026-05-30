# BeyondNetCode.Shell.Ddd — Guía de Desarrollo

> **Parte de:** [Shell Libraries](README.md)
> **Paquetes NuGet:** `BeyondNetCode.Shell.Ddd` · `BeyondNetCode.Shell.Ddd.ValueObjects`
> **Dependencias:** `MediatR` (para `INotification` en domain events)
> **Repositorio:** `github.com/beyondnetcode/Shell.Ddd`

`BeyondNetCode.Shell.Ddd` es la fundación DDD para cada aggregate, entity y value object en el dominio UMS. Proporciona los tipos building-block que enforce **invariants at construction time**, **domain event sourcing**, **tracking state**, y **broken-rule collection** sin dependencias NuGet runtime más allá de MediatR.

---

## 1. Estructura de Paquetes

```
BeyondNetCode.Shell.Ddd/
├── AggregateRoot.cs             ← abstract AggregateRoot<TAgg, TProps>
├── Entity.cs                    ← abstract Entity<TEntity, TProps>
├── ValueObject.cs               ← abstract ValueObject<TValue>
├── IdValueObject.cs             ← ValueObject<Guid>; Create() / Load()
├── DomainEvent.cs               ← abstract record DomainEvent : IDomainEvent
├── DomainEnumeration.cs         ← type-safe enum alternative
├── Interfaces/
│   ├── IAggregateRoot.cs
│   ├── IEntity.cs
│   ├── IDomainEvent.cs
│   ├── IMetadata.cs
│   └── IProps.cs                ← marker: must implement ICloneable
├── Rules/
│   ├── BrokenRule.cs            ← (Property, Message) record
│   └── Impl/
│       ├── AbstractRuleValidator.cs
│       └── ValidatorRuleManager.cs
└── Services/
    ├── Interfaces/
    │   ├── IBrokenRulesManager.cs
    │   ├── IDomainEventsManager.cs
    │   └── ITrackingStateManager.cs
    └── Impl/
        ├── BrokenRulesManager.cs
        ├── DomainEventsManager.cs
        └── TrackingStateManager.cs

BeyondNetCode.Shell.Ddd.ValueObjects/
├── Audit/
│   └── AuditValueObject.cs      ← (CreatedBy, CreatedAt, UpdatedBy?, UpdatedAt?)
└── Common/
    ├── StringValueObject.cs
    ├── IntValueObject.cs
    ├── BoolValueObject.cs
    └── DecimalValueObject.cs
```

---

## 2. Conceptos Core

### Patrón Props

Cada entity y aggregate define un record immutable de props que sostiene su estado. Entity recibe una instancia `TProps` en el constructor; el acceso es siempre a través de `Props` o `GetPropsCopy()`.

```csharp
public record OrderProps : IProps
{
    public OrderId Id { get; init; }
    public CustomerId CustomerId { get; init; }
    public Money Total { get; init; }
    public OrderStatus Status { get; init; }
}

public class Order : AggregateRoot<Order, OrderProps>
{
    internal Order(OrderProps props) : base(props) { }

    public OrderId Id => Props.Id;
    public Money Total => Props.Total;
    public OrderStatus Status => Props.Status;
}
```

---

## 3. Value Objects

```csharp
// Crear
var email = StringValueObject.Create<Email>("user@example.com");

// Validación
var result = StringValueObject.Create<Email>("invalid");
if (result.IsFailure)
    Console.WriteLine(result.Error); // "Email is invalid"

// Load desde DB
var loaded = Email.Load(existingGuid, "user@example.com");
```

**Reglas:**
- Immutable después de creación
- Equality por valor
- Validación en construcción

---

## 4. Entities

```csharp
public class Tenant : Entity<Tenant, TenantProps>
{
    internal Tenant(TenantProps props) : base(props) { }

    public TenantId Id => Props.Id;
    public TenantName Name => Props.Name;
    public TenantStatus Status => Props.Status;

    public Result Activate(ActorId activatedBy)
    {
        if (Status == TenantStatus.Active)
            return Result.Failure("Already active");

        Props = Props with { Status = TenantStatus.Active };
        return Result.Success();
    }
}
```

**Características:**
- Identity única (`Id`)
- Evolución de estado a través de métodos
- Broken rules collection

---

## 5. Aggregate Roots

```csharp
public class Order : AggregateRoot<Order, OrderProps>
{
    private readonly List<OrderLine> _lines = new();

    internal Order(OrderProps props) : base(props)
    {
        DomainEvents = new OrderDomainEventsManager(this);
    }

    public IReadOnlyCollection<OrderLine> Lines => _lines.AsReadOnly();
    public new OrderDomainEventsManager DomainEvents { get; }

    public static Result<Order> Create(CustomerId customer, ActorId createdBy)
    {
        var props = new OrderProps(
            Id: IdValueObject.Create(),
            CustomerId: customer,
            Status: OrderStatus.Draft,
            CreatedBy: createdBy,
            CreatedAt: DateTime.UtcNow);

        var order = new Order(props);

        if (!order.IsValid())
            return Result<Order>.Failure(order.BrokenRules.GetBrokenRulesAsString());

        order.DomainEvents.RaiseEvent(new OrderCreatedEvent(order.Id, customer));

        return Result<Order>.Success(order);
    }
}
```

**Características:**
- Root de agregado con identidad
- Domain events via manager
- Tracking state (New/Modified/Deleted)
- Broken rules collection

---

## 6. Domain Events

```csharp
// Definir evento
public record OrderCreatedEvent(Guid OrderId, Guid CustomerId)
    : DomainEvent;

//Raisar en aggregate
DomainEvents.RaiseEvent(new OrderCreatedEvent(Id, Props.CustomerId.GetValue()));

// Suscribir (en handlers o infrastructure)
aggregate.DomainEvents.Subscribe<OrderCreatedEvent>(e =>
{
    // Handle event
});
```

---

## 7. Domain Enumerations

```csharp
public class OrderStatus : DomainEnumeration
{
    public static readonly OrderStatus Draft = new(0, "DRAFT");
    public static readonly OrderStatus Submitted = new(1, "SUBMITTED");
    public static readonly OrderStatus Confirmed = new(2, "CONFIRMED");
    public static readonly OrderStatus Cancelled = new(3, "CANCELLED");

    public OrderStatus(int value, string name) : base(value, name) { }
}

// Uso
if (order.Status == OrderStatus.Draft)
    order.Submit();
```

---

## 8. Broken Rules & Validation

```csharp
public Result<Profile> Create(...)
{
    var profile = new Profile(props);

    if (string.IsNullOrEmpty(profile.Props.Name))
        profile.BrokenRules.Add(new BrokenRule(nameof(Name), "Name is required"));

    if (!profile.IsValid())
        return Result<Profile>.Failure(profile.BrokenRules.GetBrokenRulesAsString());

    return Result<Profile>.Success(profile);
}
```

---

## 9. Tracking State

```csharp
// En constructor, cuando TrackingState.IsNew
if (TrackingState.IsNew)
    DomainEvents.RaiseEvent(new ProfileCreatedEvent(...));

// En aplicación
await _profileRepository.AddAsync(profile, ct);
// Repository marca como Modified
await _profileRepository.UnitOfWork.SaveEntitiesAsync(ct);
```

| Estado | Significado |
|--------|-------------|
| `New` | Objeto recién creado, no persiste |
| `Modified` |Cambió después de cargar desde DB |
| `Deleted` | Marcado para deletion |
| `Unchanged` | Sin cambios desde carga |

---

## 10. Ejemplos de Uso Independiente

```csharp
// Crear aggregate
var orderResult = Order.Create(customerId, actorId);
if (orderResult.IsFailure)
    return Result.Failure(orderResult.Error);

var order = orderResult.Value;

// Ejecutar comando de dominio
var addItemResult = order.AddLine(productId, quantity, price);
if (addItemResult.IsFailure)
    return Result.Failure(addItemResult.Error);

// Verificar estado
if (!order.IsValid())
    Console.WriteLine(order.BrokenRules);

// Obtener eventos
var events = order.DomainEvents.GetEvents();
```

---

## 11. Ejemplos de Integración UMS

```csharp
// Profile aggregate en UMS
public sealed class Profile : AggregateRoot<Profile, ProfileProps>
{
    public new ProfileDomainEventsManager DomainEvents { get; }

    private Profile(ProfileProps props) : base(props)
    {
        DomainEvents = new ProfileDomainEventsManager(this);

        if (TrackingState.IsNew)
        {
            DomainEvents.RaiseEvent(new ProfileCreatedEvent(
                Props.Id.GetValue(),
                Props.TenantId.GetValue(),
                Props.UserId.GetValue(),
                Props.RoleId.GetValue(),
                Props.BranchId?.GetValue()));
        }
    }

    public static Result<Profile> Create(...) { ... }
    public Result AssignTemplate(...) { ... }
}
```