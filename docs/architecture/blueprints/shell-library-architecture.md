# Shell Library Architecture

**Type:** Architecture Blueprint  
**Status:** Accepted · Amended 2026-05-24 — AOP and Bootstrapper libraries added  
**Runtime:** .NET 10 LTS  
**Code location:** `src/libs/shell`

## Purpose

UMS isolates reusable implementation patterns in a dedicated **Shell Library Layer**. This layer wraps and normalizes inherited library code under the UMS namespace so the application can use DDD, Factory, AOP, and Bootstrapper patterns without leaking upstream naming, repository structure, or implementation details into product code.

The shell layer is not a generic utility folder. It is an architectural boundary with four distinct library groups:

| Group | Responsibility |
|-------|---------------|
| `Ums.Shell.Ddd` | Tactical DDD primitives: entities, aggregate roots, domain events, value objects, specifications, result/error conventions |
| `Ums.Shell.Ddd.ValueObjects` | Reusable value object patterns built on the DDD shell |
| `Ums.Shell.Factory` | Creation and resolution patterns used by the DDD shell and domain model |
| `Ums.Shell.Aop` | Attribute-driven cross-cutting concerns via `DispatchProxy`: logging, tracing, metrics, retry |
| `Ums.Shell.Bootstrapper` | Application startup orchestration: DI, AutoMapper, observability |

Upstream library naming must not appear in application namespaces.

---

## Dependency Diagram

```mermaid
flowchart TD
    A["Ums.Presentation<br/>Minimal API (.NET 10)"] --> B["Ums.Application"]
    A --> C["Ums.Infrastructure"]
    C --> B
    B --> D["Ums.Domain"]
    C --> D

    %% Domain → DDD shell (Factory is transitive via Ddd)
    D --> E["Ums.Shell.Ddd"]
    D --> F["Ums.Shell.Ddd.ValueObjects"]
    F --> E
    E --> G["Ums.Shell.Factory"]

    %% Application → AOP attribute contract only
    B --> H["Ums.Shell.Aop.Aspects<br/>(attribute contract)"]

    %% Infrastructure → full AOP wiring + Bootstrapper
    C --> I["Ums.Shell.Aop.Microsoft.Extensions<br/>.DependencyInjection.Aspects.Installer"]
    C --> J["Ums.Shell.Aop.Aspects.Logger.Serilog"]
    I --> K["Ums.Shell.Aop.Aspects"]
    K --> L["Ums.Shell.Aop"]
    L --> M["Ums.Shell.Aop.DispatchProxy"]
    H --> L

    %% Presentation/Infrastructure → Bootstrapper
    A --> N["Ums.Shell.Bootstrapper.DependencyInjection"]
    C --> N
    N --> O["Ums.Shell.Bootstrapper"]
    N --> P["Ums.Shell.Bootstrapper.AutoMapper"]
    N --> Q["Ums.Shell.Bootstrapper.Observability"]

    %% Upstream isolation
    R["csdevlib/ddd<br/>upstream source"] -. "isolated and renamed" .-> E
    R -. "isolated and renamed" .-> F
    S["csdevlib/factory-pattern<br/>upstream source"] -. "isolated and renamed" .-> G

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

## Library Groups

### Ums.Shell.Ddd

Provides core tactical DDD primitives. All domain aggregates, entities, and value objects extend these base types.

**Projects:**
- `Ums.Shell.Ddd` — `IAggregateRoot`, `Entity`, `AggregateRoot`, `ValueObject<T>`, `DomainEvent`, `DomainEnumeration`, `BrokenRules`, `TrackingState`
- `Ums.Shell.Ddd.ValueObjects` — `AuditValueObject`, `IdValueObject`, and other reusable VO patterns

**Consumed by:** `Ums.Domain` (direct), `Ums.Shell.Ddd.ValueObjects` (extends Ddd)

```xml
<!-- Ums.Domain.csproj -->
<ProjectReference Include="../../../libs/shell/ddd/src/Ums.Shell.Ddd/Ums.Shell.Ddd.csproj" />
<ProjectReference Include="../../../libs/shell/ddd/src/Ums.Shell.Ddd.ValueObjects/Ums.Shell.Ddd.ValueObjects.csproj" />
```

---

### Ums.Shell.Factory

Provides fluent factory/resolution patterns used by the DDD shell internally and optionally by Infrastructure.

**Projects:**
- `Ums.Shell.Factory` — `AbstractFactorySetupSource`, `For<TTarget, TService>().Create<TImpl>().When(pred)` DSL, `IFactoryInterceptor`, named factory groups
- `Ums.Shell.Factory.Installer` — `AddFactory()` DI extension, factory group scanning

**Consumed by:** `Ums.Shell.Ddd` (transitive — Domain gets it via DDD shell, not directly)

> **Important:** `Ums.Domain.csproj` must **not** reference `Ums.Shell.Factory` directly. The reference is transitive through `Ums.Shell.Ddd`. See ADR-0054 (2026-05-24 correction).

---

### Ums.Shell.Aop

Provides attribute-driven AOP via `System.Reflection.DispatchProxy`. Applies selective, per-method cross-cutting concerns without modifying handler business logic.

**Projects:**
- `Ums.Shell.Aop` — `IAspect`, `IJoinPoint`, `IPointCut`, `AspectExecutor`, `AopProxy`
- `Ums.Shell.Aop.DispatchProxy` — `DispatchProxy` implementation, proxy factory
- `Ums.Shell.Aop.Aspects` — `OnMethodBoundaryAspect<T>`, `LoggerAspect`, `RetryAspect`, `AdviceAspect`, `ILogger` interface, `[LoggerAspect]` attribute
- `Ums.Shell.Aop.Aspects.Logger.Serilog` — `SerilogLogger` adapter (destructured values, opt-in)
- `Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer` — `AddAop()`, `AddAopProxy<TService, TImpl>()`

**Consumed by:**
- `Ums.Application` — attribute contract only (`Ums.Shell.Aop.Aspects`): handlers declare `[LoggerAspect]` without coupling to proxy infrastructure
- `Ums.Infrastructure` — full DI wiring: `AddAop()`, `AddAopProxy<>()`, `SerilogLogger` adapter

```xml
<!-- Ums.Application.csproj -->
<ProjectReference Include="../../../libs/shell/aop/src/Ums.Shell.Aop.Aspects/Ums.Shell.Aop.Aspects.csproj" />

<!-- Ums.Infrastructure.csproj -->
<ProjectReference Include="../../../libs/shell/aop/src/Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer/..." />
<ProjectReference Include="../../../libs/shell/aop/src/Ums.Shell.Aop.Aspects.Logger.Serilog/..." />
```

**Async correctness:** `OnMethodBoundaryAspect.Apply` detects `Task`/`Task<TResult>` return types and wraps them in continuation tasks via `ConfigureAwait(false)`. `OnSuccess` and `OnExit` fire *after* the awaited result, not when the `Task` object is returned.

**MelLogger pattern:** `IMelLogger` (marker interface in `Ums.Application.Common.Aop`) extends `Ums.Shell.Aop.Aspects.ILogger`. `MelLogger` in `Ums.Infrastructure.Aop` implements it via `ILoggerFactory`. PII policy: argument values are **never** logged; only method names and types.

```csharp
// Application layer — attribute declaration (no proxy import)
[LoggerAspect(Type = typeof(IMelLogger), LogDuration = true, LogException = true, LogArguments = [])]
public async Task<Result<CreateTenantResponse>> Handle(CreateTenantCommand request, CancellationToken ct)
{ ... }

// Infrastructure DI wiring
services.AddAop();
services.AddKeyedTransient<Ums.Shell.Aop.Aspects.ILogger, MelLogger>(typeof(IMelLogger));
services.AddAopProxy<IRequestHandler<CreateTenantCommand, Result<CreateTenantResponse>>,
                     CreateTenantCommandHandler>();
```

---

### Ums.Shell.Bootstrapper

Provides composable application startup orchestration. Separates concerns (DI, mapping, observability) into independent bootstrapper units.

**Projects:**
- `Ums.Shell.Bootstrapper` — `IBootstrapper<T>`, `CompositeBootstrapper` (fan-out)
- `Ums.Shell.Bootstrapper.DependencyInjection` — `DependencyInjectionBootstrapper` (registers services from assemblies)
- `Ums.Shell.Bootstrapper.AutoMapper` — `AutoMapperBootstrapper` (profile scanning + `IMapper` registration)
- `Ums.Shell.Bootstrapper.Observability` — `ObservabilityBootstrapper`, `ObservabilityConfiguration` (OTLP endpoint, service name, sampling rate)

**Consumed by:** `Ums.Infrastructure` and `Ums.Presentation` (startup only)

```csharp
// Example composite startup
var bootstrapper = new CompositeBootstrapper<IServiceCollection>(
    new DependencyInjectionBootstrapper(Assembly.GetExecutingAssembly()),
    new AutoMapperBootstrapper(Assembly.GetExecutingAssembly()),
    new ObservabilityBootstrapper(new ObservabilityConfiguration
    {
        ServiceName    = "ums-api",
        OtlpEndpoint   = "http://localhost:4317",
        SamplingRatio  = 1.0
    }));
bootstrapper.Bootstrap(services);
```

---

## Architectural Rules

| Rule | Decision |
|------|----------|
| Namespace ownership | Shell libraries use `Ums.Shell.*`; upstream namespaces (`BeyondNet.*`, `csdevlib.*`) are not allowed in UMS application code. |
| Runtime baseline | Shell libraries target the same stable runtime as the API: `net10.0`. |
| Domain purity | `Ums.Domain` must **not** reference `Ums.Shell.Aop.*`, `Ums.Shell.Bootstrapper.*`, or `Ums.Shell.Factory` directly. |
| Application AOP contract | `Ums.Application` references only `Ums.Shell.Aop.Aspects` (attribute declarations). No proxy, no DI installer, no runtime infrastructure. |
| Infrastructure wiring | `Ums.Infrastructure` owns AOP proxy registration and Bootstrapper startup wiring. |
| Pattern encapsulation | DDD, Factory, AOP, and Bootstrapper implementation details are centralized in shell libraries instead of being copied into bounded contexts. |
| Replacement strategy | If an upstream library changes, UMS adapts it inside `src/libs/shell`; app layers should not change because of upstream implementation movement. |
| Cross-platform | Project references use relative portable paths and .NET SDK-style projects. No OS-specific build paths are allowed. |

### Authorised reference graph (summary)

```
Ums.Domain       → Ums.Shell.Ddd, Ums.Shell.Ddd.ValueObjects
Ums.Application  → Ums.Domain, Ums.Shell.Aop.Aspects (attr contract only)
Ums.Infrastructure → Ums.Application, Ums.Domain,
                     Ums.Shell.Aop.*.Installer, Ums.Shell.Aop.Aspects.Logger.Serilog,
                     Ums.Shell.Bootstrapper.*
Ums.Presentation → All layers + Ums.Shell.Bootstrapper.* (startup)
```

---

## Compliance Checks

Run after any change to shell library references or aspect registrations:

```bash
# 1. Build full solution
dotnet build src/apps/ums.api/Ums.sln

# 2. Run shell library tests
dotnet test src/libs/shell/aop/src/Ums.Shell.Aop.Tests/Ums.Shell.Aop.Tests.csproj --verbosity minimal
dotnet test src/libs/shell/factory/src/Ums.Shell.Factory.Test/Ums.Shell.Factory.Test.csproj --verbosity minimal

# 3. Verify Domain purity — no AOP refs
grep -r "Ums.Shell.Aop" src/apps/ums.api/Ums.Domain/ --include="*.csproj"
# Expected: no output

# 4. Verify no direct Factory ref in Domain
grep "Ums.Shell.Factory" src/apps/ums.api/Ums.Domain/Ums.Domain.csproj
# Expected: no output
```

---

## Related Decisions and Guides

- [ADR-0054: Shell Library Isolation — DDD, Factory, AOP, Bootstrapper](../adrs/0054-shell-library-isolation.md)
- [ADR-0060: AOP Cross-Cutting Concern Strategy](../adrs/0060-aop-cross-cutting-concern-strategy.md)
- [Shell Library Developer Guides](../shell-libraries/README.md) — DDD · Factory · AOP · Bootstrapper · Combined Usage
- [DDD Primitives](../../governance/construction/ddd-design/11-ddd-primitives.md)
- [Architecture Portal](../index.md)
