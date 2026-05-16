namespace Ums.Domain.Approvals.Rules;

using Ums.Domain.Kernel;

public class DraftStateToSubmitRule : AbstractRuleValidator<object>
{
    private readonly ApprovalRequestStatus _status;

    public DraftStateToSubmitRule(ApprovalRequestStatus status) : base(new object())
    {
        _status = status;
    }

    public override void AddRules(RuleContext? context)
    {
        if (_status != ApprovalRequestStatus.Draft)
        {
            AddBrokenRule("Status", DomainErrors.Approval.OnlyDraftCanSubmit);
        }
    }
}
