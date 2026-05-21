using Ums.Application.Approvals.ApprovalWorkflow.DTOs;
using Ums.Domain.Approvals;
using Ums.Domain.Approvals.ApprovalWorkflow;

namespace Ums.Application.Approvals.ApprovalWorkflow.Queries;

public sealed class GetApprovalWorkflowByIdQueryHandler : IQueryHandler<GetApprovalWorkflowByIdQuery, ApprovalWorkflowDto>
{
    private readonly IApprovalWorkflowRepository _repository;

    public GetApprovalWorkflowByIdQueryHandler(IApprovalWorkflowRepository repository) => _repository = repository;

    public async Task<Result<ApprovalWorkflowDto>> Handle(GetApprovalWorkflowByIdQuery request, CancellationToken cancellationToken)
    {
        var workflow = await _repository.GetByIdAsync(request.ApprovalWorkflowId, cancellationToken);
        if (workflow is null) return Result<ApprovalWorkflowDto>.Failure("Approval workflow not found.");

        return Result<ApprovalWorkflowDto>.Success(new ApprovalWorkflowDto(
            workflow.Props.Id.GetValue(), workflow.Props.TenantId.GetValue(), workflow.Props.SystemSuiteId?.GetValue(),
            workflow.Props.Code.GetValue(), workflow.Props.Name.GetValue(), workflow.Props.Description.GetValue(),
            workflow.Props.TargetUserCategory.ToString(), workflow.Props.RequiresApproval));
    }
}
