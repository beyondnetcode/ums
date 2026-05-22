namespace Ums.Presentation.Endpoints.Approvals.DocumentType;

using Ums.Application.Common;
using Ums.Application.Approvals.DocumentType.Commands;
using Ums.Application.Approvals.DocumentType.DTOs;
using Ums.Application.Approvals.DocumentType.Queries;

public static class DocumentTypeEndpoints
{
    public static IEndpointRouteBuilder MapDocumentTypeEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/document-types").WithTags("DocumentTypes");

        group.MapGet("/", async (
            [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? search,
            [FromQuery] string? criteria, [FromQuery] string? sortBy, [FromQuery] string? sortOrder,
            [FromQuery] Guid? tenantId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllDocumentTypesQuery(
                page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
                string.IsNullOrWhiteSpace(criteria) ? "name" : criteria,
                string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, tenantId), ct);
            return result.ToOk(context);
        }).WithName("GetAllDocumentTypes").Produces<PagedResult<DocumentTypeDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateDocumentTypeCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/document-types/{r.DocumentTypeId}", context);
        }).WithName("CreateDocumentType").Produces<CreateDocumentTypeResponse>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest);

        // TODO(api-aggregate-tracker): Expose update, notification-rule management, and enforcement-policy management endpoints for DocumentType.
        return app;
    }
}
