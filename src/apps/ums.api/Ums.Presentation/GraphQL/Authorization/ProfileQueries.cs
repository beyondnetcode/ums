namespace Ums.Presentation.GraphQL.Authorization;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Common;
using Ums.Application.Authorization.Profile.DTOs;
using Ums.Application.Authorization.Profile.Queries;

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
            page <= 0 ? 1 : page,
            pageSize <= 0 ? 20 : pageSize,
            search,
            string.IsNullOrWhiteSpace(criteria) ? "userId" : criteria,
            string.IsNullOrWhiteSpace(status) ? "all" : status,
            string.IsNullOrWhiteSpace(sortBy) ? "userId" : sortBy,
            string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder,
            tenantId,
            userId), cancellationToken);

        if (result.IsFailure)
        {
            throw BuildQueryException(result.Error);
        }

        return result.Value;
    }

    public async Task<ProfileDto?> GetProfileByIdAsync(
        Guid profileId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetProfileByIdQuery(profileId), cancellationToken);

        if (result.IsFailure)
        {
            return null;
        }

        return result.Value;
    }

    private static GraphQLException BuildQueryException(string message) =>
        new(ErrorBuilder.New()
            .SetMessage(message)
            .SetCode("UMS_QUERY_ERROR")
            .Build());
}
