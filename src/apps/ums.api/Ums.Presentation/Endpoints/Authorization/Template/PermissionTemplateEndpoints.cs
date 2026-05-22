namespace Ums.Presentation.Endpoints.Authorization.Template;

using Ums.Application.Common;
using Ums.Application.Authorization.Template.Commands;
using Ums.Application.Authorization.Template.DTOs;
using Ums.Application.Authorization.Template.Queries;

public static class PermissionTemplateEndpoints
{
    public static IEndpointRouteBuilder MapPermissionTemplateEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/permission-templates")
            .WithTags("PermissionTemplates");

        group.MapGet("/", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search,
            [FromQuery] string? criteria,
            [FromQuery] string? status,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder,
            [FromQuery] Guid? tenantId,
            IMediator mediator,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllPermissionTemplatesQuery(
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 20 : pageSize,
                search,
                string.IsNullOrWhiteSpace(criteria) ? "version" : criteria,
                string.IsNullOrWhiteSpace(status) ? "all" : status,
                string.IsNullOrWhiteSpace(sortBy) ? "version" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder,
                tenantId), ct);
            return result.ToOk(context);
        })
        .WithName("GetAllPermissionTemplates")
        .WithSummary("Get permission templates using server-side pagination")
        .Produces<PagedResult<PermissionTemplateDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreatePermissionTemplateCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/permission-templates/{r.TemplateId}", context);
        })
        .WithName("CreatePermissionTemplate")
        .WithSummary("Create a new permission template")
        .Produces<CreatePermissionTemplateResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{templateId:guid}/publish", async (Guid templateId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new PublishPermissionTemplateCommand(templateId), ct);
            return result.ToNoContent(context);
        })
        .WithName("PublishPermissionTemplate")
        .WithSummary("Publish a draft permission template")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        // TODO(api-aggregate-tracker): Expose item lifecycle, effect overrides, activation toggles, and deprecate endpoint for PermissionTemplate.
        return app;
    }
}
