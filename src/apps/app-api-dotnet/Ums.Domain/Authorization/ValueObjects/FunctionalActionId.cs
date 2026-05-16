namespace Ums.Domain.Authorization.ValueObjects;

public class FunctionalActionId : IdValueObject
{
    private FunctionalActionId(Guid value) : base(value) { }
    public static new FunctionalActionId Create() => new FunctionalActionId(Guid.NewGuid());
    public static new FunctionalActionId Load(Guid value) => new FunctionalActionId(value);
    public static new FunctionalActionId Load(string value) => new FunctionalActionId(Guid.Parse(value));
}
