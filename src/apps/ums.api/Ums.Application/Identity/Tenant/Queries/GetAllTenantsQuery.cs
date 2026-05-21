using Ums.Application.Identity.Tenant.DTOs;

namespace Ums.Application.Identity.Tenant.Queries;

public sealed record GetAllTenantsQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string Criteria = "name",
    string Status = "all",
    string SortBy = "name",
    string SortOrder = "asc") : IQuery<PagedResult<TenantDto>>;
