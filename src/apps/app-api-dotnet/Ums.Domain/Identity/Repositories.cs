namespace Ums.Domain.Identity;
using Ums.Domain.Identity.Tenant;
using TenantAggregate = Ums.Domain.Identity.Tenant.Tenant;

public interface ITenantRepository : ITenantScopedRepository<TenantAggregate>
{
    Task<TenantAggregate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
}
