namespace Ums.Domain.Identity;

using Ums.Domain.Identity.TenantSignupRequest;
using TenantSignupRequestAggregate = Ums.Domain.Identity.TenantSignupRequest.TenantSignupRequest;

public interface ITenantSignupRequestRepository
{
    Ums.Domain.Kernel.IUnitOfWork UnitOfWork { get; }
    Task<TenantSignupRequestAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantSignupRequestAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TenantSignupRequestAggregate>> GetPendingAsync(CancellationToken cancellationToken = default);
    Task AddAsync(TenantSignupRequestAggregate aggregate, CancellationToken cancellationToken = default);
    Task UpdateAsync(TenantSignupRequestAggregate aggregate, CancellationToken cancellationToken = default);
}
