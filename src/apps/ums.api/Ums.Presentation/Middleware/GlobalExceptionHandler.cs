namespace Ums.Presentation.Middleware;

using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Text.Json;

public sealed class GlobalExceptionHandler
{
    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(
        RequestDelegate next,
        IHostEnvironment environment,
        ILogger<GlobalExceptionHandler> logger)
    {
        _next = next;
        _environment = environment;
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

        if (_environment.IsDevelopment())
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            problemDetails.Extensions["exceptionType"] = exception.GetType().Name;
        }

        return problemDetails;
    }

    private string GetErrorTitle(Exception exception) => exception switch
    {
        UnauthorizedAccessException => "Unauthorized",
        KeyNotFoundException => "Not Found",
        InvalidOperationException => "Invalid Operation",
        ArgumentException => "Bad Request",
        _ => "Internal Server Error",
    };

    private string GetErrorDetail(Exception exception) => exception switch
    {
        UnauthorizedAccessException => "The request requires valid authentication credentials.",
        KeyNotFoundException => "The requested resource was not found.",
        InvalidOperationException => exception.Message,
        ArgumentException => exception.Message,
        _ => "An unexpected error occurred. Please try again later.",
    };

    private int GetStatusCode(Exception exception) => exception switch
    {
        UnauthorizedAccessException => StatusCodes.Status401Unauthorized,
        KeyNotFoundException => StatusCodes.Status404NotFound,
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
