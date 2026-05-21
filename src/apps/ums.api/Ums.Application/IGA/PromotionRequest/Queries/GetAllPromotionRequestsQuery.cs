using Ums.Application.IGA.PromotionRequest.DTOs;

namespace Ums.Application.IGA.PromotionRequest.Queries;

public sealed record GetAllPromotionRequestsQuery(
    int Page = 1, int PageSize = 20, string? Search = null, string Criteria = "status",
    string Status = "all", string SortBy = "status", string SortOrder = "asc",
    Guid? TenantId = null, Guid? UserId = null) : IQuery<PagedResult<PromotionRequestDto>>;
