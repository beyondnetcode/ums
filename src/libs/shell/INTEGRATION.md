# Shell Libraries Integration Guide

## Overview

Three complementary libraries form the **Ums.Shell** ecosystem:

| Library | Purpose | Namespace |
|---------|---------|-----------|
| `Ums.Shell.Factory` | Configuration-driven factory/abstract factory pattern | `Ums.Shell.Factory` |
| `Ums.Shell.Bootstrapper` | Composition root bootstrapping with typed results | `Ums.Shell.Bootstrapper.*` |
| `Ums.Shell.Aop` | Aspect-Oriented Programming via `DispatchProxy` | `Ums.Shell.Aop.*` |

Location: `src/libs/shell/{factory,bootstrapper,aop}/src/`

---

## 1. Ums.Shell.Factory

**Core concept**: Register components by key, resolve by interface + key.

```csharp
// Registration
var locator = new ServiceLocator();
locator.Register<IService, ServiceImpl>("myKey");

// Resolution
var service = locator.Resolve<IService>("myKey");

// Factory usage
var factory = new Factory<IService>(key => locator.Resolve<IService>(key));
var instance = factory.Create("myKey");
```

**Key types**:
- `IServiceLocator` / `ServiceLocator` - component registry
- `IFactory<T>` / `Factory<T>` - keyed factory
- `IFactoryCreator` / `FactoryCreator` - type-based creation

---

## 2. Ums.Shell.Bootstrapper

**Core concept**: Typed bootstrappers that produce a result after `Run()`.

```csharp
// Single bootstrapper
var di = new DependencyInjectionBootstrapper(services => {
    services.AddScoped<IMyService, MyService>();
});
di.Run();
var container = di.Result; // IServiceCollection

// Composite bootstrapper
var composite = new CompositeBootstrapper()
    .Add(new DependencyInjectionBootstrapper(services => { ... }))
    .Add(new AutoMapperBootstrapper(cfg => { cfg.AddProfile<MyProfile>(); }));
composite.Run();
```

**Available bootstrappers**:

| Project | Class | Result Type |
|---------|-------|-------------|
| `Ums.Shell.Bootstrapper` | `CompositeBootstrapper` | void (runs children) |
| `Ums.Shell.Bootstrapper.DependencyInjection` | `DependencyInjectionBootstrapper` | `IServiceCollection` |
| `Ums.Shell.Bootstrapper.AutoMapper` | `AutoMapperBootstrapper` | `MapperConfigurationExpression` |
| `Ums.Shell.Bootstrapper.Observability` | `ObservabilityBootstrapper` | Observability config |

---

## 3. Ums.Shell.Aop

**Core concept**: Intercept method calls via `DispatchProxy` with a chain of aspects.

### 3.1 Core Abstractions

```csharp
// Aspect interface - chain of responsibility
public interface IAspect {
    void SetNext(IAspect aspect);
    IAspect GetNext();
    void Apply(IJoinPoint joinPoint);
    int GetOrder(IJoinPoint joinPoint);
}

// JoinPoint - method invocation context
public interface IJoinPoint {
    MethodInfo MethodInfo { get; }
    object[] Arguments { get; set; }
    object TargetObject { get; }
    Type TargetType { get; }
    object Return { get; set; }
    void Proceed();
}
```

### 3.2 Built-in Aspects

| Aspect | Attribute | Purpose |
|--------|-----------|---------|
| `LoggerAspect` | `[LoggerAspect]` | Method entry/exit logging |
| `AdviceAspect` | `[AdviceAspect]` | Custom advice execution |
| `RetryAspect` | `[RetryAspect]` | Automatic retry on failure |

### 3.3 OnMethodBoundaryAspect

Base class for method boundary interception:

```csharp
public abstract class OnMethodBoundaryAspect<T> : AbstractAspect<T> {
    protected virtual void OnEntry(IJoinPoint joinPoint) { }
    protected virtual void OnSuccess(IJoinPoint joinPoint) { }
    protected virtual void OnExit(IJoinPoint joinPoint) { }
    protected virtual void OnException(IJoinPoint joinPoint, Exception ex) { }
}
```

### 3.4 DI Integration

```csharp
// Register AOP infrastructure
services.AddAop(builder => {
    builder.AddAspect<MyCustomAspect>();
    builder.AddAdvice<MyAdvice>();
    builder.AddLogger<SerilogLogger>();
});

// Register proxied service
services.AddAopProxy<IMyService, MyService>(ServiceLifetime.Scoped);
```

**Important**: Singleton AOP proxies are **not supported** because aspects may depend on scoped services.

---

## 4. Using All Three Together

### 4.1 Recommended Composition in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

// 1. Bootstrapper: DI setup
var diBootstrapper = new DependencyInjectionBootstrapper(builder.Services);
diBootstrapper.Run();

// 2. AOP: Register aspect infrastructure
builder.Services.AddAop(aop => {
    aop.AddAspect<TenantValidationAspect>();
    aop.AddAspect<TransactionAspect>();
    aop.AddLogger<SerilogLogger>();
});

// 3. AOP: Register proxied services
builder.Services.AddAopProxy<IUserService, UserService>(ServiceLifetime.Scoped);
builder.Services.AddAopProxy<IOrderService, OrderService>(ServiceLifetime.Scoped);

// 4. Factory: Register keyed components (if needed)
builder.Services.AddSingleton<IServiceLocator>(sp => {
    var locator = new ServiceLocator();
    locator.Register<IValidator, EmailValidator>("email");
    locator.Register<IValidator, PhoneValidator>("phone");
    return locator;
});

var app = builder.Build();
```

### 4.2 Composite Bootstrapper Pattern

```csharp
var bootstrapper = new CompositeBootstrapper()
    .Add(new DependencyInjectionBootstrapper(services => {
        services.AddAop(aop => { ... });
        services.AddAopProxy<IX, X>();
    }))
    .Add(new AutoMapperBootstrapper(cfg => {
        cfg.AddProfile<MappingProfile>();
    }));

bootstrapper.Run();
```

---

## 5. Project References

Add to your `.csproj`:

```xml
<ItemGroup>
  <ProjectReference Include="../../libs/shell/factory/src/Ums.Shell.Factory/Ums.Shell.Factory.csproj" />
  <ProjectReference Include="../../libs/shell/bootstrapper/src/Ums.Shell.Bootstrapper/Ums.Shell.Bootstrapper.csproj" />
  <ProjectReference Include="../../libs/shell/bootstrapper/src/Ums.Shell.Bootstrapper.DependencyInjection/Ums.Shell.Bootstrapper.DependencyInjection.csproj" />
  <ProjectReference Include="../../libs/shell/aop/src/Ums.Shell.Aop/Ums.Shell.Aop.csproj" />
  <ProjectReference Include="../../libs/shell/aop/src/Ums.Shell.Aop.Aspects/Ums.Shell.Aop.Aspects.csproj" />
  <ProjectReference Include="../../libs/shell/aop/src/Ums.Shell.Aop.DispatchProxy/Ums.Shell.Aop.DispatchProxy.csproj" />
  <ProjectReference Include="../../libs/shell/aop/src/Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer/Ums.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer.csproj" />
</ItemGroup>
```

---

## 6. Build Status

All libraries build successfully as part of `Ums.sln`:

```
dotnet build Ums.sln
```

0 errors, only OpenTelemetry vulnerability warnings (pre-existing).
