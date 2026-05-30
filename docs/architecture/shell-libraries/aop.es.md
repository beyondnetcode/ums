# BeyondNetCode.Shell.Aop — Guía de Desarrollo

> **Parte de:** [Shell Libraries](README.md)
> **Paquetes NuGet:** `BeyondNetCode.Shell.Aop` · `BeyondNetCode.Shell.DispatchProxy` · `BeyondNetCode.Shell.Aspects` · `BeyondNetCode.Shell.Logger.Serilog` · `BeyondNetCode.Shell.DI`
> **Dependencias:** `Microsoft.Extensions.DependencyInjection` · `Serilog` (opcional) · `System.Linq.Dynamic.Core`
> **Repositorio:** `github.com/beyondnetcode/Shell.Aop`

`BeyondNetCode.Shell.Aop` proporciona **programación orientada a aspectos no invasiva** vía `System.Reflection.DispatchProxy`. Los concerns cross-cutting (logging, retry, advice) se aplican como una cadena ordenada de objetos `IAspect` alrededor de cualquier servicio basado en interfaces — sin modificación a la implementación del servicio.

---

## 1. Arquitectura

```
Caller
  │
  ▼
AopProxy<TService, TImpl>          ← DispatchProxy subclass
  │ Invoke(MethodInfo, args[])
  ▼
AspectExecutor
  │ for each matching aspect (ordered by GetOrder)
  ▼
IAspect chain  →  OnMethodBoundaryAspect<TAttribute>
  │                 OnEntry()
  │                 Proceed()  ──────────────────────────► real TImpl.Method()
  │                 OnSuccess() (after Task completes)
  │                 OnExit()
  │                 OnException() (if throws)
  ▼
return value (Task or sync)
```

**Decisiones de diseño clave:**
- **Selección driven por atributos**: `PointCut.CanApply` verifica el tipo de atributo en `aspect.BaseType.GetGenericArguments()`. Un aspecto solo dispara si el método objetivo lleva su atributo correspondiente.
- **Cadena ordenada**: aspectos se ordenan por `GetOrder(joinPoint)` — números de orden menores corren primero, más externos en el call stack.
- **Async-aware**: `OnMethodBoundaryAspect.Apply` detecta retornos `Task`/`Task<T>` y difiere `OnSuccess`/`OnExit`/`OnException` a una tarea de continuación.

---

## 2. Estructura de Paquetes

```
BeyondNetCode.Shell.Aop/
├── Interface/
│   ├── IAspect.cs           ← void Apply(IJoinPoint), SetNext/GetNext, GetOrder
│   ├── IAspectExecutor.cs   ← void Execute(IJoinPoint)
│   ├── IJoinPoint.cs        ← MethodInfo, Arguments, Return, TargetType, Proceed()
│   └── IPointCut.cs         ← bool CanApply(IJoinPoint, Type aspectType)
└── Impl/
    ├── AbstractAspect.cs              ← chain linkage + GetAttribute<TAttr>()
    ├── AbstractAspectAttribute.cs     ← marker base for aspect attributes
    ├── AspectExecutor.cs              ← filter + order + chain execution
    ├── OnMethodBoundaryAspect.cs      ← template: OnEntry/OnSuccess/OnExit/OnException + async support
    ├── OnRetryAspect.cs               ← retry-aware boundary
    ├── JoinPoint.cs                   ← IJoinPoint implementation
    └── PointCut.cs                    ← attribute-based CanApply with cache

BeyondNetCode.Shell.DispatchProxy/
├── AopProxy.cs              ← System.Reflection.DispatchProxy subclass
└── AopProxyCreator.cs       ← static Create<TService,TImpl>(target, executor)

BeyondNetCode.Shell.Aspects/
├── Impl/
│   ├── LoggerAspect.cs      ← [LoggerAspect] attribute + OnMethodBoundaryAspect
│   ├── AdviceAspect.cs      ← [AdviceAspect] — advice around method
│   └── RetryAspect.cs       ← [RetryAspect] — retry with configurable policy
└── Attributes/
    ├── LoggerAspectAttribute.cs
    ├── AdviceAspectAttribute.cs
    └── RetryAspectAttribute.cs

BeyondNetCode.Shell.DI/
└── DIAopInstaller.cs        ← AddAop(), AddAopProxy<TService, TImpl>(lifetime)
```

---

## 3. Uso Independiente — sin DI

```csharp
var executor = new AspectExecutor(new IAspect[]
{
    new LoggerAspectAttribute { Type = typeof(ILogger) },
    new RetryAspectAttribute { MaxRetries = 3 }
});

var proxy = AopProxyCreator.Create<IHandler, RealHandler>(
    target: new RealHandler(),
    executor: executor);

// Los métodos de IHandler ahora tienen logging + retry
await proxy.HandleAsync(command);
```

---

## 4. Uso con DI — `AddAop()` + `AddAopProxy()`

```csharp
// En DependencyInjection.cs
services.AddAop();
services.AddAopProxy<IRequestHandler<CreateTenantCommand, Result<CreateTenantResponse>>,
                     CreateTenantCommandHandler>(ServiceLifetime.Scoped);
```

`AddAop()` registra:
- `AspectExecutor` como singleton
- `IAspect[]` aspects
- `AopProxyCreator`

`AddAopProxy<TService, TImpl>()`:
- Registra `TService` mapeado al proxy
- El proxy delega a `TImpl` real a través del executor

---

## 5. Aspectos Incorporados

| Aspecto | Atributo | Propósito |
|---------|----------|-----------|
| `LoggerAspect` | `[LoggerAspect]` | Logging de entrada/salida con duración |
| `AdviceAspect` | `[AdviceAspect]` | Advice alrededor del método |
| `RetryAspect` | `[RetryAspect]` | Retry con política configurable |

---

## 6. Escribir un Aspecto Custom

```csharp
public class MyLoggingAttribute : AbstractAspectAttribute
{
    public override IAspect CreateAspect(Type aspectType) =>
        new MyLoggingAspect(this);
}

public class MyLoggingAspect : OnMethodBoundaryAspect
{
    private readonly MyLoggingAttribute _attr;

    public MyLoggingAspect(MyLoggingAttribute attr) => _attr = attr;

    public override void OnEntry(IJoinPoint jp)
    {
        Console.WriteLine($"Calling {jp.Method.Name}");
    }

    public override void OnExit(IJoinPoint jp)
    {
        Console.WriteLine($"Finished {jp.Method.Name}");
    }
}
```

---

## 7. Soporte Async

`System.Reflection.DispatchProxy.Invoke` es síncrono. Para métodos async:

```csharp
// OnMethodBoundaryAspect.Apply detecta Task y unwraps
if (returnType.IsGenericType && returnType.GetGenericTypeDefinition() == typeof(Task<>))
{
    // Wrap con continuation para async hooks
    return Task.FromResult(result).ContinueWith(_ =>
    {
        OnSuccess(joinPoint);
        return result;
    }, TaskScheduler.Default);
}
```

Esto asegura que `OnSuccess`/`OnExit` disparen después de que la Task completa, no cuando se crea.

---

## 8. Logging PII-Safe — MelLogger

| Logger | Argumentos logueados | Cuándo usar |
|--------|---------------------|------------|
| `MelLogger` | ❌ Nunca — solo nombres/tipos | Default; todos los handlers |
| `SerilogLogger` | ✅ Destructure (opt-in) | Solo después de revisión de PII |

```csharp
[LoggerAspect(Type = typeof(IMelLogger), LogArguments = [])]
// LogArguments = [] = PII-safe default
```

---

## 9. Referencia API

| Tipo | Descripción |
|------|-------------|
| `IAspect` | `Apply(IJoinPoint)`, `SetNext(IAspect)`, `GetOrder(IJoinPoint)` |
| `IJoinPoint` | `Method`, `Arguments`, `Return`, `TargetType`, `Proceed()` |
| `AspectExecutor` | Ejecuta cadena de aspectos en orden |
| `OnMethodBoundaryAspect` | Template para aspectos de entrada/salida/excepción |
| `AddAop()` | Registra executor y aspectos |
| `AddAopProxy<TService, TImpl>()` | Registra proxy DI |

---

## 10. Integración UMS

```csharp
// CreateTenantCommandHandler con AOP
[LoggerAspect(Type = typeof(IMelLogger), LogDuration = true, LogException = true, LogArguments = [])]
public async Task<Result<CreateTenantResponse>> Handle(
    CreateTenantCommand request,
    CancellationToken cancellationToken)
{
    // Logging automático de entrada/salida/duración/excepción
}
```

---

## 11. Convención de Orden de Aspectos

| Orden | Aspecto | Razón |
|-------|---------|-------|
| 10 | Tracing | Capturar span completo |
| 20 | Authorization | Rechazar temprano |
| 30 | Validation | Pre-condiciones |
| 40 | Idempotency | Verificar dedup |
| 50 | Logging | Observar ejecución |
| 60 | Metrics | Post-logging |
| 70 | Transaction | Retry más externo |

---

## 12. Troubleshooting

| Problema | Solución |
|----------|----------|
| Aspecto no dispara | Verificar que el método es interface-based (DispatchProxy requiere interface) |
| Singleton proxy error | `AddAopProxy` lanza `ArgumentException` para singleton — use scoped |
| Async hooks disparan antes | Asegurar que se usa el fix de `OnMethodBoundaryAspect` con continuations |