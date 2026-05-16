using Ums.Shell.Ddd.ValueObjects.Common;
namespace Ums.Domain.Identity.ValueObjects;

using Ums.Shell.Ddd;
using Ums.Domain.Kernel.ValueObjects;

public class UserAccountId : IdValueObject
{
    private UserAccountId(Guid value) : base(value) { }
    public static new UserAccountId Create() => new UserAccountId(Guid.NewGuid());
    public static new UserAccountId Load(Guid value) => new UserAccountId(value);
    public static new UserAccountId Load(string value) => new UserAccountId(Guid.Parse(value));
}

public class IdentityReference : StringValueObject
{
    private IdentityReference(string value) : base(value) { }
    public static IdentityReference Create(string value) => new IdentityReference(value?.Trim() ?? string.Empty);
    public static IdentityReference Default() => new IdentityReference(string.Empty);
    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new GenericStringValidator(this, nameof(IdentityReference)));
    }
}
