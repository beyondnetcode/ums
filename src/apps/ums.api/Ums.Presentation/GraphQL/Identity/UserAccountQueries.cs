namespace Ums.Presentation.GraphQL.Identity;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Identity.UserAccount.DTOs;
using Ums.Application.Identity.UserAccount.Queries;

[ExtendObjectType("Query")]
public sealed class UserAccountQueries
{
    public async Task<PagedResult<UserAccountDto>> GetUserAccountsAsync(
        int page,
        int pageSize,
        string? search,
        string? criteria,
        string? status,
        string? sortBy,
        string? sortOrder,
        Guid? tenantId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllUserAccountsQuery(
            page <= 0 ? 1 : page,
            pageSize <= 0 ? 20 : pageSize,
            search,
            string.IsNullOrWhiteSpace(criteria) ? "email" : criteria,
            string.IsNullOrWhiteSpace(status) ? "all" : status,
            string.IsNullOrWhiteSpace(sortBy) ? "email" : sortBy,
            string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder,
            tenantId), cancellationToken);

        if (result.IsFailure)
        {
            throw BuildQueryException(result.Error);
        }

        return result.Value;
    }

    public async Task<UserAccountDto?> GetUserAccountByIdAsync(
        Guid userAccountId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserAccountByIdQuery(userAccountId), cancellationToken);

        if (result.IsFailure)
        {
            return null;
        }

        return result.Value;
    }

    private static GraphQLException BuildQueryException(string message) =>
        new(ErrorBuilder.New()
            .SetMessage(message)
            .SetCode("UMS_QUERY_ERROR")
            .Build());
}
