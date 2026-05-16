namespace Ums.Domain.Approvals.Rules;

using Ums.Domain.Kernel;

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
            AddBrokenRule("ApprovalRequestId", DomainErrors.Approval.RequestIdRequired);
        }
    }
}
