namespace Ums.Domain.Iga.ValueObjects;

public class PromotionProcessId : IdValueObject
{
    private PromotionProcessId(Guid value) : base(value) { }
    public static new PromotionProcessId Create() => new PromotionProcessId(Guid.NewGuid());
    public static new PromotionProcessId Load(Guid value) => new PromotionProcessId(value);
    public static new PromotionProcessId Load(string value) => new PromotionProcessId(Guid.Parse(value));
}
