namespace Ums.Presentation.Middleware;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Ums.Infrastructure.Persistence;

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
        var correlationId = context.TraceIdentifier;
        var problemDetails = CreateProblemDetails(context, exception, correlationId);

        LogException(exception, correlationId);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }

    private ProblemDetails CreateProblemDetails(HttpContext context, Exception exception, string correlationId)
    {
        var problemDetails = new ProblemDetails
        {
            Title = GetErrorTitle(exception),
            Detail = GetErrorDetail(exception),
            Status = GetStatusCode(exception),
            Type = $"https://httpstatuses.io/{GetStatusCode(exception)}",
            Instance = $"{context.Request.Path}{context.Request.QueryString}",
            Extensions =
            {
                ["traceId"] = correlationId,
                ["timestamp"] = DateTimeOffset.UtcNow,
            },
        };

        return problemDetails;
    }

    private string GetErrorTitle(Exception exception) => exception switch
    {
        ConcurrencyConflictException => "Conflict",                          // FIX-03
        UnauthorizedAccessException => "Unauthorized",
        System.Collections.Generic.KeyNotFoundException => "Not Found",
        InvalidOperationException => "Invalid Operation",
        ArgumentException => "Bad Request",
        _ => "Internal Server Error",
    };

    private string GetErrorDetail(Exception exception) => exception switch
    {
        ConcurrencyConflictException => "The resource was updated by another operation. Please try again.",
        UnauthorizedAccessException => "The request requires valid authentication credentials.",
        System.Collections.Generic.KeyNotFoundException => "The requested resource was not found.",
        InvalidOperationException => "The request could not be completed.",
        ArgumentException => "The request is invalid.",
        _ => "An unexpected error occurred. Please try again later.",
    };

    private int GetStatusCode(Exception exception) => exception switch
    {
        ConcurrencyConflictException => StatusCodes.Status409Conflict,       // FIX-03
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        System.Collections.Generic.KeyNotFoundException => StatusCodes.Status404NotFound,
        InvalidOperationException => StatusCodes.Status400BadRequest,
        ArgumentException => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status500InternalServerError,
    };

    private void LogException(Exception exception, string correlationId)
    {
        _logger.LogError(
            exception,
            "Unhandled exception occurred. CorrelationId: {CorrelationId}, ExceptionType: {ExceptionType}, Message: {Message}",
            correlationId,
            exception.GetType().Name,
            exception.Message);
    }
}

public static class GlobalExceptionHandlerExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionHandler>();
}
