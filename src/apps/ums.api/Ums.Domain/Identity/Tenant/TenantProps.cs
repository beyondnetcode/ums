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
    public bool IsManagementOwner { get; private set; }
    public TenantStatus Status { get; private set; }
    public AuditValueObject Audit { get; private set; }

    public TenantProps(
        IdValueObject id,
        Code code,
        Name name,
        OrganizationType type,
        IdpStrategy idpStrategy,
        CompanyReference? companyReference,
        TenantId? parentTenantId,
        bool isManagementOwner,
        ActorId createdBy)
    {
        Id = id;
        Code = code;
        Name = name;
        Type = type;
        IdpStrategy = idpStrategy;
        CompanyReference = companyReference;
        ParentTenantId = parentTenantId;
        IsManagementOwner = isManagementOwner;
        Status = TenantStatus.Active;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public TenantProps(
        IdValueObject id,
        Code code,
        Name name,
        OrganizationType type,
        IdpStrategy idpStrategy,
        CompanyReference? companyReference,
        TenantId? parentTenantId,
        bool isManagementOwner,
        TenantStatus status,
        AuditValueObject audit)
    {
        Id = id;
        Code = code;
        Name = name;
        Type = type;
        IdpStrategy = idpStrategy;
        CompanyReference = companyReference;
        ParentTenantId = parentTenantId;
        IsManagementOwner = isManagementOwner;
        Status = status;
        Audit = audit;
    }

    public TenantProps WithManagementOwner(bool isManagementOwner)
    {
        var clone = (TenantProps)MemberwiseClone();
        clone.IsManagementOwner = isManagementOwner;
        return clone;
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
