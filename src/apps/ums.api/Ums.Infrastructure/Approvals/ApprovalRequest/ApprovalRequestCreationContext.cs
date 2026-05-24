namespace Ums.Infrastructure.Approvals.ApprovalRequest;

using ApprovalWorkflowAggregate = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalWorkflow;

internal sealed record ApprovalRequestCreationContext(
    ApprovalWorkflowAggregate Workflow,
    UserId TargetUserId,
    ProfileId? TargetProfileId,
    ActorId ActorId);
