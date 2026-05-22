namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Ums.Domain.Approvals;
using Ums.Domain.Kernel;
using AccessEnforcementPolicyAggregate = Ums.Domain.Approvals.AccessEnforcementPolicy.AccessEnforcementPolicy;

public sealed class InMemoryAccessEnforcementPolicyRepository : IAccessEnforcementPolicyRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, AccessEnforcementPolicyAggregate> _store = new();
    public IUnitOfWork UnitOfWork => this;

    public Task<AccessEnforcementPolicyAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    { _store.TryGetValue(id, out var e); e?.BrokenRules.Clear(); return Task.FromResult(e); }

    public Task<AccessEnforcementPolicyAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<AccessEnforcementPolicyAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    { var all = _store.Values.ToList(); all.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<AccessEnforcementPolicyAggregate>>(all); }

    public Task<IReadOnlyList<AccessEnforcementPolicyAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    { var f = _store.Values.Where(e => e.Props.TenantId.GetValue() == tenantId).ToList(); f.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<AccessEnforcementPolicyAggregate>>(f); }

    public Task AddAsync(AccessEnforcementPolicyAggregate a, CancellationToken c = default) { _store[a.Props.Id.GetValue()] = a; return Task.CompletedTask; }
    public Task UpdateAsync(AccessEnforcementPolicyAggregate a, CancellationToken c = default) { _store[a.Props.Id.GetValue()] = a; return Task.CompletedTask; }
    public Task<int> SaveChangesAsync(CancellationToken c = default) => Task.FromResult(1);
    public Task<bool> SaveEntitiesAsync(CancellationToken c = default) => Task.FromResult(true);
    public void Seed(AccessEnforcementPolicyAggregate a)
    {
        a.DomainEvents.MarkChangesAsCommitted();
        _store[a.Props.Id.GetValue()] = a;
    }
    public void Dispose() { }
}
