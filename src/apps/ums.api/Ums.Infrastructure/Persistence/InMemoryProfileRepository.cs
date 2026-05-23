namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Authorization;
using Ums.Domain.Kernel;
using ProfileAggregate = Ums.Domain.Authorization.Profile.Profile;

public sealed class InMemoryProfileRepository : IProfileRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, ProfileAggregate> _store = new();
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public InMemoryProfileRepository(IHttpContextAccessor? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // REC-05: Tenant filter for dev/test isolation
    private Guid? CurrentTenantId =>
        _httpContextAccessor?.HttpContext?.RequestServices
            .GetService<ITenantContext>()?.OrganizationId;

    public IUnitOfWork UnitOfWork => this;

    public Task<ProfileAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var profile);
        profile?.BrokenRules.Clear();
        return Task.FromResult(profile);
    }

    public Task<ProfileAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<ProfileAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // REC-05: filter by tenant when a request context is available
        var tid = CurrentTenantId;
        var all = (tid.HasValue
            ? _store.Values.Where(p => p.Props.TenantId.GetValue() == tid.Value)
            : _store.Values).ToList();
        all.ForEach(p => p.BrokenRules.Clear());
        return Task.FromResult<IReadOnlyList<ProfileAggregate>>(all);
    }

    public Task<IReadOnlyList<ProfileAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var filtered = _store.Values.Where(p => p.Props.TenantId.GetValue() == tenantId).ToList();
        filtered.ForEach(p => p.BrokenRules.Clear());
        return Task.FromResult<IReadOnlyList<ProfileAggregate>>(filtered);
    }

    public Task<IReadOnlyList<ProfileAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var filtered = _store.Values.Where(p => p.Props.UserId.GetValue() == userId).ToList();
        filtered.ForEach(p => p.BrokenRules.Clear());
        return Task.FromResult<IReadOnlyList<ProfileAggregate>>(filtered);
    }

    public Task AddAsync(ProfileAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ProfileAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

    public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    public void Seed(ProfileAggregate aggregate)
    {
        aggregate.DomainEvents.MarkChangesAsCommitted();
        _store[aggregate.Props.Id.GetValue()] = aggregate;
    }

    public void Dispose() { }
}
