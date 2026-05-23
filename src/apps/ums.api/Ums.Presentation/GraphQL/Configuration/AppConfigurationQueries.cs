namespace Ums.Presentation.GraphQL.Configuration;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Configuration.AppConfiguration.DTOs;
using Ums.Application.Configuration.AppConfiguration.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class AppConfigurationQueries
{
    public async Task<PagedResult<AppConfigurationDto>> GetAppConfigurationsAsync(
        int page,
        int pageSize,
        string? search,
        string? criteria,
        string? status,
        string? sortBy,
        string? sortOrder,
        string? scope,
        Guid? tenantId,
        Guid? systemSuiteId,
        Guid? moduleId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllAppConfigurationsQuery(
            NormalizePage(page),
            NormalizePageSize(pageSize),
            search,
            NormalizeText(criteria, "code"),
            NormalizeText(status, "all"),
            NormalizeText(sortBy, "code"),
            NormalizeText(sortOrder, "asc"),
            NormalizeSearch(scope),
            tenantId,
            systemSuiteId,
            moduleId), cancellationToken);
        return result.UnwrapGraphQl();
    }

    public async Task<AppConfigurationDto?> GetAppConfigurationByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAppConfigurationByIdQuery(id), cancellationToken);
        return result.UnwrapGraphQlOrNull();
    }
}
