namespace Ums.Domain.Compliance;

using Ums.Domain.Common;
using Ums.Domain.Enums;
using Ums.Domain.Events;
using Ums.Shell.Ddd;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Shell.Ddd.Interfaces;
using Ums.Shell.Ddd.ValueObjects.Common;

public class DocumentTypeProps : ParametricCatalogProps
{
    public bool RequiresExpirationDate { get; set; }
    public bool BlocksAccessOnExpiration { get; set; }

    public DocumentTypeProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}

public sealed class DocumentType : ParametricCatalogEntity<DocumentType, DocumentTypeProps>
{
    private DocumentType(DocumentTypeProps props) : base(props) { }

    public bool RequiresExpirationDate => Props.RequiresExpirationDate;
    public bool BlocksAccessOnExpiration => Props.BlocksAccessOnExpiration;

    public static Result<DocumentType> Create(Guid tenantId, string code, string value, string description, bool requiresExpirationDate, bool blocksAccessOnExpiration, string version = "1.0.0")
    {
        var props = new DocumentTypeProps
        {
            RequiresExpirationDate = requiresExpirationDate,
            BlocksAccessOnExpiration = blocksAccessOnExpiration
        };

        var documentType = new DocumentType(props);
        var result = documentType.SetCatalogFields(tenantId, code, value, description, version);
        
        return result.IsFailure ? Result<DocumentType>.Failure(result.Error) : Result<DocumentType>.Success(documentType);
    }
}

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
            return Result<UserDocument>.Failure("Tenant, user, and document type identifiers are required.");

        if (string.IsNullOrWhiteSpace(storageReference))
            return Result<UserDocument>.Failure("Storage reference is required.");

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
            return Result.Failure("Review comment is required.");

        Props.Status = UserDocumentStatus.Valid;
        Props.ReviewComment = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(comment.Trim());
        Props.Audit.Update("system");
        
        DomainEvents.ApplyChange(new UserDocumentStatusChangedEvent(TenantId, GetId(), Props.Status.Name), true);
        return Result.Success();
    }

    public Result Reject(string comment)
    {
        if (string.IsNullOrWhiteSpace(comment))
            return Result.Failure("Review comment is required.");

        Props.Status = UserDocumentStatus.Rejected;
        Props.ReviewComment = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(comment.Trim());
        Props.Audit.Update("system");
        
        DomainEvents.ApplyChange(new UserDocumentStatusChangedEvent(TenantId, GetId(), Props.Status.Name), true);
        return Result.Success();
    }
    
    public Guid GetId() => Props.Id.GetValue();
}

public class NotificationRuleProps : ParametricCatalogProps
{
    public StringValueObject TriggerEvent { get; set; } = default!;
    public StringValueObject Channel { get; set; } = default!;

    public NotificationRuleProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}

public sealed class NotificationRule : ParametricCatalogEntity<NotificationRule, NotificationRuleProps>
{
    private NotificationRule(NotificationRuleProps props) : base(props) { }

    public string TriggerEvent => Props.TriggerEvent.GetValue();
    public string Channel => Props.Channel.GetValue();

    public static Result<NotificationRule> Create(Guid tenantId, string code, string value, string description, string triggerEvent, string channel, string version = "1.0.0")
    {
        if (string.IsNullOrWhiteSpace(triggerEvent) || string.IsNullOrWhiteSpace(channel))
            return Result<NotificationRule>.Failure("Trigger event and channel are required.");

        var props = new NotificationRuleProps
        {
            TriggerEvent = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(triggerEvent.Trim()),
            Channel = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(channel.Trim())
        };

        var rule = new NotificationRule(props);
        var result = rule.SetCatalogFields(tenantId, code, value, description, version);
        
        return result.IsFailure ? Result<NotificationRule>.Failure(result.Error) : Result<NotificationRule>.Success(rule);
    }
}

public class AccessEnforcementPolicyProps : ParametricCatalogProps
{
    public EnforcementEffect Effect { get; set; } = default!;
    public StringValueObject ResourceScope { get; set; } = default!;

    public AccessEnforcementPolicyProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}

public sealed class AccessEnforcementPolicy : ParametricCatalogEntity<AccessEnforcementPolicy, AccessEnforcementPolicyProps>
{
    private AccessEnforcementPolicy(AccessEnforcementPolicyProps props) : base(props) { }

    public EnforcementEffect Effect => Props.Effect;
    public string ResourceScope => Props.ResourceScope.GetValue();

    public static Result<AccessEnforcementPolicy> Create(Guid tenantId, string code, string value, string description, EnforcementEffect effect, string resourceScope, string version = "1.0.0")
    {
        if (string.IsNullOrWhiteSpace(resourceScope))
            return Result<AccessEnforcementPolicy>.Failure("Resource scope is required.");

        var props = new AccessEnforcementPolicyProps
        {
            Effect = effect,
            ResourceScope = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(resourceScope.Trim())
        };

        var policy = new AccessEnforcementPolicy(props);
        var result = policy.SetCatalogFields(tenantId, code, value, description, version);
        if (result.IsFailure)
            return Result<AccessEnforcementPolicy>.Failure(result.Error);

        policy.DomainEvents.ApplyChange(new AccessEnforcementPolicyChangedEvent(tenantId, policy.GetId(), policy.Code), true);
        return Result<AccessEnforcementPolicy>.Success(policy);
    }
}

