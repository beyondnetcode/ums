namespace Ums.Domain.Approvals.ApprovalRequest;

using Ums.Domain.Approvals.ApprovalRequest.Events;

public sealed class ApprovalRequest : AggregateRoot<ApprovalRequest, ApprovalRequestProps>
{
    public new ApprovalRequestDomainEventsManager DomainEvents { get; }

    private ApprovalRequest(ApprovalRequestProps props) : base(props)
    {
        DomainEvents = new ApprovalRequestDomainEventsManager(this);
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
        DomainEvents.RaiseEvent(new ApprovalRequestApprovedEvent(
            Props.Id.GetValue(),
            Props.WorkflowId.GetValue(),
            approvedBy.GetValue(),
            DateTime.UtcNow));
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
        DomainEvents.RaiseEvent(new ApprovalRequestRejectedEvent(
            Props.Id.GetValue(),
            Props.WorkflowId.GetValue(),
            rejectedBy.GetValue(),
            string.Empty,
            DateTime.UtcNow));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(rejectedBy.GetValue());
        return Result.Success();
    }
}
