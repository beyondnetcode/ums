# Arquitectura de Librerías Shell

**Tipo:** Blueprint de Arquitectura  
**Estado:** Aceptado · Enmendado 2026-05-24 — librerías AOP y Bootstrapper agregadas  
**Runtime:** .NET 10 LTS  
**Ubicación en código:** `src/libs/shell`

## Propósito

UMS aísla los patrones reutilizables de implementación en una **Capa de Librerías Shell**. Esta capa encapsula y normaliza código heredado bajo namespaces propios de UMS para que la aplicación use patrones DDD, Factory, AOP y Bootstrapper sin filtrar nombres, estructura de repositorio ni detalles internos de la fuente original.

La capa shell no es una carpeta genérica de utilidades. Es una frontera arquitectónica con cuatro grupos de librerías distintos:

| Grupo | Responsabilidad |
|-------|-----------------|
| `BeyondNetCode.Shell.Ddd` | Primitivas tácticas DDD: entidades, aggregate roots, eventos de dominio, value objects, especificaciones, convenciones de resultado/error |
| `BeyondNetCode.Shell.Ddd.ValueObjects` | Patrones reutilizables de value objects construidos sobre el shell DDD |
| `BeyondNetCode.Shell.Factory` | Patrones de creación y resolución usados por el shell DDD y el modelo de dominio |
| `BeyondNetCode.Shell.Aop` | Preocupaciones transversales dirigidas por atributos vía `DispatchProxy`: logging, trazabilidad, métricas, reintentos |
| `BeyondNetCode.Shell.Bootstrapper` | Orquestación de inicio de aplicación: DI, AutoMapper, observabilidad |

Los namespaces heredados de la fuente original no deben aparecer en el código de aplicación UMS.

---

## Diagrama de Dependencias

```mermaid
flowchart TD
    A["Ums.Presentation<br/>Minimal API (.NET 10)"] --> B["Ums.Application"]
    A --> C["Ums.Infrastructure"]
    C --> B
    B --> D["Ums.Domain"]
    C --> D

    %% Dominio → shell DDD (Factory es transitivo vía Ddd)
    D --> E["BeyondNetCode.Shell.Ddd"]
    D --> F["BeyondNetCode.Shell.Ddd.ValueObjects"]
    F --> E
    E --> G["BeyondNetCode.Shell.Factory"]

    %% Application → solo contrato de atributos AOP
    B --> H["BeyondNetCode.Shell.Aspects<br/>(contrato de atributos)"]

    %% Infrastructure → cableado AOP completo + Bootstrapper
    C --> I["BeyondNetCode.Shell.Aop.Microsoft.Extensions<br/>.DependencyInjection.Aspects.Installer"]
    C --> J["BeyondNetCode.Shell.Logger.Serilog"]
    I --> K["BeyondNetCode.Shell.Aspects"]
    K --> L["BeyondNetCode.Shell.Aop"]
    L --> M["BeyondNetCode.Shell.DispatchProxy"]
    H --> L

    %% Presentation/Infrastructure → Bootstrapper
    A --> N["BeyondNetCode.Shell.DI"]
    C --> N
    N --> O["BeyondNetCode.Shell.Bootstrapper"]
    N --> P["BeyondNetCode.Shell.AutoMapper"]
    N --> Q["BeyondNetCode.Shell.Observability"]

    subgraph Apps["src/apps/ums.api"]
      A
      B
      C
      D
    end

    subgraph Shell["src/libs/shell"]
      E
      F
      G
      H
      I
      J
      K
      L
      M
      N
      O
      P
      Q
    end
```

---

## Grupos de Librerías

### BeyondNetCode.Shell.Ddd

Provee las primitivas tácticas DDD base. Todos los agregados, entidades y value objects del dominio extienden estos tipos base.

**Proyectos:**
- `BeyondNetCode.Shell.Ddd` — `IAggregateRoot`, `Entity`, `AggregateRoot`, `ValueObject<T>`, `DomainEvent`, `DomainEnumeration`, `BrokenRules`, `TrackingState`
- `BeyondNetCode.Shell.Ddd.ValueObjects` — `AuditValueObject`, `IdValueObject` y otros patrones VO reutilizables

**Consumida por:** `Ums.Domain` (directo), `BeyondNetCode.Shell.Ddd.ValueObjects` (extiende Ddd)

```xml
<!-- Ums.Domain.csproj -->
<ProjectReference Include="../../../libs/shell/ddd/src/BeyondNetCode.Shell.Ddd/BeyondNetCode.Shell.Ddd.csproj" />
<ProjectReference Include="../../../libs/shell/ddd/src/BeyondNetCode.Shell.Ddd.ValueObjects/BeyondNetCode.Shell.Ddd.ValueObjects.csproj" />
```

---

### BeyondNetCode.Shell.Factory

Provee patrones de factory/resolución fluentes utilizados internamente por el shell DDD y opcionalmente por Infrastructure.

**Proyectos:**
- `BeyondNetCode.Shell.Factory` — `AbstractFactorySetupSource`, DSL `For<TTarget, TService>().Create<TImpl>().When(pred)`, `IFactoryInterceptor`, grupos de factory nombrados
- `BeyondNetCode.Shell.DI` — extensión DI `AddFactory()`, escaneo de grupos de factory

**Consumida por:** `BeyondNetCode.Shell.Ddd` (transitivo — Domain lo recibe vía DDD shell, no directamente)

> **Importante:** `Ums.Domain.csproj` **no debe** referenciar `BeyondNetCode.Shell.Factory` directamente. La referencia es transitiva a través de `BeyondNetCode.Shell.Ddd`. Ver ADR-0054 (corrección 2026-05-24).

---

### BeyondNetCode.Shell.Aop

Provee AOP dirigido por atributos mediante `System.Reflection.DispatchProxy`. Aplica preocupaciones transversales selectivas, por método, sin modificar la lógica de negocio del handler.

**Proyectos:**
- `BeyondNetCode.Shell.Aop` — `IAspect`, `IJoinPoint`, `IPointCut`, `AspectExecutor`, `AopProxy`
- `BeyondNetCode.Shell.DispatchProxy` — implementación `DispatchProxy`, fábrica de proxies
- `BeyondNetCode.Shell.Aspects` — `OnMethodBoundaryAspect<T>`, `LoggerAspect`, `RetryAspect`, `AdviceAspect`, interfaz `ILogger`, atributo `[LoggerAspect]`
- `BeyondNetCode.Shell.Logger.Serilog` — adaptador `SerilogLogger` (valores destructurados, opt-in)
- `BeyondNetCode.Shell.DI` — `AddAop()`, `AddAopProxy<TService, TImpl>()`

**Consumida por:**
- `Ums.Application` — solo contrato de atributos (`BeyondNetCode.Shell.Aspects`): los handlers declaran `[LoggerAspect]` sin acoplarse a la infraestructura de proxy
- `Ums.Infrastructure` — cableado DI completo: `AddAop()`, `AddAopProxy<>()`, adaptador `SerilogLogger`

```xml
<!-- Ums.Application.csproj -->
<ProjectReference Include="../../../libs/shell/aop/src/BeyondNetCode.Shell.Aspects/BeyondNetCode.Shell.Aspects.csproj" />

<!-- Ums.Infrastructure.csproj -->
<ProjectReference Include="../../../libs/shell/aop/src/BeyondNetCode.Shell.DI/..." />
<ProjectReference Include="../../../libs/shell/aop/src/BeyondNetCode.Shell.Logger.Serilog/..." />
```

**Corrección async:** `OnMethodBoundaryAspect.Apply` detecta tipos de retorno `Task`/`Task<TResult>` y los envuelve en tareas de continuación vía `ConfigureAwait(false)`. `OnSuccess` y `OnExit` se disparan *después* del resultado esperado, no cuando se devuelve el objeto `Task`.

**Patrón MelLogger:** `IMelLogger` (interfaz marcadora en `Ums.Application.Common.Aop`) extiende `BeyondNetCode.Shell.Aspects.ILogger`. `MelLogger` en `Ums.Infrastructure.Aop` lo implementa vía `ILoggerFactory`. Política PII: los valores de argumentos **nunca** se registran; solo nombres de métodos y tipos.

```csharp
// Capa Application — declaración de atributo (sin importar proxy)
[LoggerAspect(Type = typeof(IMelLogger), LogDuration = true, LogException = true, LogArguments = [])]
public async Task<Result<CreateTenantResponse>> Handle(CreateTenantCommand request, CancellationToken ct)
{ ... }

// Cableado DI en Infrastructure
services.AddAop();
services.AddKeyedTransient<BeyondNetCode.Shell.Aspects.ILogger, MelLogger>(typeof(IMelLogger));
services.AddAopProxy<IRequestHandler<CreateTenantCommand, Result<CreateTenantResponse>>,
                     CreateTenantCommandHandler>();
```

---

### BeyondNetCode.Shell.Bootstrapper

Provee orquestación composable del inicio de la aplicación. Separa responsabilidades (DI, mapping, observabilidad) en unidades de bootstrapper independientes.

**Proyectos:**
- `BeyondNetCode.Shell.Bootstrapper` — `IBootstrapper<T>`, `CompositeBootstrapper` (fan-out)
- `BeyondNetCode.Shell.DI` — `DependencyInjectionBootstrapper` (registra servicios desde ensamblados)
- `BeyondNetCode.Shell.AutoMapper` — `AutoMapperBootstrapper` (escaneo de perfiles + registro de `IMapper`)
- `BeyondNetCode.Shell.Observability` — `ObservabilityBootstrapper`, `ObservabilityConfiguration` (endpoint OTLP, nombre de servicio, tasa de muestreo)

**Consumida por:** `Ums.Infrastructure` y `Ums.Presentation` (solo inicio)

```csharp
// Ejemplo de inicio compuesto
var bootstrapper = new CompositeBootstrapper<IServiceCollection>(
    new DependencyInjectionBootstrapper(Assembly.GetExecutingAssembly()),
    new AutoMapperBootstrapper(Assembly.GetExecutingAssembly()),
    new ObservabilityBootstrapper(new ObservabilityConfiguration
    {
        ServiceName   = "ums-api",
        OtlpEndpoint  = "http://localhost:4317",
        SamplingRatio = 1.0
    }));
bootstrapper.Bootstrap(services);
```

---

## Reglas Arquitectónicas

| Regla | Decisión |
|-------|----------|
| Propiedad de namespace | Las librerías shell usan `BeyondNetCode.Shell.*`; los namespaces heredados (`BeyondNet.*`, `csdevlib.*`) no se permiten en el código de aplicación UMS. |
| Runtime base | Las librerías shell apuntan al mismo runtime estable que la API: `net10.0`. |
| Pureza de dominio | `Ums.Domain` **no debe** referenciar `BeyondNetCode.Shell.Aop.*`, `BeyondNetCode.Shell.Bootstrapper.*` ni `BeyondNetCode.Shell.Factory` directamente. |
| Contrato AOP en Application | `Ums.Application` referencia únicamente `BeyondNetCode.Shell.Aspects` (declaraciones de atributos). Sin proxy, sin instalador DI, sin infraestructura de runtime. |
| Cableado en Infrastructure | `Ums.Infrastructure` gestiona el registro de proxies AOP y el cableado de inicio con Bootstrapper. |
| Encapsulación de patrones | Los detalles de DDD, Factory, AOP y Bootstrapper viven centralizados en shell libraries, no copiados en cada bounded context. |
| Estrategia de reemplazo | Si una fuente upstream cambia, UMS adapta el cambio dentro de `src/libs/shell`; las capas de aplicación no deberían cambiar por movimiento interno upstream. |
| Requisito cross-platform | Las referencias de proyecto usan rutas relativas portables y proyectos SDK-style de .NET. No se permiten rutas de build específicas de sistema operativo. |

### Grafo de referencias autorizado (resumen)

```
Ums.Domain       → BeyondNetCode.Shell.Ddd, BeyondNetCode.Shell.Ddd.ValueObjects
Ums.Application  → Ums.Domain, BeyondNetCode.Shell.Aspects (solo contrato de atributos)
Ums.Infrastructure → Ums.Application, Ums.Domain,
                     BeyondNetCode.Shell.Aop.*.Installer, BeyondNetCode.Shell.Logger.Serilog,
                     BeyondNetCode.Shell.Bootstrapper.*
Ums.Presentation → Todas las capas + BeyondNetCode.Shell.Bootstrapper.* (inicio)
```

---

## Validaciones

Ejecutar después de cualquier cambio en referencias de librerías shell o registros de aspectos:

```bash
# 1. Compilar la solución completa
dotnet build src/apps/ums.api/Ums.sln

# 2. Ejecutar tests de librerías shell
dotnet test src/libs/shell/aop/src/BeyondNetCode.Shell.Aop.Tests/BeyondNetCode.Shell.Aop.Tests.csproj --verbosity minimal
dotnet test src/libs/shell/factory/src/BeyondNetCode.Shell.Factory.Test/BeyondNetCode.Shell.Factory.Test.csproj --verbosity minimal

# 3. Verificar pureza de Domain — sin refs AOP
grep -r "BeyondNetCode.Shell.Aop" src/apps/ums.api/Ums.Domain/ --include="*.csproj"
# Esperado: sin salida

# 4. Verificar sin referencia directa a Factory en Domain
grep "BeyondNetCode.Shell.Factory" src/apps/ums.api/Ums.Domain/Ums.Domain.csproj
# Esperado: sin salida
```

---

## Decisiones y Guías Relacionadas

- [ADR-0054: Aislamiento de Librerías Shell — DDD, Factory, AOP, Bootstrapper](../adrs/0054-shell-library-isolation.md)
- [ADR-0060: Estrategia de Preocupaciones Transversales con AOP](../adrs/0060-aop-cross-cutting-concern-strategy.md)
- [Guías de Desarrollo Shell Libraries](../shell-libraries/README.md) — DDD · Factory · AOP · Bootstrapper · Uso Combinado
- [Primitivas DDD](../../governance/construction/ddd-design/11-ddd-primitives.md)
- [Portal de Arquitectura](../index.es.md)
