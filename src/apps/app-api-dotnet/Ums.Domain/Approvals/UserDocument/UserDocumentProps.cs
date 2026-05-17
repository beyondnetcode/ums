namespace Ums.Domain.Approvals.UserDocument;

public class UserDocumentProps : IProps
{
    public IdValueObject Id { get; set; }
    public UserId UserId { get; set; }
    public DocumentTypeId DocumentTypeId { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DocumentStatus Status { get; set; }
    public DocumentCriticity Criticity { get; set; }
    public TextValueObject FileStoragePath { get; set; }
    public AuditValueObject Audit { get; private set; }

    public UserDocumentProps(
        IdValueObject id,
        UserId userId,
        DocumentTypeId documentTypeId,
        DateTime issueDate,
        DateTime expirationDate,
        DocumentStatus status,
        DocumentCriticity criticity,
        TextValueObject fileStoragePath,
        ActorId createdBy)
    {
        Id = id;
        UserId = userId;
        DocumentTypeId = documentTypeId;
        IssueDate = issueDate;
        ExpirationDate = expirationDate;
        Status = status;
        Criticity = criticity;
        FileStoragePath = fileStoragePath;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
