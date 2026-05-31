# UMS .NET SDK

> **Language:** English | [Español](../../sdk-es/dotnet/README.md)

Distribution: NuGet · Namespace: `Ums.Sdk.*` · Runtime: .NET 10+

This is the **.NET distribution** of the UMS SDK. It is the reference implementation — the conceptual model and the four authorization primitives originate here and are mirrored idiomatically in TypeScript and NestJS.

For a 5-minute integration, jump to [quickstart.md](./quickstart.md). This document covers the full package reference and architectural integration.

---

## 1. Package Family

| Package | Purpose | Depends on |
|---|---|---|
| `Ums.Sdk.Contracts` | DTOs for the AuthorizationGraph, error code constants, `schemaVersion` metadata | — |
| `Ums.Sdk.Authorization` | Pure validator (deny-wins, override, expiry), `IAuthGraphAccessor` port | `Ums.Sdk.Contracts` |
| `Ums.Sdk.Authorization.Aop` | Attributes + DispatchProxy aspect using Shell.AOP | `Ums.Sdk.Authorization`, `Shell.Aop`, `Shell.DispatchProxy` |
| `Ums.Sdk.Authorization.Testing` | `AuthGraphBuilder` fluent API for unit tests | `Ums.Sdk.Authorization` |

You can use `Ums.Sdk.Authorization` standalone (without AOP) if you prefer imperative checks; the validator is the core.

---

## 2. Conceptual Layers

```
Your code
   │
   ▼
[RequiresScope("X.Y")]    ←  attribute on interface method
   │
   ▼
AuthorizationAspect       ←  Shell.AOP DispatchProxy aspect
   │   reads from IAuthGraphAccessor (scoped DI)
   ▼
IAuthorizationValidator   ←  pure validator (no I/O)
   │   applies rules from AuthorizationGraph
   ▼
AuthorizationDecision     ←  Granted / Denied(reason) / Expired
   │
   ├── Throw mode      → UnauthorizedAccessException
   └── ReturnFailure mode → Result.Failure("AUTH_xxx")
```

The validator is intentionally pure: given a graph and a probe, it returns a decision. No HTTP, no DI, no logging. This makes it trivial to unit-test the rules and reuse it from non-AOP contexts (controllers, Blazor, workers).

---

## 3. `IAuthGraphAccessor` — How the SDK Finds the Graph

The SDK does not assume how your application obtains and stores the graph. You implement `IAuthGraphAccessor`:

```csharp
public interface IAuthGraphAccessor
{
    AuthorizationGraph? Current { get; }
}
```

Three typical implementations:

| Scenario | Implementation |
|---|---|
| **ASP.NET Core HTTP app** | Read from `HttpContext.Items["UmsAuthGraph"]`, populated by a middleware that decodes the JWT body and parses the graph once per request. |
| **Worker / CLI** | Use `AsyncLocal<AuthorizationGraph?>` wrapped in a `using` block at the boundary where the graph is loaded. |
| **Existing UMS apps using ADR-0061** | Extend `IExecutionContextAccessor` with an `AuthorizationGraph` property and provide an adapter `IAuthGraphAccessor` that reads from it. **Recommended for UMS-internal services.** |

A default `HttpContextAuthGraphAccessor` is shipped in `Ums.Sdk.Authorization` (depends on `Microsoft.AspNetCore.Http.Abstractions`).

---

## 4. Attribute Reference

All four attributes inherit from `RequiresAuthorizationAttribute` and share two properties:

| Property | Type | Default | Meaning |
|---|---|---|---|
| `OnDenied` | `DenialBehavior` | `Throw` | `Throw` raises `UnauthorizedAccessException`; `ReturnFailure` returns `Result.Failure("AUTH_xxx")` (requires method to return `Result` or `Result<T>`) |
| `AuditOnly` | `bool` | `false` (overridden globally) | When `true`, the aspect logs the denial event but does not block execution |

### 4.1 `[RequiresScope]`

```csharp
[RequiresScope("PURCHASE_ORDER.APPROVE")]
public Task<Result> ApproveOrderAsync(Guid orderId) { ... }

[RequiresScope("PURCHASE_ORDER.APPROVE", OnDenied = DenialBehavior.ReturnFailure)]
public Task<Result<Order>> GetOrderAsync(Guid orderId) { ... }
```

Maps to graph section `scopes[]`. The aspect verifies the scope string is in `graph.scopes` AND not present in resolved denies.

### 4.2 `[RequiresMenuOption]`

```csharp
[RequiresMenuOption("STOCK_ADJUST")]
public Task<Result> AdjustStockAsync(StockAdjustment adjustment) { ... }
```

Maps to graph section `menuAccess[].menus[].subMenus[].options[]`. The aspect searches for the option code; the decision is `Allow` only if `effect == "Allow"`.

### 4.3 `[RequiresDomainAccess]`

```csharp
[RequiresDomainAccess("PURCHASE_ORDER", "VIEW")]
public Task<Order> GetOrderAsync(Guid orderId) { ... }
```

Maps to graph section `domainPermissions[]`. The aspect finds the resource by `code` and the action by `actionCode`; allows only on `effect == "Allow"`.

### 4.4 `[RequiresFeatureFlag]`

```csharp
[RequiresFeatureFlag("WMS_NEW_PICKING_UI")]
public Task<PickList> BuildPickListAsync(Guid orderId) { ... }
```

Maps to graph section `featureFlags[]`. Allows only if the flag is found and `isEnabled == true`. (Note: this attribute is conceptually a feature toggle, not strictly authorization — but lives alongside the others because the integration pattern is identical.)

---

## 5. Wiring Up

### 5.1 With Shell.AOP and DI

```csharp
services.AddAop(aop => aop
    .AddLogger<SerilogLogger>()
    .AddAspect<AuthorizationAspect>());

services.AddSingleton<IAuthorizationValidator, AuthorizationValidator>();
services.AddScoped<IAuthGraphAccessor, HttpContextAuthGraphAccessor>();

// Apply AOP to an interface-backed service:
services.AddAopProxy<IOrderService, OrderService>();
```

`OrderService` implements `IOrderService` and carries the attributes. The DI container resolves `IOrderService` to the AOP proxy, which intercepts calls and runs the aspect.

### 5.2 Without AOP (manual checks)

```csharp
public class OrderService : IOrderService
{
    private readonly IAuthorizationValidator _validator;
    private readonly IAuthGraphAccessor _accessor;

    public async Task<Result> ApproveOrderAsync(Guid orderId)
    {
        var decision = _validator.RequireScope(_accessor.Current, "PURCHASE_ORDER.APPROVE");
        if (decision.IsDenied) return Result.Failure(decision.ErrorCode, decision.Reason);
        // ... business logic
    }
}
```

Useful for controllers/endpoints where AOP isn't appropriate, or for fine-grained checks that don't map to a single method boundary.

---

## 6. Result Pattern Integration

When `OnDenied = ReturnFailure`, the method must return one of:
- `Result`
- `Result<T>`
- `Task<Result>`
- `Task<Result<T>>`

The aspect inspects the return type at startup and refuses to attach to incompatible methods (fail-fast — a clear configuration error at startup beats a runtime surprise).

The returned failure uses the error codes from [`error-codes.md`](../contracts/error-codes.md): `AUTH_101` for missing scope, `AUTH_102` for explicit deny, etc.

---

## 7. Audit-Only Mode

For progressive rollouts of new authorization rules:

```csharp
services.Configure<AuthorizationOptions>(o => o.Mode = AuthorizationMode.AuditOnly);
```

The aspect runs unchanged but never blocks; denials are logged as `AuthorizationDeniedEvent` structured log entries. Once you've cleaned up the surfaced denials, flip to `AuthorizationMode.Enforce`.

---

## 8. Testing Your Consumer Code

`Ums.Sdk.Authorization.Testing` provides `AuthGraphBuilder`:

```csharp
var graph = AuthGraphBuilder
    .ForTenant("LOGISTICS_CORE")
    .WithUser("ana.flores@example.com")
    .WithScope("PURCHASE_ORDER.VIEW")
    .WithScope("PURCHASE_ORDER.APPROVE")
    .WithDeny("STOCK_DELETE.DELETE")
    .WithFeatureFlag("WMS_NEW_PICKING_UI", enabled: true)
    .Build();

var accessor = new TestAuthGraphAccessor(graph);
var validator = new AuthorizationValidator();

var decision = validator.RequireScope(graph, "PURCHASE_ORDER.APPROVE");
Assert.True(decision.IsGranted);
```

The builder is fluent and produces a fully-valid `AuthorizationGraph` instance — no JSON, no HTTP, no UMS.

---

## 9. References

- [Quickstart](./quickstart.md) — 5-minute integration
- [Schema Overview](../contracts/schema-overview.md)
- [Error Codes](../contracts/error-codes.md)
- [Versioning Policy](../contracts/versioning.md)
- [ADR-0073: UMS SDK Multi-Runtime](../../architecture/adrs/0073-ums-sdk-multi-runtime.md)
- [Shell.AOP Developer Guide](../../architecture/shell-libraries/aop.md)
- [ADR-0061: Execution Context Accessor](../../architecture/adrs/0061-execution-context-accessor.md)
