namespace Ums.Domain.Approvals.ApprovalRequest;

public class ApprovalRequestProps : IProps
{
    public IdValueObject Id { get; set; }
    public ApprovalWorkflowId WorkflowId { get; set; }
    public UserId TargetUserId { get; set; }
    public ProfileId? TargetProfileId { get; set; }
    public ApprovalStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public ApprovalRequestProps(
        IdValueObject id,
        ApprovalWorkflowId workflowId,
        UserId targetUserId,
        ProfileId? targetProfileId,
        ApprovalStatus status,
        ActorId createdBy)
    {
        Id = id;
        WorkflowId = workflowId;
        TargetUserId = targetUserId;
        TargetProfileId = targetProfileId;
        Status = status;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
