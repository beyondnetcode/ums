namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Ums.Domain.Approvals;
using Ums.Domain.Kernel;
using ApprovalWorkflowAggregate = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalWorkflow;

public sealed class InMemoryApprovalWorkflowRepository : IApprovalWorkflowRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, ApprovalWorkflowAggregate> _store = new();
    public IUnitOfWork UnitOfWork => this;

    public Task<ApprovalWorkflowAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    { _store.TryGetValue(id, out var e); e?.BrokenRules.Clear(); return Task.FromResult(e); }

    public Task<ApprovalWorkflowAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<ApprovalWorkflowAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    { var all = _store.Values.ToList(); all.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<ApprovalWorkflowAggregate>>(all); }

    public Task<IReadOnlyList<ApprovalWorkflowAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    { var f = _store.Values.Where(e => e.Props.TenantId.GetValue() == tenantId).ToList(); f.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<ApprovalWorkflowAggregate>>(f); }

    public Task AddAsync(ApprovalWorkflowAggregate a, CancellationToken c = default) { _store[a.Props.Id.GetValue()] = a; return Task.CompletedTask; }
    public Task UpdateAsync(ApprovalWorkflowAggregate a, CancellationToken c = default) { _store[a.Props.Id.GetValue()] = a; return Task.CompletedTask; }
    public Task<int> SaveChangesAsync(CancellationToken c = default) => Task.FromResult(1);
    public Task<bool> SaveEntitiesAsync(CancellationToken c = default) => Task.FromResult(true);
    public void Seed(ApprovalWorkflowAggregate a) => _store[a.Props.Id.GetValue()] = a;
    public void Dispose() { }
}
