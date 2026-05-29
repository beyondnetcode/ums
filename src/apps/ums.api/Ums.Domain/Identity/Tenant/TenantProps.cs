namespace Ums.Domain.Identity.Tenant;

public class TenantProps : IProps
{
    public IdValueObject Id { get; private set; }
    public Code Code { get; private set; }
    public Name Name { get; private set; }
    public OrganizationType Type { get; private set; }
    public IdpStrategy IdpStrategy { get; private set; }
    public CompanyReference? CompanyReference { get; private set; }
    public TenantId? ParentTenantId { get; private set; }
    public TenantStatus Status { get; private set; }
    public AuditValueObject Audit { get; private set; }

    public TenantProps(IdValueObject id, Code code, Name name, OrganizationType type, IdpStrategy idpStrategy, CompanyReference? companyReference, TenantId? parentTenantId, ActorId createdBy)
    {
        Id = id;
        Code = code;
        Name = name;
        Type = type;
        IdpStrategy = idpStrategy;
        CompanyReference = companyReference;
        ParentTenantId = parentTenantId;
        Status = TenantStatus.Active;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public TenantProps(IdValueObject id, Code code, Name name, OrganizationType type, IdpStrategy idpStrategy, CompanyReference? companyReference, TenantId? parentTenantId, TenantStatus status, AuditValueObject audit)
    {
        Id = id;
        Code = code;
        Name = name;
        Type = type;
        IdpStrategy = idpStrategy;
        CompanyReference = companyReference;
        ParentTenantId = parentTenantId;
        Status = status;
        Audit = audit;
    }

    public TenantProps WithStatus(TenantStatus status)
    {
        var clone = (TenantProps)MemberwiseClone();
        clone.Status = status;
        return clone;
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
