namespace Ums.Infrastructure.Approvals.ApprovalRequest;

using Ums.Application.Approvals.ApprovalRequest.Services;
using Ums.Shell.Factory.Interfaces;
using ApprovalWorkflowAggregate = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalWorkflow;
using ApprovalRequestAggregate = Ums.Domain.Approvals.ApprovalRequest.ApprovalRequest;

internal sealed class ApprovalRequestCreationPolicyResolver : IApprovalRequestCreationPolicyResolver
{
    private readonly IFactory _factory;

    public ApprovalRequestCreationPolicyResolver(IFactory factory)
    {
        _factory = factory;
    }

    public Result<ApprovalRequestAggregate> Create(
        ApprovalWorkflowAggregate workflow,
        UserId targetUserId,
        ProfileId? targetProfileId,
        ActorId actorId)
    {
        var strategy = _factory.Create<ApprovalRequestCreationStrategyCriteria, IApprovalRequestCreationStrategy>(
                new ApprovalRequestCreationStrategyCriteria(workflow.RequiresApproval))
            .SingleOrDefault();

        if (strategy is null)
        {
            return Result<ApprovalRequestAggregate>.Failure(
                $"No approval request creation strategy is registered for workflow '{workflow.Code.GetValue()}'.");
        }

        return strategy.Create(new ApprovalRequestCreationContext(workflow, targetUserId, targetProfileId, actorId));
    }
}
