namespace Ums.Presentation.GraphQL.Approvals;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Approvals.AccessEnforcementPolicy.DTOs;
using Ums.Application.Approvals.AccessEnforcementPolicy.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class AccessEnforcementPolicyQueries
{
    public async Task<PagedResult<AccessEnforcementPolicyDto>> GetAccessEnforcementPoliciesAsync(
        int page, int pageSize, string? search, string? criteria, string? status, string? sortBy, string? sortOrder,
        Guid? tenantId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllAccessEnforcementPoliciesQuery(
            NormalizePage(page), NormalizePageSize(pageSize), search,
            NormalizeText(criteria, "enforcementAction"),
            NormalizeText(status, "all"),
            NormalizeText(sortBy, "enforcementAction"),
            NormalizeText(sortOrder, "asc"), tenantId), cancellationToken);
        return result.UnwrapGraphQl();
    }

    public async Task<AccessEnforcementPolicyDto?> GetAccessEnforcementPolicyByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAccessEnforcementPolicyByIdQuery(id), cancellationToken);
        return result.UnwrapGraphQlOrNull();
    }
}
