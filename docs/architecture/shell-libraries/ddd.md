# BeyondNetCode.Shell.Ddd — Developer Guide

> **Part of:** [Shell Libraries](README.md)  
> **Projects:** `BeyondNetCode.Shell.Ddd` · `BeyondNetCode.Shell.Ddd.ValueObjects`  
> **Dependencies:** `MediatR` (for `INotification` on domain events)

`BeyondNetCode.Shell.Ddd` is the DDD foundation for every aggregate, entity and value object in the UMS domain. It provides the building-block types that enforce **invariants at construction time**, **domain event sourcing**, **tracking state**, and **broken-rule collection** with no NuGet runtime dependencies beyond MediatR.

---

## Table of Contents

1. [Project Structure](#1-project-structure)
2. [Core Concepts](#2-core-concepts)
3. [Value Objects](#3-value-objects)
4. [Entities](#4-entities)
5. [Aggregate Roots](#5-aggregate-roots)
6. [Domain Events](#6-domain-events)
7. [Domain Enumerations](#7-domain-enumerations)
8. [Broken Rules & Validation](#8-broken-rules--validation)
9. [Tracking State](#9-tracking-state)
10. [Independent Usage Examples](#10-independent-usage-examples)
11. [UMS Integration Examples](#11-ums-integration-examples)

---

## 1. Project Structure

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
│   ├── Impl/
│   │   ├── AbstractRuleValidator.cs
│   │   └── ValidatorRuleManager.cs
│   └── PropertyChange/
│       └── AbstractNotifyPropertyChanged.cs
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

## 2. Core Concepts

### Props pattern

Every entity and aggregate defines an immutable props record that holds its state. Entity receives a `TProps` instance in the constructor; access is always through `Props` or `GetPropsCopy()`.

```csharp
// Props = state snapshot (IProps : ICloneable)
public record ProductProps : IProps
{
    public required string Sku    { get; init; }
    public required decimal Price { get; init; }
    public object Clone() => MemberwiseClone();
}

public class Product : AggregateRoot<Product, ProductProps>
{
    private Product(ProductProps props) : base(props) { }

    public static Product Create(string sku, decimal price)
        => new(new ProductProps { Sku = sku, Price = price });
}
```

### Invariant enforcement

Validators are added in `AddValidators()`. They run on every `Validate()` call, which is triggered automatically on construction and whenever props change.

---

## 3. Value Objects

`ValueObject<TValue>` wraps a primitive value with identity-by-value semantics and optional validation.

### 3.1 Built-in value objects

```csharp
// String with max-length validator
public class ProductName : ValueObject<string>
{
    public static ProductName Create(string value) => new(value);
    private ProductName(string value) : base(value) { }

    public override void AddValidators()
    {
        ValidatorRules.Add(new MaxLengthValidator<ProductName, string>(this, 200));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return GetValue();
    }
}

// Usage
var name = ProductName.Create("Laptop Pro");
Console.WriteLine(name.GetValue());          // "Laptop Pro"
Console.WriteLine(name.IsValid);             // true

name.SetValue(""); // triggers Validate()
Console.WriteLine(name.IsValid);             // false (if you add NotEmpty validator)
```

### 3.2 AuditValueObject

```csharp
// Create on first save
var audit = AuditValueObject.Create("user-001");

// Update on mutation
audit.Update("user-002");

var props = audit.GetValue();
// props.CreatedBy   = "user-001"
// props.CreatedAt   = <utc timestamp>
// props.UpdatedBy   = "user-002"
// props.UpdatedAt   = <utc timestamp>
```

### 3.3 Primitive value objects (with implicit conversion)

```csharp
IntValueObject qty     = 5;        // implicit operator
BoolValueObject active = true;
DecimalValueObject price = 99.99m;
```

### 3.4 IdValueObject

```csharp
var id1 = IdValueObject.Create();             // random Guid
var id2 = IdValueObject.Load(Guid.Parse("...")); // load existing
var id3 = IdValueObject.Load("550e8400-...");    // parse string
var def = IdValueObject.DefaultValue;            // Guid.Empty
```

---

## 4. Entities

`Entity<TEntity, TProps>` provides identity (GUID), broken-rules collection, tracking state, and optional FSM transitions.

### 4.1 Minimal entity

```csharp
public record LineItemProps(Guid ProductId, int Quantity, decimal UnitPrice) : IProps
{
    public object Clone() => MemberwiseClone();
}

public class LineItem : Entity<LineItem, LineItemProps>
{
    private LineItem(LineItemProps props) : base(props) { }

    public static LineItem Create(Guid productId, int qty, decimal unitPrice)
        => new(new LineItemProps(productId, qty, unitPrice));

    public override void AddValidators()
    {
        // add custom AbstractRuleValidator<LineItem> rules here
    }
}

// Usage
var item = LineItem.Create(productId, 3, 49.99m);
item.IsValid();                          // true
item.BrokenRules.GetBrokenRules();       // empty
item.TrackingState.IsNew;               // true
```

### 4.2 Finite State Machine

```csharp
public class OrderLine : Entity<OrderLine, OrderLineProps>
{
    // Allowed transitions
    protected override void DefineValidTransitions(
        Dictionary<object, List<object>> transitions)
    {
        transitions[OrderStatus.Pending]    = [OrderStatus.Confirmed, OrderStatus.Cancelled];
        transitions[OrderStatus.Confirmed]  = [OrderStatus.Shipped];
        transitions[OrderStatus.Shipped]    = [OrderStatus.Delivered];
    }

    public Result Confirm()
    {
        if (!CanTransitionTo(Props.Status, OrderStatus.Confirmed))
            return Result.Failure("Invalid transition.");

        SetProps(Props with { Status = OrderStatus.Confirmed });
        return Result.Success();
    }
}
```

### 4.3 Equality

Entities compare by `Id` (not by reference):

```csharp
var a = LineItem.Create(pid, 1, 10m);
var b = LineItem.Create(pid, 1, 10m);
// a != b  (different Ids)

var same = Entity<...> with the same Id
// a == same  (same Id → equal)
```

---

## 5. Aggregate Roots

`AggregateRoot<TAgg, TProps>` extends `Entity` with a `DomainEventsManager` and a `Version` counter.

### 5.1 Full aggregate example

```csharp
// Props
public record OrderProps(
    Guid CustomerId,
    OrderStatus Status,
    List<LineItem> Lines) : IProps
{
    public object Clone() => MemberwiseClone();
}

// Events
public record OrderPlaced(Guid OrderId, Guid CustomerId) : DomainEvent;
public record OrderConfirmed(Guid OrderId) : DomainEvent;

// Aggregate
public class Order : AggregateRoot<Order, OrderProps>
{
    private Order(OrderProps props) : base(props) { }

    // Factory method — the ONLY way to create an Order
    public static Result<Order> Place(Guid customerId, List<LineItem> lines)
    {
        if (!lines.Any())
            return Result<Order>.Failure("Order must have at least one line.");

        var order = new Order(new OrderProps(
            CustomerId: customerId,
            Status:     OrderStatus.Pending,
            Lines:      lines));

        // Raise domain event — persisted by outbox pattern
        order.DomainEvents.RaiseEvent(new OrderPlaced(order.Id.GetValue(), customerId));

        return Result<Order>.Success(order);
    }

    public Result Confirm()
    {
        if (Props.Status != OrderStatus.Pending)
            return Result.Failure("Only pending orders can be confirmed.");

        SetProps(Props with { Status = OrderStatus.Confirmed });
        DomainEvents.RaiseEvent(new OrderConfirmed(Id.GetValue()));
        TrackingState.MarkAsDirty();

        return Result.Success();
    }

    // Always expose props as read-only via Props accessor
    public override void AddValidators() { /* add domain invariants */ }
}

// Usage
var result = Order.Place(customerId, lines);
if (result.IsFailure) return Result.Failure(result.Error);

var order = result.Value;
order.DomainEvents.GetUncommittedChanges(); // [OrderPlaced]
order.Confirm();
order.DomainEvents.GetUncommittedChanges(); // [OrderPlaced, OrderConfirmed]

// After persistence:
order.DomainEvents.MarkChangesAsCommitted();
```

---

## 6. Domain Events

`DomainEvent` is an abstract record (equality-by-value) that implements both `IDomainEvent` and `INotification` (MediatR).

```csharp
// Define
public record TenantCreated(Guid TenantId, string Code) : DomainEvent;

// Raise (inside aggregate)
DomainEvents.RaiseEvent(new TenantCreated(Id.GetValue(), code));

// Handle with MediatR (outside aggregate, in Application/Infrastructure)
public class TenantCreatedHandler : INotificationHandler<TenantCreated>
{
    public async Task Handle(TenantCreated evt, CancellationToken ct)
    {
        // send welcome email, provision resources, etc.
    }
}

// Access metadata
evt.Metadata.EventId;       // Guid
evt.Metadata.AggregateName; // "Order"
evt.Metadata.AggregateId;   // <aggregate GUID>
evt.CreatedAt;              // UTC timestamp
```

### Replay / Event Sourcing

```csharp
// Load from history (Event Sourcing)
var history = await eventStore.LoadAsync(aggregateId, cancellationToken);
order.DomainEvents.LoadFromHistory(history);
```

---

## 7. Domain Enumerations

`DomainEnumeration` replaces C# `enum` for domain concepts that need a name, integer Id, and rich behavior.

```csharp
public abstract class OrderStatus : DomainEnumeration
{
    public static readonly OrderStatus Pending   = new PendingStatus();
    public static readonly OrderStatus Confirmed = new ConfirmedStatus();
    public static readonly OrderStatus Shipped   = new ShippedStatus();

    private OrderStatus(int id, string name) : base(id, name) { }

    private class PendingStatus   : OrderStatus { public PendingStatus()   : base(1, "PENDING")   { } }
    private class ConfirmedStatus : OrderStatus { public ConfirmedStatus() : base(2, "CONFIRMED") { } }
    private class ShippedStatus   : OrderStatus { public ShippedStatus()   : base(3, "SHIPPED")   { } }
}

// Usage
var all    = DomainEnumeration.GetAll<OrderStatus>(); // [Pending, Confirmed, Shipped]
var parsed = DomainEnumeration.FromDisplayName<OrderStatus>("PENDING"); // OrderStatus.Pending
var byId   = DomainEnumeration.FromValue<OrderStatus>(2);              // OrderStatus.Confirmed

// Comparison (by Id)
OrderStatus.Pending == OrderStatus.Confirmed  // false
OrderStatus.Pending.CompareTo(OrderStatus.Shipped) // -1
```

---

## 8. Broken Rules & Validation

```csharp
// Custom validator
public class NonEmptyNameValidator : AbstractRuleValidator<Product>
{
    public NonEmptyNameValidator(Product entity) : base(entity) { }

    public override IReadOnlyCollection<BrokenRule> Validate()
    {
        var rules = new List<BrokenRule>();

        if (string.IsNullOrWhiteSpace(Target.Props.Name))
            rules.Add(new BrokenRule(nameof(Product.Props.Name), "Name cannot be empty."));

        return rules;
    }
}

// Register in entity
public override void AddValidators()
{
    ValidatorRules.Add(new NonEmptyNameValidator(this));
}

// Check
product.IsValid();                           // false
product.BrokenRules.GetBrokenRules();        // [BrokenRule("Name", "Name cannot be empty.")]
product.BrokenRules.GetBrokenRulesAsString();// "Name: Name cannot be empty."
```

---

## 9. Tracking State

```csharp
// States
entity.TrackingState.IsNew;         // true after Create()
entity.TrackingState.IsDirty;       // true after SetProps()
entity.TrackingState.IsDeleted;     // true after MarkAsDeleted()
entity.TrackingState.IsClean;       // true after MarkAsClean()

// Repository pattern: save based on state
if (entity.TrackingState.IsNew)
    await _repo.AddAsync(entity, ct);
else if (entity.TrackingState.IsDirty)
    await _repo.UpdateAsync(entity, ct);
else if (entity.TrackingState.IsDeleted)
    await _repo.DeleteAsync(entity.Id.GetValue(), ct);
```

---

## 10. Independent Usage Examples

> **No DI required.** All types can be used standalone.

### Example A — Value Object with validation

```csharp
// Create a constrained email value object
public class Email : ValueObject<string>
{
    public static Email Create(string value) => new(value);
    private Email(string value) : base(value) { }

    public override void AddValidators()
    {
        ValidatorRules.Add(new EmailFormatValidator(this));
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return GetValue().ToLowerInvariant();
    }
}

var email = Email.Create("alice@example.com");
email.IsValid;       // true
email.GetValue();    // "alice@example.com"

var bad = Email.Create("not-an-email");
bad.IsValid;         // false
bad.BrokenRules.GetBrokenRules().First().Message; // "Invalid email format."

// Equality is by value
Email.Create("Alice@Example.COM") == Email.Create("alice@example.com"); // true
```

### Example B — Aggregate with events (no DI)

```csharp
var result = Order.Place(customerId, lines);
var order  = result.Value;

// raise event
order.Confirm();

// inspect uncommitted events
var events = order.DomainEvents.GetUncommittedChanges();
// [OrderPlaced, OrderConfirmed]

// dispatch (simulated)
foreach (var evt in events)
    Console.WriteLine($"{evt.Metadata.EventName} at {evt.CreatedAt}");

order.DomainEvents.MarkChangesAsCommitted();
```

---

## 11. UMS Integration Examples

### AggregateRoot in UMS Tenant context

```csharp
// src/apps/ums.api/Ums.Domain/Identity/Tenant/Tenant.cs

public record TenantProps(...) : IProps { ... }

public class Tenant : AggregateRoot<Tenant, TenantProps>
{
    private Tenant(TenantProps props) : base(props) { }

    public static Result<Tenant> Create(Code code, Name name, OrganizationType type,
        ActorId actorId, IdpStrategy strategy, ...)
    {
        var tenant = new Tenant(new TenantProps { ... });

        if (!tenant.IsValid())
            return Result<Tenant>.Failure(tenant.BrokenRules.GetBrokenRulesAsString());

        tenant.DomainEvents.RaiseEvent(new TenantCreatedDomainEvent(tenant.Id.GetValue(), ...));
        return Result<Tenant>.Success(tenant);
    }
}
```

### Value Object in UMS

```csharp
// ActorId, Email, Code, Name — all extend ValueObject<string>
// They validate at construction; the domain never holds an invalid value object.
var code = Code.Create("ACME");   // validates max-length, pattern
var name = Name.Create("ACME Corp");

// AuditValueObject on every entity
var audit = AuditValueObject.Create(actorId.GetValue()); // stamps CreatedBy + CreatedAt
```

---

## Related Docs

- [Factory](factory.md) — `BeyondNetCode.Shell.Factory` uses `AbstractFactorySetupSource` to dispatch based on aggregate state
- [AOP](aop.md) — handlers that operate on aggregates are decorated with `[LoggerAspect]`
- [Combined Usage](combined-usage.md) — full walkthrough with DDD + Factory + AOP + Bootstrapper
