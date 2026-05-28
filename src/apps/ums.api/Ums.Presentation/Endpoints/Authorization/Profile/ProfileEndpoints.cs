namespace Ums.Presentation.Endpoints.Authorization.Profile;

using Ums.Application.Common;
using Ums.Application.Authorization.Profile.Commands;
using Ums.Application.Authorization.Profile.DTOs;
using Ums.Application.Authorization.Profile.Queries;
using Ums.Application.Authorization.Profile.Exporters;
using Ums.Shell.Factory.Interfaces;

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

        group.MapPost("/{profileId:guid}/templates/{templateId:guid}", async (Guid profileId, Guid templateId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new AssignProfileTemplateCommand(profileId, templateId), ct);
            return result.ToNoContent(context);
        })
        .WithName("AssignProfileTemplate")
        .WithSummary("Assign a published permission template to the profile")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{profileId:guid}/permissions/{permissionId:guid}/override", async (Guid profileId, Guid permissionId, [FromQuery] string effect, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new OverrideProfilePermissionCommand(profileId, permissionId, effect), ct);
            return result.ToNoContent(context);
        })
        .WithName("OverrideProfilePermission")
        .WithSummary("Override a profile permission effect")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{profileId:guid}/permissions/{permissionId:guid}/activate", async (Guid profileId, Guid permissionId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SetProfilePermissionStatusCommand(profileId, permissionId, true), ct);
            return result.ToNoContent(context);
        })
        .WithName("ActivateProfilePermission")
        .WithSummary("Activate a profile permission")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{profileId:guid}/permissions/{permissionId:guid}/deactivate", async (Guid profileId, Guid permissionId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SetProfilePermissionStatusCommand(profileId, permissionId, false), ct);
            return result.ToNoContent(context);
        })
        .WithName("DeactivateProfilePermission")
        .WithSummary("Deactivate a profile permission")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/{profileId:guid}/export", async (
            Guid profileId,
            [FromQuery] string format,
            IMediator mediator,
            IFactory factory,
            HttpContext context,
            CancellationToken ct) =>
        {
            var profileResult = await mediator.Send(new GetProfileByIdQuery(profileId), ct);
            if (profileResult.IsFailure)
            {
                return Results.BadRequest(profileResult.Error);
            }

            var criteria = new ProfileExportCriteria(format.ToUpperInvariant());
            var exporters = factory.Create<ProfileExportCriteria, IProfileExporter>(criteria);
            if (exporters.Length == 0)
            {
                return Results.BadRequest($"Export format '{format}' is not supported.");
            }

            var exporter = exporters[0];
            var exportResult = exporter.Export(profileResult.Value);

            var contentType = format.ToLowerInvariant() switch
            {
                "xml" => "application/xml",
                "csv" => "text/csv",
                _ => "application/json"
            };

            var fileName = $"profile_{profileId}.{format.ToLowerInvariant()}";
            return Results.File(System.Text.Encoding.UTF8.GetBytes(exportResult), contentType, fileName);
        })
        .WithName("ExportProfileGraph")
        .WithSummary("Export the materialized profile authorization graph in JSON, XML or CSV formats using our corporate factory.")
        .Produces(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
