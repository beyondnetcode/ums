namespace Ums.Domain.Approvals.Rules;

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
            AddBrokenRule("ResolutionComment", "Resolution comment is required.");
        }
    }
}
