namespace Ums.Presentation.GraphQL.IGA;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.IGA.RoleMaturityStatus.DTOs;
using Ums.Application.IGA.RoleMaturityStatus.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class RoleMaturityStatusQueries
{
    public async Task<PagedResult<RoleMaturityStatusDto>> GetRoleMaturityStatusesAsync(
        int page, int pageSize, string? search, string? criteria, string? sortBy, string? sortOrder,
        Guid? tenantId, Guid? userId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllRoleMaturityStatusesQuery(
            NormalizePage(page), NormalizePageSize(pageSize), search,
            NormalizeText(criteria, "currentMaturityLevel"),
            NormalizeText(sortBy, "currentMaturityLevel"),
            NormalizeText(sortOrder, "asc"), tenantId, userId), cancellationToken);
        return result.UnwrapGraphQl();
    }

    public async Task<RoleMaturityStatusDto?> GetRoleMaturityStatusByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRoleMaturityStatusByIdQuery(id), cancellationToken);
        return result.UnwrapGraphQlOrNull();
    }
}
