namespace Ums.Domain.Identity.Tenant.Branch;

public class BranchProps : IProps
{
    public IdValueObject Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public Code Code { get; private set; }
    public Name Name { get; private set; }
    public Value? GeofencingMetadata { get; private set; }
    public bool IsActive { get; set; }
    public AuditValueObject Audit { get; private set; }

    public BranchProps(IdValueObject id, TenantId tenantId, Code code, Name name, Value? geofencingMetadata, ActorId createdBy)
    {
        Id = id;
        TenantId = tenantId;
        Code = code;
        Name = name;
        GeofencingMetadata = geofencingMetadata;
        IsActive = true;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
