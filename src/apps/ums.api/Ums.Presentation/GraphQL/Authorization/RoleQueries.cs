namespace Ums.Presentation.GraphQL.Authorization;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Authorization.Role.DTOs;
using Ums.Application.Authorization.Role.Queries;
using Ums.Presentation.Extensions;

[ExtendObjectType("Query")]
public sealed class RoleQueries
{
    public async Task<IReadOnlyList<RoleDto>> GetRolesBySystemSuiteAsync(
        Guid systemSuiteId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetRolesBySystemSuiteQuery(systemSuiteId), cancellationToken);
        return result.UnwrapGraphQl();
    }
}
