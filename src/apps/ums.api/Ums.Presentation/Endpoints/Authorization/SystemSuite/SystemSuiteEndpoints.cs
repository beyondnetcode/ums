namespace Ums.Presentation.Endpoints.Authorization.SystemSuite;

using Ums.Application.Common;
using Ums.Application.Authorization.SystemSuite.Commands;
using Ums.Application.Authorization.SystemSuite.DTOs;
using Ums.Application.Authorization.SystemSuite.Queries;

public static class SystemSuiteEndpoints
{
    public static IEndpointRouteBuilder MapSystemSuiteEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/system-suites")
            .WithTags("SystemSuites");

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
            var result = await mediator.Send(new GetAllSystemSuitesQuery(
                page <= 0 ? 1 : page,
                pageSize <= 0 ? 20 : pageSize,
                search,
                string.IsNullOrWhiteSpace(criteria) ? "name" : criteria,
                string.IsNullOrWhiteSpace(status) ? "all" : status,
                string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder,
                tenantId), ct);
            return result.ToOk(context);
        })
        .WithName("GetAllSystemSuites")
        .WithSummary("Get system suites using server-side pagination")
        .Produces<PagedResult<SystemSuiteDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateSystemSuiteCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/system-suites/{r.SystemSuiteId}", context);
        })
        .WithName("CreateSystemSuite")
        .WithSummary("Create a new system suite")
        .Produces<CreateSystemSuiteResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{systemSuiteId:guid}", async (Guid systemSuiteId, UpdateSystemSuiteCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { SystemSuiteId = systemSuiteId }, ct);
            return result.ToNoContent(context);
        })
        .WithName("UpdateSystemSuite")
        .WithSummary("Update a system suite")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{systemSuiteId:guid}/status", async (Guid systemSuiteId, string status, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SetSystemSuiteStatusCommand(systemSuiteId, status), ct);
            return result.ToNoContent(context);
        })
        .WithName("SetSystemSuiteStatus")
        .WithSummary("Set system suite status")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        // TODO(api-aggregate-tracker): Expose module, menu, option, action, and app-setting lifecycle endpoints for SystemSuite.
        return app;
    }
}
