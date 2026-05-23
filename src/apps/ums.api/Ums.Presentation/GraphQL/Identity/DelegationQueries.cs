namespace Ums.Presentation.GraphQL.Identity;

using HotChocolate;
using HotChocolate.Types;
using Ums.Application.Identity.UserManagementDelegation.DTOs;
using Ums.Application.Identity.UserManagementDelegation.Queries;
using Ums.Presentation.Extensions;

[ExtendObjectType("Query")]
public sealed class DelegationQueries
{
    public async Task<DelegationDto?> GetDelegationByIdAsync(
        Guid delegationId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDelegationByIdQuery(delegationId), cancellationToken);
        return result.UnwrapGraphQlOrNull();
    }

    public async Task<IReadOnlyList<DelegationDto>> GetDelegationsByDelegatedAdminAsync(
        Guid delegatedAdminId,
        Guid tenantId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDelegationsByDelegatedAdminQuery(delegatedAdminId, tenantId), cancellationToken);
        return result.UnwrapGraphQl();
    }

    public async Task<IReadOnlyList<DelegationDto>> GetDelegationsByDelegatingAdminAsync(
        Guid delegatingAdminId,
        Guid tenantId,
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetDelegationsByDelegatingAdminQuery(delegatingAdminId, tenantId), cancellationToken);
        return result.UnwrapGraphQl();
    }

    public async Task<IReadOnlyList<DelegationDto>> GetAllDelegationsAsync(
        [Service] IMediator mediator,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetAllDelegationsQuery(), cancellationToken);
        return result.UnwrapGraphQl();
    }
}
