namespace Ums.Application.Abstractions.Persistence;

using Ums.Domain.Identity.Tenant;
using Ums.Domain.Kernel.ValueObjects;

public interface ITenantRepository
{
    Task<Tenant?> FindByIdAsync(TenantId tenantId, CancellationToken cancellationToken = default);
    Task<Tenant?> FindByCodeAsync(Code code, CancellationToken cancellationToken = default);
    Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default);
    Task UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default);
}
