namespace Ums.Presentation.Middleware;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Ums.Globalization.Access;
using Ums.Infrastructure.Persistence;
using Ums.Presentation.Extensions;

/// <summary>
/// Global exception handler middleware.
///
/// SECURITY CONTRACT (OBS-01):
/// - Response body NEVER contains stack traces, exception types, or internal identifiers.
/// - The <c>errorId</c> is the only handle exposed to the caller; all technical details
///   (exception type, stack trace, correlation IDs) are emitted to the structured log
///   (Serilog → Grafana Loki) keyed on that same <c>errorId</c>.
/// - Operators look up <c>errorId</c> in Loki to retrieve the full diagnostic context.
/// </summary>
public sealed class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        RequestDelegate next,
        ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var errorId = UserFacingErrorContext.GetOrCreateErrorId(context);
        var statusCode = GetStatusCode(exception);
        var problemDetails = CreateProblemDetails(context, exception, errorId, statusCode);

        // Full diagnostic context goes to the log — never to the response body.
        LogException(exception, context, errorId);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(problemDetails, UmsProblemDetailsJsonOptions.Instance));
    }

    private static ProblemDetails CreateProblemDetails(
        HttpContext context,
        Exception exception,
        string errorId,
        int statusCode)
    {
        return new ProblemDetails
        {
            Title   = GetErrorTitle(exception),
            Detail  = GetErrorDetail(exception),   // localized, user-friendly — no internal info
            Status  = statusCode,
            Type    = $"https://httpstatuses.io/{statusCode}",
            Instance = $"{context.Request.Path}{context.Request.QueryString}",
            Extensions =
            {
                // errorId is the only handle the user gives to support.
                // Support queries Grafana Loki: {app="ums-api"} |= "<errorId>"
                ["errorId"]   = errorId,
                ["timestamp"] = DateTimeOffset.UtcNow,
            },
        };
    }

    private static string GetErrorTitle(Exception exception) => exception switch
    {
        ConcurrencyConflictException                          => "Conflict",
        UnauthorizedAccessException                           => "Unauthorized",
        System.Collections.Generic.KeyNotFoundException       => "Not Found",
        InvalidOperationException                             => "Invalid Operation",
        ArgumentException                                     => "Bad Request",
        _                                                     => "Internal Server Error",
    };

    private static string GetErrorDetail(Exception exception) => exception switch
    {
        ConcurrencyConflictException                    => StringLocalizer.T("error.operation.conflict"),
        UnauthorizedAccessException                     => StringLocalizer.T("error.authentication.required"),
        System.Collections.Generic.KeyNotFoundException => StringLocalizer.T("error.resource.not_found"),
        ArgumentException                               => StringLocalizer.T("error.request.invalid"),
        _                                               => StringLocalizer.T("error.unexpected"),
    };

    private static int GetStatusCode(Exception exception) => exception switch
    {
        ConcurrencyConflictException                          => StatusCodes.Status409Conflict,
        UnauthorizedAccessException                           => StatusCodes.Status401Unauthorized,
        System.Collections.Generic.KeyNotFoundException       => StatusCodes.Status404NotFound,
        InvalidOperationException                             => StatusCodes.Status400BadRequest,
        ArgumentException                                     => StatusCodes.Status400BadRequest,
        _                                                     => StatusCodes.Status500InternalServerError,
    };

    /// <summary>
    /// Logs the full exception with all diagnostic context.
    /// The <paramref name="errorId"/> is the correlation key between this log entry
    /// and what the user sees on screen — operators use it to look up the full details
    /// in Grafana Loki without the user needing to understand technical internals.
    /// </summary>
    private void LogException(Exception exception, HttpContext context, string errorId)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["ErrorId"]         = errorId,
            ["CorrelationId"]   = context.TraceIdentifier,
            ["RequestPath"]     = $"{context.Request.Method} {context.Request.Path}",
            ["ExceptionType"]   = exception.GetType().FullName ?? exception.GetType().Name,
        }))
        {
            _logger.LogError(
                exception,
                "Unhandled exception [{ExceptionType}] on {RequestMethod} {RequestPath}. " +
                "ErrorId: {ErrorId} — look up in Loki to see full stack trace.",
                exception.GetType().Name,
                context.Request.Method,
                context.Request.Path,
                errorId);
        }
    }
}

public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionHandler>();
}
