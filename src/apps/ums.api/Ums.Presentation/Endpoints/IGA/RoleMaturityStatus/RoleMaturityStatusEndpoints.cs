namespace Ums.Presentation.Endpoints.IGA.RoleMaturityStatus;

using Ums.Application.Common;
using Ums.Application.IGA.RoleMaturityStatus.Commands;
using Ums.Application.IGA.RoleMaturityStatus.DTOs;
using Ums.Application.IGA.RoleMaturityStatus.Queries;

public static class RoleMaturityStatusEndpoints
{
    public static IEndpointRouteBuilder MapRoleMaturityStatusEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/role-maturity-statuses").WithTags("RoleMaturityStatuses");

        group.MapGet("/", async (
            [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? search,
            [FromQuery] string? criteria, [FromQuery] string? sortBy, [FromQuery] string? sortOrder,
            [FromQuery] Guid? tenantId, [FromQuery] Guid? userId,
            IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllRoleMaturityStatusesQuery(
                page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
                string.IsNullOrWhiteSpace(criteria) ? "currentMaturityLevel" : criteria,
                string.IsNullOrWhiteSpace(sortBy) ? "currentMaturityLevel" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, tenantId, userId), ct);
            return result.ToOk(context);
        }).WithName("GetAllRoleMaturityStatuses").Produces<PagedResult<RoleMaturityStatusDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateRoleMaturityStatusCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/role-maturity-statuses/{r.RoleMaturityStatusId}", context);
        }).WithName("CreateRoleMaturityStatus").Produces<CreateRoleMaturityStatusResponse>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}/level", async (Guid id, UpdateRoleMaturityLevelCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { RoleMaturityStatusId = id }, ct);
            return result.ToNoContent(context);
        }).WithName("UpdateRoleMaturityLevel").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status400BadRequest);

        // TODO(api-aggregate-tracker): Expose certification, training, performance, compliance issue, and eligibility review actions for RoleMaturityStatus.
        return app;
    }
}
