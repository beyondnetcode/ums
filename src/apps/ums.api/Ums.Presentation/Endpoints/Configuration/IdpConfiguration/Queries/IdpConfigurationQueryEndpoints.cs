namespace Ums.Presentation.Endpoints.Configuration.IdpConfiguration.Queries;

using Ums.Application.Configuration.IdpConfiguration.DTOs;
using Ums.Application.Configuration.IdpConfiguration.Queries;

public static class IdpConfigurationQueryEndpoints
{
    public static IEndpointRouteBuilder MapIdpConfigurationQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/idp-configurations").WithTags("IdpConfigurations - Queries");

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetIdpConfigurationByIdQuery(id), ct);
            return result.ToOk(context);
        })
        .WithName("GetIdpConfigurationById")
        .Produces<IdpConfigurationDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
