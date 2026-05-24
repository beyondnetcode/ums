namespace Ums.Presentation.Endpoints.Configuration.AppConfiguration;

using Ums.Application.Common;
using Ums.Application.Configuration.AppConfiguration.Commands;
using Ums.Application.Configuration.AppConfiguration.DTOs;
using Ums.Application.Configuration.AppConfiguration.Queries;
using Ums.Presentation.Extensions;

public static class AppConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapAppConfigurationEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/app-configurations").WithTags("AppConfigurations");

        group.MapGet("/", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search,
            [FromQuery] string? criteria,
            [FromQuery] string? status,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder,
            [FromQuery] string? scope,
            [FromQuery] Guid? tenantId,
            [FromQuery] Guid? systemSuiteId,
            [FromQuery] Guid? moduleId,
            IMediator mediator,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllAppConfigurationsQuery(
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 20 : pageSize,
                search,
                string.IsNullOrWhiteSpace(criteria) ? "code" : criteria,
                string.IsNullOrWhiteSpace(status) ? "all" : status,
                string.IsNullOrWhiteSpace(sortBy) ? "code" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder,
                scope,
                tenantId,
                systemSuiteId,
                moduleId), ct);
            return result.ToOk(context);
        })
        .WithName("GetAllAppConfigurations")
        .Produces<PagedResult<AppConfigurationDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateAppConfigurationCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/app-configurations/{r.AppConfigurationId}", context);
        })
        .WithName("CreateAppConfiguration")
        .Produces<CreateAppConfigurationResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // REC-10: Reads If-Match header for optimistic concurrency.
        // Clients should include the ETag received from the GET-by-ID response.
        // Omitting If-Match still works — EF Core tracks the version loaded during UpdateAsync.
        group.MapPut("/{appConfigurationId:guid}", async (
            Guid appConfigurationId,
            UpdateAppConfigurationCommand command,
            IMediator mediator,
            HttpContext context,
            CancellationToken ct) =>
        {
            var rowVersion = ETagHelper.DecodeIfMatch(context.Request.Headers.IfMatch);
            var result = await mediator.Send(
                command with { AppConfigurationId = appConfigurationId, RowVersion = rowVersion }, ct);
            return result.ToNoContent(context);
        })
        .WithName("UpdateAppConfiguration")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{appConfigurationId:guid}/publish", async (Guid appConfigurationId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new PublishAppConfigurationCommand(appConfigurationId), ct);
            return result.ToNoContent(context);
        })
        .WithName("PublishAppConfiguration")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{appConfigurationId:guid}/archive", async (Guid appConfigurationId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ArchiveAppConfigurationCommand(appConfigurationId), ct);
            return result.ToNoContent(context);
        })
        .WithName("ArchiveAppConfiguration")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // DEFERRED: Rollback and version-comparison endpoints require a persisted
        // configuration history (audit table) — not yet implemented in the storage layer.
        return app;
    }
}
