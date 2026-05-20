namespace Ums.Domain.Identity;
using Ums.Domain.Identity.Tenant;
using TenantAggregate = Ums.Domain.Identity.Tenant.Tenant;

public interface ITenantRepository : IAggregateRepository<TenantAggregate>
{
    Task<TenantAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<TenantAggregate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
}
