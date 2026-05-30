# BeyondNetCode.Shell Libraries — Referencia de Arquitectura

> **Estado:** Production-ready · Adoptado activamente
> **Última revisión:** 2026-05-30
> **Responsable:** Equipo de arquitectura

Las shell libraries son paquetes NuGet internos compartidos a través de todos los bounded contexts de UMS. Se consumen como `PackageReference` desde `github.com/beyondnetcode/Shell.*` — nunca como project references locales.

---

## Catálogo de Librerías

| Librería | Paquetes NuGet | Propósito | Estado en UMS API |
|---|---|---|---|
| [`BeyondNetCode.Shell.Ddd`](ddd.md) | `BeyondNetCode.Shell.Ddd` · `BeyondNetCode.Shell.Ddd.ValueObjects` | Tipos base DDD: Entity, AggregateRoot, ValueObject, DomainEvent, BrokenRules | ✅ **Fundación core** — cada aggregate la extiende |
| [`BeyondNetCode.Shell.Factory`](factory.md) | `BeyondNetCode.Shell.Factory` · `BeyondNetCode.Shell.DI` | Abstract factory basada en selector con DSL fluente | ✅ **Activo** — transitivo vía BeyondNetCode.Shell.Ddd; disponible para todas las capas |
| [`BeyondNetCode.Shell.Aop`](aop.md) | `BeyondNetCode.Shell.Aop` · `BeyondNetCode.Shell.DispatchProxy` · `BeyondNetCode.Shell.Aspects` · `BeyondNetCode.Shell.Logger.Serilog` · `BeyondNetCode.Shell.DI` | AOP basado en DispatchProxy: logging, retry, advice aspects | ✅ **Activo** — wireado a `CreateTenantCommandHandler`; expandir a otros handlers |
| [`BeyondNetCode.Shell.Bootstrapper`](bootstrapper.md) | `BeyondNetCode.Shell.Bootstrapper` · `BeyondNetCode.Shell.DI` · `BeyondNetCode.Shell.Observability` | Pipeline de inicialización composable (patrón Composite) | 🔵 **Disponible** — usar para cadenas de startup multi-paso complejas |

---

## Layout de Paquetes NuGet

```
github.com/beyondnetcode/Shell.Ddd/
├── BeyondNetCode.Shell.Ddd              ← Entity, AggregateRoot, ValueObject, DomainEvent, DomainEnumeration
├── BeyondNetCode.Shell.Ddd.ValueObjects ← AuditValueObject, StringValueObject, IntValueObject, BoolValueObject, DecimalValueObject

github.com/beyondnetcode/Shell.Factory/
├── BeyondNetCode.Shell.Factory          ← IFactory, AbstractFactorySetupSource, fluent DSL
├── BeyondNetCode.Shell.DI               ← AddFactory() DI extension

github.com/beyondnetcode/Shell.Aop/
├── BeyondNetCode.Shell.Aop              ← IAspect, IJoinPoint, IPointCut, OnMethodBoundaryAspect, OnRetryAspect
├── BeyondNetCode.Shell.DispatchProxy    ← AopProxy<TService,TImpl>, AopProxyCreator
├── BeyondNetCode.Shell.Aspects          ← LoggerAspect, AdviceAspect, RetryAspect + attributes
├── BeyondNetCode.Shell.Logger.Serilog   ← SerilogLogger : ILogger (AOP)
├── BeyondNetCode.Shell.DI               ← AddAop(), AddAopProxy<>()

github.com/beyondnetcode/Shell.Bootstrapper/
├── BeyondNetCode.Shell.Bootstrapper     ← IBootstrapper, IBootstrapperAsync, CompositeBootstrapper
├── BeyondNetCode.Shell.DI               ← DependencyInjectionBootstrapper
├── BeyondNetCode.Shell.Observability    ← ObservabilityBootstrapper + ObservabilityConfiguration
```

---

## Grafo de Dependencias

```
Ums.Domain
  └── BeyondNetCode.Shell.Ddd              ← Entity, AggregateRoot
        └── BeyondNetCode.Shell.Factory    ← IFactory (transitivo)
              └── BeyondNetCode.Shell.Ddd.ValueObjects  ← AuditValueObject

Ums.Application
  ├── Ums.Domain (arriba)
  └── BeyondNetCode.Shell.Aspects          ← [LoggerAspect], [RetryAspect] contrato de atributos

Ums.Infrastructure
  ├── Ums.Application (arriba)
  ├── BeyondNetCode.Shell.DI               ← AddAop(), AddAopProxy<>()
  └── BeyondNetCode.Shell.Logger.Serilog   ← SerilogLogger
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
| [bootstrapper.md](bootstrapper.md) | IBootstrapper · CompositeBootstrapper · DI/Observability bootstrappers |
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
| PII en Serilog | `SensitiveDataResolver` + atributo `[Sensitive]` | `BeyondNetCode.Shell.Logger.Serilog` |
| Pureza de Domain | `Ums.Domain.csproj` tiene cero referencias NuGet de dominio | AGENTS.md |
| Orden de Factory | `FactorySetupProvider` evalúa predicados en orden de declaración | `BeyondNetCode.Shell.Factory/Impl/FactorySetupProvider.cs` |
| Aspectos async | `OnMethodBoundaryAspect` unwraps `Task`/`Task<T>` vía continuación | `BeyondNetCode.Shell.Aop/Impl/OnMethodBoundaryAspect.cs` |

---

## Checklist de Validación

Ejecutar esto antes de cualquier PR que toque código que use las shell libraries:

```bash
# Desde src/
cd apps/ums.api
dotnet build Ums.sln
dotnet test Ums.sln --verbosity minimal
```

Esperado: **0 errores de build**, **0 fallos de tests** en todas las suites.