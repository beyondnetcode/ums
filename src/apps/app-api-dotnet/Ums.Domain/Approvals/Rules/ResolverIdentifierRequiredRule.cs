namespace Ums.Domain.Approvals.Rules;

using Ums.Domain.Kernel;

public class ResolverIdentifierRequiredRule : AbstractRuleValidator<object>
{
    private readonly Guid _resolvedBy;

    public ResolverIdentifierRequiredRule(Guid resolvedBy) : base(new object())
    {
        _resolvedBy = resolvedBy;
    }

    public override void AddRules(RuleContext? context)
    {
        if (_resolvedBy == Guid.Empty)
        {
            AddBrokenRule("ResolvedBy", DomainErrors.Approval.ResolverIdRequired);
        }
    }
}
