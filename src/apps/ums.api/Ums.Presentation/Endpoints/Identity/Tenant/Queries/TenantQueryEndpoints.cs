namespace Ums.Presentation.Endpoints.Identity.Tenant.Queries;

using Microsoft.EntityFrameworkCore;
using Ums.Application.Identity.Tenant.DTOs;
using Ums.Application.Identity.Tenant.Queries; // GetTenantByIdQuery
using Ums.Infrastructure.Persistence;
using Ums.Presentation.Extensions;

public static class TenantQueryEndpoints
{
    public static IEndpointRouteBuilder MapTenantQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/tenants")
            .WithTags("Tenants - Queries");

        // GetAllTenants lives in TenantEndpoints to share the same route group
        // and avoid Asp.Versioning GET-root shadowing on duplicate MapGroup("/tenants").

        // REC-10: ETag header set from SQL RowVersion for optimistic-locking support.
        group.MapGet("/{tenantId:guid}", async (
            Guid tenantId,
            IMediator mediator,
            IServiceProvider services,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetTenantByIdQuery(tenantId), ct);

            if (result.IsSuccess)
            {
                var dbContext = services.GetService<UmsPlatformDbContext>();
                if (dbContext != null)
                {
                    var rv = await dbContext.Tenants
                        .Where(x => x.Id == tenantId)
                        .Select(x => x.RowVersion)
                        .FirstOrDefaultAsync(ct);

                    var etag = ETagHelper.Encode(rv);
                    if (etag is not null)
                        context.Response.Headers.ETag = etag;
                }
            }

            return result.ToOk(context);
        })
        .WithName("GetTenantById")
        .WithSummary("Get tenant by ID")
        .Produces<TenantDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
