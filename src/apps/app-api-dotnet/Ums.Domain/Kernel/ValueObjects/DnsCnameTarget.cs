namespace Ums.Domain.Kernel.ValueObjects;

public class DnsCnameTarget : StringValueObject
{
    private static readonly string ExpectedTarget = "edge.platform.io";

    private DnsCnameTarget(string value) : base(value) { }

    public static DnsCnameTarget Create() => new DnsCnameTarget(ExpectedTarget);

    public static string GetExpectedTarget() => ExpectedTarget;

    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new DnsCnameTargetValidator(this));
    }
}

public class DnsCnameTargetValidator : AbstractRuleValidator<ValueObject<string>>
{
    private static readonly string ExpectedTarget = DnsCnameTarget.GetExpectedTarget();

    public DnsCnameTargetValidator(ValueObject<string> subject) : base(subject) { }

    public override void AddRules(Ums.Shell.Ddd.Rules.RuleContext? context)
    {
        var value = Subject.GetValue();

        if (string.IsNullOrWhiteSpace(value))
        {
            AddBrokenRule(nameof(DnsCnameTarget), DomainErrors.ValueObject.PropertyRequired);
            return;
        }

        if (!value.Equals(ExpectedTarget, StringComparison.OrdinalIgnoreCase))
        {
            AddBrokenRule(nameof(DnsCnameTarget), DomainErrors.Branding.InvalidCnameTarget);
        }
    }
}
