namespace Ums.Domain.Authorization.ValueObjects;

public class GraphHash : StringValueObject
{
    private GraphHash(string value) : base(value) { }
    public static GraphHash Create(string value) => new GraphHash(value?.Trim() ?? string.Empty);
    public static GraphHash Default() => new GraphHash(string.Empty);
    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new GenericStringValidator(this, nameof(GraphHash)));
    }
}
