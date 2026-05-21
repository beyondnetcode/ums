namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Ums.Domain.Approvals;
using Ums.Domain.Kernel;
using ApprovalRequestAggregate = Ums.Domain.Approvals.ApprovalRequest.ApprovalRequest;

public sealed class InMemoryApprovalRequestRepository : IApprovalRequestRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, ApprovalRequestAggregate> _store = new();
    public IUnitOfWork UnitOfWork => this;

    public Task<ApprovalRequestAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    { _store.TryGetValue(id, out var e); e?.BrokenRules.Clear(); return Task.FromResult(e); }

    public Task<ApprovalRequestAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<ApprovalRequestAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    { var all = _store.Values.ToList(); all.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<ApprovalRequestAggregate>>(all); }

    public Task<IReadOnlyList<ApprovalRequestAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    { var f = _store.Values.ToList(); f.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<ApprovalRequestAggregate>>(f); }

    public Task AddAsync(ApprovalRequestAggregate a, CancellationToken c = default) { _store[a.Props.Id.GetValue()] = a; return Task.CompletedTask; }
    public Task UpdateAsync(ApprovalRequestAggregate a, CancellationToken c = default) { _store[a.Props.Id.GetValue()] = a; return Task.CompletedTask; }
    public Task<int> SaveChangesAsync(CancellationToken c = default) => Task.FromResult(1);
    public Task<bool> SaveEntitiesAsync(CancellationToken c = default) => Task.FromResult(true);
    public void Seed(ApprovalRequestAggregate a) => _store[a.Props.Id.GetValue()] = a;
    public void Dispose() { }
}
