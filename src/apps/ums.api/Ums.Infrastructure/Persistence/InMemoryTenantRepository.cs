namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Ums.Domain.Identity;
using Ums.Domain.Kernel;
using TenantAggregate = Ums.Domain.Identity.Tenant.Tenant;

public sealed class InMemoryTenantRepository : ITenantRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, TenantAggregate> _store = new();

    public IUnitOfWork UnitOfWork => this;

    public Task<TenantAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var tenant);
        tenant?.BrokenRules.Clear();
        return Task.FromResult(tenant);
    }

    public Task<TenantAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<TenantAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var all = _store.Values.ToList();
        all.ForEach(t => t.BrokenRules.Clear());
        return Task.FromResult<IReadOnlyList<TenantAggregate>>(all);
    }

    public Task<TenantAggregate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var tenant = _store.Values.FirstOrDefault(t => string.Equals(t.Code.GetValue(), code, StringComparison.Ordinal));
        tenant?.BrokenRules.Clear();
        return Task.FromResult<TenantAggregate?>(tenant);
    }

    public Task AddAsync(TenantAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TenantAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

    public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    /// <summary>
    /// Dev-only: stores a tenant aggregate under a specified well-known GUID.
    /// Uses reflection to ensure the aggregate's own Props.Id matches the known key
    /// so that GET /tenants/{id} responses return the expected identifier.
    /// Used by <see cref="DevDataSeeder"/>.
    /// </summary>
    public void SeedWithKnownId(Guid knownId, TenantAggregate aggregate)
    {
        // Use reflection to overwrite the auto-generated Id inside TenantProps
        // so the DTO response matches the known dev seed GUID.
        var propsId = aggregate.Props.Id;
        var valueField = propsId.GetType().BaseType?.BaseType?
            .GetField("_value", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        valueField?.SetValue(propsId, knownId);

        _store[knownId] = aggregate;
    }

    public void Dispose() { }
}
