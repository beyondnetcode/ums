namespace Ums.Domain.Authorization.ValueObjects;

public class Payload : StringValueObject
{
    private Payload(string value) : base(value) { }
    public static Payload Create(string value) => new Payload(value?.Trim() ?? string.Empty);
    public static Payload Default() => new Payload(string.Empty);
    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new GenericStringValidator(this, nameof(Payload)));
    }
}
