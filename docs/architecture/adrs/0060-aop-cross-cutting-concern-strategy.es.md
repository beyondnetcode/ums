# ADR-0060: Estrategia de Concerns Cross-Cutting AOP — DispatchProxy sobre MediatR Behaviors

## Estado

Aceptado

## Fecha

2026-05-24

## Responsable de Decisión

Arquitectura

## Relacionados

- [ADR-0053: Observabilidad OpenTelemetry](./0053-opentelemetry-observability.md) — define las señales que los aspectos AOP deben emitir
- [ADR-0054: Aislamiento de Shell Libraries](./0054-shell-library-isolation.md) — gobierna cómo se consume `BeyondNetCode.Shell.Aop`
- [Shell Libraries — Guía AOP](../shell-libraries/aop.md) — referencia de implementación
- [Shell Libraries — Uso Combinado](../shell-libraries/combined-usage.md) — walkthrough end-to-end

---

## Contexto

Los command handlers de UMS necesitan concerns cross-cutting estructurados: **logging de entrada/salida**con duración, **distributed tracing**con tags de tenant, **métricas** (señales RED), y**captura de excepciones**. Estos concerns deben ser:

1. **Selectivos** — aplicados por-handler o por-método, no uniformemente a todos los requests.
2. **No invasivos** — cero cambios a la lógica de negocio del handler.
3. **Async-correct** — firing hooks *después* del resultado awaited, no cuando el objeto `Task` es retornado.
4. **Testeables en aislamiento** — handlers unit-testados sin infraestructura cross-cutting.

UMS ya usa MediatR `IPipelineBehavior<TRequest, TResponse>` para concerns**uniformes**de pipeline (`ValidationBehavior`). La pregunta es si extender ese mecanismo para concerns cross-cutting o adoptar un modelo diferente.

### Alternativas consideradas

| Opción | Mecanismo | Selectivo? | Async-correct? | Dep. externa? | Decisión |
|---|---|---|---|---|---|
| **A** | MediatR `IPipelineBehavior<,>` | all-or-nothing por tipo | | | Rechazado para cross-cutting |
| **B** | Clases decorator por handler | manual | | | Rechazado — O(n) boilerplate |
| **C** | Castle.DynamicProxy / Autofac interceptors | | | nuevo NuGet requerido | Rechazado — stack pollution |
| **D** | `BeyondNetCode.Shell.Aop` + `System.Reflection.DispatchProxy` | attribute-driven | (after fix) | owned shell lib | **Adoptado** | #### Por qué MediatR `IPipelineBehavior` no fue suficiente

`IPipelineBehavior<TRequest, TResponse>` aplica a cada comando que coincide con su constraint de tipo. Este es el modelo correcto para concerns**uniformes** (validación, idempotencia) pero crea acoplamiento inaceptable para concerns**selectivos**:

- Un behavior de logging para `CreateTenantCommand` necesitaría condiciones específicas de tipo o registros de behavior separados por tipo de request.
- La lógica de behavior condicional (`if request is X then log, else skip`) es un anti-pattern que defeats el propósito de la abstracción de pipeline.
- Los behaviors de MediatR corren dentro de un único scope de request — no pueden distinguir entre un handler que debe emitir Serilog structured logs versus uno que debe emitir solo MEL Debug logs.

**Resolución:**Los behaviors de MediatR permanecen como el mecanismo canónico para concerns de pipeline uniformes. `BeyondNetCode.Shell.Aop` es el mecanismo canónico para decoración selectiva, por-método.

#### Por qué se eligió `BeyondNetCode.Shell.Aop` sobre una nueva dependencia NuGet

- `BeyondNetCode.Shell.Aop` es una**owned**shell library — sin gestión de paquetes externos, sin cambios rupturistas upstream, sin superficie CVE adicional.
- La librería ya implementa `DispatchProxy`, `AspectExecutor`, `PointCut`, `IAspect` chain, `OnMethodBoundaryAspect`, `LoggerAspect`, `RetryAspect`, y `AdviceAspect`.
- La integración DI vía `AddAop()` + `AddAopProxy<TService, TImpl>()` ya está construida y testeada.
- La única capacidad faltante era async-correctness (ver abajo).

---

## Decisión**Adoptar `BeyondNetCode.Shell.Aop` con `System.Reflection.DispatchProxy` como el mecanismo para concerns cross-cutting selectivos, por-método en los command handlers de UMS.**

### 1. Separación de responsabilidades

| Concern | Mecanismo | Aplica a |
|---|---|---|
| Validación de input | `ValidationBehavior` (MediatR) | Todos los commands uniformemente |
| Idempotencia | `IdempotencyMiddleware` (HTTP) | Todos los endpoints mutantes |
| Logging (selectivo) | `LoggerAspect` vía `BeyondNetCode.Shell.Aop` | Por-handler, opt-in vía `[LoggerAspect]` |
| Tracing (Fase 2) | `TracingAspect` vía `BeyondNetCode.Shell.Aop` | Por-handler, opt-in vía `[Tracing]` |
| Métricas (Fase 2) | `MetricsAspect` vía `BeyondNetCode.Shell.Aop` | Por-handler, opt-in vía `[Metrics]` |
| Retry (selectivo) | `RetryAspect` vía `BeyondNetCode.Shell.Aop` | Por-método, opt-in vía `[RetryAspect]` | ### 2. Fix async de proxy — prerrequisito obligatorio

`System.Reflection.DispatchProxy.Invoke` es síncrono. Antes de este ADR, `OnMethodBoundaryAspect.OnSuccess` y `OnExit` disparaban cuando un `Task` era *retornado*, no cuando *completaba*. Esto causaba que los hooks observaran estado incompleto.

**Fix (implementado en `BeyondNetCode.Shell.Aop/Impl/OnMethodBoundaryAspect.cs`):**Después de `joinPoint.Proceed()`, detectar tipos de retorno `Task` / `Task<TResult>` y envolvarlos en continuation tasks vía `ConfigureAwait(false)`. El path original `Task<TResult>` se maneja vía un `MethodInfo` genérico cacheado (`WrapAsyncOfT<TResult>`) para preservar el valor de resultado. El bloque síncrono `finally { OnExit() }` se skippea para paths async para prevenir double-firing.

### 3. Alcance de adopción

#### Fase 1 (implementada — 2026-05-24)

- `[LoggerAspect(Type = typeof(IMelLogger), LogDuration = true, LogException = true)]` en `CreateTenantCommandHandler.Handle`
- `MelLogger` registrado como keyed `ILogger` (AOP) bajo `typeof(IMelLogger)` en Infrastructure DI
- `IMelLogger` marker interface en `Ums.Application.Common.Aop` — decouple Application layer del tipo concreto de Infrastructure
- `AddAopProxy<IRequestHandler<CreateTenantCommand, Result<CreateTenantResponse>>, CreateTenantCommandHandler>()` en `Infrastructure.DependencyInjection`

#### Fase 2 (planificada)

- `TracingAspect` implementando `ActivitySource.StartActivity()` con tag `tenant_id` (alinea con ADR-0053)
- `MetricsAspect` implementando `Histogram<long>` vía `IMeterFactory`
- Expandir `AddAopProxy<>` a todos los command handlers en los bounded contexts Identity y Authorization
- Orden de aspectos: Tracing(10) → Logging(50) → Metrics(60)

#### Fase 3 (consideración futura)

- `RetryAspect` en servicios de Infrastructure que llaman a endpoints externos de IdP
- `AdviceAspect` para hooks de audit específicos de dominio

### 4. Política PII para logging aspects

| Logger | Valores de argumentos logueados | Cuando usar |
|---|---|---|
| `MelLogger` | Nunca — solo nombres/tipos | Default; todos los handlers |
| `SerilogLogger` | Destructureados (opt-in) | Solo después de revisión y aprobación de PII explícita | `[LoggerAspect(LogArguments = [])]` (array vacío) es el default PII-safe y debe setearse en todos los handlers a menos que un argumento específico sea revisado y clearance.

### 5. Referencias de capa introducidas

```
Ums.Application.csproj
 └── BeyondNetCode.Shell.Aspects ← contrato de atributos solo ([LoggerAspect], etc.)

Ums.Infrastructure.csproj
 ├── BeyondNetCode.Shell.DI ← AddAop(), AddAopProxy<>()
 └── BeyondNetCode.Shell.Logger.Serilog ← Adaptador SerilogLogger
```

`Ums.Domain` **no**referencia ningún proyecto `BeyondNetCode.Shell.Aop`. La pureza del Domain se preserva.

---

## Consecuencias

### Positivas

- Los handlers permanecen como lógica de negocio pura — sin imports de logging o telemetry en código de Application layer.
- Los concerns cross-cutting se aplican selectivamente sin modificar el pipeline MediatR para todos los handlers.
- Los atributos `[LoggerAspect]` + `[Tracing]` hacen la decoración de concern visible y searchable en code review.
- Los hooks async-correct disparan después de la completación real, no después de la creación del objeto Task — los logs y métricas son precisos.
- `MelLogger` bridgea el `ILogger` custom de AOP a `Microsoft.Extensions.Logging` — no se requiere un custom Serilog sink en Application.
- El mismo mecanismo de proxy aplica a cualquier interfaz registrada en DI, no solo a MediatR handlers — repositorios, domain services, e IdP gateways pueden ser decorados con patrones idénticos.

### Trade-offs

- `System.Reflection.DispatchProxy` requiere que el servicio sea un**interface** (o clase abstracta). El proxying de clase concreta no está soportado — esto es enforce por `AddAopProxy<TService, TImpl>()`.
- `DispatchProxy.Invoke` es intrínsecamente síncrono; el wrapper de continuación async añade overhead menor (~1 allocación por llamada de método async).
- `PointCut` cachea `(MethodInfo, Type) → bool` por tipo de proxy — el cache crece proporcionalmente con el número de métodos proxied; insignificante en la práctica.
- El `RegisterServicesFromAssembly` de MediatR registra handlers antes de `AddAopProxy<>` — los llamadores deben asegurar que `AddAopProxy<>` se llame**después**del registro de MediatR para que el proxy gane la resolución de último-registro.
- Los proxies singleton están explícitamente prohibidos (`AddAopProxy` lanza `ArgumentException` para `ServiceLifetime.Singleton`) porque los aspectos pueden resolver servicios scoped (ej., `IUserContext`).

### No-decisiones

- **Compile-time weaving** (ej., PostSharp, Fody) no fue evaluado. UMS actualmente no tiene infraestructura de build-time weaving; la complejidad añadida no se justifica en la escala actual.
- **Castle.DynamicProxy** / **Autofac interceptors**permanecen disponibles como alternativas futuras si la constraint de interface de `DispatchProxy` se vuelve limitante.

---

## Cumplimiento

Las siguientes verificaciones son obligatorias después de cualquier cambio a un aspecto AOP o registro de proxy AOP:

```bash
# Build de la solución completa
dotnet build src/apps/ums.api/Ums.sln

# Ejecutar todos los test suites
dotnet test src/apps/ums.api/Ums.sln --verbosity minimal
dotnet test src/libs/shell/aop/src/BeyondNetCode.Shell.Aop.Tests/BeyondNetCode.Shell.Aop.Tests.csproj --verbosity minimal

# Verificar que no hay violación de pureza del Domain
grep -r "BeyondNetCode.Shell.Aop" src/apps/ums.api/Ums.Domain/ --include="*.csproj"
# Esperado: sin output
```

---

**[Índice ADR](./index.md)** | **[Guía de Desarrollo AOP](../shell-libraries/aop.md)** | **[ADR-0053 OpenTelemetry](./0053-opentelemetry-observability.md)** | **[ADR-0054 Aislamiento de Shell](./0054-shell-library-isolation.md)**