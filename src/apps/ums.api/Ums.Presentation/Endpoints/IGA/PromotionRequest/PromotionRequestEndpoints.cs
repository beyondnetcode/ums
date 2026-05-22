namespace Ums.Presentation.Endpoints.IGA.PromotionRequest;

using Ums.Application.Common;
using Ums.Application.IGA.PromotionRequest.Commands;
using Ums.Application.IGA.PromotionRequest.DTOs;
using Ums.Application.IGA.PromotionRequest.Queries;

public static class PromotionRequestEndpoints
{
    public static IEndpointRouteBuilder MapPromotionRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/promotion-requests").WithTags("PromotionRequests");

        group.MapGet("/", async (
            [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? search,
            [FromQuery] string? criteria, [FromQuery] string? status, [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder, [FromQuery] Guid? tenantId, [FromQuery] Guid? userId,
            IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllPromotionRequestsQuery(
                page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
                string.IsNullOrWhiteSpace(criteria) ? "status" : criteria,
                string.IsNullOrWhiteSpace(status) ? "all" : status,
                string.IsNullOrWhiteSpace(sortBy) ? "status" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, tenantId, userId), ct);
            return result.ToOk(context);
        }).WithName("GetAllPromotionRequests").Produces<PagedResult<PromotionRequestDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreatePromotionRequestCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/promotion-requests/{r.PromotionRequestId}", context);
        }).WithName("CreatePromotionRequest").Produces<CreatePromotionRequestResponse>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/submit", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SubmitPromotionRequestCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("SubmitPromotionRequest").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);

        // TODO(api-aggregate-tracker): Expose manager review, security review, approval, execution, verification, and impact-analysis actions for PromotionRequest.
        return app;
    }
}
