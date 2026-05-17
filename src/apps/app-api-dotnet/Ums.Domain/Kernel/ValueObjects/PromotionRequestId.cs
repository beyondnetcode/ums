namespace Ums.Domain.Kernel.ValueObjects;

public class PromotionRequestId : IdValueObject
{
    private PromotionRequestId(Guid value) : base(value) { }
    public static new PromotionRequestId Create() => new PromotionRequestId(Guid.NewGuid());
    public static new PromotionRequestId Load(Guid value) => new PromotionRequestId(value);
    public static new PromotionRequestId Load(string value) => new PromotionRequestId(Guid.Parse(value));
}
