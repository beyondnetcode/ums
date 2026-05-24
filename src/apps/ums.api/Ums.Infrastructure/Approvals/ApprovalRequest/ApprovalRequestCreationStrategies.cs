namespace Ums.Infrastructure.Approvals.ApprovalRequest;

using ApprovalRequestAggregate = Ums.Domain.Approvals.ApprovalRequest.ApprovalRequest;

internal sealed class ManualApprovalRequestCreationStrategy : IApprovalRequestCreationStrategy
{
    public Result<ApprovalRequestAggregate> Create(ApprovalRequestCreationContext context)
    {
        return ApprovalRequestAggregate.Create(
            context.Workflow.GetId(),
            context.TargetUserId,
            context.TargetProfileId,
            context.ActorId);
    }
}

internal sealed class AutoApproveApprovalRequestCreationStrategy : IApprovalRequestCreationStrategy
{
    public Result<ApprovalRequestAggregate> Create(ApprovalRequestCreationContext context)
    {
        var created = ApprovalRequestAggregate.Create(
            context.Workflow.GetId(),
            context.TargetUserId,
            context.TargetProfileId,
            context.ActorId);

        if (created.IsFailure)
        {
            return created;
        }

        var approval = created.Value.Approve(context.ActorId);
        if (approval.IsFailure)
        {
            return Result<ApprovalRequestAggregate>.Failure(approval.Error);
        }

        return created;
    }
}
