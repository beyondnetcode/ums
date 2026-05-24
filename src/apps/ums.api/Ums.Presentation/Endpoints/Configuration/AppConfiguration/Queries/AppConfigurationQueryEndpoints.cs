namespace Ums.Presentation.Endpoints.Configuration.AppConfiguration.Queries;

using Microsoft.EntityFrameworkCore;
using Ums.Application.Configuration.AppConfiguration.DTOs;
using Ums.Application.Configuration.AppConfiguration.Queries;
using Ums.Infrastructure.Persistence;
using Ums.Presentation.Extensions;

public static class AppConfigurationQueryEndpoints
{
    public static IEndpointRouteBuilder MapAppConfigurationQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/app-configurations").WithTags("AppConfigurations - Queries");

        // REC-10: GET-by-ID sets ETag header from SQL RowVersion for optimistic-locking support.
        // Clients should send the ETag value as an If-Match header on subsequent PUT requests.
        group.MapGet("/{id:guid}", async (
            Guid id,
            [FromServices] IMediator mediator,
            [FromServices] IServiceProvider services,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAppConfigurationByIdQuery(id), ct);

            var dbContext = services.GetService<UmsPlatformDbContext>();

            if (result.IsSuccess && dbContext is not null)
            {
                // Attach ETag from RowVersion — read-side query straight from DB context
                var rv = await dbContext.AppConfigurations
                    .Where(x => x.Id == id)
                    .Select(x => x.RowVersion)
                    .FirstOrDefaultAsync(ct);

                var etag = ETagHelper.Encode(rv);
                if (etag is not null)
                    context.Response.Headers.ETag = etag;
            }

            return result.ToOk(context);
        })
        .WithName("GetAppConfigurationById")
        .Produces<AppConfigurationDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
