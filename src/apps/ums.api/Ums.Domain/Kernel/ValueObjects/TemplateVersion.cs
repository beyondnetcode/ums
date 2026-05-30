namespace Ums.Domain.Kernel.ValueObjects;

public class TemplateVersion : StringValueObject
{
    private TemplateVersion(string value) : base(value) { }

    public static TemplateVersion Create(int major, int minor, int patch)
    {
        return new TemplateVersion($"{major}.{minor}.{patch}");
    }

    public static TemplateVersion Initial() => new TemplateVersion("0.1.0");

    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new TemplateVersionValidator(this));
    }
}

public partial class TemplateVersionValidator : AbstractRuleValidator<ValueObject<string>>
{
    public TemplateVersionValidator(ValueObject<string> subject) : base(subject) { }

    public override void AddRules(BeyondNetCode.Shell.Ddd.Rules.RuleContext? context)
    {
        var value = Subject.GetValue();

        if (string.IsNullOrWhiteSpace(value))
        {
            AddBrokenRule(nameof(TemplateVersion), DomainErrors.ValueObject.PropertyRequired);
            return;
        }

        if (!SemVerPattern().IsMatch(value))
        {
            AddBrokenRule(nameof(TemplateVersion), DomainErrors.Authorization.InvalidTemplateVersion);
        }
    }

    [global::System.Text.RegularExpressions.GeneratedRegex(@"^\d+\.\d+\.\d+$")]
    private static partial global::System.Text.RegularExpressions.Regex SemVerPattern();
}
