namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Identity;
using Ums.Domain.Kernel;
using TenantAggregate = Ums.Domain.Identity.Tenant.Tenant;

public sealed class InMemoryTenantRepository : ITenantRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, TenantAggregate> _store = new();
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public InMemoryTenantRepository(IHttpContextAccessor? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // REC-05: Returns OrganizationId from the current HTTP request scope, or null (no filter) if none.
    private Guid? CurrentTenantId =>
        _httpContextAccessor?.HttpContext?.RequestServices
            .GetService<ITenantContext>()?.OrganizationId;

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
        // REC-05: Tenants are the roots themselves; no per-tenant filter needed.
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
    /// Dev-only seed helper. The aggregate must already carry the desired deterministic Id.
    /// </summary>
    public void Seed(TenantAggregate aggregate)
    {
        aggregate.DomainEvents.MarkChangesAsCommitted();
        _store[aggregate.Props.Id.GetValue()] = aggregate;
    }

    public void Dispose() { }
}
