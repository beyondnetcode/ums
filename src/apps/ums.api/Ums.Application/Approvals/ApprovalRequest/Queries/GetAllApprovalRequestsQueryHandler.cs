using Ums.Application.Approvals.ApprovalRequest.DTOs;
using Ums.Domain.Approvals;

namespace Ums.Application.Approvals.ApprovalRequest.Queries;

public sealed class GetAllApprovalRequestsQueryHandler : IQueryHandler<GetAllApprovalRequestsQuery, PagedResult<ApprovalRequestDto>>
{
    private readonly IApprovalRequestRepository _repository;

    public GetAllApprovalRequestsQueryHandler(IApprovalRequestRepository repository) => _repository = repository;

    public async Task<Result<PagedResult<ApprovalRequestDto>>> Handle(GetAllApprovalRequestsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var status = request.Status.Trim();
        var sortBy = request.SortBy.Trim().ToLowerInvariant();
        var sortOrder = request.SortOrder.Trim().ToLowerInvariant();
        var search = request.Search?.Trim();

        var items = request.TenantId.HasValue
            ? await _repository.GetByTenantIdAsync(request.TenantId.Value, cancellationToken)
            : await _repository.GetAllAsync(cancellationToken);

        var query = items.Select(r => new ApprovalRequestDto(
            r.Props.Id.GetValue(), r.Props.WorkflowId.GetValue(), r.Props.TargetUserId.GetValue(),
            r.Props.TargetProfileId?.GetValue(), r.Props.Status.ToString()));

        if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
            query = query.Where(r => string.Equals(r.Status, status, StringComparison.OrdinalIgnoreCase));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(r => r.WorkflowId.ToString().Contains(search, StringComparison.OrdinalIgnoreCase));

        query = (sortBy, sortOrder) switch
        {
            ("status", "desc") => query.OrderByDescending(r => r.Status),
            _ => query.OrderBy(r => r.Status),
        };

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var paged = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Result<PagedResult<ApprovalRequestDto>>.Success(new PagedResult<ApprovalRequestDto>(paged, page, pageSize, totalItems, totalPages));
    }
}
