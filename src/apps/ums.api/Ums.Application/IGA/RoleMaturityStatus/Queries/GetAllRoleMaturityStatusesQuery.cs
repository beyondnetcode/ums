using Ums.Application.IGA.RoleMaturityStatus.DTOs;

namespace Ums.Application.IGA.RoleMaturityStatus.Queries;

public sealed record GetAllRoleMaturityStatusesQuery(
    int Page = 1, int PageSize = 20, string? Search = null, string Criteria = "currentMaturityLevel",
    string SortBy = "currentMaturityLevel", string SortOrder = "asc",
    Guid? TenantId = null, Guid? UserId = null) : IQuery<PagedResult<RoleMaturityStatusDto>>;
