namespace Ums.Domain.Approvals.Rules;

public class MinimumApprovalsRule : AbstractRuleValidator<object>
{
    private readonly int _requiredApprovals;

    public MinimumApprovalsRule(int requiredApprovals) : base(new object())
    {
        _requiredApprovals = requiredApprovals;
    }

    public override void AddRules(RuleContext? context)
    {
        if (_requiredApprovals <= 0)
        {
            AddBrokenRule("RequiredApprovals", "At least one approval is required.");
        }
    }
}
