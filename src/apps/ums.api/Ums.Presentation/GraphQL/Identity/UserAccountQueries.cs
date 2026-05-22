namespace Ums.Presentation.GraphQL.Identity;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Identity.UserAccount.DTOs;
using Ums.Application.Identity.UserAccount.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

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
            NormalizePage(page),
            NormalizePageSize(pageSize),
            search,
            NormalizeText(criteria, "email"),
            NormalizeText(status, "all"),
            NormalizeText(sortBy, "email"),
            NormalizeText(sortOrder, "asc"),
            tenantId), cancellationToken);

        return result.UnwrapGraphQl();
    }

    public async Task<UserAccountDto?> GetUserAccountByIdAsync(
        Guid userAccountId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetUserAccountByIdQuery(userAccountId), cancellationToken);

        return result.UnwrapGraphQlOrNull();
    }
}
