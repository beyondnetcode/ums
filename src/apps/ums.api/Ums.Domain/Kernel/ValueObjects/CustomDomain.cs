namespace Ums.Domain.Kernel.ValueObjects;

using global::System.Text.RegularExpressions;

public partial class CustomDomain : StringValueObject
{
    private CustomDomain(string value) : base(value) { }

    public static CustomDomain Create(string value) => new CustomDomain(value?.Trim().ToLowerInvariant() ?? string.Empty);

    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new CustomDomainRuleValidator(this));
    }
}

public partial class CustomDomainRuleValidator : AbstractRuleValidator<ValueObject<string>>
{
    private static readonly Regex DomainPattern = DomainRegex();

    public CustomDomainRuleValidator(ValueObject<string> subject) : base(subject) { }

    public override void AddRules(Ums.Shell.Ddd.Rules.RuleContext? context)
    {
        var value = Subject.GetValue();

        if (string.IsNullOrWhiteSpace(value))
        {
            AddBrokenRule(nameof(CustomDomain), DomainErrors.ValueObject.PropertyRequired);
            return;
        }

        if (!DomainPattern.IsMatch(value))
        {
            AddBrokenRule(nameof(CustomDomain), DomainErrors.Branding.InvalidCustomDomain);
        }
    }

    [GeneratedRegex(@"^([a-z0-9]([a-z0-9-]{0,61}[a-z0-9])?\.)+[a-z]{2,}$")]
    private static partial Regex DomainRegex();
}
