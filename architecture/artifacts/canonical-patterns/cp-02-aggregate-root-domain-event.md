# CP-02: Aggregate Root + Domain Event

**Runtime:** C# / .NET 8  
**Backing ADR:** [ADR-0019 — Tactical Design Patterns](../../adrs/0019-tactical-design-patterns-future-proofing.md) · [ADR-0015 — Event-Driven Architecture](../../adrs/0015-event-driven-architecture-intra-domain.md)  
**Constraint:** `Ums.Domain` has zero NuGet references. All base classes are hand-rolled POCOs.

---

## The Problem This Solves

Without a clear aggregate boundary, multiple services modify entity state directly, invariants break, and domain events get fired at the wrong layer (infrastructure or presentation). Aggregates enforce that all state changes go through the root and that events are raised consistently.

---

## Base Infrastructure (Domain Layer — no NuGet)

```csharp
// Ums.Domain/Shared/DomainEvent.cs
namespace Ums.Domain.Shared;

public abstract record DomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
```

```csharp
// Ums.Domain/Shared/AggregateRoot.cs
namespace Ums.Domain.Shared;

public abstract class AggregateRoot<TId>
{
    private readonly List<DomainEvent> _events = [];

    public TId Id { get; protected init; } = default!;
    public int Version { get; private set; }

    public IReadOnlyList<DomainEvent> DomainEvents => _events.AsReadOnly();

    protected void Raise(DomainEvent @event) => _events.Add(@event);

    // Called by the Unit of Work after persisting — clears the collection
    public void ClearDomainEvents() => _events.Clear();

    // Optimistic concurrency: EF Core uses this as row version
    public void IncrementVersion() => Version++;
}
```

```csharp
// Ums.Domain/Shared/ValueObject.cs
namespace Ums.Domain.Shared;

public abstract record ValueObject;
```

---

## Domain Events (Examples)

```csharp
// Ums.Domain/Users/Events/UserCreatedEvent.cs
namespace Ums.Domain.Users.Events;

public sealed record UserCreatedEvent(
    UserId UserId,
    Email Email,
    OrganizationId OrganizationId
) : DomainEvent;
```

```csharp
// Ums.Domain/Users/Events/ProfileAssignedEvent.cs
namespace Ums.Domain.Users.Events;

public sealed record ProfileAssignedEvent(
    UserId UserId,
    ProfileId ProfileId,
    TemplateId TemplateId
) : DomainEvent;
```

---

## Value Objects

```csharp
// Ums.Domain/Users/UserId.cs
namespace Ums.Domain.Users;

public sealed record UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public static UserId From(Guid value) => new(value);
    public override string ToString() => Value.ToString();
}
```

```csharp
// Ums.Domain/Users/Email.cs
namespace Ums.Domain.Users;

public sealed record Email
{
    public string Value { get; }

    private Email(string value) => Value = value;

    public static Result<Email> Create(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return DomainErrors.User.EmailEmpty;

        raw = raw.Trim().ToLowerInvariant();

        if (!raw.Contains('@') || raw.Length > 256)
            return DomainErrors.User.EmailInvalid;

        return new Email(raw);
    }

    public override string ToString() => Value;
}
```

---

## Aggregate Root — Full Example

```csharp
// Ums.Domain/Users/User.cs
using Ums.Domain.Shared;
using Ums.Domain.Users.Events;

namespace Ums.Domain.Users;

public sealed class User : AggregateRoot<UserId>
{
    private readonly List<ProfileAssignment> _profiles = [];

    public Email Email { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public OrganizationId OrganizationId { get; private set; } = default!;
    public UserStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private init; }

    public IReadOnlyList<ProfileAssignment> Profiles => _profiles.AsReadOnly();

    // Private constructor — EF Core uses the parameterless one via reflection
    private User() { }

    // Factory method — the only way to create a valid User
    public static User Create(Email email, string displayName, OrganizationId orgId)
    {
        var user = new User
        {
            Id = UserId.New(),
            Email = email,
            DisplayName = displayName,
            OrganizationId = orgId,
            Status = UserStatus.Active,
            CreatedAt = DateTimeOffset.UtcNow,
        };

        user.Raise(new UserCreatedEvent(user.Id, email, orgId));
        return user;
    }

    // Business operation — enforces invariant, raises event
    public Result AssignProfile(ProfileId profileId, TemplateId templateId)
    {
        if (Status != UserStatus.Active)
            return DomainErrors.User.InactiveUserCannotReceiveProfile;

        var alreadyAssigned = _profiles.Any(p => p.ProfileId == profileId);
        if (alreadyAssigned)
            return DomainErrors.User.ProfileAlreadyAssigned;

        _profiles.Add(new ProfileAssignment(profileId, templateId, DateTimeOffset.UtcNow));
        Raise(new ProfileAssignedEvent(Id, profileId, templateId));

        return Result.Success();
    }

    public Result Deactivate()
    {
        if (Status == UserStatus.Inactive)
            return DomainErrors.User.AlreadyInactive;

        Status = UserStatus.Inactive;
        Raise(new UserDeactivatedEvent(Id));
        return Result.Success();
    }
}
```

---

## Dispatching Domain Events (Application / Unit of Work)

Domain events are dispatched **after the transaction commits** — not before. The Unit of Work owns this responsibility:

```csharp
// Ums.Infrastructure/Persistence/EfCoreUnitOfWork.cs
using MediatR;

internal sealed class EfCoreUnitOfWork(UmsDbContext db, IPublisher publisher) : IUnitOfWork
{
    public async Task CommitAsync(CancellationToken ct = default)
    {
        // 1. Collect events before saving (EF tracking)
        var aggregates = db.ChangeTracker.Entries<AggregateRoot<object>>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var events = aggregates
            .SelectMany(a => a.DomainEvents)
            .ToList();

        // 2. Persist state
        await db.SaveChangesAsync(ct);

        // 3. Clear events after successful persist
        aggregates.ForEach(a => a.ClearDomainEvents());

        // 4. Dispatch events (MediatR notifications → event handlers)
        foreach (var domainEvent in events)
            await publisher.Publish(domainEvent, ct);
    }
}
```

---

## Event Handler Example

```csharp
// Ums.Application/Users/EventHandlers/InvalidateAuthGraphOnProfileAssigned.cs
using MediatR;
using Ums.Domain.Ports;
using Ums.Domain.Users.Events;

internal sealed class InvalidateAuthGraphOnProfileAssigned(IAuthGraphCache cache)
    : INotificationHandler<ProfileAssignedEvent>
{
    public async Task Handle(ProfileAssignedEvent notification, CancellationToken ct) =>
        await cache.InvalidateAsync(notification.UserId, ct);
}
```

---

## Rules at a Glance

| Rule | Rationale |
| :--- | :--- |
| Use factory methods, not `new` | Ensures invariants are checked at construction |
| Raise events inside the aggregate | The aggregate knows when its state changes — nobody else does |
| Dispatch after commit | Prevents phantom events if the transaction rolls back |
| `private set` on all properties | External code cannot bypass business operations |
| One aggregate = one transaction boundary | Never modify two aggregate roots in one use case |

---

## Related Patterns

- [CP-01 — Hexagonal Port/Adapter](./cp-01-hexagonal-port-adapter.md)
- [CP-03 — Result Pattern](./cp-03-result-pattern.md)
- [TE-04 — Transactional Outbox](../../blueprints/technical-enablers/te-04-transactional-outbox.md)
