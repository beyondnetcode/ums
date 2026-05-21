namespace Ums.Presentation.Endpoints.Approvals.AccessEnforcementPolicy;

using Ums.Application.Common;
using Ums.Application.Approvals.AccessEnforcementPolicy.Commands;
using Ums.Application.Approvals.AccessEnforcementPolicy.DTOs;
using Ums.Application.Approvals.AccessEnforcementPolicy.Queries;

public static class AccessEnforcementPolicyEndpoints
{
    public static IEndpointRouteBuilder MapAccessEnforcementPolicyEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/access-enforcement-policies").WithTags("AccessEnforcementPolicies");

        group.MapGet("/", async (
            [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? search,
            [FromQuery] string? criteria, [FromQuery] string? status, [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder, [FromQuery] Guid? tenantId,
            IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllAccessEnforcementPoliciesQuery(
                page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
                string.IsNullOrWhiteSpace(criteria) ? "enforcementAction" : criteria,
                string.IsNullOrWhiteSpace(status) ? "all" : status,
                string.IsNullOrWhiteSpace(sortBy) ? "enforcementAction" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, tenantId), ct);
            return result.ToOk(context);
        }).WithName("GetAllAccessEnforcementPolicies").Produces<PagedResult<AccessEnforcementPolicyDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateAccessEnforcementPolicyCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/access-enforcement-policies/{r.AccessEnforcementPolicyId}", context);
        }).WithName("CreateAccessEnforcementPolicy").Produces<CreateAccessEnforcementPolicyResponse>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/deactivate", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeactivateAccessEnforcementPolicyCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("DeactivateAccessEnforcementPolicy").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
