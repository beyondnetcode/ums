namespace Ums.Domain.Compliance;

public class UserDocumentProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.UserId UserId { get; private set; }
    public IdValueObject DocumentTypeId { get; private set; }
    public StringValueObject StorageReference { get; private set; }
    public DateTimeOffset? ExpiresAt { get; private set; }
    public UserDocumentStatus Status { get; set; }
    public StringValueObject? ReviewComment { get; set; }
    public AuditValueObject Audit { get; private set; }

    public UserDocumentProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.UserId userId, IdValueObject documentTypeId, StringValueObject storageReference, DateTimeOffset? expiresAt)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        DocumentTypeId = documentTypeId;
        StorageReference = storageReference;
        ExpiresAt = expiresAt;
        Status = UserDocumentStatus.PendingReview;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
