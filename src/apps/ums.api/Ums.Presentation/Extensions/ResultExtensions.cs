namespace Ums.Presentation.Extensions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ums.Globalization.Access;
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
        var userMessage = GetUserMessage(error, status);

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
            var errorId = UserFacingErrorContext.GetOrCreateErrorId(context);
            problemDetails.Extensions["traceId"] = context.TraceIdentifier;
            problemDetails.Extensions["errorId"] = errorId;

            context.RequestServices
                .GetService<ILoggerFactory>()?
                .CreateLogger("Ums.Presentation.ResultFailure")
                .LogWarning(
                    "Request returned an expected failure. ErrorId: {ErrorId}, StatusCode: {StatusCode}, ErrorCategory: {ErrorCategory}",
                    errorId,
                    status,
                    title);
        }

        problemDetails.Extensions["userMessage"] = userMessage;

        return Results.Problem(problemDetails);
    }

    private static string GetUserMessage(string error, int status)
    {
        const string validationPrefix = "Validation.Failed:";

        if (status == StatusCodes.Status422UnprocessableEntity
            && error.StartsWith(validationPrefix, StringComparison.OrdinalIgnoreCase))
        {
            var message = error[validationPrefix.Length..].Trim();
            return string.IsNullOrWhiteSpace(message)
                ? StringLocalizer.T("error.validation.invalid")
                : message;
        }

        if (error.Contains(DomainErrors.SystemSuite.ModuleCodeNotUnique, StringComparison.OrdinalIgnoreCase))
        {
            return StringLocalizer.T("system_suite.module.code_not_unique");
        }

        return status switch
        {
            StatusCodes.Status401Unauthorized => StringLocalizer.T("error.authentication.required"),
            StatusCodes.Status403Forbidden => StringLocalizer.T("error.authorization.forbidden"),
            StatusCodes.Status404NotFound => StringLocalizer.T("error.resource.not_found"),
            StatusCodes.Status409Conflict => StringLocalizer.T("error.operation.conflict"),
            StatusCodes.Status422UnprocessableEntity => StringLocalizer.T("error.validation.invalid"),
            _ => StringLocalizer.T("error.request.invalid"),
        };
    }

    private static string GetSafeDetail(int status) => status switch
    {
        StatusCodes.Status401Unauthorized => StringLocalizer.T("error.authentication.required"),
        StatusCodes.Status403Forbidden => StringLocalizer.T("error.authorization.forbidden"),
        StatusCodes.Status404NotFound => StringLocalizer.T("error.resource.not_found"),
        StatusCodes.Status409Conflict => StringLocalizer.T("error.operation.conflict"),
        StatusCodes.Status422UnprocessableEntity => StringLocalizer.T("error.validation.invalid"),
        _ => StringLocalizer.T("error.request.invalid"),
    };
}
