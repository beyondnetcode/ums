namespace Ums.Presentation.Endpoints;

using Microsoft.AspNetCore.Http;
using Ums.Domain.Kernel;

internal static class ResultExtensions
{
    public static IResult ToCreated<T>(this Result<T> result, Func<T, string> locationFactory)
        => result.IsSuccess
            ? Results.Created(locationFactory(result.Value), result.Value)
            : ToProblem(result.Error);

    public static IResult ToNoContent(this Result result)
        => result.IsSuccess ? Results.NoContent() : ToProblem(result.Error);

    private static IResult ToProblem(string error)
    {
        var status = ClassifyError(error);
        return Results.Problem(
            detail: error,
            statusCode: status,
            title: TitleFor(status),
            type: $"https://httpstatuses.io/{status}");
    }

    private static int ClassifyError(string error)
    {
        if (string.IsNullOrEmpty(error)) return StatusCodes.Status400BadRequest;
        if (error.Contains("not found", StringComparison.OrdinalIgnoreCase)) return StatusCodes.Status404NotFound;
        if (error.Contains("Authenticated user is required", StringComparison.OrdinalIgnoreCase)) return StatusCodes.Status401Unauthorized;
        if (error.Contains("already", StringComparison.OrdinalIgnoreCase)) return StatusCodes.Status409Conflict;
        return StatusCodes.Status400BadRequest;
    }

    private static string TitleFor(int status) => status switch
    {
        StatusCodes.Status401Unauthorized => "Unauthorized",
        StatusCodes.Status404NotFound => "Not Found",
        StatusCodes.Status409Conflict => "Conflict",
        _ => "Bad Request",
    };
}
