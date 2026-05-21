using Ums.Application.Common.Interfaces;
using Ums.Application.Identity.Tenant.DTOs;
using Ums.Domain.Identity;

namespace Ums.Application.Identity.Tenant.Queries;

public sealed class GetAllTenantsQueryHandler : IQueryHandler<GetAllTenantsQuery, PagedResult<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository;

    public GetAllTenantsQueryHandler(ITenantRepository tenantRepository)
    {
        _tenantRepository = tenantRepository;
    }

    public async Task<Result<PagedResult<TenantDto>>> Handle(
        GetAllTenantsQuery request,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.Page);
        var pageSize = Math.Clamp(request.PageSize, 1, 100);
        var criteria = request.Criteria.Trim().ToLowerInvariant();
        var status = request.Status.Trim();
        var sortBy = request.SortBy.Trim().ToLowerInvariant();
        var sortOrder = request.SortOrder.Trim().ToLowerInvariant();
        var search = request.Search?.Trim();

        var tenants = await _tenantRepository.GetAllAsync(cancellationToken);

        var query = tenants.Select(t => new TenantDto(
            t.Props.Id.GetValue(),
            t.Props.Code.GetValue(),
            t.Props.Name.GetValue(),
            t.Props.Type.ToString(),
            t.Props.Status.ToString(),
            t.Props.ParentTenantId?.GetValue(),
            t.Props.CompanyReference?.GetValue()));

        if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(t => string.Equals(t.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = criteria switch
            {
                "code" => query.Where(t => t.Code.Contains(search, StringComparison.OrdinalIgnoreCase)),
                "id" => query.Where(t => t.TenantId.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)),
                _ => query.Where(t => t.Name.Contains(search, StringComparison.OrdinalIgnoreCase)),
            };
        }

        query = (sortBy, sortOrder) switch
        {
            ("code", "desc") => query.OrderByDescending(t => t.Code),
            ("code", _) => query.OrderBy(t => t.Code),
            ("status", "desc") => query.OrderByDescending(t => t.Status),
            ("status", _) => query.OrderBy(t => t.Status),
            ("name", "desc") => query.OrderByDescending(t => t.Name),
            _ => query.OrderBy(t => t.Name),
        };

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Result<PagedResult<TenantDto>>.Success(new PagedResult<TenantDto>(
            items,
            page,
            pageSize,
            totalItems,
            totalPages));
    }
}
