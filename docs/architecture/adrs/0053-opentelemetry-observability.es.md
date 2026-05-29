# ADR-0053: Estrategia de Observabilidad con OpenTelemetry

## Estado

Aceptado

## Fecha

2026-05-15

## Contexto

El ADR-0007 de Evolith establece OpenTelemetry (OTel) como el estándar unificado de observabilidad para todos los productos satelite. UMS es un sistema multi-tenant con múltiples bounded contexts y flujos saga cross-context (ej., promoción de usuario que requiere aprobaciones + mutaciones de autorización). Sin tracing estructurado, diagnosticar latencia y fallos a través de límites de contexto es impráctico.

Se deben abordar tres pilares:

1. **Traces** — propagación de contexto distribuido a través de handlers HTTP, command handlers, consumidores de eventos, y processors de outbox
2. **Metrics** — RED (Rate, Error, Duration) para todos los endpoints y operaciones de dominio; agregaciones aware de tenant
3. **Logs** — estructurados, correlacionados con IDs de trace/span; enviados a un store central; sin líneas de log en texto plano

---

## Decisión

**Adoptar OpenTelemetry SDK para .NET como el estándar único de instrumentación. Serilog para logs estructurados, Jaeger/Tempo para traces, Prometheus + Grafana para metrics.**

### 1. Dependencias de Paquetes

```xml
<!-- src/UMS.Infrastructure/UMS.Infrastructure.csproj -->
<PackageReference Include="OpenTelemetry.Extensions.Hosting"        Version="1.8.*" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.8.*" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http"       Version="1.8.*" />
<PackageReference Include="OpenTelemetry.Instrumentation.SqlClient"  Version="1.8.*" />
<PackageReference Include="OpenTelemetry.Exporter.Otlp"             Version="1.8.*" />
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.8.*" />
<PackageReference Include="Serilog.AspNetCore"                       Version="8.*" />
<PackageReference Include="Serilog.Sinks.OpenTelemetry"             Version="3.*" />
```

### 2. Registro del OTel SDK

```csharp
// src/UMS.Infrastructure/DependencyInjection.cs
public static IServiceCollection AddObservability(
    this IServiceCollection services,
    IConfiguration config)
{
    var otlpEndpoint = config["Observability:OtlpEndpoint"] ?? "http://localhost:4317";
    var serviceName  = "ums";
    var serviceVersion = Assembly.GetEntryAssembly()!
        .GetCustomAttribute<AssemblyInformationalVersionAttribute>()!
        .InformationalVersion;

    services.AddOpenTelemetry()
        .ConfigureResource(r => r
            .AddService(serviceName, serviceVersion: serviceVersion)
            .AddAttributes(new Dictionary<string, object>
            {
                ["deployment.environment"] = config["ASPNETCORE_ENVIRONMENT"] ?? "production",
            }))
        .WithTracing(t => t
            .AddAspNetCoreInstrumentation(o => o.RecordException = true)
            .AddHttpClientInstrumentation()
            .AddSqlClientInstrumentation(o => o.SetDbStatementForText = true)
            .AddSource(UmsDiagnostics.ActivitySourceName)
            .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)))
        .WithMetrics(m => m
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddMeter(UmsDiagnostics.MeterName)
            .AddPrometheusExporter()
            .AddOtlpExporter(o => o.Endpoint = new Uri(otlpEndpoint)));

    return services;
}
```

### 3. UMS Activity Source y Meter

```csharp
// src/UMS.Infrastructure/Diagnostics/UmsDiagnostics.cs
public static class UmsDiagnostics
{
    public const string ActivitySourceName = "UMS";
    public const string MeterName          = "UMS";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName, "1.0.0");

    public static readonly Meter Meter = new(MeterName, "1.0.0");

    // Contadores de operaciones de dominio
    public static readonly Counter<long> CommandsExecuted =
        Meter.CreateCounter<long>("ums.commands.executed",
            description: "Total de invocaciones de command handler");

    public static readonly Counter<long> CommandsFailed =
        Meter.CreateCounter<long>("ums.commands.failed",
            description: "Total de fallos de command handler");

    public static readonly Histogram<double> CommandDuration =
        Meter.CreateHistogram<double>("ums.commands.duration_ms",
            unit: "ms",
            description: "Duración de ejecución del command handler");

    public static readonly Counter<long> EventsPublished =
        Meter.CreateCounter<long>("ums.events.published",
            description: "Total de domain events publicados al event bus");
}
```

### 4. Tracing de Command Handler (Decorator Pattern)

```csharp
// src/UMS.Application/Behaviors/ObservabilityBehavior.cs
public sealed class ObservabilityBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICommand<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var commandName = typeof(TRequest).Name;
        var tenantId    = (request as ITenantScoped)?.TenantId.ToString() ?? "unknown";

        using var activity = UmsDiagnostics.ActivitySource.StartActivity(
            $"command.{commandName}",
            ActivityKind.Internal);

        activity?.SetTag("ums.command",   commandName);
        activity?.SetTag("ums.tenant_id", tenantId);

        var sw = Stopwatch.StartNew();
        try
        {
            var result = await next();
            UmsDiagnostics.CommandsExecuted.Add(1,
                new TagList { ["command"] = commandName, ["tenant_id"] = tenantId });
            return result;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.RecordException(ex);
            UmsDiagnostics.CommandsFailed.Add(1,
                new TagList { ["command"] = commandName, ["tenant_id"] = tenantId });
            throw;
        }
        finally
        {
            sw.Stop();
            UmsDiagnostics.CommandDuration.Record(sw.Elapsed.TotalMilliseconds,
                new TagList { ["command"] = commandName });
        }
    }
}
```

### 5. Logging Estructurado con Serilog

```csharp
// Program.cs
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("service", "ums")
    .WriteTo.OpenTelemetry(o =>
    {
        o.Endpoint = builder.Configuration["Observability:OtlpEndpoint"]
            ?? "http://localhost:4317";
        o.ResourceAttributes = new Dictionary<string, object>
        {
            ["service.name"] = "ums",
        };
    })
    .WriteTo.Console(new RenderedCompactJsonFormatter())
    .CreateLogger();
```

Las líneas de log deben siempre llevar campos de correlación de trace. El sink OTel de Serilog inyecta `TraceId` y `SpanId` automáticamente cuando existe un Activity activo.

Patrones de log prohibidos (enforzados por code review):
- `_logger.LogInformation("User " + userId)` — concatenación de strings (sin campos estructurados)
- `_logger.LogInformation(userObject.ToString())` — dump de objeto no estructurado

Patrón requerido:
```csharp
_logger.LogInformation("User {UserId} blocked by {ActorId} for reason {Reason}",
    userId, actorId, reason);
```

### 6. Dimensión Tenant en Todas las Métricas

Cada métrica emitida por UMS debe incluir `tenant_id` como tag para habilitar dashboards por tenant y tracking de SLA:

```csharp
UmsDiagnostics.CommandsExecuted.Add(1, new TagList
{
    ["command"]   = commandName,
    ["tenant_id"] = tenantId,   // siempre presente
    ["success"]   = "true",
});
```

Riesgo de cardinalidad: `tenant_id` es un tag de baja cardinalidad en UMS (limitado por el número de organizaciones tenant, no por usuarios finales).

### 7. Stack de Infraestructura

| Señal | Collector | Almacenamiento | Visualización |
|-------|-----------|----------------|---------------|
| Traces | OTel Collector | Tempo | Grafana |
| Metrics | OTel Collector + Prometheus scrape | Prometheus | Grafana |
| Logs | OTel Collector | Loki | Grafana |

Las tres señales se exportan vía OTLP a un único endpoint de OTel Collector (`OBSERVABILITY__OTLPENDPOINT`). El collector distribuye a los backends apropiados.

Definiciones de servicio Docker Compose (entorno dev):

```yaml
# docker-compose.observability.yml
services:
  otel-collector:
    image: otel/opentelemetry-collector-contrib:0.101.0
    ports: ["4317:4317", "4318:4318", "8889:8889"]
    volumes: ["./otel-collector-config.yml:/etc/otelcol/config.yaml"]

  tempo:
    image: grafana/tempo:2.5.0
    ports: ["3200:3200"]

  loki:
    image: grafana/loki:3.0.0
    ports: ["3100:3100"]

  prometheus:
    image: prom/prometheus:v2.52.0
    ports: ["9090:9090"]

  grafana:
    image: grafana/grafana:11.0.0
    ports: ["3000:3000"]
    environment:
      GF_AUTH_ANONYMOUS_ENABLED: "true"
```

### 8. Health Checks

```csharp
services.AddHealthChecks()
    .AddSqlServer(
        connectionString: config.GetConnectionString("UmsDb")!,
        name: "sql-server",
        tags: ["db", "ready"])
    .AddRabbitMQ(
        name: "rabbitmq",
        tags: ["bus", "ready"])
    .AddCheck<OutboxHealthCheck>("outbox-lag", tags: ["ready"]);

app.MapHealthChecks("/health/live",  new() { Predicate = c => c.Tags.Contains("live") });
app.MapHealthChecks("/health/ready", new() { Predicate = c => c.Tags.Contains("ready") });
```

Las métricas de health check endpoint son scrapedas por Prometheus y mostradas en el dashboard SLA de Grafana.

---

## Consecuencias

### Positivas

- Un único OTel SDK instrumenta traces, metrics, y logs — sin agente APM separado que mantener
- `ObservabilityBehavior` pipeline decorator da cobertura uniforme para todos los command handlers sin boilerplate por handler
- Serilog + sink OTel asegura que cada línea de log esté correlacionada con el trace activo — sin mismatch de contexto log/trace
- Métricas con tag de tenant habilitan monitoreo SLA por tenant desde el día uno
- Entorno dev es completamente observable vía Docker Compose sin dependencias de cloud

### Negativas

- Tag `tenant_id` en cada métrica incrementa el conteo de series de Prometheus proporcionalmente con el crecimiento de tenants — debe monitorearse la cardinalidad si el conteo de tenants excede ~1,000
- OTel Collector añade un network hop; un collector mal configurado silently drop signals — se requiere health check en el pipeline de exportación del collector
- `SetDbStatementForText = true` en instrumentación SQL puede capturar texto de query parametrizada — asegurar que no aparezca PII en los parámetros bind de SQL

---

**[Índice ADR](./index.md)** | **[ADR-0007 de Evolith](https://github.com/beyondnetcode/evolith_arch32/blob/main/reference/architecture/adrs/nodejs/0007-observability-telemetry-loki-opentelemetry.md)**