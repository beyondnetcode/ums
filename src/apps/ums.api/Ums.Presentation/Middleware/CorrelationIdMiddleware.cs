namespace Ums.Presentation.Middleware;

using System.Diagnostics;
using Microsoft.AspNetCore.Http;

/// <summary>
/// REC-17: Injects a correlation-id into every request so distributed traces
/// can be correlated by a client-provided or auto-generated opaque ID.
///
/// Propagation chain:
///   1. Inbound  X-Correlation-Id header  →  stored on <see cref="HttpContext.TraceIdentifier"/>
///   2. TraceIdentifier                   →  Activity baggage  ("correlation.id")
///   3. TraceIdentifier                   →  ILogger scope     (key "CorrelationId")
///   4. Outbound X-Correlation-Id header  ←  echoed back to caller
///
/// OTEL baggage travels with outbound HttpClient calls (W3C baggage propagator),
/// so the correlation-id flows to every downstream service automatically.
/// The ILogger scope is picked up by any structured-log sink that reads scopes
/// (Serilog, OpenTelemetry.Logs, Application Insights).
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(
        RequestDelegate next,
        ILogger<CorrelationIdMiddleware> logger)
    {
        _next   = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrAddCorrelationId(context);
        context.Response.Headers[ObservabilityHeaders.CorrelationId] = correlationId;

        // Propagate into current OTEL Activity baggage so it travels
        // with all downstream HttpClient calls via W3C baggage header.
        var activity = Activity.Current;
        if (activity is not null)
        {
            activity.SetBaggage(ObservabilityKeys.CorrelationId, correlationId);
            // Also tag the root span so the ID appears in the trace UI.
            activity.SetTag(ObservabilityKeys.CorrelationId, correlationId);
        }

        // Enrich every log line emitted during this request.
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId,
        }))
        {
            await _next(context);
        }
    }

    private static string GetOrAddCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(ObservabilityHeaders.CorrelationId, out var existingId)
            && !string.IsNullOrWhiteSpace(existingId))
        {
            context.TraceIdentifier = existingId!;
            return existingId!;
        }

        var newId = Guid.NewGuid().ToString("N");
        context.TraceIdentifier = newId;
        return newId;
    }
}

public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}
