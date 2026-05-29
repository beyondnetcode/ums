namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Ums.Domain.IGA;
using Ums.Domain.Kernel;
using RoleMaturityStatusAggregate = Ums.Domain.IGA.RoleMaturityStatus.RoleMaturityStatus;

public sealed class InMemoryRoleMaturityStatusRepository : IRoleMaturityStatusRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, RoleMaturityStatusAggregate> _store = new();
    public IUnitOfWork UnitOfWork => this;

    public Task<RoleMaturityStatusAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    { _store.TryGetValue(id, out var e); e?.BrokenRules.Clear(); return Task.FromResult(e); }

    public Task<RoleMaturityStatusAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<RoleMaturityStatusAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    { var all = tenantId.HasValue ? _store.Values.Where(e => e.Props.TenantId.GetValue() == tenantId.Value).ToList() : _store.Values.ToList(); all.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<RoleMaturityStatusAggregate>>(all); }

    public Task<IReadOnlyList<RoleMaturityStatusAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    { var f = _store.Values.Where(e => e.Props.TenantId.GetValue() == tenantId).ToList(); f.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<RoleMaturityStatusAggregate>>(f); }

    public Task<IReadOnlyList<RoleMaturityStatusAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    { var f = _store.Values.Where(e => e.Props.UserId.GetValue() == userId).ToList(); f.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<RoleMaturityStatusAggregate>>(f); }

    public Task AddAsync(RoleMaturityStatusAggregate a, CancellationToken c = default) { _store[a.Props.Id.GetValue()] = a; return Task.CompletedTask; }
    public Task UpdateAsync(RoleMaturityStatusAggregate a, CancellationToken c = default) { _store[a.Props.Id.GetValue()] = a; return Task.CompletedTask; }
    public Task<int> SaveChangesAsync(CancellationToken c = default) => Task.FromResult(1);
    public Task<bool> SaveEntitiesAsync(CancellationToken c = default) => Task.FromResult(true);
    public void Seed(RoleMaturityStatusAggregate a)
    {
        a.DomainEvents.MarkChangesAsCommitted();
        _store[a.Props.Id.GetValue()] = a;
    }
    public void Dispose() { }
}
