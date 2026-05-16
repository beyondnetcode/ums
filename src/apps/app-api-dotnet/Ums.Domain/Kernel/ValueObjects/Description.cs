using Ums.Shell.Ddd.ValueObjects.Common;

namespace Ums.Domain.Kernel.ValueObjects;

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
