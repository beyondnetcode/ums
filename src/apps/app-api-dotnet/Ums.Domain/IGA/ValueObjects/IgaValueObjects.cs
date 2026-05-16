using Ums.Shell.Ddd.ValueObjects.Common;
namespace Ums.Domain.Iga.ValueObjects;

using Ums.Shell.Ddd;

public class CriteriaId : IdValueObject
{
    private CriteriaId(Guid value) : base(value) { }
    public static new CriteriaId Create() => new CriteriaId(Guid.NewGuid());
    public static new CriteriaId Load(Guid value) => new CriteriaId(value);
    public static new CriteriaId Load(string value) => new CriteriaId(Guid.Parse(value));
}

public class DelegationId : IdValueObject
{
    private DelegationId(Guid value) : base(value) { }
    public static new DelegationId Create() => new DelegationId(Guid.NewGuid());
    public static new DelegationId Load(Guid value) => new DelegationId(value);
    public static new DelegationId Load(string value) => new DelegationId(Guid.Parse(value));
}

public class PromotionProcessId : IdValueObject
{
    private PromotionProcessId(Guid value) : base(value) { }
    public static new PromotionProcessId Create() => new PromotionProcessId(Guid.NewGuid());
    public static new PromotionProcessId Load(Guid value) => new PromotionProcessId(value);
    public static new PromotionProcessId Load(string value) => new PromotionProcessId(Guid.Parse(value));
}
