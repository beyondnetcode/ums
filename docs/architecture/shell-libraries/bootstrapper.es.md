# Ums.Shell.Bootstrapper -- Guia del Desarrollador

> **Parte de:** [Shell Libraries](README.es.md)  
> **Proyectos:** `Ums.Shell.Bootstrapper` · `Ums.Shell.Bootstrapper.DependencyInjection` · `Ums.Shell.Bootstrapper.AutoMapper` · `Ums.Shell.Bootstrapper.Observability`  
> **Dependencias:** `Microsoft.Extensions.DependencyInjection` · `AutoMapper` · `Serilog.Sinks.OpenTelemetry` · `OpenTelemetry`

`Ums.Shell.Bootstrapper` implementa el **Patron Composite Bootstrapper** -- una forma estructurada y testeable de descomponer el inicio complejo de una aplicacion en pequenas unidades independientes que se componen en un pipeline.

---

## Tabla de Contenidos

1. [Cuando Usar](#1-cuando-usar)
2. [Estructura del Proyecto](#2-estructura-del-proyecto)
3. [Interfaces Core](#3-interfaces-core)
4. [CompositeBootstrapper](#4-compositebootstrapper)
5. [Bootstrappers Incluidos](#5-bootstrappers-incluidos)
6. [Escribir un Bootstrapper Custom](#6-escribir-un-bootstrapper-custom)
7. [Bootstrappers Async](#7-bootstrappers-async)
8. [Ejemplos de Uso Standalone](#8-ejemplos-de-uso-standalone)
9. [Referencia API](#9-referencia-api)
10. [Patron de Integracion UMS](#10-patron-de-integracion-ums)

---

## 1. Cuando Usar

Usa `Ums.Shell.Bootstrapper` cuando:

- El inicio tiene **multiples fases independientes** que deben ser testeables en aislamiento.
- Quires una forma **fluente y composable** de describir el orden de inicializacion.
- Necesitas un `Result` tipado desde una fase de inicializacion (ej., el `IServiceCollection` despues de configurar DI).

Prefiere `IHostedService` o `IStartupFilter` para:
- Trabajo que debe happen **dentro del host en ejecucion** (despues de `app.Run()`).
- Inicializacion de un solo paso que no necesita ser compuesta o testeada en aislamiento.

---

## 2. Estructura del Proyecto

```
Ums.Shell.Bootstrapper/
└── src/
    ├── Ums.Shell.Bootstrapper/
    │   ├── Interface/
    │   │   ├── IBootstrapper.cs          ← IBootstrapper + IBootstrapper<out T>
    │   │   └── IBootstrapperAsync.cs     ← IBootstrapperAsync + IBootstrapperAsync<out T>
    │   └── Impl/
    │       ├── CompositeBootstrapper.cs       ← runner sync secuencial
    │       └── CompositeBootstrapperAsync.cs  ← runner async secuencial
    ├── Ums.Shell.Bootstrapper.DependencyInjection/
    │   └── DependencyInjectionBootstrapper.cs ← envuelve configuracion de IServiceCollection
    ├── Ums.Shell.Bootstrapper.AutoMapper/
    │   └── AutoMapperBootstrapper.cs          ← envuelve configuracion de AutoMapper
    ├── Ums.Shell.Bootstrapper.Observability/
    │   ├── ObservabilityBootstrapper.cs       ← cableado de Serilog + OpenTelemetry
    │   └── ObservabilityConfiguration.cs      ← endpoint OTLP, nombre/servicio/version, atributos de recurso
    └── Ums.Shell.Bootstrapper.Tests/
```

---

## 3. Interfaces Core

```csharp
// Bootstrapper sync -- sin resultado
public interface IBootstrapper
{
    void Run();
}

// Bootstrapper sync con resultado tipado
public interface IBootstrapper<out T> : IBootstrapper
{
    T? Result { get; }
}

// Bootstrapper async -- sin resultado
public interface IBootstrapperAsync
{
    Task RunAsync(CancellationToken cancellationToken = default);
}

// Bootstrapper async con resultado tipado
public interface IBootstrapperAsync<out T> : IBootstrapperAsync
{
    T? Result { get; }
}
```

---

## 4. CompositeBootstrapper

Ejecuta una secuencia de bootstrappers uno tras otro. API fluida via `.Add(bootstrapper)`.

### Sync

```csharp
new CompositeBootstrapper()
    .Add(new PhaseABootstrapper())
    .Add(new PhaseBBootstrapper())
    .Add(new PhaseCBootstrapper())
    .Run();
```

Tambien puedes pasar la lista en el constructor:

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

## 5. Bootstrappers Incluidos

### 5.1 DependencyInjectionBootstrapper

Envuelve configuracion de `IServiceCollection`. `Result` es el `IServiceCollection` poblado.

```csharp
var di = new DependencyInjectionBootstrapper(services =>
{
    services.AddSingleton<IMyService, MyService>();
    services.AddScoped<IOrderRepository, SqlOrderRepository>();
});
di.Run();

IServiceCollection configured = di.Result!;
// construye y usa
var sp = configured.BuildServiceProvider();
```

Alternativamente, pasa un `IServiceCollection` existente:

```csharp
var services = new ServiceCollection();
var di = new DependencyInjectionBootstrapper(services, s =>
{
    s.AddSingleton<IMyService, MyService>();
});
di.Run();
// services ahora esta poblado
```

### 5.2 AutoMapperBootstrapper

Envuelve `MapperConfigurationExpression` de AutoMapper. `Result` mantiene el objeto de expresion que se paso al lambda de configuracion de AutoMapper.

> **Nota:** `MapperConfigurationExpression` no expone `CreateMapper()` directamente.
> Usa el bootstrapper para colectar tus declaraciones de mapeo, luego pasa la misma Action
> a `new MapperConfiguration(...)` para obtener un `IMapper`. El bootstrapper es mas
> util como unidad testeable para verificar que las declaraciones de mapeo fueron registradas.

```csharp
// Declara la configuracion del mapper en el bootstrapper
Action<IMapperConfigurationExpression> mappings = cfg =>
{
    cfg.CreateMap<OrderEntity, OrderDto>();
    cfg.CreateMap<LineItemEntity, LineItemDto>();
};

var autoMapper = new AutoMapperBootstrapper(mappings);
autoMapper.Run();

// autoMapper.Result no es null -- las declaraciones fueron aplicadas
Debug.Assert(autoMapper.Result != null);

// Para obtener un IMapper funcional, envuelve la misma action en MapperConfiguration:
var mapperConfig = new MapperConfiguration(mappings);
mapperConfig.AssertConfigurationIsValid();
IMapper mapper = mapperConfig.CreateMapper();

OrderDto dto = mapper.Map<OrderDto>(entity);
```

**Patron DI (recomendado):** Registra AutoMapper directamente con `services.AddAutoMapper(typeof(MyProfile))` para uso en produccion. Usa `AutoMapperBootstrapper` cuando quieres testing aislado y composable de declaraciones de mapeo.

### 5.3 ObservabilityBootstrapper

Configura **Serilog** (via sink OTLP) y **OpenTelemetry tracing** (via exportador OTLP) en un paso.

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
// Despues de Run():
// - Serilog.Log.Logger esta configurado con sink OTLP
// - OpenTelemetry tracing con exportador OTLP esta registrado en services
```

#### ObservabilityConfiguration

| Propiedad | Default | Descripcion |
|---|---|---|
| `OTLPEndpoint` | `http://localhost:4317` | Endpoint del collector OTLP gRPC |
| `ServiceName` | `"UnknownService"` | Aparece en traces y contexto Serilog |
| `ServiceVersion` | `"1.0.0"` | Atributo de recurso `service.version` |
| `ResourceAttributes` | `null` | Atributos de recurso OTLP adicionales (pares key-value) |

---

## 6. Escribir un Bootstrapper Custom

### Sync con resultado

```csharp
public class DatabaseSchemaBootstrapper(string connectionString)
    : IBootstrapper<bool>
{
    public bool? Result { get; private set; }

    public void Run()
    {
        // Aplica migraciones, valida schema, etc.
        using var conn = new SqlConnection(connectionString);
        conn.Open();
        // ... schema checks ...
        Result = true;
    }
}

// Uso
var schema = new DatabaseSchemaBootstrapper(connectionString);
schema.Run();
if (schema.Result != true) throw new InvalidOperationException("Schema validation failed.");
```

### Async con resultado

```csharp
public class SeedBootstrapper(IServiceProvider sp)
    : IBootstrapperAsync<int>  // int = numero de registros sembrados
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

### Componer custom + incluido

```csharp
await new CompositeBootstrapperAsync()
    .Add(new DatabaseSchemaBootstrapper(connectionString))  // bootstrapper envuelto? no -- ver nota
    .Add(new SeedBootstrapper(serviceProvider))
    .RunAsync(cancellationToken);
```

> **Nota:** `CompositeBootstrapperAsync` espera instancias de `IBootstrapperAsync`. Envuelve un bootstrapper sync si es necesario:
> ```csharp
> public class SyncToAsyncWrapper(IBootstrapper inner) : IBootstrapperAsync
> {
>     public Task RunAsync(CancellationToken ct = default) { inner.Run(); return Task.CompletedTask; }
> }
> ```

---

## 7. Bootstrappers Async

`CompositeBootstrapperAsync` ejecuta todas las instancias de `IBootstrapperAsync` registradas secuencialmente usando `await`.

```csharp
await new CompositeBootstrapperAsync()
    .Add(new CheckConnectivityBootstrapper())
    .Add(new ApplyMigrationsBootstrapper())
    .Add(new WarmupCacheBootstrapper())
    .RunAsync(stoppingToken);
```

Cada bootstrapper recibe el mismo `CancellationToken` -- maneja la cancelacion en tu implementacion de `RunAsync`.

---

## 8. Ejemplos de Uso Standalone

### Ejemplo A -- Pipeline DI + AutoMapper (sin host)

```csharp
using Ums.Shell.Bootstrapper.Impl;
using Ums.Shell.Bootstrapper.DependencyInjection;
using Ums.Shell.Bootstrapper.AutoMapper;

var services = new ServiceCollection();

// Fase 1: configura DI
var di = new DependencyInjectionBootstrapper(services, s =>
{
    s.AddSingleton<IDiscountService, DiscountService>();
    s.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
});

// Fase 2: configura AutoMapper
Action<IMapperConfigurationExpression> mappings = cfg =>
{
    cfg.CreateMap<OrderEntity, OrderDto>();
    cfg.CreateMap<Order, OrderEntity>().ReverseMap();
};
var autoMapper = new AutoMapperBootstrapper(mappings);

// Ejecuta ambas fases en secuencia
new CompositeBootstrapper()
    .Add(di)
    .Add(autoMapper)
    .Run();

// Construye
var sp     = services.BuildServiceProvider();
var mapper = new MapperConfiguration(mappings).CreateMapper();
```

### Ejemplo B -- Inicio async con verificacion de schema

```csharp
await new CompositeBootstrapperAsync()
    .Add(new ConnectivityCheckBootstrapper(connectionString))   // custom
    .Add(new DatabaseSchemaBootstrapper(connectionString))       // custom
    .Add(new SeedBootstrapper(serviceProvider))                  // custom
    .RunAsync(CancellationToken.None);
```

### Ejemplo C -- Observabilidad cableada standalone (ej., un worker service)

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
    .ConfigureServices((_, s) => { /* copia registros */ })
    .Build();

await host.RunAsync();
```

---

## 9. Referencia API

### `CompositeBootstrapper`

| Metodo | Descripcion |
|---|---|
| `CompositeBootstrapper Add(IBootstrapper b)` | Agrega un bootstrapper; devuelve `this` para encadenar |
| `void Run()` | Ejecuta todos los bootstrappers en orden de adicion |

### `CompositeBootstrapperAsync`

| Metodo | Descripcion |
|---|---|
| `CompositeBootstrapperAsync Add(IBootstrapperAsync b)` | Agrega; devuelve `this` |
| `Task RunAsync(CancellationToken ct = default)` | Ejecuta todos secuencialmente con `await` |

### `DependencyInjectionBootstrapper`

| Constructor | Descripcion |
|---|---|
| `(Action<IServiceCollection>?)` | Crea un nuevo `ServiceCollection` internamente |
| `(IServiceCollection, Action<IServiceCollection>?)` | Usa la coleccion proporcionada |

`Result` → el `IServiceCollection` configurado.

### `AutoMapperBootstrapper`

| Constructor | Descripcion |
|---|---|
| `(Action<IMapperConfigurationExpression>?)` | Registra perfil de mapeo |

`Result` → `MapperConfigurationExpression`. Llama `.CreateMapper()` para obtener `IMapper`.

### `ObservabilityBootstrapper`

| Constructor | Descripcion |
|---|---|
| `(IServiceCollection, ObservabilityConfiguration, Action<IServiceCollection>?)` | Configura Serilog + tracing OTLP |

`Result` → el `IServiceCollection` (misma referencia pasada).

---

## 10. Patron de Integracion UMS

UMS actualmente usa `IHostedService`, `IStartupFilter`, y cableado directo de `Program.cs` para inicializacion de inicio. El patron Bootstrapper puede superponerse para casos complejos de multiples pasos.

### Patron recomendado para bootstrap de schema SQL Server

```csharp
// En Ums.Infrastructure/Hosting/SchemaBootstrapperService.cs
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

// Implementacion de cada fase:
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

### Observabilidad en Program.cs

```csharp
// En Ums.Presentation/Program.cs (o donde se componga el host)
var obsConfig = new ObservabilityConfiguration
{
    OTLPEndpoint   = builder.Configuration["OpenTelemetry:Endpoint"] ?? "http://localhost:4317",
    ServiceName    = "ums-api",
    ServiceVersion = "2.0.0"
};

new ObservabilityBootstrapper(builder.Services, obsConfig).Run();
// Serilog + OTel tracing ahora estan configurados
```

---

## Documentos Relacionados

- [AOP](aop.es.md) -- concerns cross-cutting para servicios inicializados por bootstrappers
- [Uso Combinado](combined-usage.md) -- ejemplo completo de Bootstrapper + DDD + Factory + AOP