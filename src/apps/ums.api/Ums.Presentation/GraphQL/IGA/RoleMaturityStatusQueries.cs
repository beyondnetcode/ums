namespace Ums.Presentation.GraphQL.IGA;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.IGA.RoleMaturityStatus.DTOs;
using Ums.Application.IGA.RoleMaturityStatus.Queries;

[ExtendObjectType("Query")]
public sealed class RoleMaturityStatusQueries
{
    public async Task<PagedResult<RoleMaturityStatusDto>> GetRoleMaturityStatusesAsync(
        int page, int pageSize, string? search, string? criteria, string? sortBy, string? sortOrder,
        Guid? tenantId, Guid? userId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllRoleMaturityStatusesQuery(
            page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
            string.IsNullOrWhiteSpace(criteria) ? "currentMaturityLevel" : criteria,
            string.IsNullOrWhiteSpace(sortBy) ? "currentMaturityLevel" : sortBy,
            string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, tenantId, userId), cancellationToken);
        if (result.IsFailure) throw new GraphQLException(ErrorBuilder.New().SetMessage(result.Error).SetCode("UMS_QUERY_ERROR").Build());
        return result.Value;
    }

    public async Task<RoleMaturityStatusDto?> GetRoleMaturityStatusByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRoleMaturityStatusByIdQuery(id), cancellationToken);
        return result.IsFailure ? null : result.Value;
    }
}
