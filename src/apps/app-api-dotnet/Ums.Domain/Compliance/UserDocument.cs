namespace Ums.Domain.Compliance;

public sealed class UserDocument : AggregateRoot<UserDocument, UserDocumentProps>
{
    private UserDocument(UserDocumentProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new UserDocumentUploadedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.UserId.GetValue()), true);
        }
    }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid UserId => Props.UserId.GetValue();
    public Guid DocumentTypeId => Props.DocumentTypeId.GetValue();
    public string StorageReference => Props.StorageReference.GetValue();
    public DateTimeOffset? ExpiresAt => Props.ExpiresAt;
    public UserDocumentStatus Status => Props.Status;
    public string? ReviewComment => Props.ReviewComment?.GetValue();

    public static Result<UserDocument> Upload(Guid tenantId, Guid userId, Guid documentTypeId, string storageReference, DateTimeOffset? expiresAt = null)
    {
        if (tenantId == Guid.Empty || userId == Guid.Empty || documentTypeId == Guid.Empty)
            return Result<UserDocument>.Failure(DomainErrors.Compliance.UserDocumentIdentifiersRequired);

        if (string.IsNullOrWhiteSpace(storageReference))
            return Result<UserDocument>.Failure(DomainErrors.Compliance.StorageReferenceRequired);

        var props = new UserDocumentProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.UserId.Load(userId),
            IdValueObject.Load(documentTypeId),
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(storageReference.Trim()),
            expiresAt);

        var document = new UserDocument(props);
        return Result<UserDocument>.Success(document);
    }

    public Result MarkValid(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return Result.Failure(DomainErrors.Compliance.ReviewCommentRequired);

        Props.Status = UserDocumentStatus.Valid;
        Props.ReviewComment = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(comment.Trim());
        Props.Audit.Update("system");
        
        DomainEvents.ApplyChange(new UserDocumentStatusChangedEvent(TenantId, GetId(), Props.Status.Name), true);
        return Result.Success();
    }

    public Result Reject(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return Result.Failure(DomainErrors.Compliance.ReviewCommentRequired);

        Props.Status = UserDocumentStatus.Rejected;
        Props.ReviewComment = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(comment.Trim());
        Props.Audit.Update("system");
        
        DomainEvents.ApplyChange(new UserDocumentStatusChangedEvent(TenantId, GetId(), Props.Status.Name), true);
        return Result.Success();
    }
    
    public Guid GetId() => Props.Id.GetValue();
}
