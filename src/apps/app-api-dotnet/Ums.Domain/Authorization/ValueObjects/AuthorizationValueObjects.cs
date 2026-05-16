using Ums.Shell.Ddd.ValueObjects.Common;
namespace Ums.Domain.Authorization.ValueObjects;

using Ums.Shell.Ddd;
using Ums.Domain.Kernel.ValueObjects;

public class RoleId : IdValueObject
{
    private RoleId(Guid value) : base(value) { }
    public static new RoleId Create() => new RoleId(Guid.NewGuid());
    public static new RoleId Load(Guid value) => new RoleId(value);
    public static new RoleId Load(string value) => new RoleId(Guid.Parse(value));
}

public class TemplateId : IdValueObject
{
    private TemplateId(Guid value) : base(value) { }
    public static new TemplateId Create() => new TemplateId(Guid.NewGuid());
    public static new TemplateId Load(Guid value) => new TemplateId(value);
    public static new TemplateId Load(string value) => new TemplateId(Guid.Parse(value));
}

public class ProfileId : IdValueObject
{
    private ProfileId(Guid value) : base(value) { }
    public static new ProfileId Create() => new ProfileId(Guid.NewGuid());
    public static new ProfileId Load(Guid value) => new ProfileId(value);
    public static new ProfileId Load(string value) => new ProfileId(Guid.Parse(value));
}

public class FunctionalActionId : IdValueObject
{
    private FunctionalActionId(Guid value) : base(value) { }
    public static new FunctionalActionId Create() => new FunctionalActionId(Guid.NewGuid());
    public static new FunctionalActionId Load(Guid value) => new FunctionalActionId(value);
    public static new FunctionalActionId Load(string value) => new FunctionalActionId(Guid.Parse(value));
}

public class PredicateExpression : StringValueObject
{
    private PredicateExpression(string value) : base(value) { }
    public static PredicateExpression Create(string value) => new PredicateExpression(value?.Trim() ?? string.Empty);
    public static PredicateExpression Default() => new PredicateExpression(string.Empty);
    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new GenericStringValidator(this, nameof(PredicateExpression), isRequired: false));
    }
}

public class GraphHash : StringValueObject
{
    private GraphHash(string value) : base(value) { }
    public static GraphHash Create(string value) => new GraphHash(value?.Trim() ?? string.Empty);
    public static GraphHash Default() => new GraphHash(string.Empty);
    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new GenericStringValidator(this, nameof(GraphHash)));
    }
}

public class Payload : StringValueObject
{
    private Payload(string value) : base(value) { }
    public static Payload Create(string value) => new Payload(value?.Trim() ?? string.Empty);
    public static Payload Default() => new Payload(string.Empty);
    public override void AddValidators()
    {
        base.AddValidators();
        ValidatorRules.Add(new GenericStringValidator(this, nameof(Payload)));
    }
}
