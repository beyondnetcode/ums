namespace Ums.Domain.Kernel.ValueObjects;

public class LoginText : StringValueObject
{
    private LoginText(string value) : base(value) { }

    public static LoginText Create(string value) => new LoginText(value?.Trim() ?? string.Empty);

    public static LoginText Default() => new LoginText(string.Empty);

    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new LoginTextValidator(this));
    }
}

public class LoginTextValidator : AbstractRuleValidator<ValueObject<string>>
{
    public LoginTextValidator(ValueObject<string> subject) : base(subject) { }

    public override void AddRules(Ums.Shell.Ddd.Rules.RuleContext? context)
    {
        var value = Subject.GetValue();

        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        if (value.Length > 200)
        {
            AddBrokenRule(nameof(LoginText), DomainErrors.Branding.LoginTextTooLong);
        }
    }
}
