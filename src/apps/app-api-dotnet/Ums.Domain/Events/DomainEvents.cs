namespace Ums.Domain.Events;

public abstract record UmsDomainEvent(Guid TenantId) : DomainEvent;

public sealed record TenantCreatedEvent(Guid TenantId, string Code, string Name) : UmsDomainEvent(TenantId);
public sealed record TenantSuspendedEvent(Guid TenantId) : UmsDomainEvent(TenantId);
public sealed record TenantActivatedEvent(Guid TenantId) : UmsDomainEvent(TenantId);

public sealed record BranchCreatedEvent(Guid TenantId, Guid BranchId, string Code) : UmsDomainEvent(TenantId);
public sealed record BranchRemovedEvent(Guid TenantId, Guid BranchId) : UmsDomainEvent(TenantId);
public sealed record BranchDeactivatedEvent(Guid TenantId, Guid BranchId) : UmsDomainEvent(TenantId);
public sealed record BranchReactivatedEvent(Guid TenantId, Guid BranchId) : UmsDomainEvent(TenantId);

public sealed record IdentityProviderRegisteredEvent(Guid TenantId, Guid IdentityProviderId, string Code, string Strategy) : UmsDomainEvent(TenantId);
public sealed record IdentityProviderActivatedEvent(Guid TenantId, Guid IdentityProviderId, string Code) : UmsDomainEvent(TenantId);
public sealed record IdentityProviderDeactivatedEvent(Guid TenantId, Guid IdentityProviderId, string Code) : UmsDomainEvent(TenantId);
public sealed record IdentityProviderRemovedEvent(Guid TenantId, Guid IdentityProviderId) : UmsDomainEvent(TenantId);

public sealed record BrandingCreatedEvent(Guid TenantId, Guid BrandingId) : UmsDomainEvent(TenantId);
public sealed record BrandingUpdatedEvent(Guid TenantId, Guid BrandingId) : UmsDomainEvent(TenantId);
public sealed record BrandingRemovedEvent(Guid TenantId, Guid BrandingId) : UmsDomainEvent(TenantId);
public sealed record BrandingDnsVerifiedEvent(Guid TenantId, Guid BrandingId) : UmsDomainEvent(TenantId);
public sealed record BrandingDnsFailedEvent(Guid TenantId, Guid BrandingId) : UmsDomainEvent(TenantId);

public sealed record UserRegisteredEvent(Guid UserId, Guid TenantId, Guid? BranchId, string Category, string? IdentityReference) : UmsDomainEvent(TenantId);
public sealed record UserActivatedEvent(Guid UserId, Guid TenantId) : UmsDomainEvent(TenantId);
public sealed record UserBlockedEvent(Guid UserId, Guid TenantId, string Reason) : UmsDomainEvent(TenantId);
public sealed record UserRestoredEvent(Guid UserId, Guid TenantId) : UmsDomainEvent(TenantId);
public sealed record MfaEnrolledEvent(Guid UserId, Guid TenantId, string Method) : UmsDomainEvent(TenantId);
public sealed record MfaVerifiedEvent(Guid UserId, Guid TenantId, string Method) : UmsDomainEvent(TenantId);
public sealed record AuthenticationAttemptedEvent(Guid? UserId, Guid TenantId, bool Success, string Reason, string IpAddress) : UmsDomainEvent(TenantId);

public abstract record AuthorizationDomainEvent : DomainEvent;

public sealed record PermissionTemplateCreatedEvent(Guid TemplateId, Guid TenantId, Guid RoleId, Guid SystemSuiteId, string Version) : AuthorizationDomainEvent;
public sealed record PermissionTemplatePublishedEvent(Guid TemplateId, string Version) : AuthorizationDomainEvent;
public sealed record PermissionTemplateMutatedEvent(Guid TemplateId, string Version) : AuthorizationDomainEvent;
public sealed record PermissionTemplateDeprecatedEvent(Guid TemplateId, string Version) : AuthorizationDomainEvent;

public sealed record ProfileCreatedEvent(Guid ProfileId, Guid UserId, Guid RoleId, Guid? BranchId) : AuthorizationDomainEvent;
public sealed record TemplateLinkedToProfileEvent(Guid ProfileId, Guid TemplateId) : AuthorizationDomainEvent;
public sealed record PermissionOverriddenEvent(Guid ProfileId, Guid PermissionId, string Effect) : AuthorizationDomainEvent;
public sealed record PermissionStatusChangedEvent(Guid ProfileId, Guid PermissionId, string Status) : AuthorizationDomainEvent;
public sealed record ProfileDeactivatedEvent(Guid ProfileId) : AuthorizationDomainEvent;
public sealed record ProfileActivatedEvent(Guid ProfileId) : AuthorizationDomainEvent;
