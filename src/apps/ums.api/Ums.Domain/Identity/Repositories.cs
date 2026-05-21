namespace Ums.Domain.Identity;
using Ums.Domain.Identity.Tenant;
using Ums.Domain.Identity.UserAccount;
using TenantAggregate = Ums.Domain.Identity.Tenant.Tenant;
using UserAccountAggregate = Ums.Domain.Identity.UserAccount.UserAccount;

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
