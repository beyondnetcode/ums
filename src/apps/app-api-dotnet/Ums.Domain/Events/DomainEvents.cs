namespace Ums.Domain.Events;

using Ums.Shell.Ddd;
using Ums.Domain.Kernel.ValueObjects;

public abstract record UmsDomainEvent(Guid TenantId) : DomainEvent;

public sealed record TenantCreatedEvent(Guid TenantId, string Code, string Name) : UmsDomainEvent(TenantId);
public sealed record BranchCreatedEvent(Guid TenantId, Guid BranchId, string Code) : UmsDomainEvent(TenantId);
public sealed record UserRegisteredEvent(Guid TenantId, Guid UserId, string Email) : UmsDomainEvent(TenantId);
public sealed record UserActivatedEvent(Guid TenantId, Guid UserId) : UmsDomainEvent(TenantId);
public sealed record UserBlockedEvent(Guid TenantId, Guid UserId, string Reason) : UmsDomainEvent(TenantId);

public sealed record SystemSuiteRegisteredEvent(Guid TenantId, Guid SystemSuiteId, string Code) : UmsDomainEvent(TenantId);
public sealed record FunctionalTopologyChangedEvent(Guid TenantId, Guid SystemSuiteId, string Code) : UmsDomainEvent(TenantId);
public sealed record RoleCreatedEvent(Guid TenantId, Guid RoleId, string Code) : UmsDomainEvent(TenantId);
public sealed record ProfileCreatedEvent(Guid TenantId, Guid ProfileId, string Name) : UmsDomainEvent(TenantId);
public sealed record PermissionTemplatePublishedEvent(Guid TenantId, Guid TemplateId, string Code, string Version) : UmsDomainEvent(TenantId);
public sealed record ProfilePermissionChangedEvent(Guid TenantId, Guid ProfileId, Guid FunctionalActionId) : UmsDomainEvent(TenantId);

public sealed record ConfigurationChangedEvent(Guid TenantId, Guid ConfigurationId, string Code, string Version) : UmsDomainEvent(TenantId);
public sealed record FeatureFlagChangedEvent(Guid TenantId, Guid FeatureFlagId, string Code, string Version) : UmsDomainEvent(TenantId);

public sealed record ApprovalRequestedEvent(Guid TenantId, Guid ApprovalRequestId, string RequestType) : UmsDomainEvent(TenantId);
public sealed record ApprovalCompletedEvent(Guid TenantId, Guid ApprovalRequestId, string Decision) : UmsDomainEvent(TenantId);

public sealed record UserPromotionStartedEvent(Guid TenantId, Guid PromotionProcessId, Guid UserId) : UmsDomainEvent(TenantId);
public sealed record UserPromotionCompletedEvent(Guid TenantId, Guid PromotionProcessId, Guid UserId, Guid TargetRoleId) : UmsDomainEvent(TenantId);
public sealed record DelegationGrantedEvent(Guid TenantId, Guid DelegationId, Guid DelegateUserId) : UmsDomainEvent(TenantId);

public sealed record UserDocumentUploadedEvent(Guid TenantId, Guid UserDocumentId, Guid UserId) : UmsDomainEvent(TenantId);
public sealed record UserDocumentStatusChangedEvent(Guid TenantId, Guid UserDocumentId, string Status) : UmsDomainEvent(TenantId);
public sealed record AccessEnforcementPolicyChangedEvent(Guid TenantId, Guid PolicyId, string Code) : UmsDomainEvent(TenantId);

public sealed record AuditRecordAppendedEvent(Guid TenantId, Guid AuditRecordId, string EventType) : UmsDomainEvent(TenantId);

