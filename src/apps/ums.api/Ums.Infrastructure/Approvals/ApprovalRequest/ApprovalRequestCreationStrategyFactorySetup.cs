namespace Ums.Infrastructure.Approvals.ApprovalRequest;

using Ums.Shell.Factory.Impl;

internal sealed class ApprovalRequestCreationStrategyFactorySetup : AbstractFactorySetupSource
{
    public ApprovalRequestCreationStrategyFactorySetup()
    {
        For<ApprovalRequestCreationStrategyCriteria, IApprovalRequestCreationStrategy>()
            .Create<ManualApprovalRequestCreationStrategy>()
            .When(criteria => criteria.RequiresApproval);

        For<ApprovalRequestCreationStrategyCriteria, IApprovalRequestCreationStrategy>()
            .Create<AutoApproveApprovalRequestCreationStrategy>()
            .When(criteria => !criteria.RequiresApproval);
    }
}
