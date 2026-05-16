namespace Ums.Domain.Iga.ValueObjects;

public class CriteriaId : IdValueObject
{
    private CriteriaId(Guid value) : base(value) { }
    public static new CriteriaId Create() => new CriteriaId(Guid.NewGuid());
    public static new CriteriaId Load(Guid value) => new CriteriaId(value);
    public static new CriteriaId Load(string value) => new CriteriaId(Guid.Parse(value));
}
