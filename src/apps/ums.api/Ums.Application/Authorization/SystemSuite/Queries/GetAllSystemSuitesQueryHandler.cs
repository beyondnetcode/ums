using Ums.Application.Authorization.SystemSuite.DTOs;
using Ums.Domain.Authorization;
using static Ums.Application.Common.QueryRequestNormalizer;

namespace Ums.Application.Authorization.SystemSuite.Queries;

public sealed class GetAllSystemSuitesQueryHandler : IQueryHandler<GetAllSystemSuitesQuery, PagedResult<SystemSuiteDto>>
{
    private readonly ISystemSuiteRepository _systemSuiteRepository;

    public GetAllSystemSuitesQueryHandler(ISystemSuiteRepository systemSuiteRepository)
    {
        _systemSuiteRepository = systemSuiteRepository;
    }

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<PagedResult<SystemSuiteDto>>> Handle(
        GetAllSystemSuitesQuery request,
        CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);
        var criteria = NormalizeText(request.Criteria, "name").ToLowerInvariant();
        var status = NormalizeText(request.Status, "all");
        var sortBy = NormalizeText(request.SortBy, "name").ToLowerInvariant();
        var sortOrder = NormalizeText(request.SortOrder, "asc").ToLowerInvariant();
        var search = NormalizeSearch(request.Search);

        var systemSuites = request.TenantId.HasValue
            ? await _systemSuiteRepository.GetByTenantIdAsync(request.TenantId.Value, cancellationToken)
            : await _systemSuiteRepository.GetAllAsync(cancellationToken);

        var query = systemSuites.Select(SystemSuiteDto.Map);

        if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(s => string.Equals(s.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = criteria switch
            {
                "code" => query.Where(s => s.Code.Contains(search, StringComparison.OrdinalIgnoreCase)),
                "id" => query.Where(s => s.SystemSuiteId.ToString().Contains(search, StringComparison.OrdinalIgnoreCase)),
                _ => query.Where(s => s.Name.Contains(search, StringComparison.OrdinalIgnoreCase)),
            };
        }

        query = (sortBy, sortOrder) switch
        {
            ("code", "desc") => query.OrderByDescending(s => s.Code),
            ("code", _) => query.OrderBy(s => s.Code),
            ("status", "desc") => query.OrderByDescending(s => s.Status),
            ("status", _) => query.OrderBy(s => s.Status),
            ("name", "desc") => query.OrderByDescending(s => s.Name),
            _ => query.OrderBy(s => s.Name),
        };

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var items = query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Result<PagedResult<SystemSuiteDto>>.Success(new PagedResult<SystemSuiteDto>(
            items,
            page,
            pageSize,
            totalItems,
            totalPages));
    }
}
