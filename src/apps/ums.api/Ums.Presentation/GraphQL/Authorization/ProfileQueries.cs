namespace Ums.Presentation.GraphQL.Authorization;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Authorization.Profile.DTOs;
using Ums.Application.Authorization.Profile.Queries;
using Ums.Presentation.Extensions;
using static Ums.Application.Common.QueryRequestNormalizer;

[ExtendObjectType("Query")]
public sealed class ProfileQueries
{
    public async Task<PagedResult<ProfileDto>> GetProfilesAsync(
        int page,
        int pageSize,
        string? search,
        string? criteria,
        string? status,
        string? sortBy,
        string? sortOrder,
        Guid? tenantId,
        Guid? userId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllProfilesQuery(
            NormalizePage(page),
            NormalizePageSize(pageSize),
            search,
            NormalizeText(criteria, "userId"),
            NormalizeText(status, "all"),
            NormalizeText(sortBy, "userId"),
            NormalizeText(sortOrder, "asc"),
            tenantId,
            userId), cancellationToken);

        return result.UnwrapGraphQl();
    }

    public async Task<ProfileDto?> GetProfileByIdAsync(
        Guid profileId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProfileByIdQuery(profileId), cancellationToken);

        return result.UnwrapGraphQlOrNull();
    }
}
