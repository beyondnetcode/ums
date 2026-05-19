namespace Ums.Presentation.Middleware;

using Microsoft.AspNetCore.Http;

public sealed class CorrelationIdMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-Id";
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrAddCorrelationId(context);
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        await _next(context);
    }

    private static string GetOrAddCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeader, out var existingId)
            && !string.IsNullOrWhiteSpace(existingId))
        {
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
