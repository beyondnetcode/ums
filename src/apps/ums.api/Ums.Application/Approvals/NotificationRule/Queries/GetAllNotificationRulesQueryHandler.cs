using Ums.Application.Approvals.NotificationRule.DTOs;
using Ums.Domain.Approvals;

namespace Ums.Application.Approvals.NotificationRule.Queries;

public sealed class GetAllNotificationRulesQueryHandler : IQueryHandler<GetAllNotificationRulesQuery, PagedResult<NotificationRuleDto>>
{
    private readonly INotificationRuleRepository _repository;

    public GetAllNotificationRulesQueryHandler(INotificationRuleRepository repository) => _repository = repository;

    public async Task<Result<PagedResult<NotificationRuleDto>>> Handle(GetAllNotificationRulesQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var status = request.Status.Trim();
        var sortBy = request.SortBy.Trim().ToLowerInvariant();
        var sortOrder = request.SortOrder.Trim().ToLowerInvariant();

        var items = request.TenantId.HasValue
            ? await _repository.GetByTenantIdAsync(request.TenantId.Value, cancellationToken)
            : await _repository.GetAllAsync(cancellationToken);

        var query = items.Select(r => new NotificationRuleDto(
            r.Props.Id.GetValue(), r.Props.TenantId.GetValue(), r.Props.Channel.ToString(),
            r.Props.Recipient.GetValue(), r.Props.IsActive));

        if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            var isActive = string.Equals(status, "active", StringComparison.OrdinalIgnoreCase);
            query = query.Where(r => r.IsActive == isActive);
        }

        query = (sortBy, sortOrder) switch
        {
            ("channel", "desc") => query.OrderByDescending(r => r.Channel),
            _ => query.OrderBy(r => r.Channel),
        };

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var paged = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Result<PagedResult<NotificationRuleDto>>.Success(new PagedResult<NotificationRuleDto>(paged, page, pageSize, totalItems, totalPages));
    }
}
