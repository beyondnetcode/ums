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
/// - CorrelationId, SessionTrackingId — set by request middleware log scopes
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
        var loggingSection = context.Configuration.GetSection("Observability:Logging");
        var consoleFormat = loggingSection["ConsoleFormat"] ?? (env.IsDevelopment() ? "Text" : "CompactJson");
        var minimumLevel = loggingSection["MinimumLevel"] ?? (env.IsDevelopment() ? "Debug" : "Information");
        var outputTemplate = loggingSection["OutputTemplate"]
            ?? "[{Timestamp:HH:mm:ss} {Level:u3}] {CorrelationId} {SessionTrackingId} {SourceContext} {Message:lj}{NewLine}{Exception}";

        loggerConfig
            .ReadFrom.Configuration(context.Configuration)     // honour appsettings Serilog section
            .Enrich.FromLogContext()                            // picks up BeginScope() key-values (e.g. CorrelationId, SessionTrackingId)
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            // HARDENING-04: Mask PII fields (email, password, token, etc.) before any sink sees them.
            .Enrich.With<PiiSanitizerEnricher>()
            .Destructure.With<PiiMaskingPolicy>()
            .MinimumLevel.Is(ParseLevel(minimumLevel))
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.Hosting.Lifetime", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning);

        if (string.Equals(consoleFormat, "CompactJson", StringComparison.OrdinalIgnoreCase))
        {
            loggerConfig
                .WriteTo.Console(new CompactJsonFormatter());
        }
        else
        {
            loggerConfig
                .WriteTo.Console(outputTemplate: outputTemplate);
        }

        return loggerConfig;
    }

    private static LogEventLevel ParseLevel(string value)
        => Enum.TryParse<LogEventLevel>(value, ignoreCase: true, out var parsed)
            ? parsed
            : LogEventLevel.Information;
}
