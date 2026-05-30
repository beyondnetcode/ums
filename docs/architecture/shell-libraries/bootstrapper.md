# BeyondNetCode.Shell.Bootstrapper — Developer Guide

> **Part of:** [Shell Libraries](README.md)  
> **Projects:** `BeyondNetCode.Shell.Bootstrapper` · `BeyondNetCode.Shell.DI` · `BeyondNetCode.Shell.AutoMapper` · `BeyondNetCode.Shell.Observability`  
> **Dependencies:** `Microsoft.Extensions.DependencyInjection` · `AutoMapper` · `Serilog.Sinks.OpenTelemetry` · `OpenTelemetry`

`BeyondNetCode.Shell.Bootstrapper` implements the **Composite Bootstrapper pattern** — a structured, testable way to decompose complex application startup into small, independently-runnable units that compose into a pipeline.

---

## Table of Contents

1. [When to Use](#1-when-to-use)
2. [Project Structure](#2-project-structure)
3. [Core Interfaces](#3-core-interfaces)
4. [CompositeBootstrapper](#4-compositebootstrapper)
5. [Built-in Bootstrappers](#5-built-in-bootstrappers)
6. [Writing a Custom Bootstrapper](#6-writing-a-custom-bootstrapper)
7. [Async Bootstrappers](#7-async-bootstrappers)
8. [Standalone Usage Examples](#8-standalone-usage-examples)
9. [API Reference](#9-api-reference)
10. [UMS Integration Pattern](#10-ums-integration-pattern)

---

## 1. When to Use

Use `BeyondNetCode.Shell.Bootstrapper` when:

- Startup has **multiple independent phases** that should be testable in isolation.
- You want a **fluent, composable** way to describe the initialization order.
- You need a typed `Result` from an initialization phase (e.g., the `IServiceCollection` after DI is configured).

Prefer `IHostedService` or `IStartupFilter` for:
- Work that must happen **inside the running host** (after `app.Run()`).
- Simple single-step initialization that doesn't need to be composed or tested in isolation.

---

## 2. Project Structure

```
BeyondNetCode.Shell.Bootstrapper/
└── src/
    ├── BeyondNetCode.Shell.Bootstrapper/
    │   ├── Interface/
    │   │   ├── IBootstrapper.cs          ← IBootstrapper + IBootstrapper<out T>
    │   │   └── IBootstrapperAsync.cs     ← IBootstrapperAsync + IBootstrapperAsync<out T>
    │   └── Impl/
    │       ├── CompositeBootstrapper.cs       ← sequential sync runner
    │       └── CompositeBootstrapperAsync.cs  ← sequential async runner
    ├── BeyondNetCode.Shell.DI/
    │   └── DependencyInjectionBootstrapper.cs ← wraps IServiceCollection configuration
    ├── BeyondNetCode.Shell.AutoMapper/
    │   └── AutoMapperBootstrapper.cs          ← wraps AutoMapper configuration
    ├── BeyondNetCode.Shell.Observability/
    │   ├── ObservabilityBootstrapper.cs       ← Serilog + OpenTelemetry wiring
    │   └── ObservabilityConfiguration.cs      ← OTLP endpoint, service name/version, resource attributes
    └── BeyondNetCode.Shell.Bootstrapper.Tests/
```

---

## 3. Core Interfaces

```csharp
// Sync bootstrapper — no result
public interface IBootstrapper
{
    void Run();
}

// Sync bootstrapper with typed result
public interface IBootstrapper<out T> : IBootstrapper
{
    T? Result { get; }
}

// Async bootstrapper — no result
public interface IBootstrapperAsync
{
    Task RunAsync(CancellationToken cancellationToken = default);
}

// Async bootstrapper with typed result
public interface IBootstrapperAsync<out T> : IBootstrapperAsync
{
    T? Result { get; }
}
```

---

## 4. CompositeBootstrapper

Runs a sequence of bootstrappers one after another. Fluent API via `.Add(bootstrapper)`.

### Sync

```csharp
new CompositeBootstrapper()
    .Add(new PhaseABootstrapper())
    .Add(new PhaseBBootstrapper())
    .Add(new PhaseCBootstrapper())
    .Run();
```

You can also pass the list in the constructor:

```csharp
var bootstrappers = new IBootstrapper[]
{
    new PhaseABootstrapper(),
    new PhaseBBootstrapper()
};
new CompositeBootstrapper(bootstrappers).Run();
```

### Async

```csharp
await new CompositeBootstrapperAsync()
    .Add(new DatabaseMigrationBootstrapper())
    .Add(new SeedDataBootstrapper())
    .RunAsync(cancellationToken);
```

---

## 5. Built-in Bootstrappers

### 5.1 DependencyInjectionBootstrapper

Wraps `IServiceCollection` configuration. `Result` is the populated `IServiceCollection`.

```csharp
var di = new DependencyInjectionBootstrapper(services =>
{
    services.AddSingleton<IMyService, MyService>();
    services.AddScoped<IOrderRepository, SqlOrderRepository>();
});
di.Run();

IServiceCollection configured = di.Result!;
// build and use
var sp = configured.BuildServiceProvider();
```

Alternatively, pass an existing `IServiceCollection`:

```csharp
var services = new ServiceCollection();
var di = new DependencyInjectionBootstrapper(services, s =>
{
    s.AddSingleton<IMyService, MyService>();
});
di.Run();
// services is now populated
```

### 5.2 AutoMapperBootstrapper

Wraps AutoMapper `MapperConfigurationExpression`. `Result` holds the expression object
that was passed to AutoMapper's configuration lambda.

> **Note:** `MapperConfigurationExpression` does not expose `CreateMapper()` directly.
> Use the bootstrapper to collect your mapping declarations, then pass the same Action
> to `new MapperConfiguration(...)` to obtain an `IMapper`. The bootstrapper is most
> useful as a testable unit for verifying that mapping declarations were registered.

```csharp
// Declare the mapper configuration in the bootstrapper
Action<IMapperConfigurationExpression> mappings = cfg =>
{
    cfg.CreateMap<OrderEntity, OrderDto>();
    cfg.CreateMap<LineItemEntity, LineItemDto>();
};

var autoMapper = new AutoMapperBootstrapper(mappings);
autoMapper.Run();

// autoMapper.Result is not null — declarations were applied
Debug.Assert(autoMapper.Result != null);

// To get a working IMapper, wrap the same action in MapperConfiguration:
var mapperConfig = new MapperConfiguration(mappings);
mapperConfig.AssertConfigurationIsValid();
IMapper mapper = mapperConfig.CreateMapper();

OrderDto dto = mapper.Map<OrderDto>(entity);
```

**DI pattern (recommended):** Register AutoMapper directly with `services.AddAutoMapper(typeof(MyProfile))` for production usage. Use `AutoMapperBootstrapper` when you want isolated, composable testing of mapping declarations.

### 5.3 ObservabilityBootstrapper

Configures **Serilog** (via OTLP sink) and **OpenTelemetry tracing** (via OTLP exporter) in one step.

```csharp
var config = new ObservabilityConfiguration
{
    OTLPEndpoint   = "http://otel-collector:4317",
    ServiceName    = "ums-api",
    ServiceVersion = "2.1.0",
    ResourceAttributes = new Dictionary<string, object>
    {
        { "deployment.environment", "production" },
        { "cloud.region", "us-east-1" }
    }
};

var obs = new ObservabilityBootstrapper(services, config);
obs.Run();
// After Run():
// - Serilog.Log.Logger is configured with OTLP sink
// - OpenTelemetry tracing with OTLP exporter is registered in services
```

#### ObservabilityConfiguration

| Property | Default | Description |
|---|---|---|
| `OTLPEndpoint` | `http://localhost:4317` | gRPC OTLP collector endpoint |
| `ServiceName` | `"UnknownService"` | Appears in traces and Serilog context |
| `ServiceVersion` | `"1.0.0"` | `service.version` resource attribute |
| `ResourceAttributes` | `null` | Additional OTLP resource attributes (key-value pairs) |

---

## 6. Writing a Custom Bootstrapper

### Sync with result

```csharp
public class DatabaseSchemaBootstrapper(string connectionString)
    : IBootstrapper<bool>
{
    public bool? Result { get; private set; }

    public void Run()
    {
        // Apply migrations, validate schema, etc.
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        // ... schema checks ...
        Result = true;
    }
}

// Usage
var schema = new DatabaseSchemaBootstrapper(connectionString);
schema.Run();
if (schema.Result != true) throw new InvalidOperationException("Schema validation failed.");
```

### Async with result

```csharp
public class SeedBootstrapper(IServiceProvider sp)
    : IBootstrapperAsync<int>  // int = number of seeded records
{
    public int? Result { get; private set; }

    public async Task RunAsync(CancellationToken ct = default)
    {
        using var scope = sp.CreateScope();
        var repo  = scope.ServiceProvider.GetRequiredService<ITenantRepository>();
        var count = await SeedTenantsAsync(repo, ct);
        Result    = count;
    }

    private static async Task<int> SeedTenantsAsync(ITenantRepository repo, CancellationToken ct) { ... }
}
```

### Composing custom + built-in

```csharp
await new CompositeBootstrapperAsync()
    .Add(new DatabaseSchemaBootstrapper(connectionString))  // IBootstrapper wrapped? no — see note
    .Add(new SeedBootstrapper(serviceProvider))
    .RunAsync(cancellationToken);
```

> **Note:** `CompositeBootstrapperAsync` expects `IBootstrapperAsync` instances. Wrap a sync bootstrapper if needed:
> ```csharp
> public class SyncToAsyncWrapper(IBootstrapper inner) : IBootstrapperAsync
> {
>     public Task RunAsync(CancellationToken ct = default) { inner.Run(); return Task.CompletedTask; }
> }
> ```

---

## 7. Async Bootstrappers

`CompositeBootstrapperAsync` runs all registered `IBootstrapperAsync` instances sequentially using `await`.

```csharp
await new CompositeBootstrapperAsync()
    .Add(new CheckConnectivityBootstrapper())
    .Add(new ApplyMigrationsBootstrapper())
    .Add(new WarmupCacheBootstrapper())
    .RunAsync(stoppingToken);
```

Each bootstrapper receives the same `CancellationToken` — handle cancellation in your `RunAsync` implementation.

---

## 8. Standalone Usage Examples

### Example A — DI + AutoMapper pipeline (no host)

```csharp
using BeyondNetCode.Shell.Bootstrapper.Impl;
using BeyondNetCode.Shell.DI;
using BeyondNetCode.Shell.AutoMapper;

var services = new ServiceCollection();

// Phase 1: configure DI
var di = new DependencyInjectionBootstrapper(services, s =>
{
    s.AddSingleton<IDiscountService, DiscountService>();
    s.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
});

// Phase 2: configure AutoMapper
Action<IMapperConfigurationExpression> mappings = cfg =>
{
    cfg.CreateMap<OrderEntity, OrderDto>();
    cfg.CreateMap<Order, OrderEntity>().ReverseMap();
};
var autoMapper = new AutoMapperBootstrapper(mappings);

// Run both phases in sequence
new CompositeBootstrapper()
    .Add(di)
    .Add(autoMapper)
    .Run();

// Build
var sp     = services.BuildServiceProvider();
var mapper = new MapperConfiguration(mappings).CreateMapper();
```

### Example B — Async startup with schema check

```csharp
await new CompositeBootstrapperAsync()
    .Add(new ConnectivityCheckBootstrapper(connectionString))   // custom
    .Add(new DatabaseSchemaBootstrapper(connectionString))       // custom
    .Add(new SeedBootstrapper(serviceProvider))                  // custom
    .RunAsync(CancellationToken.None);
```

### Example C — Observability wired standalone (e.g., a worker service)

```csharp
var services = new ServiceCollection();

new ObservabilityBootstrapper(services, new ObservabilityConfiguration
{
    OTLPEndpoint   = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
                     ?? "http://localhost:4317",
    ServiceName    = "ums-worker",
    ServiceVersion = "1.0.0"
}).Run();

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((_, s) => { /* copy registrations */ })
    .Build();

await host.RunAsync();
```

---

## 9. API Reference

### `CompositeBootstrapper`

| Method | Description |
|---|---|
| `CompositeBootstrapper Add(IBootstrapper b)` | Append a bootstrapper; returns `this` for chaining |
| `void Run()` | Execute all bootstrappers in addition order |

### `CompositeBootstrapperAsync`

| Method | Description |
|---|---|
| `CompositeBootstrapperAsync Add(IBootstrapperAsync b)` | Append; returns `this` |
| `Task RunAsync(CancellationToken ct = default)` | Execute all sequentially with `await` |

### `DependencyInjectionBootstrapper`

| Constructor | Description |
|---|---|
| `(Action<IServiceCollection>?)` | Creates a new `ServiceCollection` internally |
| `(IServiceCollection, Action<IServiceCollection>?)` | Uses the provided collection |

`Result` → the configured `IServiceCollection`.

### `AutoMapperBootstrapper`

| Constructor | Description |
|---|---|
| `(Action<IMapperConfigurationExpression>?)` | Registers mapping profile |

`Result` → `MapperConfigurationExpression`. Call `.CreateMapper()` to get `IMapper`.

### `ObservabilityBootstrapper`

| Constructor | Description |
|---|---|
| `(IServiceCollection, ObservabilityConfiguration, Action<IServiceCollection>?)` | Configures Serilog + OTLP tracing |

`Result` → the `IServiceCollection` (same reference passed in).

---

## 10. UMS Integration Pattern

UMS currently uses `IHostedService`, `IStartupFilter`, and direct `Program.cs` wiring for startup initialization. The Bootstrapper pattern can be layered on top for complex multi-step cases.

### Recommended pattern for SQL Server schema bootstrap

```csharp
// In Ums.Infrastructure/Hosting/SchemaBootstrapperService.cs
public class SchemaBootstrapperService(
    IServiceProvider sp,
    ILogger<SchemaBootstrapperService> logger) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        await new CompositeBootstrapperAsync()
            .Add(new SqlServerSchemaBootstrapper(sp, logger))
            .Add(new DevDataSeedBootstrapper(sp, logger))
            .RunAsync(ct);
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

// Implementation of each phase:
public class SqlServerSchemaBootstrapper(IServiceProvider sp, ILogger logger)
    : IBootstrapperAsync
{
    public async Task RunAsync(CancellationToken ct)
    {
        using var scope    = sp.CreateScope();
        var bootstrapper   = scope.ServiceProvider
            .GetRequiredService<SqlServerSchemaBootstrapper>();
        await bootstrapper.InitializeAsync(ct);
    }
}
```

### Observability in Program.cs

```csharp
// In Ums.Presentation/Program.cs (or wherever the host is composed)
var obsConfig = new ObservabilityConfiguration
{
    OTLPEndpoint   = builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317",
    ServiceName    = "ums-api",
    ServiceVersion = "2.0.0"
};

new ObservabilityBootstrapper(builder.Services, obsConfig).Run();
// Serilog + OTel tracing are now configured
```

---

## Related Docs

- [AOP](aop.md) — AOP cross-cutting concerns for services initialized by bootstrappers
- [Combined Usage](combined-usage.md) — full Bootstrapper + DDD + Factory + AOP example
