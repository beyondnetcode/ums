namespace Ums.Presentation.GraphQL.Approvals;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Approvals.NotificationRule.DTOs;
using Ums.Application.Approvals.NotificationRule.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class NotificationRuleQueries
{
    public async Task<PagedResult<NotificationRuleDto>> GetNotificationRulesAsync(
        int page, int pageSize, string? search, string? criteria, string? status, string? sortBy, string? sortOrder,
        Guid? tenantId, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllNotificationRulesQuery(
            NormalizePage(page), NormalizePageSize(pageSize), search,
            NormalizeText(criteria, "channel"),
            NormalizeText(status, "all"),
            NormalizeText(sortBy, "channel"),
            NormalizeText(sortOrder, "asc"), tenantId), cancellationToken);
        return result.UnwrapGraphQl();
    }

    public async Task<NotificationRuleDto?> GetNotificationRuleByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetNotificationRuleByIdQuery(id), cancellationToken);
        return result.UnwrapGraphQlOrNull();
    }
}
