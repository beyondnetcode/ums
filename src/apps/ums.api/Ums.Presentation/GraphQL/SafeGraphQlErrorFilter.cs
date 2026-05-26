namespace Ums.Presentation.GraphQL;

using HotChocolate;
using HotChocolate.Execution;
using Ums.Globalization.Access;
using Serilog;

public sealed class SafeGraphQlErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        var errorId = Guid.NewGuid().ToString("D");

        if (error.Exception is not null)
        {
            Log.Error(
                error.Exception,
                "GraphQL request failed. ErrorId: {ErrorId}, Code: {ErrorCode}",
                errorId,
                error.Code ?? "UMS_GRAPHQL_ERROR");
        }
        else
        {
            Log.Warning(
                "GraphQL request returned a safe failure. ErrorId: {ErrorId}, Code: {ErrorCode}",
                errorId,
                error.Code ?? "UMS_GRAPHQL_ERROR");
        }

        var responseError = ErrorBuilder.FromError(error)
            .SetMessage(StringLocalizer.T("error.unexpected"))
            .ClearExtensions()
            .SetCode(error.Code ?? "UMS_GRAPHQL_ERROR")
            .SetExtension("errorId", errorId);

        return responseError.Build();
    }
}
