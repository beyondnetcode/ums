namespace Ums.Presentation.Endpoints.Identity.Tenant.Queries;

using Ums.Application.Identity.Tenant.DTOs;
using Ums.Application.Identity.Tenant.Queries;

public static class TenantQueryEndpoints
{
    public static IEndpointRouteBuilder MapTenantQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenants")
            .WithTags("Tenants - Queries");

        group.MapGet("/{tenantId:guid}", async (Guid tenantId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetTenantByIdQuery(tenantId), ct);
            return result.ToOk(context);
        })
        .WithName("GetTenantById")
        .WithSummary("Get tenant by ID")
        .Produces<TenantDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
