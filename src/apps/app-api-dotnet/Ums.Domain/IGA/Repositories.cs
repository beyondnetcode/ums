namespace Ums.Domain.IGA;

using Ums.Domain.Kernel;

public interface IRolePromotionCriteriaRepository : ITenantScopedRepository<RolePromotionCriteria>
{
    Task<IReadOnlyCollection<RolePromotionCriteria>> GetActiveAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface IUserPromotionProcessRepository : ITenantScopedRepository<UserPromotionProcess>
{
    Task<IReadOnlyCollection<UserPromotionProcess>> GetOpenByUserAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default);
}

public interface IUserManagementDelegationRepository : ITenantScopedRepository<UserManagementDelegation>
{
    Task<IReadOnlyCollection<UserManagementDelegation>> GetActiveForDelegateAsync(Guid tenantId, Guid delegateUserId, CancellationToken cancellationToken = default);
}

