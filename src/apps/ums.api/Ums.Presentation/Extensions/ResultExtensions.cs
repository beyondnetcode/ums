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
        => result.IsSuccess ? Results.NoContent() : ToBlockedOrProblem(result.Error, context);

    public static IResult ToOk<T>(this Result<T> result, HttpContext? context = null)
        => result.IsSuccess ? Results.Ok(result.Value) : ToProblem(result.Error, context);

    /// <summary>
    /// Detects structured blocking-dependency errors and returns a rich 409 response.
    /// Falls back to ToProblem for all other errors.
    /// </summary>
    private static IResult ToBlockedOrProblem(string error, HttpContext? context = null)
    {
        if (BlockedOperationError.TryDecode(error, out var errorCode, out var deps) && deps.Count > 0)
        {
            return Results.Json(new BlockedOperationResponse(
                ErrorCode:   errorCode,
                Message:     BlockedOperationMessages.GetMessage(errorCode),
                BrokenRule:  BlockedOperationMessages.GetRule(errorCode),
                BlockingDependencies: deps),
                statusCode: StatusCodes.Status409Conflict);
        }

        return ToProblem(error, context);
    }

    private static IResult ToProblem(string error, HttpContext? context = null)
    {
        var (status, title) = DomainErrorStatusMapper.Map(error);

        var problemDetails = new ProblemDetails
        {
            Title    = title,
            // Detail is user-visible; GetSafeDetail returns a localized, action-oriented string.
            // GetUserMessage is used only for domain-specific overrides (e.g. validation messages).
            Detail   = GetUserMessage(error, status),
            Status   = status,
            Type     = $"https://httpstatuses.io/{status}",
            Instance = context?.Request.Path,
            Extensions =
            {
                ["timestamp"] = DateTimeOffset.UtcNow,
            },
        };

        if (context != null)
        {
            var errorId = UserFacingErrorContext.GetOrCreateErrorId(context);
            // errorId is the only handle exposed to the caller.
            // traceId stays in logs — never in the response body.
            problemDetails.Extensions["errorId"] = errorId;

            context.RequestServices
                .GetService<ILoggerFactory>()?
                .CreateLogger("Ums.Presentation.ResultFailure")
                .LogWarning(
                    "Domain failure [{ErrorCategory}] on {RequestMethod} {RequestPath}. " +
                    "ErrorId: {ErrorId}, StatusCode: {StatusCode}",
                    title,
                    context.Request.Method,
                    context.Request.Path,
                    errorId,
                    status);
        }

        return Results.Problem(problemDetails);
    }

    private static string GetUserMessage(string error, int status)
    {
        // Updated: Validation failures now map to 422 Unprocessable Entity.
        if (status == StatusCodes.Status422UnprocessableEntity && error.StartsWith(validationPrefix, StringComparison.OrdinalIgnoreCase))
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

        if (error.Contains(DomainErrors.Tenant.BranchCodeNotUnique, StringComparison.OrdinalIgnoreCase))
        {
            return StringLocalizer.T(DomainErrors.Tenant.BranchCodeNotUnique);
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
