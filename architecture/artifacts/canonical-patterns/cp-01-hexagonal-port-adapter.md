# CP-01: Hexagonal Architecture — Port/Adapter Pattern

**Runtime:** C# / .NET 8  
**Backing ADR:** [ADR-0002 — Clean Architecture & Hexagonal Boundaries](../../adrs/0002-clean-architecture-nestjs.md) · [Blueprint — dotnet-migration-and-tech-stack-plan](../../blueprints/dotnet-migration-and-tech-stack-plan.md)  
**Layer contract:** Domain → Application → Infrastructure. Inner layers never reference outer layers.

---

## The Problem This Solves

Without explicit port/adapter separation, application code accumulates direct dependencies on EF Core, Redis, HttpClient, etc. The domain becomes untestable in isolation and the infrastructure becomes impossible to swap.

---

## Solution Structure

```
Ums.Domain/
  Ports/
    IUserRepository.cs          ← port (interface, pure C#)
    IAuthGraphCache.cs          ← port

Ums.Application/
  Users/
    Commands/
      CreateUserCommand.cs
      CreateUserCommandHandler.cs   ← depends on IUserRepository (port)

Ums.Infrastructure/
  Persistence/
    SqlUserRepository.cs        ← adapter (implements IUserRepository)
  Caching/
    RedisAuthGraphCache.cs      ← adapter (implements IAuthGraphCache)

Ums.Presentation/
  Program.cs                    ← wires ports → adapters (DI composition root)
```

---

## Domain Layer — Port Definition

```csharp
// Ums.Domain/Ports/IUserRepository.cs
// Zero NuGet references. Pure System namespace only.
namespace Ums.Domain.Ports;

public interface IUserRepository
{
    Task<User?> FindByIdAsync(UserId id, CancellationToken ct = default);
    Task<User?> FindByEmailAsync(Email email, OrganizationId orgId, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task UpdateAsync(User user, CancellationToken ct = default);
}
```

```csharp
// Ums.Domain/Ports/IAuthGraphCache.cs
namespace Ums.Domain.Ports;

public interface IAuthGraphCache
{
    Task<AuthGraph?> GetAsync(UserId userId, CancellationToken ct = default);
    Task SetAsync(UserId userId, AuthGraph graph, TimeSpan ttl, CancellationToken ct = default);
    Task InvalidateAsync(UserId userId, CancellationToken ct = default);
}
```

---

## Application Layer — Use Case (Depends Only on Ports)

```csharp
// Ums.Application/Users/Commands/CreateUserCommandHandler.cs
using MediatR;
using Ums.Domain.Ports;
using Ums.Domain.Users;

namespace Ums.Application.Users.Commands;

public sealed record CreateUserCommand(
    string Email,
    string DisplayName,
    Guid OrganizationId
) : IRequest<Result<UserId>>;

internal sealed class CreateUserCommandHandler(
    IUserRepository users,
    IUnitOfWork unitOfWork
) : IRequestHandler<CreateUserCommand, Result<UserId>>
{
    public async Task<Result<UserId>> Handle(CreateUserCommand cmd, CancellationToken ct)
    {
        var email = Email.Create(cmd.Email);
        if (email.IsFailure) return email.Error;

        var orgId = OrganizationId.From(cmd.OrganizationId);

        var existing = await users.FindByEmailAsync(email.Value, orgId, ct);
        if (existing is not null)
            return DomainErrors.User.EmailAlreadyRegistered;

        var user = User.Create(email.Value, cmd.DisplayName, orgId);
        await users.AddAsync(user, ct);
        await unitOfWork.CommitAsync(ct);

        return user.Id;
    }
}
```

---

## Infrastructure Layer — Adapter (Implements Port)

```csharp
// Ums.Infrastructure/Persistence/SqlUserRepository.cs
using Microsoft.EntityFrameworkCore;
using Ums.Domain.Ports;
using Ums.Domain.Users;

namespace Ums.Infrastructure.Persistence;

// The adapter knows about EF Core. The domain never does.
internal sealed class SqlUserRepository(UmsDbContext db) : IUserRepository
{
    public async Task<User?> FindByIdAsync(UserId id, CancellationToken ct) =>
        await db.Users
            .Include(u => u.Profiles)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> FindByEmailAsync(Email email, OrganizationId orgId, CancellationToken ct) =>
        await db.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.OrganizationId == orgId, ct);

    public async Task AddAsync(User user, CancellationToken ct) =>
        await db.Users.AddAsync(user, ct);

    public async Task UpdateAsync(User user, CancellationToken ct) =>
        db.Users.Update(user);
}
```

---

## Composition Root — DI Wiring (Presentation Layer)

```csharp
// Ums.Presentation/Program.cs  — wire ports → adapters here only
builder.Services.AddScoped<IUserRepository, SqlUserRepository>();
builder.Services.AddScoped<IAuthGraphCache, RedisAuthGraphCache>();
builder.Services.AddScoped<IUnitOfWork, EfCoreUnitOfWork>();

// MediatR auto-registers all IRequestHandlers in Application assembly
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(CreateUserCommandHandler).Assembly));
```

---

## Enforcement Rule

The `eslint-plugin-boundaries` equivalent for .NET is a project reference constraint. Enforce via `Ums.Domain.csproj`:

```xml
<!-- Ums.Domain/Ums.Domain.csproj -->
<!-- No ProjectReferences allowed — zero dependencies -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <!-- Intentionally empty: NO PackageReference, NO ProjectReference -->
</Project>
```

A CI check verifies this invariant: `grep -r "ProjectReference" src/Ums.Domain/` must return nothing.

---

## Anti-Patterns to Reject in Code Review

| Anti-Pattern | Symptom | Fix |
| :--- | :--- | :--- |
| Repository in domain | `using Microsoft.EntityFrameworkCore` in `Ums.Domain/` | Move to Infrastructure |
| Use case calls Redis | `IConnectionMultiplexer` injected into handler | Introduce `IAuthGraphCache` port |
| Presentation logic in handler | Handler builds HTTP responses | Handler returns domain types only |
| God repository | `IUserRepository` has 20+ methods | Split by query/command or use CQRS |

---

## Related Patterns

- [CP-02 — Aggregate Root + Domain Event](./cp-02-aggregate-root-domain-event.md)
- [CP-03 — Result Pattern](./cp-03-result-pattern.md)
- [CP-04 — Multi-tenant Repository with RLS](./cp-04-multitenant-repository-rls.md)
