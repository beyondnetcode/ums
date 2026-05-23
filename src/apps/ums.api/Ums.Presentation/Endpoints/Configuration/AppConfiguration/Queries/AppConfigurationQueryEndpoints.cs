namespace Ums.Presentation.Endpoints.Configuration.AppConfiguration.Queries;

using Ums.Application.Configuration.AppConfiguration.DTOs;
using Ums.Application.Configuration.AppConfiguration.Queries;

public static class AppConfigurationQueryEndpoints
{
    public static IEndpointRouteBuilder MapAppConfigurationQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/app-configurations").WithTags("AppConfigurations - Queries");

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAppConfigurationByIdQuery(id), ct);
            return result.ToOk(context);
        })
        .WithName("GetAppConfigurationById")
        .Produces<AppConfigurationDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
