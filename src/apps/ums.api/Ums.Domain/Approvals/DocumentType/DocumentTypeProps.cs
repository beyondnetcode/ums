namespace Ums.Domain.Approvals.DocumentType;

public class DocumentTypeProps : IProps
{
    public IdValueObject Id { get; set; }
    public TenantId TenantId { get; set; }
    public Code Code { get; set; }
    public Name Name { get; set; }
    public Description Description { get; set; }
    public DocumentCriticity Criticity { get; set; }
    public AuditValueObject Audit { get; private set; }

    public DocumentTypeProps(
        IdValueObject id,
        TenantId tenantId,
        Code code,
        Name name,
        Description description,
        DocumentCriticity criticity,
        ActorId createdBy)
    {
        Id = id;
        TenantId = tenantId;
        Code = code;
        Name = name;
        Description = description;
        Criticity = criticity;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
