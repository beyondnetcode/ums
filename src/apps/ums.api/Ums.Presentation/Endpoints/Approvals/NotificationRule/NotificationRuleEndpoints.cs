namespace Ums.Presentation.Endpoints.Approvals.NotificationRule;

using Ums.Application.Common;
using Ums.Application.Approvals.NotificationRule.Commands;
using Ums.Application.Approvals.NotificationRule.DTOs;
using Ums.Application.Approvals.NotificationRule.Queries;

public static class NotificationRuleEndpoints
{
    public static IEndpointRouteBuilder MapNotificationRuleEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/notification-rules").WithTags("NotificationRules");

        group.MapGet("/", async (
            [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? search,
            [FromQuery] string? criteria, [FromQuery] string? status, [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder, [FromQuery] Guid? tenantId,
            IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllNotificationRulesQuery(
                page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
                string.IsNullOrWhiteSpace(criteria) ? "channel" : criteria,
                string.IsNullOrWhiteSpace(status) ? "all" : status,
                string.IsNullOrWhiteSpace(sortBy) ? "channel" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, tenantId), ct);
            return result.ToOk(context);
        }).WithName("GetAllNotificationRules").Produces<PagedResult<NotificationRuleDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateNotificationRuleCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/notification-rules/{r.NotificationRuleId}", context);
        }).WithName("CreateNotificationRule").Produces<CreateNotificationRuleResponse>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/deactivate", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new DeactivateNotificationRuleCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("DeactivateNotificationRule").Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status404NotFound).ProducesProblem(StatusCodes.Status409Conflict);

        // TODO(api-aggregate-tracker): Expose update endpoint for NotificationRule recipient, schedule, and channel configuration.
        return app;
    }
}
