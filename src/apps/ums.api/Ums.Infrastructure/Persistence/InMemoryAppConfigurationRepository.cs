namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Ums.Domain.Configuration;
using Ums.Domain.Kernel;
using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;

public sealed class InMemoryAppConfigurationRepository : IAppConfigurationRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, AppConfigurationAggregate> _store = new();
    public IUnitOfWork UnitOfWork => this;

    public Task<AppConfigurationAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var entity);
        entity?.BrokenRules.Clear();
        return Task.FromResult(entity);
    }

    public Task<AppConfigurationAggregate?> GetByScopeAndCodeAsync(Guid? tenantId, Guid? systemSuiteId, Guid? moduleId, string code, CancellationToken cancellationToken = default)
    {
        var entity = _store.Values.FirstOrDefault(item =>
            string.Equals(item.Props.Code.GetValue(), code, StringComparison.OrdinalIgnoreCase)
            && item.Props.TenantId?.GetValue() == tenantId
            && item.Props.SystemSuiteId?.GetValue() == systemSuiteId
            && item.Props.ModuleId?.GetValue() == moduleId);
        entity?.BrokenRules.Clear();
        return Task.FromResult(entity);
    }

    public Task<IReadOnlyList<AppConfigurationAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = _store.Values.ToList();
        items.ForEach(item => item.BrokenRules.Clear());
        return Task.FromResult<IReadOnlyList<AppConfigurationAggregate>>(items);
    }

    public Task AddAsync(AppConfigurationAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(AppConfigurationAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

    public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    public void Seed(AppConfigurationAggregate aggregate)
    {
        aggregate.DomainEvents.MarkChangesAsCommitted();
        _store[aggregate.Props.Id.GetValue()] = aggregate;
    }

    public void Dispose() { }
}
