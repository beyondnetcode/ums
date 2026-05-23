namespace Ums.Domain.Identity;
using Ums.Domain.Identity.Tenant;
using Ums.Domain.Identity.UserAccount;
using Ums.Domain.Identity.UserManagementDelegation;
using TenantAggregate = Ums.Domain.Identity.Tenant.Tenant;
using UserAccountAggregate = Ums.Domain.Identity.UserAccount.UserAccount;
using UserManagementDelegationAggregate = Ums.Domain.Identity.UserManagementDelegation.UserManagementDelegation;

public interface ITenantRepository : IAggregateRepository<TenantAggregate>
{
    Task<TenantAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TenantAggregate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IUserAccountRepository : IAggregateRepository<UserAccountAggregate>
{
    Task<UserAccountAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UserAccountAggregate?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserAccountAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserAccountAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface IUserManagementDelegationRepository : IAggregateRepository<UserManagementDelegationAggregate>
{
    Task<UserManagementDelegationAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserManagementDelegationAggregate>> GetByDelegatedAdminAsync(Guid delegatedAdminId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserManagementDelegationAggregate>> GetByDelegatingAdminAsync(Guid delegatingAdminId, Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserManagementDelegationAggregate>> GetActiveAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserManagementDelegationAggregate>> GetExpiredActiveAsync(DateTimeOffset asOf, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserManagementDelegationAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
}
