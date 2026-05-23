namespace Ums.Presentation.Extensions;

using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

/// <summary>
/// REC-14: Configures Serilog as the application's structured-logging provider.
///
/// Output strategy:
/// - Development: human-readable coloured console (non-JSON, easy to read locally)
/// - Staging/Production: compact JSON to stdout (machine-readable, picked up by
///   container log drivers, Fluentd, Datadog agent, Azure Log Analytics, etc.)
///
/// Enrichers always attached:
/// - MachineName, ThreadId — correlate logs from different pods/threads
/// - CorrelationId — set by CorrelationIdMiddleware log scope (REC-17)
///
/// To ship logs to a remote sink (Seq, Elasticsearch, Application Insights):
/// Add the sink package and configure the endpoint in appsettings.json under
/// the "Serilog" section — Serilog reads its own configuration natively.
///
/// Usage in Program.cs:
/// <code>
/// builder.Host.UseSerilog((ctx, cfg) => cfg.ConfigureUmsSerilog(ctx));
/// app.UseSerilogRequestLogging(opts => opts.EnrichDiagnosticContext = ...);
/// </code>
/// </summary>
public static class LoggingExtensions
{
    public static LoggerConfiguration ConfigureUmsSerilog(
        this LoggerConfiguration loggerConfig,
        HostBuilderContext context)
    {
        var env = context.HostingEnvironment;

        loggerConfig
            .ReadFrom.Configuration(context.Configuration)     // honour appsettings Serilog section
            .Enrich.FromLogContext()                            // picks up BeginScope() key-values (e.g. CorrelationId)
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            // HARDENING-04: Mask PII fields (email, password, token, etc.) before any sink sees them.
            .Enrich.With<PiiSanitizerEnricher>()
            .Destructure.With<PiiMaskingPolicy>()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning);

        if (env.IsDevelopment())
        {
            // Human-readable output for local development
            loggerConfig
                .MinimumLevel.Debug()
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {SourceContext} {Message:lj}{NewLine}{Exception}");
        }
        else
        {
            // Compact JSON — ideal for container log aggregators
            loggerConfig
                .MinimumLevel.Information()
                .WriteTo.Console(new CompactJsonFormatter());
        }

        return loggerConfig;
    }
}
