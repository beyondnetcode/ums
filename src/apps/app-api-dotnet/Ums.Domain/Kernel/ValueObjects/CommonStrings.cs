using Ums.Shell.Ddd;
namespace Ums.Domain.Kernel.ValueObjects;

using Ums.Shell.Ddd.Rules.Impl;
using Ums.Shell.Ddd.Rules;
using Ums.Shell.Ddd.ValueObjects.Common;

public class GenericStringValidator : AbstractRuleValidator<ValueObject<string>>
{
    private readonly string _propertyName;
    private readonly bool _isRequired;

    public GenericStringValidator(ValueObject<string> subject, string propertyName, bool isRequired = true) : base(subject)
    {
        _propertyName = propertyName;
        _isRequired = isRequired;
    }

    public override void AddRules(RuleContext? context)
    {
        var value = Subject.GetValue();
        if (_isRequired && string.IsNullOrWhiteSpace(value))
        {
            AddBrokenRule(_propertyName, $"{_propertyName} is required.");
        }
    }
}

public class Name : StringValueObject
{
    private Name(string value) : base(value) { }
    public static Name Create(string value) => new Name(value?.Trim() ?? string.Empty);
    public static Name Default() => new Name(string.Empty);
    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new GenericStringValidator(this, nameof(Name)));
    }
}

public class Description : StringValueObject
{
    private Description(string value) : base(value) { }
    public static Description Create(string value) => new Description(value?.Trim() ?? string.Empty);
    public static Description Default() => new Description(string.Empty);
    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new GenericStringValidator(this, nameof(Description)));
    }
}

public class Version : StringValueObject
{
    private Version(string value) : base(value) { }
    public static Version Create(string value) => new Version(value?.Trim() ?? string.Empty);
    public static Version Default() => new Version(string.Empty);
    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new GenericStringValidator(this, nameof(Version)));
    }
}

public class Value : StringValueObject
{
    private Value(string value) : base(value) { }
    public static Value Create(string value) => new Value(value?.Trim() ?? string.Empty);
    public static Value Default() => new Value(string.Empty);
    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new GenericStringValidator(this, nameof(Value)));
    }
}
