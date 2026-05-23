namespace Ums.Presentation.Endpoints.Identity.UserManagementDelegation;

using Ums.Application.Identity.UserManagementDelegation.Commands;
using Ums.Application.Identity.UserManagementDelegation.DTOs;

public static class DelegationEndpoints
{
    public static IEndpointRouteBuilder MapDelegationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/delegations")
            .WithTags("Delegations");

        group.MapPost("/", async (CreateDelegationCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/delegations/{r.DelegationId}", context);
        })
        .WithName("CreateDelegation")
        .WithSummary("Create a new user management delegation")
        .Produces<CreateDelegationResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{delegationId:guid}/activate", async (Guid delegationId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ActivateDelegationCommand(delegationId), ct);
            return result.ToNoContent(context);
        })
        .WithName("ActivateDelegation")
        .WithSummary("Activate a draft delegation")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{delegationId:guid}/revoke", async (Guid delegationId, string reason, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RevokeDelegationCommand(delegationId, reason), ct);
            return result.ToNoContent(context);
        })
        .WithName("RevokeDelegation")
        .WithSummary("Revoke an active delegation")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{delegationId:guid}/expire", async (Guid delegationId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ExpireDelegationCommand(delegationId), ct);
            return result.ToNoContent(context);
        })
        .WithName("ExpireDelegation")
        .WithSummary("Mark an active delegation as expired (background worker use)")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
