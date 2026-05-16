namespace Ums.Domain.Kernel.ValueObjects;

using Ums.Shell.Ddd;

public class UserId : IdValueObject
{
    private UserId(Guid value) : base(value) { }
    public static new UserId Create() => new UserId(Guid.NewGuid());
    public static new UserId Load(Guid value) => new UserId(value);
    public static new UserId Load(string value) => new UserId(Guid.Parse(value));
}
