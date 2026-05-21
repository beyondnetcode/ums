namespace Ums.Presentation.Endpoints.Approvals.UserDocument;

using Ums.Application.Common;
using Ums.Application.Approvals.UserDocument.Commands;
using Ums.Application.Approvals.UserDocument.DTOs;
using Ums.Application.Approvals.UserDocument.Queries;

public static class UserDocumentEndpoints
{
    public static IEndpointRouteBuilder MapUserDocumentEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user-documents").WithTags("UserDocuments");

        group.MapGet("/", async (
            [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? search,
            [FromQuery] string? criteria, [FromQuery] string? status, [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder, [FromQuery] Guid? userId, [FromQuery] Guid? tenantId,
            IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllUserDocumentsQuery(
                page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
                string.IsNullOrWhiteSpace(criteria) ? "status" : criteria,
                string.IsNullOrWhiteSpace(status) ? "all" : status,
                string.IsNullOrWhiteSpace(sortBy) ? "status" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, userId, tenantId), ct);
            return result.ToOk(context);
        }).WithName("GetAllUserDocuments").Produces<PagedResult<UserDocumentDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (UploadUserDocumentCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/user-documents/{r.UserDocumentId}", context);
        }).WithName("UploadUserDocument").Produces<UploadUserDocumentResponse>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/validate", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ValidateUserDocumentCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("ValidateUserDocument").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
