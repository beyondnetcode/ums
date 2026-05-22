namespace Ums.Presentation.GraphQL.Approvals;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Approvals.ApprovalRequest.DTOs;
using Ums.Application.Approvals.ApprovalRequest.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class ApprovalRequestQueries
{
    public async Task<PagedResult<ApprovalRequestDto>> GetApprovalRequestsAsync(
        int page, int pageSize, string? search, string? criteria, string? status, string? sortBy, string? sortOrder,
        Guid? tenantId, Guid? userId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllApprovalRequestsQuery(
            NormalizePage(page), NormalizePageSize(pageSize), search,
            NormalizeText(criteria, "status"),
            NormalizeText(status, "all"),
            NormalizeText(sortBy, "status"),
            NormalizeText(sortOrder, "asc"), tenantId, userId), cancellationToken);
        return result.UnwrapGraphQl();
    }

    public async Task<ApprovalRequestDto?> GetApprovalRequestByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetApprovalRequestByIdQuery(id), cancellationToken);
        return result.UnwrapGraphQlOrNull();
    }
}
