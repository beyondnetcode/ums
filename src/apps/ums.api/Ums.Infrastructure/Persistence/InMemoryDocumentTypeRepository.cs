namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Ums.Domain.Approvals;
using Ums.Domain.Kernel;
using DocumentTypeAggregate = Ums.Domain.Approvals.DocumentType.DocumentType;

public sealed class InMemoryDocumentTypeRepository : IDocumentTypeRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, DocumentTypeAggregate> _store = new();
    public IUnitOfWork UnitOfWork => this;

    public Task<DocumentTypeAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    { _store.TryGetValue(id, out var e); e?.BrokenRules.Clear(); return Task.FromResult(e); }

    public Task<DocumentTypeAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<DocumentTypeAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    { var all = _store.Values.ToList(); all.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<DocumentTypeAggregate>>(all); }

    public Task<IReadOnlyList<DocumentTypeAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    { var f = _store.Values.Where(e => e.Props.TenantId.GetValue() == tenantId).ToList(); f.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<DocumentTypeAggregate>>(f); }

    public Task AddAsync(DocumentTypeAggregate a, CancellationToken c = default) { _store[a.Props.Id.GetValue()] = a; return Task.CompletedTask; }
    public Task UpdateAsync(DocumentTypeAggregate a, CancellationToken c = default) { _store[a.Props.Id.GetValue()] = a; return Task.CompletedTask; }
    public Task<int> SaveChangesAsync(CancellationToken c = default) => Task.FromResult(1);
    public Task<bool> SaveEntitiesAsync(CancellationToken c = default) => Task.FromResult(true);
    public void Seed(DocumentTypeAggregate a)
    {
        a.DomainEvents.MarkChangesAsCommitted();
        _store[a.Props.Id.GetValue()] = a;
    }
    public void Dispose() { }
}
