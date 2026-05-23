namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Ums.Domain.Configuration;
using Ums.Domain.Kernel;
using FeatureFlagAggregate = Ums.Domain.Configuration.FeatureFlag.FeatureFlag;

public sealed class InMemoryFeatureFlagRepository : IFeatureFlagRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, FeatureFlagAggregate> _store = new();
    public IUnitOfWork UnitOfWork => this;

    public Task<FeatureFlagAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var entity);
        entity?.BrokenRules.Clear();
        return Task.FromResult(entity);
    }

    public Task<FeatureFlagAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<FeatureFlagAggregate?> GetByCodeAsync(string flagCode, CancellationToken cancellationToken = default)
    {
        var entity = _store.Values.FirstOrDefault(item =>
            string.Equals(item.Props.FlagCode, flagCode, StringComparison.OrdinalIgnoreCase));
        entity?.BrokenRules.Clear();
        return Task.FromResult(entity);
    }

    public Task<IReadOnlyList<FeatureFlagAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = _store.Values.ToList();
        items.ForEach(item => item.BrokenRules.Clear());
        return Task.FromResult<IReadOnlyList<FeatureFlagAggregate>>(items);
    }

    public Task AddAsync(FeatureFlagAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(FeatureFlagAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

    public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    public void Seed(FeatureFlagAggregate aggregate)
    {
        aggregate.DomainEvents.MarkChangesAsCommitted();
        _store[aggregate.Props.Id.GetValue()] = aggregate;
    }

    public void Dispose() { }
}
