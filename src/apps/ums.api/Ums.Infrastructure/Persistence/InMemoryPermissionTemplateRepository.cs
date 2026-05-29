namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Ums.Domain.Authorization;
using Ums.Domain.Kernel;
using PermissionTemplateAggregate = Ums.Domain.Authorization.Template.PermissionTemplate;

public sealed class InMemoryPermissionTemplateRepository : IPermissionTemplateRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, PermissionTemplateAggregate> _store = new();

    public IUnitOfWork UnitOfWork => this;

    public Task<PermissionTemplateAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var template);
        template?.BrokenRules.Clear();
        return Task.FromResult(template);
    }

    public Task<PermissionTemplateAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<PermissionTemplateAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var all = tenantId.HasValue
            ? _store.Values.Where(t => t.Props.TenantId.GetValue() == tenantId.Value).ToList()
            : _store.Values.ToList();
        all.ForEach(t => t.BrokenRules.Clear());
        return Task.FromResult<IReadOnlyList<PermissionTemplateAggregate>>(all);
    }

    public Task<IReadOnlyList<PermissionTemplateAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var filtered = _store.Values.Where(t => t.Props.TenantId.GetValue() == tenantId).ToList();
        filtered.ForEach(t => t.BrokenRules.Clear());
        return Task.FromResult<IReadOnlyList<PermissionTemplateAggregate>>(filtered);
    }

    public Task AddAsync(PermissionTemplateAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(PermissionTemplateAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var removed = _store.TryRemove(id, out _);
        return Task.FromResult(removed);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);
    public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    public void Seed(PermissionTemplateAggregate aggregate)
    {
        aggregate.DomainEvents.MarkChangesAsCommitted();
        _store[aggregate.Props.Id.GetValue()] = aggregate;
    }
    public void Dispose() { }
}
