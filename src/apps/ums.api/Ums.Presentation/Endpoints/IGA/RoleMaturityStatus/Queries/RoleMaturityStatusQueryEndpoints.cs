namespace Ums.Presentation.Endpoints.IGA.RoleMaturityStatus.Queries;

using Ums.Application.IGA.RoleMaturityStatus.DTOs;
using Ums.Application.IGA.RoleMaturityStatus.Queries;

public static class RoleMaturityStatusQueryEndpoints
{
    public static IEndpointRouteBuilder MapRoleMaturityStatusQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/role-maturity-statuses").WithTags("RoleMaturityStatuses - Queries");

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetRoleMaturityStatusByIdQuery(id), ct);
            return result.ToOk(context);
        }).WithName("GetRoleMaturityStatusById").Produces<RoleMaturityStatusDto>(StatusCodes.Status200OK).ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
