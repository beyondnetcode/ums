# API AOP Implementations

This document details the concrete Aspect-Oriented Programming (AOP) implementations used in the UMS API. These aspects are built on top of the `BeyondNetCode.Shell.Aop` library and serve to decouple cross-cutting concerns from the core business logic (Commands and Queries) in the Application layer.

---

## 1. LoggerAspect

### Objective
To automatically log the entry, success, failure, and execution duration of Application layer handlers.

### Rationale
Manually injecting `ILogger<T>` into every MediatR handler adds boilerplate and leads to inconsistent logging formats. By using an aspect, we guarantee that all operations are uniformly traced, making it easier to parse logs in centralized observatories like Datadog or ELK.

### How to Implement
Apply the `[LoggerAspect]` attribute to any command or query handler.

### Code Example
```csharp
using Ums.Application.Common.Aop;

[LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]
public class CreateUserAccountCommandHandler : IRequestHandler<CreateUserAccountCommand, Result>
{
    public async Task<Result> Handle(CreateUserAccountCommand request, CancellationToken cancellationToken)
    {
        // Business logic runs here. Logging is handled automatically.
        return Result.Success();
    }
}
```

---

## 2. AuditTrailAspect

### Objective
To persist immutable records of critical state mutations to meet enterprise compliance and security standards.

### Rationale
Audit trails must never be bypassed by developers. Handling them via AOP ensures that every mutative operation (e.g., Create, Update, Delete) automatically resolves the affected entities, the actor, and the outcome (Success/Failure) and writes it to the Audit Sink without polluting the domain logic.

### How to Implement
The `AuditTrailAspect` is registered globally in the Infrastructure layer using `AddAop()`. It automatically intercepts handlers decorated with `[AuditTrailAttribute]`.

### Code Example
```csharp
using Ums.Application.Common.Aop;

[AuditTrailAttribute(EventType = "UserCreation", WhatChanged = "Created a new user account")]
public class CreateUserAccountCommandHandler : IRequestHandler<CreateUserAccountCommand, Result>
{
    public async Task<Result> Handle(CreateUserAccountCommand request, CancellationToken cancellationToken)
    {
        // Audit record is dispatched transparently upon successful completion
        return Result.Success();
    }
}
```

---

## 3. TransactionAspect

### Objective
To wrap database operations within a robust Unit of Work / Transaction scope, ensuring atomic commits and automatic rollbacks upon failure.

### Rationale
Database transactions are an infrastructural concern. Forcing the application layer to manually manage `IUnitOfWorkScope.BeginAsync()`, `CommitAsync()`, and `RollbackAsync()` clutters the code and risks dangling transactions if exceptions are not properly caught.

### How to Implement
Apply the `[TransactionAspect]` attribute to command handlers that write to the database.

### Code Example
```csharp
using Ums.Application.Common.Aop;

[TransactionAspect]
public class UpdateSystemSuiteCommandHandler : IRequestHandler<UpdateSystemSuiteCommand, Result>
{
    public async Task<Result> Handle(UpdateSystemSuiteCommand request, CancellationToken cancellationToken)
    {
        // If this method throws, or if it returns Result.Failure, the transaction rolls back.
        // If it succeeds, the transaction is automatically committed.
        await _repository.UpdateAsync(suite);
        return Result.Success();
    }
}
```

---

## 4. TenantValidationAspect

### Objective
To enforce multi-tenancy boundaries at the outermost edges of the Application layer before domain logic is executed.

### Rationale
Relying solely on SQL Server RLS (Row-Level Security) is insufficient as a primary defense (Rule 7). The application must proactively validate that the incoming payload's `TenantId` matches the authenticated session's `TenantId` (from `IUserContext`), blocking malicious cross-tenant data requests instantly.

### How to Implement
Apply the `[TenantValidationAspect]` attribute to handlers processing tenant-scoped data. The aspect intercepts the payload, extracts the `TenantId` using reflection, and verifies it against `IUserContext`.

### Code Example
```csharp
using Ums.Application.Common.Aop;

[TenantValidationAspect]
public class GetUserAccountByIdQueryHandler : IRequestHandler<GetUserAccountByIdQuery, Result<UserAccountDto>>
{
    public async Task<Result<UserAccountDto>> Handle(GetUserAccountByIdQuery request, CancellationToken cancellationToken)
    {
        // Aspect ensures request.TenantId == _userContext.TenantId.
        // If they differ, an UnauthorizedAccessException is thrown before reaching here.
        var user = await _repository.GetByIdAsync(request.Id);
        return Result<UserAccountDto>.Success(user);
    }
}
```
