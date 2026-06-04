namespace Ums.Presentation.Endpoints.Identity.UserAccount.Queries;

using System.Collections.Generic;
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

        group.MapGet("/{userAccountId:guid}/mfa-enrollments", async (Guid userAccountId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetUserAccountMfaEnrollmentsQuery(userAccountId), ct);
            return result.ToOk(context);
        })
        .WithName("GetUserAccountMfaEnrollments")
        .WithSummary("Get all MFA enrollments for a user account")
        .Produces<IReadOnlyList<MfaEnrollmentDto>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
