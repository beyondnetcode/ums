namespace Ums.Domain.Approvals.UserDocument;

public sealed class UserDocument : AggregateRoot<UserDocument, UserDocumentProps>
{
    private UserDocument(UserDocumentProps props) : base(props)
    {
    }

    public UserId UserId => Props.UserId;
    public DocumentTypeId DocumentTypeId => Props.DocumentTypeId;
    public DateTime IssueDate => Props.IssueDate;
    public DateTime ExpirationDate => Props.ExpirationDate;
    public DocumentStatus Status => Props.Status;
    public DocumentCriticity Criticity => Props.Criticity;
    public TextValueObject FileStoragePath => Props.FileStoragePath;

    public UserDocumentId GetId() => UserDocumentId.Load(Props.Id.GetValue());

    public static Result<UserDocument> Create(
        UserId userId,
        DocumentTypeId documentTypeId,
        DateTime issueDate,
        DateTime expirationDate,
        DocumentCriticity criticity,
        TextValueObject fileStoragePath,
        ActorId createdBy)
    {
        var props = new UserDocumentProps(IdValueObject.Create(), userId, documentTypeId, issueDate, expirationDate, DocumentStatus.Valid, criticity, fileStoragePath, createdBy);
        var document = new UserDocument(props);

        if (!document.IsValid())
        {
            return Result<UserDocument>.Failure(document.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<UserDocument>.Success(document);
    }

    public Result MarkAsExpired(ActorId updatedBy)
    {
        if (Status == DocumentStatus.Expired)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Approvals.DocumentAlreadyExpired));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = DocumentStatus.Expired;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result MarkAsPendingRenewal(ActorId updatedBy)
    {
        if (Status == DocumentStatus.Expired)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Approvals.DocumentAlreadyExpired));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = DocumentStatus.PendingRenewal;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Renew(DateTime newIssueDate, DateTime newExpirationDate, TextValueObject newFileStoragePath, ActorId updatedBy)
    {
        Props.IssueDate = newIssueDate;
        Props.ExpirationDate = newExpirationDate;
        Props.FileStoragePath = newFileStoragePath;
        Props.Status = DocumentStatus.Valid;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }
}
