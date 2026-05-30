namespace Ums.Domain.Kernel.ValueObjects;

using global::System.Text.RegularExpressions;

public partial class HexColor : StringValueObject
{
    private HexColor(string value) : base(value) { }

    public static HexColor Create(string value) => new HexColor(value?.Trim() ?? string.Empty);

    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new HexColorRuleValidator(this));
    }
}

public partial class HexColorRuleValidator : AbstractRuleValidator<ValueObject<string>>
{
    private static readonly Regex HexPattern = HexRegex();

    public HexColorRuleValidator(ValueObject<string> subject) : base(subject) { }

    public override void AddRules(BeyondNetCode.Shell.Ddd.Rules.RuleContext? context)
    {
        var value = Subject.GetValue();

        if (string.IsNullOrWhiteSpace(value))
        {
            AddBrokenRule(nameof(HexColor), DomainErrors.ValueObject.PropertyRequired);
            return;
        }

        if (!HexPattern.IsMatch(value))
        {
            AddBrokenRule(nameof(HexColor), DomainErrors.Branding.InvalidHexColor);
        }
    }

    [GeneratedRegex(@"^#([0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$")]
    private static partial Regex HexRegex();
}
