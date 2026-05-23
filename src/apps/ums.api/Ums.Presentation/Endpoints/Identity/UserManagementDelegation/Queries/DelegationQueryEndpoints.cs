namespace Ums.Presentation.Endpoints.Identity.UserManagementDelegation.Queries;

using Ums.Application.Identity.UserManagementDelegation.DTOs;
using Ums.Application.Identity.UserManagementDelegation.Queries;

public static class DelegationQueryEndpoints
{
    public static IEndpointRouteBuilder MapDelegationQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/delegations")
            .WithTags("Delegations - Queries");

        group.MapGet("/{delegationId:guid}", async (Guid delegationId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetDelegationByIdQuery(delegationId), ct);
            return result.ToOk(context);
        })
        .WithName("GetDelegationById")
        .WithSummary("Get delegation by ID")
        .Produces<DelegationDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/by-delegated-admin/{delegatedAdminId:guid}", async (Guid delegatedAdminId, Guid tenantId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetDelegationsByDelegatedAdminQuery(delegatedAdminId, tenantId), ct);
            return result.ToOk(context);
        })
        .WithName("GetDelegationsByDelegatedAdmin")
        .WithSummary("Get all delegations received by an admin")
        .Produces<IReadOnlyList<DelegationDto>>(StatusCodes.Status200OK);

        group.MapGet("/by-delegating-admin/{delegatingAdminId:guid}", async (Guid delegatingAdminId, Guid tenantId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetDelegationsByDelegatingAdminQuery(delegatingAdminId, tenantId), ct);
            return result.ToOk(context);
        })
        .WithName("GetDelegationsByDelegatingAdmin")
        .WithSummary("Get all delegations granted by an admin")
        .Produces<IReadOnlyList<DelegationDto>>(StatusCodes.Status200OK);

        return app;
    }
}
