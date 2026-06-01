using Ums.Application.Approvals.ApprovalRequest.DTOs;
using Ums.Domain.Approvals;
using Ums.Domain.Approvals.ApprovalRequest;

namespace Ums.Application.Approvals.ApprovalRequest.Queries;

public sealed class GetApprovalRequestByIdQueryHandler : IQueryHandler<GetApprovalRequestByIdQuery, ApprovalRequestDto>
{
    private readonly IApprovalRequestRepository _repository;

    public GetApprovalRequestByIdQueryHandler(IApprovalRequestRepository repository) => _repository = repository;

    [LoggerAspect(Type = typeof(IUmsLogger), LogDuration = true, LogException = true, LogArguments = [])]

    public async Task<Result<ApprovalRequestDto>> Handle(GetApprovalRequestByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await _repository.GetByIdAsync(request.ApprovalRequestId, cancellationToken);
        if (entity is null) return Result<ApprovalRequestDto>.Failure("Approval request not found.");

        return Result<ApprovalRequestDto>.Success(new ApprovalRequestDto(
            entity.Props.Id.GetValue(), entity.Props.WorkflowId.GetValue(), entity.Props.TargetUserId.GetValue(),
            entity.Props.TargetProfileId?.GetValue(), entity.Props.Status.ToString(),
            entity.RequestedSystemId.GetValue(), entity.RequestedBranchId?.GetValue(), entity.RequestedRoleId.GetValue(),
            entity.Justification, entity.GrantedRoleId?.GetValue(), entity.DecisionReason));
    }
}
