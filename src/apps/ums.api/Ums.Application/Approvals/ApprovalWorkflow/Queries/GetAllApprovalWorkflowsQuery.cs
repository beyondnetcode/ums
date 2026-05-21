using Ums.Application.Approvals.ApprovalWorkflow.DTOs;

namespace Ums.Application.Approvals.ApprovalWorkflow.Queries;

public sealed record GetAllApprovalWorkflowsQuery(
    int Page = 1, int PageSize = 20, string? Search = null, string Criteria = "name",
    string SortBy = "name", string SortOrder = "asc", Guid? TenantId = null) : IQuery<PagedResult<ApprovalWorkflowDto>>;
