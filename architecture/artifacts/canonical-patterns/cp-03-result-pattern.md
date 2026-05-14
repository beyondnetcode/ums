# CP-03: Result Pattern — Error Handling Without Exceptions

**Runtime:** C# / .NET 8  
**Backing ADR:** [dotnet-migration-and-tech-stack-plan — Executive Guideline #1](../../blueprints/dotnet-migration-and-tech-stack-plan.md)  
**Mandate:** Throwing exceptions for business flow control is **prohibited**. All MediatR command handlers MUST return `Result<T>`.

---

## The Problem This Solves

`throw new Exception()` for business errors forces callers to use `try/catch` for control flow, hides error paths from the type system, and makes it impossible to enforce at compile time that every error case is handled. It also pollutes exception telemetry with expected business errors.

---

## Result Type (Domain Layer — zero NuGet)

```csharp
// Ums.Domain/Shared/Result.cs
namespace Ums.Domain.Shared;

public sealed class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public DomainError? Error { get; }

    private Result(bool isSuccess, DomainError? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(DomainError error) => new(false, error);

    public static implicit operator Result(DomainError error) => Failure(error);
}
```

```csharp
// Ums.Domain/Shared/Result{T}.cs
namespace Ums.Domain.Shared;

public sealed class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public DomainError? Error { get; }

    private Result(T value) { IsSuccess = true; Value = value; }
    private Result(DomainError error) { IsSuccess = false; Error = error; }

    public static Result<T> Success(T value) => new(value);
    public static Result<T> Failure(DomainError error) => new(error);

    // Implicit conversions for clean call sites
    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(DomainError error) => Failure(error);
}
```

---

## Domain Error Catalog

```csharp
// Ums.Domain/Shared/DomainError.cs
namespace Ums.Domain.Shared;

public sealed record DomainError(string Code, string Description)
{
    public static DomainError None => new(string.Empty, string.Empty);
}
```

```csharp
// Ums.Domain/Users/DomainErrors.User.cs
namespace Ums.Domain.Users;

public static partial class DomainErrors
{
    public static class User
    {
        public static readonly DomainError EmailEmpty =
            new("user.email.empty", "Email address cannot be empty.");

        public static readonly DomainError EmailInvalid =
            new("user.email.invalid", "Email address format is invalid.");

        public static readonly DomainError EmailAlreadyRegistered =
            new("user.email.duplicate", "A user with this email already exists in the organization.");

        public static readonly DomainError InactiveUserCannotReceiveProfile =
            new("user.profile.inactive", "Cannot assign a profile to an inactive user.");

        public static readonly DomainError ProfileAlreadyAssigned =
            new("user.profile.duplicate", "This profile is already assigned to the user.");

        public static readonly DomainError AlreadyInactive =
            new("user.status.already_inactive", "User is already inactive.");

        public static readonly DomainError NotFound =
            new("user.not_found", "User was not found.");
    }
}
```

---

## Usage in Domain — Value Object Factory

```csharp
// Inside Email.Create():
public static Result<Email> Create(string raw)
{
    if (string.IsNullOrWhiteSpace(raw))
        return DomainErrors.User.EmailEmpty;  // implicit conversion → Result<Email>.Failure

    raw = raw.Trim().ToLowerInvariant();

    if (!raw.Contains('@') || raw.Length > 256)
        return DomainErrors.User.EmailInvalid;

    return new Email(raw);  // implicit conversion → Result<Email>.Success
}
```

---

## Usage in Application — Command Handler

```csharp
// Ums.Application/Users/Commands/CreateUserCommandHandler.cs
public async Task<Result<UserId>> Handle(CreateUserCommand cmd, CancellationToken ct)
{
    // Each step returns a Result — error paths are explicit
    var emailResult = Email.Create(cmd.Email);
    if (emailResult.IsFailure) return emailResult.Error!;  // short-circuit

    var orgId = OrganizationId.From(cmd.OrganizationId);

    var existing = await users.FindByEmailAsync(emailResult.Value!, orgId, ct);
    if (existing is not null)
        return DomainErrors.User.EmailAlreadyRegistered;

    var user = User.Create(emailResult.Value!, cmd.DisplayName, orgId);
    await users.AddAsync(user, ct);
    await unitOfWork.CommitAsync(ct);

    return user.Id;  // implicit → Result<UserId>.Success
}
```

---

## Usage in Presentation — Mapping Result to HTTP Response

```csharp
// Ums.Presentation/Endpoints/UsersEndpoints.cs
app.MapPost("/v1/users", async (
    CreateUserRequest req,
    ISender sender,
    CancellationToken ct) =>
{
    var result = await sender.Send(new CreateUserCommand(
        req.Email,
        req.DisplayName,
        req.OrganizationId
    ), ct);

    return result.IsSuccess
        ? Results.Created($"/v1/users/{result.Value}", new { id = result.Value })
        : result.Error!.ToHttpResult();  // see error mapping below
});
```

```csharp
// Ums.Presentation/Extensions/DomainErrorExtensions.cs
public static IResult ToHttpResult(this DomainError error) =>
    error.Code switch
    {
        "user.email.duplicate"    => Results.Conflict(new ProblemDetails(error)),
        "user.not_found"          => Results.NotFound(new ProblemDetails(error)),
        "user.email.empty" or
        "user.email.invalid"      => Results.UnprocessableEntity(new ProblemDetails(error)),
        _                         => Results.BadRequest(new ProblemDetails(error))
    };
```

---

## Chain Pattern (Railway-Oriented)

For sequential operations, use extension methods to avoid deeply nested `if (result.IsFailure)` checks:

```csharp
// Ums.Domain/Shared/ResultExtensions.cs
public static class ResultExtensions
{
    public static Result<TOut> Bind<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Result<TOut>> next) =>
        result.IsFailure ? result.Error! : next(result.Value!);

    public static async Task<Result<TOut>> BindAsync<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, Task<Result<TOut>>> next) =>
        result.IsFailure ? result.Error! : await next(result.Value!);
}

// Usage:
var result = Email.Create(rawEmail)
    .Bind(email => OrganizationId.Validate(rawOrgId)
        .Bind(orgId => User.CanBeCreatedIn(orgId)));
```

---

## What NOT to Do

```csharp
// ❌ PROHIBITED: exception for business flow
public User GetUserOrThrow(UserId id)
{
    var user = db.Users.Find(id);
    if (user is null) throw new NotFoundException("User not found");  // ← forbidden
    return user;
}

// ✓ CORRECT: Result communicates absence
public async Task<Result<User>> FindUserAsync(UserId id, CancellationToken ct)
{
    var user = await db.Users.FindAsync([id], ct);
    return user is null ? DomainErrors.User.NotFound : user;
}
```

Exceptions are reserved for **infrastructure failures** (network timeouts, SQL Server unreachable, unexpected null reference) — not for expected business outcomes.

---

## Related Patterns

- [CP-01 — Hexagonal Port/Adapter](./cp-01-hexagonal-port-adapter.md)
- [CP-02 — Aggregate Root + Domain Event](./cp-02-aggregate-root-domain-event.md)
- [CP-04 — Multi-tenant Repository with RLS](./cp-04-multitenant-repository-rls.md)
