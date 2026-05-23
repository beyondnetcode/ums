namespace Ums.Presentation.Endpoints.Configuration.FeatureFlag;

using Ums.Application.Common;
using Ums.Application.Configuration.FeatureFlag.Commands;
using Ums.Application.Configuration.FeatureFlag.DTOs;
using Ums.Application.Configuration.FeatureFlag.Queries;

public static class FeatureFlagEndpoints
{
    public static IEndpointRouteBuilder MapFeatureFlagEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/feature-flags").WithTags("FeatureFlags");

        group.MapGet("/", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search,
            [FromQuery] string? criteria,
            [FromQuery] string? status,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder,
            [FromQuery] string? flagType,
            [FromQuery] string? linkedResourceType,
            IMediator mediator,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllFeatureFlagsQuery(
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 20 : pageSize,
                search,
                string.IsNullOrWhiteSpace(criteria) ? "flagCode" : criteria,
                string.IsNullOrWhiteSpace(status) ? "all" : status,
                string.IsNullOrWhiteSpace(sortBy) ? "flagCode" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder,
                flagType,
                linkedResourceType), ct);
            return result.ToOk(context);
        })
        .WithName("GetAllFeatureFlags")
        .Produces<PagedResult<FeatureFlagDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateFeatureFlagCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/feature-flags/{r.FeatureFlagId}", context);
        })
        .WithName("CreateFeatureFlag")
        .Produces<CreateFeatureFlagResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{featureFlagId:guid}/activate", async (Guid featureFlagId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ActivateFeatureFlagCommand(featureFlagId), ct);
            return result.ToNoContent(context);
        })
        .WithName("ActivateFeatureFlag")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{featureFlagId:guid}/deactivate", async (Guid featureFlagId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeactivateFeatureFlagCommand(featureFlagId), ct);
            return result.ToNoContent(context);
        })
        .WithName("DeactivateFeatureFlag")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{featureFlagId:guid}/archive", async (Guid featureFlagId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ArchiveFeatureFlagCommand(featureFlagId), ct);
            return result.ToNoContent(context);
        })
        .WithName("ArchiveFeatureFlag")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{featureFlagId:guid}/evaluate", async (Guid featureFlagId, EvaluateFeatureFlagRequest request, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new EvaluateFeatureFlagCommand(featureFlagId, request.Context), ct);
            return result.ToOk(context);
        })
        .WithName("EvaluateFeatureFlag")
        .Produces<EvaluateFeatureFlagResponse>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // TODO(api-aggregate-tracker): Expose update endpoint for FeatureFlag targeting rules, linked resource bindings, and rollout strategy changes.
        return app;
    }

    public sealed record EvaluateFeatureFlagRequest(string Context);
}
