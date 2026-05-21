namespace Ums.Presentation.GraphQL.Approvals;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Approvals.ApprovalWorkflow.DTOs;
using Ums.Application.Approvals.ApprovalWorkflow.Queries;

[ExtendObjectType("Query")]
public sealed class ApprovalWorkflowQueries
{
    public async Task<PagedResult<ApprovalWorkflowDto>> GetApprovalWorkflowsAsync(
        int page, int pageSize, string? search, string? criteria, string? sortBy, string? sortOrder,
        Guid? tenantId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllApprovalWorkflowsQuery(
            page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
            string.IsNullOrWhiteSpace(criteria) ? "name" : criteria,
            string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy,
            string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, tenantId), cancellationToken);
        if (result.IsFailure) throw new GraphQLException(ErrorBuilder.New().SetMessage(result.Error).SetCode("UMS_QUERY_ERROR").Build());
        return result.Value;
    }

    public async Task<ApprovalWorkflowDto?> GetApprovalWorkflowByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetApprovalWorkflowByIdQuery(id), cancellationToken);
        return result.IsFailure ? null : result.Value;
    }
}
