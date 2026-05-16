namespace Ums.Domain.Authorization.ValueObjects;

public class PredicateExpression : StringValueObject
{
    private PredicateExpression(string value) : base(value) { }
    public static PredicateExpression Create(string value) => new PredicateExpression(value?.Trim() ?? string.Empty);
    public static PredicateExpression Default() => new PredicateExpression(string.Empty);
    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new GenericStringValidator(this, nameof(PredicateExpression), isRequired: false));
    }
}
