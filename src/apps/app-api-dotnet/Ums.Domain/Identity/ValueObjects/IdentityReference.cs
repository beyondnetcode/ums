namespace Ums.Domain.Identity.ValueObjects;

public class IdentityReference : StringValueObject
{
    private IdentityReference(string value) : base(value) { }
    public static IdentityReference Create(string value) => new IdentityReference(value?.Trim() ?? string.Empty);
    public static IdentityReference Default() => new IdentityReference(string.Empty);
    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new GenericStringValidator(this, nameof(IdentityReference)));
    }
}
