namespace Ums.Domain.Kernel.ValueObjects;

public class PermissionTemplateId : IdValueObject
{
    private PermissionTemplateId(Guid value) : base(value) { }
    public static new PermissionTemplateId Create() => new PermissionTemplateId(Guid.NewGuid());
    public static new PermissionTemplateId Load(Guid value) => new PermissionTemplateId(value);
    public static new PermissionTemplateId Load(string value) => new PermissionTemplateId(Guid.Parse(value));
}
