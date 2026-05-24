namespace Ums.Application.Approvals.ApprovalRequest.Services;

using ApprovalWorkflowAggregate = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalWorkflow;
using ApprovalRequestAggregate = Ums.Domain.Approvals.ApprovalRequest.ApprovalRequest;

public interface IApprovalRequestCreationPolicyResolver
{
    Result<ApprovalRequestAggregate> Create(
        ApprovalWorkflowAggregate workflow,
        UserId targetUserId,
        ProfileId? targetProfileId,
        ActorId actorId);
}
