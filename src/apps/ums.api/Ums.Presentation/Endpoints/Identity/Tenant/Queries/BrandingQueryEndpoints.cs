namespace Ums.Presentation.Endpoints.Identity.Tenant.Queries;

using Ums.Application.Identity.Tenant.Branding.DTOs;
using Ums.Application.Identity.Tenant.Branding.Queries;

public static class BrandingQueryEndpoints
{
    public static IEndpointRouteBuilder MapBrandingQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenants/{tenantId:guid}/branding")
            .WithTags("Branding - Queries");

        group.MapGet("/", async (Guid tenantId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetBrandingByTenantIdQuery(tenantId), ct);
            return result.ToOk(context);
        })
        .WithName("GetBrandingByTenantId")
        .WithSummary("Get branding configuration for a tenant")
        .Produces<BrandingDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
