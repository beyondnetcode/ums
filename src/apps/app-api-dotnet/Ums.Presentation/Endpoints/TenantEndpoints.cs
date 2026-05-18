namespace Ums.Presentation.Endpoints;

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Ums.Application.Tenants.ActivateTenant;
using Ums.Application.Tenants.AddBranch;
using Ums.Application.Tenants.CreateTenant;
using Ums.Application.Tenants.SuspendTenant;

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

        group.MapPost("/{tenantId:guid}/branches", async (
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

        return app;
    }
}

public sealed record AddBranchRequest(string Code, string Name, string? GeofencingMetadata);
