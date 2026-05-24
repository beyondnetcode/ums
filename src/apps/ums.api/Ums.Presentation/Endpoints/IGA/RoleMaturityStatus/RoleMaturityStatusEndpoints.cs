namespace Ums.Presentation.Endpoints.IGA.RoleMaturityStatus;

using Ums.Application.Common;
using Ums.Application.IGA.RoleMaturityStatus.Commands;
using Ums.Application.IGA.RoleMaturityStatus.DTOs;
using Ums.Application.IGA.RoleMaturityStatus.Queries;

public static class RoleMaturityStatusEndpoints
{
    public static IEndpointRouteBuilder MapRoleMaturityStatusEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/role-maturity-statuses").WithTags("RoleMaturityStatuses");

        group.MapGet("/", async (
            [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? search,
            [FromQuery] string? criteria, [FromQuery] string? sortBy, [FromQuery] string? sortOrder,
            [FromQuery] Guid? tenantId, [FromQuery] Guid? userId,
            IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllRoleMaturityStatusesQuery(
                page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
                string.IsNullOrWhiteSpace(criteria) ? "currentMaturityLevel" : criteria,
                string.IsNullOrWhiteSpace(sortBy) ? "currentMaturityLevel" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, tenantId, userId), ct);
            return result.ToOk(context);
        }).WithName("GetAllRoleMaturityStatuses").Produces<PagedResult<RoleMaturityStatusDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateRoleMaturityStatusCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/role-maturity-statuses/{r.RoleMaturityStatusId}", context);
        }).WithName("CreateRoleMaturityStatus").Produces<CreateRoleMaturityStatusResponse>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{id:guid}/level", async (Guid id, UpdateRoleMaturityLevelCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { RoleMaturityStatusId = id }, ct);
            return result.ToNoContent(context);
        }).WithName("UpdateRoleMaturityLevel")
          .WithSummary("Update the maturity level of a role assignment")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/certifications", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RecordCertificationCompletionCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("RecordCertificationCompletion")
          .WithSummary("Record a completed certification — increments the certification counter")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/trainings", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new RecordTrainingCompletionCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("RecordTrainingCompletion")
          .WithSummary("Record a completed training — increments the training counter")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{id:guid}/performance-score", async (Guid id, UpdatePerformanceScoreCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { RoleMaturityStatusId = id }, ct);
            return result.ToNoContent(context);
        }).WithName("UpdatePerformanceScore")
          .WithSummary("Update the performance score (0.0 – 5.0) for eligibility evaluation")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status400BadRequest)
          .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/compliance-issue", async (Guid id, MarkComplianceIssueCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { RoleMaturityStatusId = id }, ct);
            return result.ToNoContent(context);
        }).WithName("MarkComplianceIssue")
          .WithSummary("Flag a compliance issue — blocks promotion eligibility until resolved")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status400BadRequest)
          .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapDelete("/{id:guid}/compliance-issue", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ResolveComplianceIssueCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("ResolveComplianceIssue")
          .WithSummary("Clear a compliance issue — re-enables promotion eligibility evaluation")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{id:guid}/eligibility-review", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ReviewEligibilityCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("ReviewEligibility")
          .WithSummary("Trigger an eligibility review — sets EligibleForPromotionAt if criteria are met")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound);

        return app;
    }
}
