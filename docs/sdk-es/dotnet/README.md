# UMS .NET SDK

> **Idioma:** [English](../../sdk/dotnet/README.md) | Español

Distribución: NuGet · Namespace: `Ums.Sdk.*` · Runtime: .NET 10+

Esta es la **distribución .NET** del UMS SDK. Es la implementación de referencia — el modelo conceptual y los cuatro primitivos de autorización se originan aquí y se reflejan idiomáticamente en TypeScript y NestJS.

Para una integración en 5 minutos, salta a [quickstart.md](./quickstart.md). Este documento cubre la referencia completa de paquetes e integración arquitectural.

---

## 1. Familia de Paquetes

| Paquete | Propósito | Depende de |
|---|---|---|
| `Ums.Sdk.Contracts` | DTOs del AuthorizationGraph, constantes de códigos de error, metadata de `schemaVersion` | — |
| `Ums.Sdk.Authorization` | Validador puro (deny-wins, override, expiry), puerto `IAuthGraphAccessor` | `Ums.Sdk.Contracts` |
| `Ums.Sdk.Authorization.Aop` | Atributos + aspecto DispatchProxy usando Shell.AOP | `Ums.Sdk.Authorization`, `Shell.Aop`, `Shell.DispatchProxy` |
| `Ums.Sdk.Authorization.Testing` | API fluida `AuthGraphBuilder` para tests unitarios | `Ums.Sdk.Authorization` |

Puedes usar `Ums.Sdk.Authorization` standalone (sin AOP) si prefieres checks imperativos; el validador es el núcleo.

---

## 2. Capas Conceptuales

```
Tu código
   │
   ▼
[RequiresScope("X.Y")]    ←  atributo en método de interfaz
   │
   ▼
AuthorizationAspect       ←  aspecto DispatchProxy de Shell.AOP
   │   lee desde IAuthGraphAccessor (DI scoped)
   ▼
IAuthorizationValidator   ←  validador puro (sin I/O)
   │   aplica reglas del AuthorizationGraph
   ▼
AuthorizationDecision     ←  Granted / Denied(reason) / Expired
   │
   ├── Modo Throw           → UnauthorizedAccessException
   └── Modo ReturnFailure   → Result.Failure("AUTH_xxx")
```

El validador es intencionalmente puro: dado un grafo y una probe, retorna una decisión. Sin HTTP, sin DI, sin logging. Esto hace trivial unit-testear las reglas y reusarlo desde contextos no-AOP (controllers, Blazor, workers).

---

## 3. `IAuthGraphAccessor` — Cómo el SDK Encuentra el Grafo

El SDK no asume cómo tu aplicación obtiene y almacena el grafo. Tú implementas `IAuthGraphAccessor`:

```csharp
public interface IAuthGraphAccessor
{
    AuthorizationGraph? Current { get; }
}
```

Tres implementaciones típicas:

| Escenario | Implementación |
|---|---|
| **App HTTP ASP.NET Core** | Leer desde `HttpContext.Items["UmsAuthGraph"]`, poblado por un middleware que decodifica el body del JWT y parsea el grafo una vez por request. |
| **Worker / CLI** | Usar `AsyncLocal<AuthorizationGraph?>` envuelto en un bloque `using` en el límite donde se carga el grafo. |
| **Apps UMS existentes usando ADR-0061** | Extender `IExecutionContextAccessor` con una propiedad `AuthorizationGraph` y proveer un adapter `IAuthGraphAccessor` que lea desde ahí. **Recomendado para servicios internos UMS.** |

Un `HttpContextAuthGraphAccessor` default viene en `Ums.Sdk.Authorization` (depende de `Microsoft.AspNetCore.Http.Abstractions`).

---

## 4. Referencia de Atributos

Los cuatro atributos heredan de `RequiresAuthorizationAttribute` y comparten dos propiedades:

| Propiedad | Tipo | Default | Significado |
|---|---|---|---|
| `OnDenied` | `DenialBehavior` | `Throw` | `Throw` lanza `UnauthorizedAccessException`; `ReturnFailure` retorna `Result.Failure("AUTH_xxx")` (requiere que el método retorne `Result` o `Result<T>`) |
| `AuditOnly` | `bool` | `false` (sobreescrito globalmente) | Cuando es `true`, el aspecto loguea el evento de denegación pero no bloquea ejecución |

### 4.1 `[RequiresScope]`

```csharp
[RequiresScope("PURCHASE_ORDER.APPROVE")]
public Task<Result> ApproveOrderAsync(Guid orderId) { ... }

[RequiresScope("PURCHASE_ORDER.APPROVE", OnDenied = DenialBehavior.ReturnFailure)]
public Task<Result<Order>> GetOrderAsync(Guid orderId) { ... }
```

Mapea a la sección `scopes[]` del grafo. El aspecto verifica que el string de scope esté en `graph.scopes` Y no presente en denies resueltos.

### 4.2 `[RequiresMenuOption]`

```csharp
[RequiresMenuOption("STOCK_ADJUST")]
public Task<Result> AdjustStockAsync(StockAdjustment adjustment) { ... }
```

Mapea a la sección `menuAccess[].menus[].subMenus[].options[]` del grafo. El aspecto busca el código de opción; la decisión es `Allow` solo si `effect == "Allow"`.

### 4.3 `[RequiresDomainAccess]`

```csharp
[RequiresDomainAccess("PURCHASE_ORDER", "VIEW")]
public Task<Order> GetOrderAsync(Guid orderId) { ... }
```

Mapea a la sección `domainPermissions[]` del grafo. El aspecto encuentra el recurso por `code` y la acción por `actionCode`; permite solo en `effect == "Allow"`.

### 4.4 `[RequiresFeatureFlag]`

```csharp
[RequiresFeatureFlag("WMS_NEW_PICKING_UI")]
public Task<PickList> BuildPickListAsync(Guid orderId) { ... }
```

Mapea a la sección `featureFlags[]` del grafo. Permite solo si el flag se encuentra y `isEnabled == true`. (Nota: este atributo es conceptualmente un feature toggle, no estrictamente autorización — pero vive junto a los otros porque el patrón de integración es idéntico.)

---

## 5. Wiring

### 5.1 Con Shell.AOP y DI

```csharp
services.AddAop(aop => aop
    .AddLogger<SerilogLogger>()
    .AddAspect<AuthorizationAspect>());

services.AddSingleton<IAuthorizationValidator, AuthorizationValidator>();
services.AddScoped<IAuthGraphAccessor, HttpContextAuthGraphAccessor>();

// Aplicar AOP a un servicio backed por interfaz:
services.AddAopProxy<IOrderService, OrderService>();
```

`OrderService` implementa `IOrderService` y carga los atributos. El contenedor DI resuelve `IOrderService` al proxy AOP, que intercepta las llamadas y corre el aspecto.

### 5.2 Sin AOP (checks manuales)

```csharp
public class OrderService : IOrderService
{
    private readonly IAuthorizationValidator _validator;
    private readonly IAuthGraphAccessor _accessor;

    public async Task<Result> ApproveOrderAsync(Guid orderId)
    {
        var decision = _validator.RequireScope(_accessor.Current, "PURCHASE_ORDER.APPROVE");
        if (decision.IsDenied) return Result.Failure(decision.ErrorCode, decision.Reason);
        // ... lógica de negocio
    }
}
```

Útil para controllers/endpoints donde AOP no es apropiado, o para checks de grano fino que no mapean a un solo límite de método.

---

## 6. Integración con el Patrón Result

Cuando `OnDenied = ReturnFailure`, el método debe retornar uno de:
- `Result`
- `Result<T>`
- `Task<Result>`
- `Task<Result<T>>`

El aspecto inspecciona el tipo de retorno al inicio y se niega a attach a métodos incompatibles (fail-fast — un error claro de configuración al inicio gana a una sorpresa de runtime).

El failure retornado usa los códigos de error de [`error-codes.md`](../contracts/error-codes.md): `AUTH_101` para scope faltante, `AUTH_102` para deny explícito, etc.

---

## 7. Modo Audit-Only

Para roll-outs progresivos de nuevas reglas de autorización:

```csharp
services.Configure<AuthorizationOptions>(o => o.Mode = AuthorizationMode.AuditOnly);
```

El aspecto corre sin cambios pero nunca bloquea; las denegaciones se loguean como entradas estructuradas `AuthorizationDeniedEvent`. Una vez que limpiaste las denegaciones surfaceadas, cambia a `AuthorizationMode.Enforce`.

---

## 8. Probando Tu Código Consumidor

`Ums.Sdk.Authorization.Testing` provee `AuthGraphBuilder`:

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

El builder es fluido y produce una instancia `AuthorizationGraph` completamente válida — sin JSON, sin HTTP, sin UMS.

---

## 9. Referencias

- [Quickstart](./quickstart.md) — integración en 5 minutos
- [Schema Overview](../contracts/schema-overview.md)
- [Códigos de Error](../contracts/error-codes.md)
- [Política de Versionado](../contracts/versioning.md)
- [ADR-0073: UMS SDK Multi-Runtime](../../architecture/adrs/0073-ums-sdk-multi-runtime.es.md)
- [Guía de Desarrollador Shell.AOP](../../architecture/shell-libraries/aop.es.md)
- [ADR-0061: Execution Context Accessor](../../architecture/adrs/0061-execution-context-accessor.md)
