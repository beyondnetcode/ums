namespace Ums.Domain.IGA;
using Ums.Domain.IGA.PromotionRequest;
using Ums.Domain.IGA.RoleMaturityStatus;
using PromotionRequestAggregate = Ums.Domain.IGA.PromotionRequest.PromotionRequest;
using RoleMaturityStatusAggregate = Ums.Domain.IGA.RoleMaturityStatus.RoleMaturityStatus;

public interface IPromotionRequestRepository : IAggregateRepository<PromotionRequestAggregate>
{
    Task<PromotionRequestAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PromotionRequestAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PromotionRequestAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PromotionRequestAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IRoleMaturityStatusRepository : IAggregateRepository<RoleMaturityStatusAggregate>
{
    Task<RoleMaturityStatusAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleMaturityStatusAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleMaturityStatusAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<RoleMaturityStatusAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
