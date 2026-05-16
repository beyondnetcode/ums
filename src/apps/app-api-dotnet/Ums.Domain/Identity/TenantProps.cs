namespace Ums.Domain.Identity;

public class TenantProps : IProps
{
    public IdValueObject Id { get; private set; }
    public Code Code { get; private set; }
    public Name Name { get; private set; }
    public OrganizationType Type { get; private set; }
    public IdpStrategy IdpStrategy { get; private set; }
    public Value? CompanyReference { get; private set; }
    public TenantId? ParentTenantId { get; private set; }
    public TenantStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public TenantProps(IdValueObject id, Code code, Name name, OrganizationType type, IdpStrategy idpStrategy, Value? companyReference, TenantId? parentTenantId, string createdBy)
    {
        Id = id;
        Code = code;
        Name = name;
        Type = type;
        IdpStrategy = idpStrategy;
        CompanyReference = companyReference;
        ParentTenantId = parentTenantId;
        Status = TenantStatus.Active;
        Audit = AuditValueObject.Create(createdBy);
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
