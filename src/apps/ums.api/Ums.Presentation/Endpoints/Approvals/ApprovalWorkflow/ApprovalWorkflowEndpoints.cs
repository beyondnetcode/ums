namespace Ums.Presentation.Endpoints.Approvals.ApprovalWorkflow;

using Ums.Application.Common;
using Ums.Application.Approvals.ApprovalWorkflow.Commands;
using Ums.Application.Approvals.ApprovalWorkflow.DTOs;
using Ums.Application.Approvals.ApprovalWorkflow.Queries;

public static class ApprovalWorkflowEndpoints
{
    public static IEndpointRouteBuilder MapApprovalWorkflowEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/approval-workflows").WithTags("ApprovalWorkflows");

        group.MapGet("/", async (
            [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? search,
            [FromQuery] string? criteria, [FromQuery] string? sortBy, [FromQuery] string? sortOrder,
            [FromQuery] Guid? tenantId, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllApprovalWorkflowsQuery(
                page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
                string.IsNullOrWhiteSpace(criteria) ? "name" : criteria,
                string.IsNullOrWhiteSpace(sortBy) ? "name" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, tenantId), ct);
            return result.ToOk(context);
        }).WithName("GetAllApprovalWorkflows").Produces<PagedResult<ApprovalWorkflowDto>>(StatusCodes.Status200OK);

        group.MapPost("/", async (CreateApprovalWorkflowCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/approval-workflows/{r.ApprovalWorkflowId}", context);
        }).WithName("CreateApprovalWorkflow").Produces<CreateApprovalWorkflowResponse>(StatusCodes.Status201Created).ProducesProblem(StatusCodes.Status400BadRequest);

        // TODO(api-aggregate-tracker): Expose required-document and lifecycle management endpoints for ApprovalWorkflow.
        return app;
    }
}
