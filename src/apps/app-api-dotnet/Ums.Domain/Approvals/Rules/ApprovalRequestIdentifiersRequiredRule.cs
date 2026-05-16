namespace Ums.Domain.Approvals.Rules;

using Ums.Domain.Kernel;

public class ApprovalRequestIdentifiersRequiredRule : AbstractRuleValidator<object>
{
    private readonly Guid _tenantId;
    private readonly Guid _workflowId;
    private readonly Guid _requestedBy;

    public ApprovalRequestIdentifiersRequiredRule(Guid tenantId, Guid workflowId, Guid requestedBy) : base(new object())
    {
        _tenantId = tenantId;
        _workflowId = workflowId;
        _requestedBy = requestedBy;
    }

    public override void AddRules(RuleContext? context)
    {
        if (_tenantId == Guid.Empty || _workflowId == Guid.Empty || _requestedBy == Guid.Empty)
        {
            AddBrokenRule("Identifiers", DomainErrors.Approval.IdentifiersRequired);
        }
    }
}
