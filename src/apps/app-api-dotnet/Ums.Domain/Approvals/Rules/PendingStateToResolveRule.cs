namespace Ums.Domain.Approvals.Rules;

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
            AddBrokenRule("Status", "Only pending approval requests can be resolved.");
        }
    }
}
