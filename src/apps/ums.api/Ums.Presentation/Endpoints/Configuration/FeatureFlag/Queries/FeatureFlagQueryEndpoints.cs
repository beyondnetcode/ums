namespace Ums.Presentation.Endpoints.Configuration.FeatureFlag.Queries;

using Ums.Application.Configuration.FeatureFlag.DTOs;
using Ums.Application.Configuration.FeatureFlag.Queries;

public static class FeatureFlagQueryEndpoints
{
    public static IEndpointRouteBuilder MapFeatureFlagQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/feature-flags").WithTags("FeatureFlags - Queries");

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetFeatureFlagByIdQuery(id), ct);
            return result.ToOk(context);
        })
        .WithName("GetFeatureFlagById")
        .Produces<FeatureFlagDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
