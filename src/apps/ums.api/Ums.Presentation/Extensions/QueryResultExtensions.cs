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

        throw CreateGraphQlException(result.Error, code);
    }

    public static T? UnwrapGraphQlOrNull<T>(this Result<T> result)
        where T : class
        => result.IsSuccess ? result.Value : null;

    public static GraphQLException ToGraphQlException(this string message, string code = "UMS_QUERY_ERROR")
        => CreateGraphQlException(message, code);

    private static GraphQLException CreateGraphQlException(string message, string code)
        => new(ErrorBuilder.New()
            .SetMessage(message)
            .SetCode(code)
            .Build());
}
