namespace Ums.Presentation.GraphQL.Approvals;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Approvals.AccessEnforcementPolicy.DTOs;
using Ums.Application.Approvals.AccessEnforcementPolicy.Queries;

[ExtendObjectType("Query")]
public sealed class AccessEnforcementPolicyQueries
{
    public async Task<PagedResult<AccessEnforcementPolicyDto>> GetAccessEnforcementPoliciesAsync(
        int page, int pageSize, string? search, string? criteria, string? status, string? sortBy, string? sortOrder,
        Guid? tenantId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllAccessEnforcementPoliciesQuery(
            page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
            string.IsNullOrWhiteSpace(criteria) ? "enforcementAction" : criteria,
            string.IsNullOrWhiteSpace(status) ? "all" : status,
            string.IsNullOrWhiteSpace(sortBy) ? "enforcementAction" : sortBy,
            string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, tenantId), cancellationToken);
        if (result.IsFailure) throw new GraphQLException(ErrorBuilder.New().SetMessage(result.Error).SetCode("UMS_QUERY_ERROR").Build());
        return result.Value;
    }

    public async Task<AccessEnforcementPolicyDto?> GetAccessEnforcementPolicyByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAccessEnforcementPolicyByIdQuery(id), cancellationToken);
        return result.IsFailure ? null : result.Value;
    }
}
