using Ums.Application.Approvals.UserDocument.DTOs;

namespace Ums.Application.Approvals.UserDocument.Queries;

public sealed record GetAllUserDocumentsQuery(
    int Page = 1, int PageSize = 20, string? Search = null, string Criteria = "status",
    string Status = "all", string SortBy = "status", string SortOrder = "asc",
    Guid? UserId = null, Guid? TenantId = null) : IQuery<PagedResult<UserDocumentDto>>;
