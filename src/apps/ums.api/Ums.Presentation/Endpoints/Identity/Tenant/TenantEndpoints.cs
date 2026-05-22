namespace Ums.Presentation.Endpoints.Identity.Tenant;

using Ums.Application.Common;
using Ums.Application.Identity.Tenant.Commands;
using Ums.Application.Identity.Tenant.DTOs;
using Ums.Application.Identity.Tenant.Queries;

public static class TenantEndpoints
{
    public static IEndpointRouteBuilder MapTenantEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenants")
            .WithTags("Tenants");

        // ── Queries ──────────────────────────────────────────────────────────

        group.MapGet("/", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search,
            [FromQuery] string? criteria,
            [FromQuery] string? status,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder,
            IMediator mediator,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllTenantsQuery(
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 20 : pageSize,
                search,
                string.IsNullOrWhiteSpace(criteria) ? "name" : criteria,
                string.IsNullOrWhiteSpace(status) ? "all" : status,
                string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder), ct);
            return result.ToOk(context);
        })
        .WithName("GetAllTenants")
        .WithSummary("Get tenants using server-side pagination")
        .Produces<PagedResult<TenantDto>>(StatusCodes.Status200OK);

        // ── Commands ─────────────────────────────────────────────────────────

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

        // TODO(api-aggregate-tracker): Expose tenant update, branch lifecycle, and identity-provider lifecycle commands for Tenant.
        return app;
    }
}
