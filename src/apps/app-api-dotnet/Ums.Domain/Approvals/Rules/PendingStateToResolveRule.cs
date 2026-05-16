namespace Ums.Domain.Approvals.Rules;

using Ums.Domain.Kernel;

public class PendingStateToResolveRule : AbstractRuleValidator<object>
{
    private readonly ApprovalRequestStatus _status;

    public PendingStateToResolveRule(ApprovalRequestStatus status) : base(new object())
    {
        _status = status;
    }

    public override void AddRules(RuleContext? context)
    {
        if (_status != ApprovalRequestStatus.Pending)
        {
            AddBrokenRule("Status", DomainErrors.Approval.OnlyPendingCanResolve);
        }
    }
}
