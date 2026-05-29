namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Ums.Domain.IGA;
using Ums.Domain.Kernel;
using PromotionRequestAggregate = Ums.Domain.IGA.PromotionRequest.PromotionRequest;

public sealed class InMemoryPromotionRequestRepository : IPromotionRequestRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, PromotionRequestAggregate> _store = new();
    public IUnitOfWork UnitOfWork => this;

    public Task<PromotionRequestAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    { _store.TryGetValue(id, out var e); e?.BrokenRules.Clear(); return Task.FromResult(e); }

    public Task<PromotionRequestAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<PromotionRequestAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    { var all = tenantId.HasValue ? _store.Values.Where(e => e.Props.TenantId.GetValue() == tenantId.Value).ToList() : _store.Values.ToList(); all.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<PromotionRequestAggregate>>(all); }

    public Task<IReadOnlyList<PromotionRequestAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    { var f = _store.Values.Where(e => e.Props.TenantId.GetValue() == tenantId).ToList(); f.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<PromotionRequestAggregate>>(f); }

    public Task<IReadOnlyList<PromotionRequestAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    { var f = _store.Values.Where(e => e.Props.UserId.GetValue() == userId).ToList(); f.ForEach(e => e.BrokenRules.Clear()); return Task.FromResult<IReadOnlyList<PromotionRequestAggregate>>(f); }

    public Task AddAsync(PromotionRequestAggregate a, CancellationToken c = default) { _store[a.Props.Id.GetValue()] = a; return Task.CompletedTask; }
    public Task UpdateAsync(PromotionRequestAggregate a, CancellationToken c = default) { _store[a.Props.Id.GetValue()] = a; return Task.CompletedTask; }
    public Task<int> SaveChangesAsync(CancellationToken c = default) => Task.FromResult(1);
    public Task<bool> SaveEntitiesAsync(CancellationToken c = default) => Task.FromResult(true);
    public void Seed(PromotionRequestAggregate a)
    {
        a.DomainEvents.MarkChangesAsCommitted();
        _store[a.Props.Id.GetValue()] = a;
    }
    public void Dispose() { }
}
