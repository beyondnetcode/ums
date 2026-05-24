using Ums.Application.Configuration.AppConfiguration.DTOs;
using Ums.Domain.Configuration;
using static Ums.Application.Common.QueryRequestNormalizer;

namespace Ums.Application.Configuration.AppConfiguration.Queries;

public sealed class GetAllAppConfigurationsQueryHandler : IQueryHandler<GetAllAppConfigurationsQuery, PagedResult<AppConfigurationDto>>
{
    private readonly IAppConfigurationRepository _repository;

    public GetAllAppConfigurationsQueryHandler(IAppConfigurationRepository repository) => _repository = repository;

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<PagedResult<AppConfigurationDto>>> Handle(GetAllAppConfigurationsQuery request, CancellationToken cancellationToken)
    {
        var page = NormalizePage(request.Page);
        var pageSize = NormalizePageSize(request.PageSize);
        var search = NormalizeSearch(request.Search);
        var status = NormalizeText(request.Status, "all");
        var sortBy = NormalizeText(request.SortBy, "code").ToLowerInvariant();
        var sortOrder = NormalizeText(request.SortOrder, "asc").ToLowerInvariant();
        var scope = NormalizeSearch(request.Scope);

        var query = (await _repository.GetAllAsync(cancellationToken))
            .Select(configuration => new AppConfigurationDto(
                configuration.Props.Id.GetValue(),
                configuration.Props.TenantId?.GetValue(),
                configuration.Props.SystemSuiteId?.GetValue(),
                configuration.Props.ModuleId?.GetValue(),
                configuration.Props.Code.GetValue(),
                configuration.Props.Value.GetValue(),
                configuration.Props.Description.GetValue(),
                configuration.Props.Scope.Name,
                configuration.Props.IsInheritable,
                configuration.Props.IsEncrypted,
                configuration.Props.Version,
                configuration.Props.Status.Name));

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(configuration =>
                configuration.Code.Contains(search, StringComparison.OrdinalIgnoreCase)
                || configuration.Value.Contains(search, StringComparison.OrdinalIgnoreCase)
                || configuration.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(configuration => string.Equals(configuration.Status, status, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(scope))
        {
            query = query.Where(configuration => string.Equals(configuration.Scope, scope, StringComparison.OrdinalIgnoreCase));
        }

        if (request.TenantId.HasValue)
        {
            query = query.Where(configuration => configuration.TenantId == request.TenantId.Value);
        }

        if (request.SystemSuiteId.HasValue)
        {
            query = query.Where(configuration => configuration.SystemSuiteId == request.SystemSuiteId.Value);
        }

        if (request.ModuleId.HasValue)
        {
            query = query.Where(configuration => configuration.ModuleId == request.ModuleId.Value);
        }

        query = (sortBy, sortOrder) switch
        {
            ("scope", "desc") => query.OrderByDescending(configuration => configuration.Scope).ThenByDescending(configuration => configuration.Code),
            ("scope", _) => query.OrderBy(configuration => configuration.Scope).ThenBy(configuration => configuration.Code),
            ("status", "desc") => query.OrderByDescending(configuration => configuration.Status).ThenByDescending(configuration => configuration.Code),
            ("status", _) => query.OrderBy(configuration => configuration.Status).ThenBy(configuration => configuration.Code),
            ("version", "desc") => query.OrderByDescending(configuration => configuration.Version).ThenByDescending(configuration => configuration.Code),
            ("version", _) => query.OrderBy(configuration => configuration.Version).ThenBy(configuration => configuration.Code),
            ("code", "desc") => query.OrderByDescending(configuration => configuration.Code),
            _ => query.OrderBy(configuration => configuration.Code),
        };

        var totalItems = query.Count();
        var totalPages = totalItems == 0 ? 0 : (int)Math.Ceiling(totalItems / (double)pageSize);
        var paged = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

        return Result<PagedResult<AppConfigurationDto>>.Success(new PagedResult<AppConfigurationDto>(paged, page, pageSize, totalItems, totalPages));
    }
}
