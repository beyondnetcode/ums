namespace Ums.Domain.Approvals.Rules;

public class ApprovalRequestIdentifierRequiredRule : AbstractRuleValidator<object>
{
    private readonly Guid _approvalRequestId;

    public ApprovalRequestIdentifierRequiredRule(Guid approvalRequestId) : base(new object())
    {
        _approvalRequestId = approvalRequestId;
    }

    public override void AddRules(RuleContext? context)
    {
        if (_approvalRequestId == Guid.Empty)
        {
            AddBrokenRule("ApprovalRequestId", "Approval request identifier is required.");
        }
    }
}
