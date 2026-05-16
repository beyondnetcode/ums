namespace Ums.Domain.Kernel.ValueObjects;

using Ums.Shell.Ddd;

public class BranchId : IdValueObject
{
    private BranchId(Guid value) : base(value) { }
    public static new BranchId Create() => new BranchId(Guid.NewGuid());
    public static new BranchId Load(Guid value) => new BranchId(value);
    public static new BranchId Load(string value) => new BranchId(Guid.Parse(value));
}
