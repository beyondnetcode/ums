namespace Ums.Presentation.Endpoints.Identity.Tenant;

using Ums.Application.Identity.Tenant.Commands;
using Ums.Application.Identity.Tenant.DTOs;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenants")
            .WithTags("Tenants");

        group.MapPost("/", async (CreateTenantCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/tenants/{r.TenantId}", context);
        })
        .WithName("CreateTenant")
        .WithSummary("Create a new tenant")
        .Produces<CreateTenantResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{tenantId:guid}/activate", async (Guid tenantId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ActivateTenantCommand(tenantId), ct);
            return result.ToNoContent(context);
        })
        .WithName("ActivateTenant")
        .WithSummary("Activate a suspended tenant")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{tenantId:guid}/suspend", async (Guid tenantId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SuspendTenantCommand(tenantId), ct);
            return result.ToNoContent(context);
        })
        .WithName("SuspendTenant")
        .WithSummary("Suspend an active tenant")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
