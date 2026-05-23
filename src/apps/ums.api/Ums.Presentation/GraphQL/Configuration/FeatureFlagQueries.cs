namespace Ums.Presentation.GraphQL.Configuration;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Configuration.FeatureFlag.DTOs;
using Ums.Application.Configuration.FeatureFlag.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class FeatureFlagQueries
{
    public async Task<PagedResult<FeatureFlagDto>> GetFeatureFlagsAsync(
        int page,
        int pageSize,
        string? search,
        string? criteria,
        string? status,
        string? sortBy,
        string? sortOrder,
        string? flagType,
        string? linkedResourceType,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllFeatureFlagsQuery(
            NormalizePage(page),
            NormalizePageSize(pageSize),
            search,
            NormalizeText(criteria, "flagCode"),
            NormalizeText(status, "all"),
            NormalizeText(sortBy, "flagCode"),
            NormalizeText(sortOrder, "asc"),
            NormalizeSearch(flagType),
            NormalizeSearch(linkedResourceType)), cancellationToken);
        return result.UnwrapGraphQl();
    }

    public async Task<FeatureFlagDto?> GetFeatureFlagByIdAsync(Guid id, [Service] IMediator mediator, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetFeatureFlagByIdQuery(id), cancellationToken);
        return result.UnwrapGraphQlOrNull();
    }
}
