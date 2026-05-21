namespace Ums.Presentation.Endpoints.Authorization.Profile;

using Ums.Application.Common;
using Ums.Application.Authorization.Profile.Commands;
using Ums.Application.Authorization.Profile.DTOs;
using Ums.Application.Authorization.Profile.Queries;

public static class ProfileEndpoints
{
    public static IEndpointRouteBuilder MapProfileEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/profiles")
            .WithTags("Profiles");

        group.MapGet("/", async (
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search,
            [FromQuery] string? criteria,
            [FromQuery] string? status,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder,
            [FromQuery] Guid? tenantId,
            [FromQuery] Guid? userId,
            IMediator mediator,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllProfilesQuery(
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 20 : pageSize,
                search,
                string.IsNullOrWhiteSpace(criteria) ? "userId" : criteria,
                string.IsNullOrWhiteSpace(status) ? "all" : status,
                string.IsNullOrWhiteSpace(sortBy) ? "userId" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder,
                tenantId,
                userId), ct);
            return result.ToOk(context);
        })
        .WithName("GetAllProfiles")
        .WithSummary("Get profiles using server-side pagination")
        .Produces<PagedResult<ProfileDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateProfileCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/profiles/{r.ProfileId}", context);
        })
        .WithName("CreateProfile")
        .WithSummary("Create a new profile")
        .Produces<CreateProfileResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{profileId:guid}/activate", async (Guid profileId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ActivateProfileCommand(profileId), ct);
            return result.ToNoContent(context);
        })
        .WithName("ActivateProfile")
        .WithSummary("Activate a deactivated profile")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{profileId:guid}/deactivate", async (Guid profileId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeactivateProfileCommand(profileId), ct);
            return result.ToNoContent(context);
        })
        .WithName("DeactivateProfile")
        .WithSummary("Deactivate an active profile")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
