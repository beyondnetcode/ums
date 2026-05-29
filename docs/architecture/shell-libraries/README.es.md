# UMS Shell Libraries — Referencia de Arquitectura

> **Estado:** Production-ready · Adoptado activamente
> **Última revisión:** 2026-05-24
> **Responsable:** Equipo de arquitectura

Las shell libraries son librerías .NET internas compartidas a través de todos los bounded contexts de UMS. Viven en `src/libs/shell/` y se consumen como project references — nunca como paquetes NuGet.

---

## Catálogo de Librerías

| Librería | Proyectos | Propósito | Estado en UMS API |
|---|---|---|---|
| [`Ums.Shell.Ddd`](ddd.md) | `Ums.Shell.Ddd` · `Ums.Shell.Ddd.ValueObjects` | Tipos base DDD: Entity, AggregateRoot, ValueObject, DomainEvent, BrokenRules | ✅ **Fundación core** — cada aggregate la extiende |
| [`Ums.Shell.Factory`](factory.md) | `Ums.Shell.Factory` · `Ums.Shell.Factory.Installer` | Abstract factory basada en selector con DSL fluente | ✅ **Activo** — transitivo vía Ums.Shell.Ddd; disponible para todas las capas |
| [`Ums.Shell.Aop`](aop.md) | `Ums.Shell.Aop` · `Ums.Shell.Aop.DispatchProxy` · `Ums.Shell.Aop.Aspects` · `Ums.Shell.Aop.Aspects.Logger.Serilog` · `Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer` | AOP basado en DispatchProxy: logging, retry, advice aspects | ✅ **Activo** — wireado a `CreateTenantCommandHandler`; expandir a otros handlers |
| [`Ums.Shell.Bootstrapper`](bootstrapper.md) | `Ums.Shell.Bootstrapper` · `Ums.Shell.Bootstrapper.DependencyInjection` · `Ums.Shell.Bootstrapper.AutoMapper` · `Ums.Shell.Bootstrapper.Observability` | Pipeline de inicialización composable (patrón Composite) | 🔵 **Disponible** — usar para cadenas de startup multi-paso complejas |

---

## Layout Físico

```
src/libs/shell/
├── ddd/
│   └── src/
│       ├── Ums.Shell.Ddd/                   ← Entity, AggregateRoot, ValueObject, DomainEvent, DomainEnumeration
│       ├── Ums.Shell.Ddd.ValueObjects/      ← AuditValueObject, StringValueObject, IntValueObject, BoolValueObject, DecimalValueObject
│       └── Ums.Shell.Ddd.Test/
├── factory/
│   └── src/
│       ├── Ums.Shell.Factory/               ← IFactory, AbstractFactorySetupSource, fluent DSL
│       ├── Ums.Shell.Factory.Installer/     ← AddFactory() DI extension
│       ├── Ums.Shell.Factory.Test/
│       └── Ums.Shell.Factory.Demo/
├── aop/
│   └── src/
│       ├── Ums.Shell.Aop/                   ← IAspect, IJoinPoint, IPointCut, OnMethodBoundaryAspect, OnRetryAspect
│       ├── Ums.Shell.Aop.DispatchProxy/     ← AopProxy<TService,TImpl>, AopProxyCreator
│       ├── Ums.Shell.Aop.Aspects/           ← LoggerAspect, AdviceAspect, RetryAspect + attributes
│       ├── Ums.Shell.Aop.Aspects.Logger/    ← ISerializer, SensitiveDataResolver
│       ├── Ums.Shell.Aop.Aspects.Logger.Serilog/ ← SerilogLogger : ILogger (AOP)
│       ├── Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer/
│       └── Ums.Shell.Aop.Tests/
└── bootstrapper/
    └── src/
        ├── Ums.Shell.Bootstrapper/          ← IBootstrapper, IBootstrapperAsync, CompositeBootstrapper
        ├── Ums.Shell.Bootstrapper.DependencyInjection/  ← DependencyInjectionBootstrapper
        ├── Ums.Shell.Bootstrapper.AutoMapper/            ← AutoMapperBootstrapper
        ├── Ums.Shell.Bootstrapper.Observability/         ← ObservabilityBootstrapper + ObservabilityConfiguration
        └── Ums.Shell.Bootstrapper.Tests/
```

---

## Grafo de Dependencias

```
Ums.Domain
  └── Ums.Shell.Ddd              ← Entity, AggregateRoot
        └── Ums.Shell.Factory    ← IFactory (transitivo)
              └── Ums.Shell.Ddd.ValueObjects  ← AuditValueObject

Ums.Application
  ├── Ums.Domain (arriba)
  └── Ums.Shell.Aop.Aspects      ← [LoggerAspect], [RetryAspect] contrato de atributos

Ums.Infrastructure
  ├── Ums.Application (arriba)
  ├── Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer ← AddAop(), AddAopProxy<>()
  └── Ums.Shell.Aop.Aspects.Logger.Serilog ← SerilogLogger
```

---

## Cómo Trabajan Juntos — Vista de 30 Segundos

```csharp
// 1. DDD: define tu aggregate
public class Order : AggregateRoot<Order, OrderProps> { ... }

// 2. Factory: despacha al handler correcto basado en estado runtime
For<Order, IFulfillmentStrategy>()
    .Create<DigitalFulfillment>().When(o => o.Props.IsDigital);

// 3. AOP: agrega logging cross-cutting al command handler
[LoggerAspect(Type = typeof(IMelLogger), LogDuration = true)]
public async Task<Result<OrderId>> Handle(PlaceOrderCommand cmd, CancellationToken ct) { ... }

// 4. Bootstrapper: compos multi-step startup
new CompositeBootstrapper()
    .Add(new DependencyInjectionBootstrapper(ConfigureServices))
    .Add(new ObservabilityBootstrapper(services, obsConfig))
    .Run();
```

Un ejemplo completo end-to-end combinando las cuatro librerías está en **[combined-usage.md](combined-usage.md)**.

---

## Guías Por Librería

| Documento | Contenidos |
|---|---|
| [ddd.md](ddd.md) | Entity · AggregateRoot · ValueObject · DomainEvent · DomainEnumeration · BrokenRules · TrackingState |
| [factory.md](factory.md) | AbstractFactorySetupSource · fluent DSL · IFactoryInterceptor · DI wiring · Patrones UMS |
| [aop.md](aop.md) | Cadena IAspect · OnMethodBoundaryAspect · LoggerAspect · async proxy gap fix · MelLogger · DI wiring |
| [bootstrapper.md](bootstrapper.md) | IBootstrapper · CompositeBootstrapper · DI/AutoMapper/Observability bootstrappers |
| [combined-usage.md](combined-usage.md) | Ejemplo completo: Aggregate Fulfillment + factory routing + AOP logging + startup bootstrapped |

---

## Convención de Orden de Aspectos

Cuando múltiples aspectos aplican al mismo método:

| Orden | Aspecto | Razón |
|---|---|---|
| 10 | Tracing | Debe capturar el span completo del request |
| 20 | Authorization | Rechazar temprano antes de cualquier trabajo real |
| 30 | Validation | Pre-condiciones de dominio |
| 40 | Idempotency | Verificar dedup store antes de ejecución |
| 50 | Logging (`LoggerAspect`) | Observar la ejecución real |
| 60 | Metrics | Registrar duración/éxito después de logging |
| 70 | Transaction | Wrapper de retry más externo |

Implementar `IAspect.GetOrder(IJoinPoint)` en cada aspecto para retornar la constante apropiada.

---

## Políticas Cross-Cutting

| Concern | Mecanismo | Archivo |
|---|---|---|
| PII en logs | `MelLogger`: log nombres/tipos de parámetros solo, nunca valores | `Ums.Infrastructure/Aop/MelLogger.cs` |
| PII en Serilog | `SensitiveDataResolver` + atributo `[Sensitive]` | `Ums.Shell.Aop.Aspects.Logger/` |
| Pureza de Domain | `Ums.Domain.csproj` tiene cero referencias NuGet | AGENTS.md |
| Orden de Factory | `FactorySetupProvider` evalúa predicados en orden de declaración | `Ums.Shell.Factory/Impl/FactorySetupProvider.cs` |
| Aspectos async | `OnMethodBoundaryAspect` unwraps `Task`/`Task<T>` vía continuación | `Ums.Shell.Aop/Impl/OnMethodBoundaryAspect.cs` |

---

## Checklist de Validación

Ejecutar esto antes de cualquier PR que toque una shell library:

```bash
# Desde raíz del repo
dotnet build src/apps/ums.api/Ums.sln
dotnet test  src/apps/ums.api/Ums.sln --verbosity minimal

dotnet test src/libs/shell/aop/src/Ums.Shell.Aop.Tests/Ums.Shell.Aop.Tests.csproj --verbosity minimal
dotnet test src/libs/shell/factory/src/Ums.Shell.Factory.Test/Ums.Shell.Factory.Test.csproj --verbosity minimal
dotnet test src/libs/shell/ddd/src/Ums.Shell.Ddd.Test/Ums.Shell.Ddd.Test.csproj --verbosity minimal
dotnet test src/libs/shell/bootstrapper/src/Ums.Shell.Bootstrapper.Tests/Ums.Shell.Bootstrapper.Tests.csproj --verbosity minimal
```

Esperado: **0 errores de build**, **0 fallos de tests** en todas las suites.