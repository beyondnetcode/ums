namespace Ums.Domain.Kernel.ValueObjects;

public class BrandingId : IdValueObject
{
    private BrandingId(Guid value) : base(value) { }
    public static new BrandingId Create() => new BrandingId(Guid.NewGuid());
    public static new BrandingId Load(Guid value) => new BrandingId(value);
    public static new BrandingId Load(string value) => new BrandingId(Guid.Parse(value));
}
