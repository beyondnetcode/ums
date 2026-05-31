# UMS .NET SDK — Quickstart

> **Language:** English | [Español](../../sdk-es/dotnet/quickstart.md)

Get a method protected by UMS authorization in **five minutes**. Assumes you already have a UMS tenant and can call `POST /api/v1/client/authenticate`.

For full reference, see [README.md](./README.md).

---

## Step 1 — Install packages

```bash
dotnet add package Ums.Sdk.Authorization
dotnet add package Ums.Sdk.Authorization.Aop
```

For tests:

```bash
dotnet add package Ums.Sdk.Authorization.Testing
```

---

## Step 2 — Register services

In `Program.cs`:

```csharp
using Ums.Sdk.Authorization;
using Ums.Sdk.Authorization.Aop;
using BeyondNetCode.Shell.DI;

builder.Services.AddAop(aop => aop
    .AddAspect<AuthorizationAspect>());

builder.Services.AddUmsSdkAuthorization();          // validator + default accessor
builder.Services.AddHttpContextAuthGraphAccessor(); // ASP.NET Core integration
```

---

## Step 3 — Populate the graph per request

Add the middleware that decodes the JWT body and stores the graph in `HttpContext.Items`:

```csharp
app.UseUmsAuthGraph();   // before UseAuthorization
```

This middleware:
1. Reads the `Authorization: Bearer ...` header.
2. Calls `POST /api/v1/client/authenticate` if no cached graph or the cached one expired.
3. Stores the parsed `AuthorizationGraph` in `HttpContext.Items["UmsAuthGraph"]`.
4. Validates `schemaVersion` against the SDK's compatibility range; rejects with `401` on `AUTH_205`.

---

## Step 4 — Protect a method

Add an attribute to the interface method:

```csharp
public interface IOrderService
{
    [RequiresScope("PURCHASE_ORDER.APPROVE")]
    Task<Result> ApproveOrderAsync(Guid orderId);
}

public class OrderService : IOrderService
{
    public async Task<Result> ApproveOrderAsync(Guid orderId)
    {
        // your business logic — runs only if authorized
        return Result.Success();
    }
}
```

Register the AOP-proxied service:

```csharp
builder.Services.AddAopProxy<IOrderService, OrderService>();
```

That's it. `IOrderService.ApproveOrderAsync` is now protected. Calling it from an endpoint or another service:

```csharp
app.MapPost("/orders/{id}/approve", async (Guid id, IOrderService svc) =>
{
    var result = await svc.ApproveOrderAsync(id);
    return result.IsSuccess ? Results.Ok() : Results.Forbid();
});
```

If the authenticated user lacks `PURCHASE_ORDER.APPROVE`, `ApproveOrderAsync` throws `UnauthorizedAccessException` before the body runs.

---

## Step 5 — Test it

```csharp
[Fact]
public async Task ApproveOrder_WithoutScope_ReturnsForbidden()
{
    var graph = AuthGraphBuilder
        .ForTenant("LOGISTICS_CORE")
        .WithUser("ana.flores@example.com")
        .WithScope("PURCHASE_ORDER.VIEW")   // VIEW but not APPROVE
        .Build();

    var accessor = new TestAuthGraphAccessor(graph);
    var service = AopProxyCreator.Create<IOrderService, OrderService>(
        new OrderService(),
        TestAspectExecutorFactory.Create(accessor));

    await Assert.ThrowsAsync<UnauthorizedAccessException>(
        () => service.ApproveOrderAsync(Guid.NewGuid()));
}
```

---

## Common adjustments

### Return `Result.Failure` instead of throwing

```csharp
[RequiresScope("PURCHASE_ORDER.APPROVE", OnDenied = DenialBehavior.ReturnFailure)]
Task<Result> ApproveOrderAsync(Guid orderId);
```

### Roll out gradually with audit-only mode

```csharp
builder.Services.Configure<AuthorizationOptions>(o =>
    o.Mode = AuthorizationMode.AuditOnly);
```

Denials are logged but not blocked. Watch logs for `AuthorizationDeniedEvent` entries, fix gaps, then switch to `Enforce`.

### Check a permission imperatively (no attribute)

```csharp
public class OrderController(IAuthorizationValidator validator, IAuthGraphAccessor accessor)
{
    public IActionResult Approve(Guid id)
    {
        var decision = validator.RequireScope(accessor.Current, "PURCHASE_ORDER.APPROVE");
        if (decision.IsDenied) return Forbid();
        // ...
    }
}
```

---

## Next Steps

- [Full Reference](./README.md)
- [Schema Overview](../contracts/schema-overview.md)
- [Error Codes](../contracts/error-codes.md)
