namespace Ums.Presentation.Endpoints.Identity.Tenant.Queries;

using Ums.Application.Identity.Tenant.Branch.DTOs;
using Ums.Application.Identity.Tenant.Branch.Queries;

public static class BranchQueryEndpoints
{
    public static IEndpointRouteBuilder MapBranchQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenants/{tenantId:guid}/branches")
            .WithTags("Branches - Queries");

        group.MapGet("/", async (Guid tenantId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetBranchesByTenantIdQuery(tenantId), ct);
            return result.ToOk(context);
        })
        .WithName("GetBranchesByTenantId")
        .WithSummary("Get all branches for a tenant")
        .Produces<IReadOnlyList<BranchDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
