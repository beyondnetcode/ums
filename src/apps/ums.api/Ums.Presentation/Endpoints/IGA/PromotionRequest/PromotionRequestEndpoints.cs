namespace Ums.Presentation.Endpoints.IGA.PromotionRequest;

using Ums.Application.Common;
using Ums.Application.IGA.PromotionRequest.Commands;
using Ums.Application.IGA.PromotionRequest.DTOs;
using Ums.Application.IGA.PromotionRequest.Queries;

public static class PromotionRequestEndpoints
{
    public static IEndpointRouteBuilder MapPromotionRequestEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/promotion-requests").WithTags("PromotionRequests");

        group.MapGet("/", async (
            [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? search,
            [FromQuery] string? criteria, [FromQuery] string? status, [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder, [FromQuery] Guid? tenantId, [FromQuery] Guid? userId,
            IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new GetAllPromotionRequestsQuery(
                page <= 0 ? 1 : page, pageSize <= 0 ? 20 : pageSize, search,
                string.IsNullOrWhiteSpace(criteria) ? "status" : criteria,
                string.IsNullOrWhiteSpace(status) ? "all" : status,
                string.IsNullOrWhiteSpace(sortBy) ? "status" : sortBy,
                string.IsNullOrWhiteSpace(sortOrder) ? "asc" : sortOrder, tenantId, userId), ct);
            return result.ToOk(context);
        }).WithName("GetAllPromotionRequests")
          .WithSummary("Get promotion requests using server-side pagination")
          .Produces<PagedResult<PromotionRequestDto>>(StatusCodes.Status200OK);



        group.MapPost("/", async (CreatePromotionRequestCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => $"/promotion-requests/{r.PromotionRequestId}", context);
        }).WithName("CreatePromotionRequest")
          .WithSummary("Create a new promotion request (Draft)")
          .Produces<CreatePromotionRequestResponse>(StatusCodes.Status201Created)
          .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{id:guid}/submit", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SubmitPromotionRequestCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("SubmitPromotionRequest")
          .WithSummary("Submit a draft promotion request for manager review")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/manager-approve", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ManagerApprovePromotionRequestCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("ManagerApprovePromotionRequest")
          .WithSummary("Manager approves the promotion request — escalates to security review")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/manager-reject", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ManagerRejectPromotionRequestCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("ManagerRejectPromotionRequest")
          .WithSummary("Manager rejects the promotion request")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/security-review", async (Guid id, [FromQuery] bool isHighRisk, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SecurityReviewPromotionRequestCommand(id, isHighRisk), ct);
            return result.ToNoContent(context);
        }).WithName("SecurityReviewPromotionRequest")
          .WithSummary("Security team records risk assessment. Low-risk moves to ApprovedReadyToExecute; high-risk escalates to PendingSecurityApproval")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/security-approve", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SecurityApprovePromotionRequestCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("SecurityApprovePromotionRequest")
          .WithSummary("Security approves a high-risk promotion request — moves to ApprovedReadyToExecute")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/security-reject", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new SecurityRejectPromotionRequestCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("SecurityRejectPromotionRequest")
          .WithSummary("Security rejects the promotion request")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/execute", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new ExecutePromotionRequestCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("ExecutePromotionRequest")
          .WithSummary("Execute an approved promotion request — applies the role change")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/verify", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new VerifyPromotionRequestCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("VerifyPromotionRequest")
          .WithSummary("Verify that an executed promotion was applied correctly")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/verification-failed", async (Guid id, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(new MarkVerificationFailedCommand(id), ct);
            return result.ToNoContent(context);
        }).WithName("MarkVerificationFailed")
          .WithSummary("Mark that post-execution verification failed — requires remediation")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/impact-analysis", async (Guid id, AddImpactAnalysisCommand command, IMediator mediator, HttpContext context, CancellationToken ct) =>
        {
            var result = await mediator.Send(command with { PromotionRequestId = id }, ct);
            return result.ToNoContent(context);
        }).WithName("AddImpactAnalysis")
          .WithSummary("Attach a promotion impact analysis (risk score, permissions delta, mitigation plan)")
          .Produces(StatusCodes.Status204NoContent)
          .ProducesProblem(StatusCodes.Status400BadRequest)
          .ProducesProblem(StatusCodes.Status404NotFound)
          .ProducesProblem(StatusCodes.Status409Conflict);

        return app;
    }
}
