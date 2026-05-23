namespace Ums.Presentation.Extensions;

using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

/// <summary>
/// REC-06: Configura OpenTelemetry para trazabilidad distribuida y métricas.
///
/// Activa:
/// - Tracing: ASP.NET Core, HttpClient, EF Core (via source "Microsoft.EntityFrameworkCore")
/// - Metrics: ASP.NET Core, HttpClient, Runtime (.NET GC/threadpool)
/// - Exporter: OTLP (compatible con Jaeger, Tempo, Datadog, Azure Monitor, etc.)
///
/// Configuración en appsettings.json:
/// <code>
/// "OpenTelemetry": {
///   "Endpoint": "http://otel-collector:4317",
///   "ServiceVersion": "1.0.0"
/// }
/// </code>
/// </summary>
public static class ObservabilityExtensions
{
    private const string ServiceName = "ums-api";

    public static IServiceCollection AddUmsObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var endpoint = configuration["OpenTelemetry:Endpoint"];
        var version  = configuration["OpenTelemetry:ServiceVersion"] ?? "1.0.0";

        var resourceBuilder = ResourceBuilder.CreateDefault()
            .AddService(
                serviceName: ServiceName,
                serviceVersion: version,
                autoGenerateServiceInstanceId: true)
            .AddTelemetrySdk()
            .AddEnvironmentVariableDetector();

        services.AddOpenTelemetry()
            .WithTracing(tracing =>
            {
                tracing
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                        // Exclude health check noise from traces
                        opts.Filter = ctx =>
                            !ctx.Request.Path.StartsWithSegments("/health");
                    })
                    .AddHttpClientInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                    })
                    // EF Core SQL queries traced via activity source
                    .AddSource("Microsoft.EntityFrameworkCore");

                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    tracing.AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = new Uri(endpoint);
                    });
                }
                // Without an endpoint configured, traces are collected in-process
                // (useful for health checks and local profiling) but not exported.
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .SetResourceBuilder(resourceBuilder)
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddRuntimeInstrumentation()
                    // Custom UMS meters (reserved for future instrumentation)
                    .AddMeter("UMS.Application")
                    .AddMeter("UMS.Infrastructure");

                if (!string.IsNullOrWhiteSpace(endpoint))
                {
                    metrics.AddOtlpExporter(otlp =>
                    {
                        otlp.Endpoint = new Uri(endpoint);
                    });
                }
            });

        return services;
    }
}
