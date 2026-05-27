namespace Ums.Presentation.Extensions;

using HotChocolate;
using HotChocolate.Execution;
using Ums.Domain.Kernel;

internal static class QueryResultExtensions
{
    public static T UnwrapGraphQl<T>(this Result<T> result, string code = "UMS_QUERY_ERROR")
    {
        if (result.IsSuccess)
        {
            return result.Value;
        }

        throw CreateGraphQlException(code);
    }

    public static T? UnwrapGraphQlOrNull<T>(this Result<T> result)
        where T : class
        => result.IsSuccess ? result.Value : null;

    public static GraphQLException ToGraphQlException(this string message, string code = "UMS_QUERY_ERROR")
        => CreateGraphQlException(code);

    private static GraphQLException CreateGraphQlException(string code)
        => new(ErrorBuilder.New()
            .SetMessage("The request could not be completed.")
            .SetCode(code)
            .Build());
}
