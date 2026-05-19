namespace Ums.Domain.Approvals.ApprovalRequest;

public sealed class ApprovalRequest : AggregateRoot<ApprovalRequest, ApprovalRequestProps>
{
    private ApprovalRequest(ApprovalRequestProps props) : base(props)
    {
    }

    public ApprovalWorkflowId WorkflowId => Props.WorkflowId;
    public UserId TargetUserId => Props.TargetUserId;
    public ProfileId? TargetProfileId => Props.TargetProfileId;
    public ApprovalStatus Status => Props.Status;

    public ApprovalRequestId GetId() => ApprovalRequestId.Load(Props.Id.GetValue());

    public static Result<ApprovalRequest> Create(
        ApprovalWorkflowId workflowId,
        UserId targetUserId,
        ProfileId? targetProfileId,
        ActorId createdBy)
    {
        var props = new ApprovalRequestProps(IdValueObject.Create(), workflowId, targetUserId, targetProfileId, ApprovalStatus.Pending, createdBy);
        var request = new ApprovalRequest(props);

        if (!request.IsValid())
        {
            return Result<ApprovalRequest>.Failure(request.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<ApprovalRequest>.Success(request);
    }

    public Result Approve(ActorId approvedBy)
    {
        if (Status != ApprovalStatus.Pending)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Approvals.RequestNotPending));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = ApprovalStatus.Approved;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(approvedBy.GetValue());
        return Result.Success();
    }

    public Result Reject(ActorId rejectedBy)
    {
        if (Status != ApprovalStatus.Pending)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Approvals.RequestNotPending));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = ApprovalStatus.Rejected;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(rejectedBy.GetValue());
        return Result.Success();
    }
}
