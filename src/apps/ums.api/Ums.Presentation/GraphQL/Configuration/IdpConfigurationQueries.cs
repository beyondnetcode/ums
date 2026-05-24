namespace Ums.Presentation.GraphQL.Configuration;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Configuration.IdpConfiguration.DTOs;
using Ums.Application.Configuration.IdpConfiguration.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class IdpConfigurationQueries
{
    public async Task<PagedResult<IdpConfigurationDto>> GetIdpConfigurationsAsync(
        int page,
        int pageSize,
        string? search,
        string? criteria,
        string? status,
        string? sortBy,
        string? sortOrder,
        Guid? tenantId,
        Guid? systemSuiteId,
        string? providerType,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllIdpConfigurationsQuery(
            NormalizePage(page),
            NormalizePageSize(pageSize),
            search,
            NormalizeText(criteria, "providerType"),
            NormalizeText(status, "all"),
            NormalizeText(sortBy, "resolutionPriority"),
            NormalizeText(sortOrder, "asc"),
            tenantId,
            systemSuiteId,
            NormalizeSearch(providerType)), cancellationToken);
        return result.UnwrapGraphQl();
    }

    public async Task<IdpConfigurationDto?> GetIdpConfigurationByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetIdpConfigurationByIdQuery(id), cancellationToken);
        return result.UnwrapGraphQlOrNull();
    }

    public async Task<ResolvedIdpConfigurationDto?> ResolveIdpConfigurationAsync(
        Guid tenantId,
        Guid? systemSuiteId,
        string? emailDomain,
        string? providerType,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new ResolveIdpConfigurationQuery(
            tenantId,
            systemSuiteId,
            NormalizeSearch(emailDomain),
            NormalizeSearch(providerType)), cancellationToken);
        return result.UnwrapGraphQlOrNull();
    }
}
