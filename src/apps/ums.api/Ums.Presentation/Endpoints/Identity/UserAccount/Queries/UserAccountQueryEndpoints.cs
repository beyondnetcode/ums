namespace Ums.Presentation.Endpoints.Identity.UserAccount.Queries;

using Ums.Application.Identity.UserAccount.DTOs;
using Ums.Application.Identity.UserAccount.Queries;

public static class UserAccountQueryEndpoints
{
    public static IEndpointRouteBuilder MapUserAccountQueryEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user-accounts")
            .WithTags("UserAccounts - Queries");

        group.MapGet("/{userAccountId:guid}", async (Guid userAccountId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetUserAccountByIdQuery(userAccountId), ct);
            return result.ToOk(context);
        })
        .WithName("GetUserAccountById")
        .WithSummary("Get user account by ID")
        .Produces<UserAccountDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
