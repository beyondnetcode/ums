namespace Ums.Presentation.GraphQL;

using HotChocolate;
using HotChocolate.Execution;
using Serilog;

public sealed class SafeGraphQlErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is not null)
        {
            Log.Error(
                error.Exception,
                "GraphQL request failed. Code: {ErrorCode}",
                error.Code ?? "UMS_GRAPHQL_ERROR");
        }

        return ErrorBuilder.FromError(error)
            .SetMessage("The request could not be completed.")
            .ClearExtensions()
            .SetCode(error.Code ?? "UMS_GRAPHQL_ERROR")
            .Build();
    }
}
