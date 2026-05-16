namespace Ums.Domain.Compliance;

using Ums.Domain.Kernel;

public interface IDocumentTypeRepository : ITenantScopedRepository<DocumentType>
{
    Task<DocumentType?> GetByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
}

public interface IUserDocumentRepository : ITenantScopedRepository<UserDocument>
{
    Task<IReadOnlyCollection<UserDocument>> GetByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}

public interface INotificationRuleRepository : ITenantScopedRepository<NotificationRule>
{
    Task<IReadOnlyCollection<NotificationRule>> GetByTriggerAsync(Guid tenantId, string triggerEvent, CancellationToken cancellationToken = default);
}

public interface IAccessEnforcementPolicyRepository : ITenantScopedRepository<AccessEnforcementPolicy>
{
    Task<IReadOnlyCollection<AccessEnforcementPolicy>> GetActiveAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

