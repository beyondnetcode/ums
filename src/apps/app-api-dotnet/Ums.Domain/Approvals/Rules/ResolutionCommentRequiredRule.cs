namespace Ums.Domain.Approvals.Rules;

using Ums.Domain.Kernel;

public class ResolutionCommentRequiredRule : AbstractRuleValidator<object>
{
    private readonly string _comment;

    public ResolutionCommentRequiredRule(string comment) : base(new object())
    {
        _comment = comment;
    }

    public override void AddRules(RuleContext? context)
    {
        if (string.IsNullOrWhiteSpace(_comment))
        {
            AddBrokenRule("ResolutionComment", DomainErrors.Approval.ResolutionCommentRequired);
        }
    }
}
