namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Ums.Domain.Authorization;
using Ums.Domain.Kernel;
using Ums.Domain.Kernel.ValueObjects;
using SystemSuiteAggregate = Ums.Domain.Authorization.SystemSuite.SystemSuite;

public sealed class InMemorySystemSuiteRepository : ISystemSuiteRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, SystemSuiteAggregate> _store = new();

    public IUnitOfWork UnitOfWork => this;

    public Task<SystemSuiteAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var systemSuite);
        systemSuite?.BrokenRules.Clear();
        return Task.FromResult(systemSuite);
    }

    public Task<SystemSuiteAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<SystemSuiteAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var all = _store.Values.ToList();
        all.ForEach(s => s.BrokenRules.Clear());
        return Task.FromResult<IReadOnlyList<SystemSuiteAggregate>>(all);
    }

    public Task<SystemSuiteAggregate?> GetByCodeAsync(Code code, CancellationToken cancellationToken = default)
    {
        var systemSuite = _store.Values.FirstOrDefault(s => string.Equals(s.Props.Code.GetValue(), code.GetValue(), StringComparison.Ordinal));
        systemSuite?.BrokenRules.Clear();
        return Task.FromResult<SystemSuiteAggregate?>(systemSuite);
    }

    public Task<IReadOnlyList<SystemSuiteAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var filtered = _store.Values.Where(s => s.Props.TenantId.GetValue() == tenantId).ToList();
        filtered.ForEach(s => s.BrokenRules.Clear());
        return Task.FromResult<IReadOnlyList<SystemSuiteAggregate>>(filtered);
    }

    public Task AddAsync(SystemSuiteAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(SystemSuiteAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

    public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    public void Seed(SystemSuiteAggregate aggregate)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
    }

    public void Dispose() { }
}
