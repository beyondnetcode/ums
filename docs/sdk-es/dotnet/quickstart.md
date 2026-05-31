# UMS .NET SDK — Quickstart

> **Idioma:** [English](../../sdk/dotnet/quickstart.md) | Español

Obtén un método protegido por autorización UMS en **cinco minutos**. Asume que ya tienes un tenant UMS y puedes llamar a `POST /api/v1/client/authenticate`.

Para referencia completa, ver [README.md](./README.md).

---

## Paso 1 — Instalar paquetes

```bash
dotnet add package Ums.Sdk.Authorization
dotnet add package Ums.Sdk.Authorization.Aop
```

Para tests:

```bash
dotnet add package Ums.Sdk.Authorization.Testing
```

---

## Paso 2 — Registrar servicios

En `Program.cs`:

```csharp
using Ums.Sdk.Authorization;
using Ums.Sdk.Authorization.Aop;
using BeyondNetCode.Shell.DI;

builder.Services.AddAop(aop => aop
    .AddAspect<AuthorizationAspect>());

builder.Services.AddUmsSdkAuthorization();          // validator + accessor default
builder.Services.AddHttpContextAuthGraphAccessor(); // integración ASP.NET Core
```

---

## Paso 3 — Poblar el grafo por request

Agrega el middleware que decodifica el body del JWT y almacena el grafo en `HttpContext.Items`:

```csharp
app.UseUmsAuthGraph();   // antes de UseAuthorization
```

Este middleware:
1. Lee el header `Authorization: Bearer ...`.
2. Llama a `POST /api/v1/client/authenticate` si no hay grafo cacheado o el cacheado expiró.
3. Almacena el `AuthorizationGraph` parseado en `HttpContext.Items["UmsAuthGraph"]`.
4. Valida `schemaVersion` contra el rango de compatibilidad del SDK; rechaza con `401` ante `AUTH_205`.

---

## Paso 4 — Proteger un método

Agrega un atributo al método de la interfaz:

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
        // tu lógica de negocio — corre solo si está autorizado
        return Result.Success();
    }
}
```

Registra el servicio con proxy AOP:

```csharp
builder.Services.AddAopProxy<IOrderService, OrderService>();
```

Eso es todo. `IOrderService.ApproveOrderAsync` ahora está protegido. Llamarlo desde un endpoint u otro servicio:

```csharp
app.MapPost("/orders/{id}/approve", async (Guid id, IOrderService svc) =>
{
    var result = await svc.ApproveOrderAsync(id);
    return result.IsSuccess ? Results.Ok() : Results.Forbid();
});
```

Si el usuario autenticado no tiene `PURCHASE_ORDER.APPROVE`, `ApproveOrderAsync` lanza `UnauthorizedAccessException` antes de que corra el body.

---

## Paso 5 — Testearlo

```csharp
[Fact]
public async Task ApproveOrder_WithoutScope_ReturnsForbidden()
{
    var graph = AuthGraphBuilder
        .ForTenant("LOGISTICS_CORE")
        .WithUser("ana.flores@example.com")
        .WithScope("PURCHASE_ORDER.VIEW")   // VIEW pero no APPROVE
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

## Ajustes comunes

### Retornar `Result.Failure` en lugar de lanzar

```csharp
[RequiresScope("PURCHASE_ORDER.APPROVE", OnDenied = DenialBehavior.ReturnFailure)]
Task<Result> ApproveOrderAsync(Guid orderId);
```

### Roll-out gradual con modo audit-only

```csharp
builder.Services.Configure<AuthorizationOptions>(o =>
    o.Mode = AuthorizationMode.AuditOnly);
```

Las denegaciones se loguean pero no se bloquean. Observa los logs por entradas `AuthorizationDeniedEvent`, corrige los gaps, luego cambia a `Enforce`.

### Verificar un permiso imperativamente (sin atributo)

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

## Próximos Pasos

- [Referencia Completa](./README.md)
- [Schema Overview](../contracts/schema-overview.md)
- [Códigos de Error](../contracts/error-codes.md)
