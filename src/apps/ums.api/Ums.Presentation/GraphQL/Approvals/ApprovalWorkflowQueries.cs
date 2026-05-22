namespace Ums.Presentation.GraphQL.Approvals;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Approvals.ApprovalWorkflow.DTOs;
using Ums.Application.Approvals.ApprovalWorkflow.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class ApprovalWorkflowQueries
{
    public async Task<PagedResult<ApprovalWorkflowDto>> GetApprovalWorkflowsAsync(
        int page, int pageSize, string? search, string? criteria, string? sortBy, string? sortOrder,
        Guid? tenantId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllApprovalWorkflowsQuery(
            NormalizePage(page), NormalizePageSize(pageSize), search,
            NormalizeText(criteria, "name"),
            NormalizeText(sortBy, "name"),
            NormalizeText(sortOrder, "asc"), tenantId), cancellationToken);
        return result.UnwrapGraphQl();
    }

    public async Task<ApprovalWorkflowDto?> GetApprovalWorkflowByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetApprovalWorkflowByIdQuery(id), cancellationToken);
        return result.UnwrapGraphQlOrNull();
    }
}
