using Ums.Application.Authorization.Template.DTOs;
using Ums.Domain.Authorization;

namespace Ums.Application.Authorization.Template.Queries;

public sealed class GetAllPermissionTemplatesQueryHandler : IQueryHandler<GetAllPermissionTemplatesQuery, PagedResult<PermissionTemplateDto>>
{
    private readonly IPermissionTemplateRepository _templateRepository;

    public GetAllPermissionTemplatesQueryHandler(IPermissionTemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<Result<PagedResult<PermissionTemplateDto>>> Handle(
        GetAllPermissionTemplatesQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var criteria = request.Criteria.Trim().ToLowerInvariant();
        var status = request.Status.Trim();
        var sortBy = request.SortBy.Trim().ToLowerInvariant();
        var sortOrder = request.SortOrder.Trim().ToLowerInvariant();
        var search = request.Search?.Trim();

        var templates = request.TenantId.HasValue
            ? await _templateRepository.GetByTenantIdAsync(request.TenantId.Value, cancellationToken)
            : await _templateRepository.GetAllAsync(cancellationToken);

        var query = templates.Select(t => new PermissionTemplateDto(
            t.Props.Id.GetValue(),
            t.Props.TenantId.GetValue(),
            t.Props.RoleId.GetValue(),
            t.Props.SystemSuiteId.GetValue(),
            t.Props.Version.GetValue(),
            t.Props.Status.ToString()));

        if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(t => string.Equals(t.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = criteria switch
            {
                "id" => query.Where(t => t.TemplateId.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)),
                _ => query.Where(t => t.Version.Contains(search, StringComparison.OrdinalIgnoreCase)),
            };
        }

        query = (sortBy, sortOrder) switch
        {
            ("status", "desc") => query.OrderByDescending(t => t.Status),
            ("status", _) => query.OrderBy(t => t.Status),
            _ => query.OrderBy(t => t.Version),
        };

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Result<PagedResult<PermissionTemplateDto>>.Success(new PagedResult<PermissionTemplateDto>(
            items, page, pageSize, totalItems, totalPages));
    }
}
