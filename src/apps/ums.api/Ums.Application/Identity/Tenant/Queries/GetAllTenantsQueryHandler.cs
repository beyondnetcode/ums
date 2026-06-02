using Ums.Application.Common.Interfaces;
using Ums.Application.Identity.Tenant.DTOs;
using Ums.Domain.Identity;
using static Ums.Application.Common.QueryRequestNormalizer;

namespace Ums.Application.Identity.Tenant.Queries;

public sealed class GetAllTenantsQueryHandler : IQueryHandler<GetAllTenantsQuery, PagedResult<TenantDto>>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantScopePolicy _tenantScopePolicy;

    public GetAllTenantsQueryHandler(ITenantRepository tenantRepository, ITenantScopePolicy tenantScopePolicy)
    {
        _tenantRepository = tenantRepository;
        _tenantScopePolicy = tenantScopePolicy;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<PagedResult<TenantDto>>> Handle(
        GetAllTenantsQuery request,
        CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);
        var criteria = NormalizeText(request.Criteria, "name").ToLowerInvariant();
        var status = NormalizeText(request.Status, "all");
        var sortBy = NormalizeText(request.SortBy, "name").ToLowerInvariant();
        var sortOrder = NormalizeText(request.SortOrder, "asc").ToLowerInvariant();
        var search = NormalizeSearch(request.Search);

        var effectiveTenantId = _tenantScopePolicy.ResolveQueryScope();

        // REC-12: Push filtering/sorting/pagination to the repository so SQL
        // implementations use DB-level Skip/Take instead of loading all rows.
        var (tenants, totalItems) = await _tenantRepository.GetPagedAsync(
            page, pageSize, search, status, sortBy, sortOrder, effectiveTenantId, cancellationToken);

        var items = tenants.Select(t => new TenantDto(
            t.Props.Id.GetValue(),
            t.Props.Code.GetValue(),
            t.Props.Name.GetValue(),
            t.Props.Type.ToString(),
            t.Props.Status.ToString(),
            t.Props.ParentTenantId?.GetValue(),
            t.Props.CompanyReference?.GetValue(),
            t.Props.IsManagementOwner))
            .ToList();

        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);

        return Result<PagedResult<TenantDto>>.Success(new PagedResult<TenantDto>(
            items,
            page,
            pageSize,
            totalItems,
            totalPages));
    }
}
