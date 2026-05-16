namespace Ums.Domain.Identity;

using Ums.Domain.Kernel;

public interface ITenantRepository : ITenantScopedRepository<Tenant>
{
    Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}

public interface IUserAccountRepository : ITenantScopedRepository<UserAccount>
{
    Task<UserAccount?> GetByEmailAsync(Guid tenantId, string email, CancellationToken cancellationToken = default);
}

