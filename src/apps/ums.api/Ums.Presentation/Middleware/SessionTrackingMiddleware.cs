namespace Ums.Presentation.Middleware;

using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Ums.Infrastructure.Services;

/// <summary>
/// Ensures every request has a session tracking identifier that can be correlated across
/// logs, traces, business flows, and background handoffs.
/// </summary>
public sealed class SessionTrackingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<SessionTrackingMiddleware> _logger;

    public SessionTrackingMiddleware(
        RequestDelegate next,
        ILogger<SessionTrackingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, RequestContextAccessor requestContextAccessor)
    {
        var sessionTrackingId = GetOrAddSessionTrackingId(context);
        context.Response.Headers[ObservabilityHeaders.SessionTrackingId] = sessionTrackingId;

        var activity = Activity.Current;
        if (activity is not null)
        {
            activity.SetBaggage(ObservabilityKeys.SessionTrackingId, sessionTrackingId);
            activity.SetTag(ObservabilityKeys.SessionTrackingId, sessionTrackingId);
        }

        requestContextAccessor.Set(new ExecutionContextSnapshot(
            CorrelationId: activity?.GetBaggageItem(ObservabilityKeys.CorrelationId)
                ?? context.TraceIdentifier
                ?? string.Empty,
            SessionTrackingId: sessionTrackingId,
            TraceId: activity?.TraceId.ToString() ?? string.Empty,
            SpanId: activity?.SpanId.ToString() ?? string.Empty));

        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["SessionTrackingId"] = sessionTrackingId,
        }))
        {
            await _next(context);
        }
    }

    private static string GetOrAddSessionTrackingId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(ObservabilityHeaders.SessionTrackingId, out var existingId)
            && !string.IsNullOrWhiteSpace(existingId))
        {
            return existingId!;
        }

        return Guid.NewGuid().ToString("N");
    }
}

public static class SessionTrackingMiddlewareExtensions
{
    public static IApplicationBuilder UseSessionTracking(this IApplicationBuilder app)
        => app.UseMiddleware<SessionTrackingMiddleware>();
}
