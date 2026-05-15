# ADR-0053: OpenTelemetry Observability Strategy

## Status

Accepted

## Date

2026-05-15

## Context

arc32 ADR-0007 mandates OpenTelemetry (OTel) as the unified observability standard for all satellite products. UMS is a multi-tenant, multi-bounded-context system with cross-context saga flows (e.g., user promotion requiring approvals + authorization mutations). Without structured tracing, diagnosing latency and failures across context boundaries is impractical.

Three pillars must be addressed:

1. **Traces** — distributed context propagation across HTTP handlers, command handlers, event consumers, and outbox processors
2. **Metrics** — RED (Rate, Error, Duration) for all endpoints and domain operations; tenant-aware aggregations
3. **Logs** — structured, correlated with trace/span IDs; shipped to a central store; no plain-text log lines

---

## Decision

**Adopt OpenTelemetry SDK for .NET as the single instrumentation standard. Serilog for structured logs, Jaeger/Tempo for traces, Prometheus + Grafana for metrics.**

### 1. Package Dependencies

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

### 2. OTel SDK Registration

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

### 3. UMS Activity Source & Meter

```csharp
// src/UMS.Infrastructure/Diagnostics/UmsDiagnostics.cs
public static class UmsDiagnostics
{
    public const string ActivitySourceName = "UMS";
    public const string MeterName          = "UMS";

    public static readonly ActivitySource ActivitySource =
        new(ActivitySourceName, "1.0.0");

    public static readonly Meter Meter = new(MeterName, "1.0.0");

    // Domain operation counters
    public static readonly Counter<long> CommandsExecuted =
        Meter.CreateCounter<long>("ums.commands.executed",
            description: "Total command handler invocations");

    public static readonly Counter<long> CommandsFailed =
        Meter.CreateCounter<long>("ums.commands.failed",
            description: "Total command handler failures");

    public static readonly Histogram<double> CommandDuration =
        Meter.CreateHistogram<double>("ums.commands.duration_ms",
            unit: "ms",
            description: "Command handler execution duration");

    public static readonly Counter<long> EventsPublished =
        Meter.CreateCounter<long>("ums.events.published",
            description: "Total domain events published to event bus");
}
```

### 4. Command Handler Tracing (Decorator Pattern)

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

### 5. Structured Logging with Serilog

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

Log lines must always carry trace correlation fields. The Serilog OTel sink injects `TraceId` and `SpanId` automatically when a current Activity exists.

Forbidden log patterns (enforced by code review):
- `_logger.LogInformation("User " + userId)` — string concatenation (no structured fields)
- `_logger.LogInformation(userObject.ToString())` — unstructured object dump

Required pattern:
```csharp
_logger.LogInformation("User {UserId} blocked by {ActorId} for reason {Reason}",
    userId, actorId, reason);
```

### 6. Tenant Dimension on All Metrics

Every metric emitted by UMS must include `tenant_id` as a tag to enable per-tenant dashboards and SLA tracking:

```csharp
UmsDiagnostics.CommandsExecuted.Add(1, new TagList
{
    ["command"]   = commandName,
    ["tenant_id"] = tenantId,   // always present
    ["success"]   = "true",
});
```

Cardinality risk: `tenant_id` is a low-cardinality tag in UMS (bounded by the number of tenant organizations, not end-users).

### 7. Infrastructure Stack

| Signal | Collector | Storage | Visualization |
|--------|-----------|---------|---------------|
| Traces | OTel Collector | Tempo | Grafana |
| Metrics | OTel Collector + Prometheus scrape | Prometheus | Grafana |
| Logs | OTel Collector | Loki | Grafana |

All three signals are exported via OTLP to a single OTel Collector endpoint (`OBSERVABILITY__OTLPENDPOINT`). The collector fans out to the appropriate backend.

Docker Compose service definitions (dev environment):

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

Health check endpoint metrics are scraped by Prometheus and displayed in the Grafana SLA dashboard.

---

## Consequences

### Positive

- Single OTel SDK instruments traces, metrics, and logs — no separate APM agent to maintain
- `ObservabilityBehavior` pipeline decorator gives uniform coverage for all command handlers without per-handler boilerplate
- Serilog + OTel sink ensures every log line is correlated with the active trace — no log/trace context mismatch
- Tenant-tagged metrics enable per-tenant SLA monitoring from day one
- Dev environment is fully observable via Docker Compose without cloud dependencies

### Negative

- `tenant_id` tag on every metric increases Prometheus series count proportionally with tenant growth — must monitor cardinality if tenant count exceeds ~1,000
- OTel Collector adds one network hop; a misconfigured collector silently drops signals — health check on collector export pipeline required
- `SetDbStatementForText = true` on SQL instrumentation may capture parameterized query text — ensure no PII appears in SQL bind parameters

---

**[ADR Registry](./index.md)** | **[arc32 ADR-0007](https://github.com/beyondnetcode/arc32_progresive_monolith/blob/main/reference/architecture/adrs/nodejs/0007-observability-telemetry-loki-opentelemetry.md)**
