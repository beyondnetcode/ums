namespace Ums.Presentation.Endpoints.Authorization.Template.Queries;

using Ums.Application.Authorization.Template.DTOs;
using Ums.Application.Authorization.Template.Queries;

public static class PermissionTemplateQueryEndpoints
{
    public static IEndpointRouteBuilder MapPermissionTemplateQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/permission-templates")
            .WithTags("PermissionTemplates - Queries");

        group.MapGet("/{templateId:guid}", async (Guid templateId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetPermissionTemplateByIdQuery(templateId), ct);
            return result.ToOk(context);
        })
        .WithName("GetPermissionTemplateById")
        .WithSummary("Get permission template by ID")
        .Produces<PermissionTemplateDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
