using Ums.Shell.Ddd.ValueObjects.Common;
namespace Ums.Domain.Compliance.ValueObjects;

using Ums.Shell.Ddd;

public class PolicyId : IdValueObject
{
    private PolicyId(Guid value) : base(value) { }
    public static new PolicyId Create() => new PolicyId(Guid.NewGuid());
    public static new PolicyId Load(Guid value) => new PolicyId(value);
    public static new PolicyId Load(string value) => new PolicyId(Guid.Parse(value));
}
