namespace Ums.Presentation.Middleware;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Ums.Globalization.Access;
using Ums.Infrastructure.Persistence;
using Ums.Presentation.Extensions;

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
        var errorId = UserFacingErrorContext.GetOrCreateErrorId(context);
        var problemDetails = CreateProblemDetails(context, exception, correlationId, errorId);

        LogException(exception, correlationId, errorId);

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problemDetails.Status ?? StatusCodes.Status500InternalServerError;

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(problemDetails, options));
    }

    private ProblemDetails CreateProblemDetails(
        HttpContext context,
        Exception exception,
        string correlationId,
        string errorId)
    {
        var userMessage = GetErrorDetail(exception);
        var problemDetails = new ProblemDetails
        {
            Title = GetErrorTitle(exception),
            Detail = userMessage,
            Status = GetStatusCode(exception),
            Type = $"https://httpstatuses.io/{GetStatusCode(exception)}",
            Instance = $"{context.Request.Path}{context.Request.QueryString}",
            Extensions =
            {
                ["traceId"] = correlationId,
                ["errorId"] = errorId,
                ["userMessage"] = userMessage,
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
        ConcurrencyConflictException => StringLocalizer.T("error.operation.conflict"),
        UnauthorizedAccessException => StringLocalizer.T("error.authentication.required"),
        System.Collections.Generic.KeyNotFoundException => StringLocalizer.T("error.resource.not_found"),
        ArgumentException => StringLocalizer.T("error.request.invalid"),
        _ => StringLocalizer.T("error.unexpected"),
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

    private void LogException(Exception exception, string correlationId, string errorId)
    {
        _logger.LogError(
            exception,
            "Unhandled exception occurred. ErrorId: {ErrorId}, CorrelationId: {CorrelationId}, ExceptionType: {ExceptionType}, Message: {Message}",
            errorId,
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
