using Ums.Application.Authorization.Template.DTOs;

namespace Ums.Application.Authorization.Template.Queries;

public sealed record GetAllPermissionTemplatesQuery(
    int Page = 1,
    int PageSize = 20,
    string? Search = null,
    string Criteria = "version",
    string Status = "all",
    string SortBy = "version",
    string SortOrder = "asc",
    Guid? TenantId = null) : IQuery<PagedResult<PermissionTemplateDto>>;
