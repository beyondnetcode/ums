namespace Ums.Presentation.Endpoints;

using Ums.Application.Identity.Tenant.ActivateTenant;
using Ums.Application.Identity.Tenant.CreateTenant;
using Ums.Application.Identity.Tenant.SuspendTenant;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants")
            .WithTags("Tenants");

        group.MapPost("/", async (CreateTenantCommand command, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/api/tenants/{r.TenantId}");
        })
        .WithName("CreateTenant")
        .WithSummary("Create a new tenant")
        .Produces<CreateTenantResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{tenantId:guid}/activate", async (Guid tenantId, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ActivateTenantCommand(tenantId), ct);
            return result.ToNoContent();
        })
        .WithName("ActivateTenant")
        .WithSummary("Activate a suspended tenant")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{tenantId:guid}/suspend", async (Guid tenantId, IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SuspendTenantCommand(tenantId), ct);
            return result.ToNoContent();
        })
        .WithName("SuspendTenant")
        .WithSummary("Suspend an active tenant")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
