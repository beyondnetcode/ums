namespace Ums.Presentation.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ums.Domain.Kernel;

internal static class ResultExtensions
{
    public static IResult ToCreated<T>(this Result<T> result, Func<T, string> locationFactory, HttpContext? context = null)
        => result.IsSuccess
            ? Results.Created(locationFactory(result.Value), result.Value)
            : ToProblem(result.Error, context);

    public static IResult ToNoContent(this Result result, HttpContext? context = null)
        => result.IsSuccess ? Results.NoContent() : ToProblem(result.Error, context);

    public static IResult ToOk<T>(this Result<T> result, HttpContext? context = null)
        => result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error, context);

    private static IResult ToProblem(string error, HttpContext? context = null)
    {
        var (status, title) = ClassifyError(error);

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = SanitizeErrorMessage(error),
            Status = status,
            Type = $"https://httpstatuses.io/{status}",
            Instance = context?.Request.Path,
            Extensions =
            {
                ["timestamp"] = DateTimeOffset.UtcNow,
            },
        };

        if (context != null)
        {
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        }

        return Results.Problem(problemDetails);
    }

    private static (int Status, string Title) ClassifyError(string error)
    {
        if (string.IsNullOrEmpty(error))
            return (StatusCodes.Status400BadRequest, "Bad Request");

        if (error.Contains("not found", StringComparison.OrdinalIgnoreCase))
            return (StatusCodes.Status404NotFound, "Not Found");

        if (error.Contains("Authenticated user is required", StringComparison.OrdinalIgnoreCase))
            return (StatusCodes.Status401Unauthorized, "Unauthorized");

        if (error.Contains("already", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("unique", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("conflict", StringComparison.OrdinalIgnoreCase))
            return (StatusCodes.Status409Conflict, "Conflict");

        if (error.Contains("unauthorized", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("forbidden", StringComparison.OrdinalIgnoreCase))
            return (StatusCodes.Status403Forbidden, "Forbidden");

        if (error.Contains("validation", StringComparison.OrdinalIgnoreCase) ||
            error.Contains("invalid", StringComparison.OrdinalIgnoreCase))
            return (StatusCodes.Status422UnprocessableEntity, "Validation Error");

        return (StatusCodes.Status400BadRequest, "Bad Request");
    }

    private static string SanitizeErrorMessage(string error)
    {
        if (string.IsNullOrEmpty(error))
            return "An error occurred processing the request.";

        var sanitized = error
            .Replace("System.", "", StringComparison.OrdinalIgnoreCase)
            .Replace("Ums.", "", StringComparison.OrdinalIgnoreCase)
            .Trim();

        return sanitized.Length > 200 ? sanitized[..200] + "..." : sanitized;
    }
}
