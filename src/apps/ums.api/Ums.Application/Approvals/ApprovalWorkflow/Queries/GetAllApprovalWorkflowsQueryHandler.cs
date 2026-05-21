using Ums.Application.Approvals.ApprovalWorkflow.DTOs;
using Ums.Domain.Approvals;

namespace Ums.Application.Approvals.ApprovalWorkflow.Queries;

public sealed class GetAllApprovalWorkflowsQueryHandler : IQueryHandler<GetAllApprovalWorkflowsQuery, PagedResult<ApprovalWorkflowDto>>
{
    private readonly IApprovalWorkflowRepository _repository;

    public GetAllApprovalWorkflowsQueryHandler(IApprovalWorkflowRepository repository) => _repository = repository;

    public async Task<Result<PagedResult<ApprovalWorkflowDto>>> Handle(GetAllApprovalWorkflowsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var sortBy = request.SortBy.Trim().ToLowerInvariant();
        var sortOrder = request.SortOrder.Trim().ToLowerInvariant();
        var search = request.Search?.Trim();

        var items = request.TenantId.HasValue
            ? await _repository.GetByTenantIdAsync(request.TenantId.Value, cancellationToken)
            : await _repository.GetAllAsync(cancellationToken);

        var query = items.Select(w => new ApprovalWorkflowDto(
            w.Props.Id.GetValue(), w.Props.TenantId.GetValue(), w.Props.SystemSuiteId?.GetValue(),
            w.Props.Code.GetValue(), w.Props.Name.GetValue(), w.Props.Description.GetValue(),
            w.Props.TargetUserCategory.ToString(), w.Props.RequiresApproval));

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(w => w.Name.Contains(search, StringComparison.OrdinalIgnoreCase));

        query = (sortBy, sortOrder) switch
        {
            ("code", "desc") => query.OrderByDescending(w => w.Code),
            ("code", _) => query.OrderBy(w => w.Code),
            _ => query.OrderBy(w => w.Name),
        };

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var paged = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Result<PagedResult<ApprovalWorkflowDto>>.Success(new PagedResult<ApprovalWorkflowDto>(paged, page, pageSize, totalItems, totalPages));
    }
}
