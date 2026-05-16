namespace Ums.Domain.Iga.ValueObjects;

public class DelegationId : IdValueObject
{
    private DelegationId(Guid value) : base(value) { }
    public static new DelegationId Create() => new DelegationId(Guid.NewGuid());
    public static new DelegationId Load(Guid value) => new DelegationId(value);
    public static new DelegationId Load(string value) => new DelegationId(Guid.Parse(value));
}
