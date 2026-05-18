namespace Ums.Domain.Kernel;

public interface IAggregateRepository<TAggregate>
    where TAggregate : class
{
    Task<TAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
    Task UpdateAsync(TAggregate aggregate, CancellationToken cancellationToken = default);
    Task<TAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default);
}
