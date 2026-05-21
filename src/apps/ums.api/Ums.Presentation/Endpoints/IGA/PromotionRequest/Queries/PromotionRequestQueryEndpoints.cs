namespace Ums.Presentation.Endpoints.IGA.PromotionRequest.Queries;

using Ums.Application.IGA.PromotionRequest.DTOs;
using Ums.Application.IGA.PromotionRequest.Queries;

public static class PromotionRequestQueryEndpoints
{
    public static IEndpointRouteBuilder MapPromotionRequestQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/promotion-requests").WithTags("PromotionRequests - Queries");

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetPromotionRequestByIdQuery(id), ct);
            return result.ToOk(context);
        }).WithName("GetPromotionRequestById").Produces<PromotionRequestDto>(StatusCodes.Status200OK).ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
