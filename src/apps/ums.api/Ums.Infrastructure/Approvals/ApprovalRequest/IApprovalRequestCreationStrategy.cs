namespace Ums.Infrastructure.Approvals.ApprovalRequest;

using ApprovalRequestAggregate = Ums.Domain.Approvals.ApprovalRequest.ApprovalRequest;

internal interface IApprovalRequestCreationStrategy
{
    Result<ApprovalRequestAggregate> Create(ApprovalRequestCreationContext context);
}
