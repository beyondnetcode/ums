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
        var (status, title) = DomainErrorStatusMapper.Map(error);

        var problemDetails = new ProblemDetails
        {
            Title = title,
            Detail = GetSafeDetail(status),
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

    private static string GetSafeDetail(int status) => status switch
    {
        StatusCodes.Status401Unauthorized => "Authentication is required to complete this request.",
        StatusCodes.Status403Forbidden => "You do not have permission to complete this request.",
        StatusCodes.Status404NotFound => "The requested resource was not found.",
        StatusCodes.Status409Conflict => "The operation conflicts with the current resource state.",
        StatusCodes.Status422UnprocessableEntity => "The submitted information is invalid.",
        _ => "The request could not be completed.",
    };
}
