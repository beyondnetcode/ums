namespace Ums.Presentation.Endpoints.Identity.UserAccount;

using Ums.Application.Common;
using Ums.Application.Identity.UserAccount.Commands;
using Ums.Application.Identity.UserAccount.DTOs;
using Ums.Application.Identity.UserAccount.Queries;

public static class UserAccountEndpoints
{
    public static IEndpointRouteBuilder MapUserAccountEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/user-accounts")
            .WithTags("UserAccounts");

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
            var result = await mediator.Send(new GetAllUserAccountsQuery(
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 20 : pageSize,
                search,
                string.IsNullOrWhiteSpace(criteria) ? "email" : criteria,
                string.IsNullOrWhiteSpace(status) ? "all" : status,
                string.IsNullOrWhiteSpace(sortBy) ? "email" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder,
                tenantId), ct);
            return result.ToOk(context);
        })
        .WithName("GetAllUserAccounts")
        .WithSummary("Get user accounts using server-side pagination")
        .Produces<PagedResult<UserAccountDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateUserAccountCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/user-accounts/{r.UserAccountId}", context);
        })
        .WithName("CreateUserAccount")
        .WithSummary("Create a new user account")
        .Produces<CreateUserAccountResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{userAccountId:guid}/activate", async (Guid userAccountId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ActivateUserAccountCommand(userAccountId), ct);
            return result.ToNoContent(context);
        })
        .WithName("ActivateUserAccount")
        .WithSummary("Activate a pending user account")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{userAccountId:guid}/block", async (Guid userAccountId, string reason, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new BlockUserAccountCommand(userAccountId, reason), ct);
            return result.ToNoContent(context);
        })
        .WithName("BlockUserAccount")
        .WithSummary("Block an active user account")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{userAccountId:guid}/restore", async (Guid userAccountId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RestoreUserAccountCommand(userAccountId), ct);
            return result.ToNoContent(context);
        })
        .WithName("RestoreUserAccount")
        .WithSummary("Restore a blocked user account")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
