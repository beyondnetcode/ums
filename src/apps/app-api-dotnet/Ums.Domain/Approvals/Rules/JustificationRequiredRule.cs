namespace Ums.Domain.Approvals.Rules;

public class JustificationRequiredRule : AbstractRuleValidator<object>
{
    private readonly string _justification;

    public JustificationRequiredRule(string justification) : base(new object())
    {
        _justification = justification;
    }

    public override void AddRules(RuleContext? context)
    {
        if (string.IsNullOrWhiteSpace(_justification))
        {
            AddBrokenRule("Justification", "Justification is required.");
        }
    }
}
