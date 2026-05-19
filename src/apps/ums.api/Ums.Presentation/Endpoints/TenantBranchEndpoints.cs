namespace Ums.Presentation.Endpoints;

using Ums.Application.Identity.Tenant.AddBranch;
using Ums.Application.Identity.Tenant.DeactivateBranch;
using Ums.Application.Identity.Tenant.ReactivateBranch;
using Ums.Application.Identity.Tenant.RemoveBranch;

public static class TenantBranchEndpoints
{
    public static IEndpointRouteBuilder MapTenantBranchEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tenants/{tenantId:guid}/branches")
            .WithTags("Tenant Branches");

        group.MapPost("/", async (
            Guid tenantId,
            [FromBody] AddBranchRequest request,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var command = new AddBranchCommand(tenantId, request.Code, request.Name, request.GeofencingMetadata);
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/api/tenants/{r.TenantId}/branches");
        })
        .WithName("AddBranch")
        .WithSummary("Add a branch to an existing tenant")
        .Produces<AddBranchResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapDelete("/{branchId:guid}", async (
            Guid tenantId,
            Guid branchId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new RemoveBranchCommand(tenantId, branchId), ct);
            return result.ToNoContent();
        })
        .WithName("RemoveBranch")
        .WithSummary("Remove a branch from a tenant")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{branchId:guid}/deactivate", async (
            Guid tenantId,
            Guid branchId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeactivateBranchCommand(tenantId, branchId), ct);
            return result.ToNoContent();
        })
        .WithName("DeactivateBranch")
        .WithSummary("Deactivate a branch")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{branchId:guid}/reactivate", async (
            Guid tenantId,
            Guid branchId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new ReactivateBranchCommand(tenantId, branchId), ct);
            return result.ToNoContent();
        })
        .WithName("ReactivateBranch")
        .WithSummary("Reactivate a branch")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}

public sealed record AddBranchRequest(string Code, string Name, string? GeofencingMetadata);
