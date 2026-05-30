# Ums.Shell.Factory -- Guia del Desarrollador

> **Parte de:** [Shell Libraries](README.es.md)  
> **Proyectos:** `Ums.Shell.Factory` · `Ums.Shell.Factory.Installer`  
> **Dependencias:** Ninguna (BCL pura)

`Ums.Shell.Factory` es una **fabrica abstracta basada en selectores** con un DSL fluido. Dado un objeto de contexto (el "objetivo") y una interfaz de servicio, evalua un conjunto de reglas predicate registradas e instancia el subconjunto de implementaciones cuya condicion `When(...)` coincide.

---

## Tabla de Contenidos

1. [Cuando Usar](#1-cuando-usar)
2. [Estructura del Proyecto](#2-estructura-del-proyecto)
3. [Conceptos Core](#3-conceptos-core)
4. [Uso Standalone](#4-uso-standalone)
5. [Uso DI con `AddFactory()`](#5-uso-di-con-addfactory)
6. [Grupos Nominados](#6-grupos-nominados)
7. [Hooks de Interceptor](#7-hooks-de-interceptor)
8. [Referencia API](#8-referencia-api)
9. [Ejemplos de Integracion UMS](#9-ejemplos-de-integracion-ums)
10. [FAQ](#10-faq)

---

## 1. Cuando Usar

Usa `Ums.Shell.Factory` cuando:

- Necesitas elegir una **estrategia / handler / loader** diferente basada en el estado runtime de un objeto de dominio.
- El conjunto de implementaciones es **open-closed** -- se pueden agregar nuevas sin modificar la fabrica.
- La logica de seleccion es un **predicate simple** sobre el contexto (sin grafos complejos, sin gestion de scope).

**No** lo uses cuando:
- Solo necesitas una implementacion fija (usa DI directamente).
- La seleccion requiere orquestacion compleja (prefiere MediatR pipeline o `IStrategyPattern` de la capa Application).
- Necesitas scope padre-hijo (usa `IServiceScopeFactory` de DI en su lugar).

---

## 2. Estructura del Proyecto

```
Ums.Shell.Factory/
├── Interface/
│   ├── IFactory.cs                ← punto de entrada principal
│   ├── IFactoryCreator.cs         ← delegado de instanciacion
│   ├── IFactoryInterceptor.cs     ← hooks de ciclo de vida
│   ├── IFactorySetupProvider.cs   ← agrega todas las fuentes
│   ├── IFactorySetupSource.cs     ← una unidad de configuracion
│   └── IFactoryBuilder.cs         ← builder DI (solo Installer)
├── Fluent/
│   └── Interface/
│       ├── IFactoryRecordSetupCreateBuilder.cs   ← .Create<TImpl>()
│       ├── IFactoryRecordSetupWhenBuilder.cs     ← .When(predicate)
│       └── IFactoryRecordSetupGroupCreateBuilder.cs ← grupos nominados
├── Impl/
│   ├── AbstractFactorySetupSource.cs  ← clase base para tus fuentes de setup
│   ├── AbstractFactoryInterceptor.cs  ← base para interceptores custom
│   ├── Factory.cs                     ← implementacion de IFactory
│   ├── FactoryCreator.cs              ← wrapper de Func<Type,object>
│   └── FactorySetupProvider.cs        ← motor de filtro de predicates
└── Model/
    ├── Setup.cs                        ← List<SetupItem>
    └── SetupItem.cs                    ← (TargetType, ImplType, ServiceType, Name, Selector)

Ums.Shell.Factory.Installer/
├── Extensions/
│   ├── ServiceCollectionExtensions.cs ← AddFactory(Action<IFactoryBuilder>?)
│   └── ServiceProviderExtensions.cs   ← GetFactory()
└── Impl/
    └── FactoryBuilder.cs              ← implementacion de IFactoryBuilder
```

---

## 3. Conceptos Core

### Setup Source -- el libro de reglas

Extiende `AbstractFactorySetupSource` y declara reglas en su constructor usando el DSL fluido:

```
For<TTarget, TService>()
    .Create<TImplementation>()
    .When(target => <predicate>);
```

### Factory -- el despachador

`IFactory.Create<TTarget, TService>(target)` evalua todas las reglas registradas contra `target` y devuelve un `TService[]` de implementaciones instanciadas cuyo predicate devolvio `true`.

### FactoryCreator -- el instanciador

`IFactoryCreator` envuelve un `Func<Type, object>`. En produccion esto es `sp.GetRequiredService`; en tests es `Activator.CreateInstance`.

### Flujo de ejecucion

```
IFactory.Create<TTarget, TService>(target, name?)
    ↓
IFactoryInterceptor.OnEntry(target, name)
    ↓
IFactorySetupProvider.Provide<TTarget, TService>(target, name)  ← filtra por predicate
    ↓ [por cada SetupItem que coincide]
IFactoryCreator.Create<TService>(implType)                       ← instancia
    ↓
IFactoryInterceptor.OnSuccess(target, name, services)
    ↓
return TService[]
```

---

## 4. Uso Standalone

No requiere DI. Cablea todo manualmente (perfecto para tests unitarios o herramientas de consola).

### Paso 1 -- Define una fuente de setup

```csharp
using Ums.Shell.Factory.Impl;

// Contexto de dominio
public record Discount(int CustomerAge, bool IsPremium);

// Interfaz de servicio
public interface IDiscountStrategy { decimal Apply(decimal price); }

// Implementaciones
public class SeniorDiscount   : IDiscountStrategy { public decimal Apply(decimal p) => p * 0.80m; }
public class PremiumDiscount  : IDiscountStrategy { public decimal Apply(decimal p) => p * 0.85m; }
public class StandardDiscount : IDiscountStrategy { public decimal Apply(decimal p) => p * 0.95m; }

// Fuente de setup -- reglas declaradas en el constructor
public class DiscountFactorySetup : AbstractFactorySetupSource
{
    public DiscountFactorySetup()
    {
        For<Discount, IDiscountStrategy>()
            .Create<SeniorDiscount>()
            .When(d => d.CustomerAge >= 65);

        For<Discount, IDiscountStrategy>()
            .Create<PremiumDiscount>()
            .When(d => d.IsPremium);

        For<Discount, IDiscountStrategy>()
            .Create<StandardDiscount>()
            .When(d => d.CustomerAge < 65 && !d.IsPremium);
    }
}
```

### Paso 2 -- Construye la fabrica

```csharp
using Ums.Shell.Factory.Impl;
using Ums.Shell.Factory.Interfaces;

// FactoryCreator usa Activator para escenarios sin DI
var creator = new FactoryCreator(
    type => Activator.CreateInstance(type)
            ?? throw new InvalidOperationException($"Cannot create {type.Name}"));

var provider = new FactorySetupProvider(
    new IFactorySetupSource[] { new DiscountFactorySetup() });

IFactory factory = new Factory(provider, creator);
```

### Paso 3 -- Usa la fabrica

```csharp
var senior  = new Discount(CustomerAge: 70, IsPremium: false);
var premium = new Discount(CustomerAge: 30, IsPremium: true);
var regular = new Discount(CustomerAge: 40, IsPremium: false);

// Cliente mayor: SeniorDiscount dispara
var strategies = factory.Create<Discount, IDiscountStrategy>(senior);
// strategies.Length == 1  →  SeniorDiscount
var finalPrice = strategies[0].Apply(100m);  // 80m

// Cliente premium: PremiumDiscount dispara
var ps = factory.Create<Discount, IDiscountStrategy>(premium);
// ps.Length == 1  →  PremiumDiscount
ps[0].Apply(100m); // 85m

// Cliente regular: StandardDiscount dispara
var rs = factory.Create<Discount, IDiscountStrategy>(regular);
// rs.Length == 1  →  StandardDiscount
```

> **Multiples coincidencias son intencionales.** Si un mayor también es premium, tanto `SeniorDiscount` COMO `PremiumDiscount` se devuelven. `Create(...)` devuelve `TService[]` -- el caller decide cual aplicar.

---

## 5. Uso DI con `AddFactory()`

```csharp
// Program.cs / DependencyInjection.cs
services.AddFactory(builder =>
{
    // Registra tus fuentes de setup (donde viven las reglas)
    builder.AddSource<DiscountFactorySetup>();

    // Registra todas las implementaciones concretas que la fabrica puede crear
    builder.AddTransient<IDiscountStrategy, SeniorDiscount>();
    builder.AddTransient<IDiscountStrategy, PremiumDiscount>();
    builder.AddTransient<IDiscountStrategy, StandardDiscount>();
});

// Inyecta y usa
public class PriceService(IFactory factory)
{
    public decimal CalculateFinalPrice(Discount discount, decimal basePrice)
    {
        var strategies = factory.Create<Discount, IDiscountStrategy>(discount);

        return strategies.Length == 0
            ? basePrice
            : strategies.Aggregate(basePrice, (price, s) => s.Apply(price));
    }
}
```

`IFactory`, `IFactorySetupProvider`, y `IFactoryCreator` se registran todos como singletons por `AddFactory()`.

---

## 6. Grupos Nominados

Cuando necesitas multiples grupos de fabricas independientes para el mismo par `(TTarget, TService)`, usa grupos nominados:

```csharp
public class OrderFulfillmentSetup : AbstractFactorySetupSource
{
    public OrderFulfillmentSetup()
    {
        // Grupo "primary"
        For<Order, IFulfillmentChannel>("primary", group =>
        {
            group.Create<EmailFulfillment>().When(o => o.Props.HasEmail);
            group.Create<SmsFulfillment>().When(o => o.Props.HasPhone);
        });

        // Grupo "backup"
        For<Order, IFulfillmentChannel>("backup", group =>
        {
            group.Create<PostalFulfillment>().When(o => o.Props.HasAddress);
        });
    }
}

// Resuelve por nombre
var primary = factory.Create<Order, IFulfillmentChannel>(order, "primary");
var backup  = factory.Create<Order, IFulfillmentChannel>(order, "backup");
```

---

## 7. Hooks de Interceptor

`IFactoryInterceptor` proporciona hooks de ciclo de vida utiles para logging, metricas o tracing.

```csharp
public class FactoryLoggingInterceptor(ILogger<FactoryLoggingInterceptor> logger)
    : AbstractFactoryInterceptor
{
    public override void OnEntry<TTarget>(TTarget target, string name)
        => logger.LogDebug("Factory.Create {Name} for {Target}", name, typeof(TTarget).Name);

    public override void OnSuccess<TTarget, TService>(TTarget target, string name, IList<TService> services)
        => logger.LogDebug("Factory resolved {Count} {Service}", services.Count, typeof(TService).Name);

    public override void OnError<TTarget, TService>(TTarget target, string name, IList<TService> services, Exception ex)
        => logger.LogError(ex, "Factory.Create failed for {Target}/{Name}", typeof(TTarget).Name, name);

    public override void OnExit<TTarget, TService>(TTarget target, string name, IList<TService> services)
        => logger.LogDebug("Factory.Create completed");
}

// Cablea via DI
services.AddFactory();
services.AddSingleton<IFactoryInterceptor, FactoryLoggingInterceptor>();

// O establece directamente en la fabrica (modo standalone)
factory.Interceptor = new FactoryLoggingInterceptor(logger);
```

---

## 8. Referencia API

### `IFactory`

| Metodo | Descripcion |
|---|---|
| `Create<TTarget, TService>(target)` | Evalua todas las reglas e instancia las implementaciones que coinciden |
| `Create<TTarget, TService>(target, name)` | Igual, filtrado al grupo nominado |
| `ConfigurationFor<TTarget, TService>(target)` | Devuelve los `SetupItem[]` que coinciden sin instanciar |
| `ConfigurationFor<TTarget, TService>(target, name)` | Igual, grupo nominado |
| `IFactoryInterceptor Interceptor { get; set; }` | Hook de ciclo de vida (default: no-op) |

### `AbstractFactorySetupSource`

| Metodo | Descripcion |
|---|---|
| `For<TTarget, TService>()` | Inicia una cadena de reglas simple → `.Create<TImpl>().When(pred)` |
| `For<TTarget, TService>(name, action)` | Inicia una cadena de reglas de grupo nominado |

### `IFactoryCreator`

| Metodo | Descripcion |
|---|---|
| `T Create<T>(Type type)` | Instancia `type` y hace cast a `T`; lanza excepcion si es null o tipo incorrecto |

### `ServiceCollectionExtensions.AddFactory()`

Registra: `IFactoryCreator` (respaldado por `sp.GetRequiredService`), `IFactory`, `IFactorySetupProvider`.

```csharp
services.AddFactory(builder =>
{
    builder.AddSource<MySetupSource>();           // registra IFactorySetupSource
    builder.AddSingleton<IMyService, ImplA>();    // registra ImplA
    builder.AddTransient<IMyService, ImplB>();    // registra ImplB
});
```

---

## 9. Ejemplos de Integracion UMS

### Estrategia de cumplimiento (Dominio → Infraestructura)

```csharp
// En una fuente de setup registrada en Ums.Infrastructure/DependencyInjection.cs
public class FulfillmentFactorySetup : AbstractFactorySetupSource
{
    public FulfillmentFactorySetup()
    {
        For<Tenant, IProvisioningStrategy>()
            .Create<InternalProvisioningStrategy>()
            .When(t => t.Props.OrganizationType == OrganizationType.INTERNAL);

        For<Tenant, IProvisioningStrategy>()
            .Create<ExternalProvisioningStrategy>()
            .When(t => t.Props.OrganizationType == OrganizationType.EXTERNAL);
    }
}

// En un command handler
public class ProvisionTenantCommandHandler(IFactory factory, ITenantRepository repo)
{
    public async Task<Result> Handle(ProvisionTenantCommand cmd, CancellationToken ct)
    {
        var tenant = await repo.GetByIdAsync(cmd.TenantId, ct);
        var strategies = factory.Create<Tenant, IProvisioningStrategy>(tenant!);

        foreach (var strategy in strategies)
            await strategy.ProvisionAsync(tenant!, ct);

        return Result.Success();
    }
}
```

### Enrutamiento de aprobaciones

```csharp
public class ApprovalRouteSetup : AbstractFactorySetupSource
{
    public ApprovalRouteSetup()
    {
        For<ApprovalRequest, IApprovalRouter>()
            .Create<ManagerApprovalRouter>()
            .When(r => r.Props.RiskScore < 50);

        For<ApprovalRequest, IApprovalRouter>()
            .Create<CommitteeApprovalRouter>()
            .When(r => r.Props.RiskScore >= 50 && r.Props.RiskScore < 80);

        For<ApprovalRequest, IApprovalRouter>()
            .Create<BoardApprovalRouter>()
            .When(r => r.Props.RiskScore >= 80);
    }
}
```

---

## 10. FAQ

**P: Que pasa si ningun predicate coincide?**  
`Create(...)` devuelve un array vacio `TService[0]`. Siempre verifica `.Length > 0` antes de usar el resultado.

**P: Las implementaciones se comparten o se crean frescas cada vez?**  
Controlado enteramente por el lifetime de DI. `AddSingleton` → compartidas; `AddTransient` → frescas por llamada.

**P: Los predicates pueden ser async?**  
No. Los predicates son `Func<TTarget, bool>` sincronicos. Para condiciones async, resuelve el resultado antes de llamar a la fabrica.

**P: Puedo tener multiples fuentes de setup?**  
Si. Todas las fuentes registradas via `AddSource<>()` se fusionan en un solo `FactorySetupProvider`.

---

## Documentos Relacionados

- [DDD](ddd.es.md) -- objetos de dominio usados como `TTarget`
- [AOP](aop.es.md) -- agrega logging a servicios resueltos por la fabrica con `[LoggerAspect]`
- [Uso Combinado](combined-usage.md) -- Factory + DDD + AOP + Bootstrapper en un ejemplo