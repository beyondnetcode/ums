namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Identity;
using Ums.Domain.Kernel;
using Ums.Domain.Kernel.ValueObjects;
using UserAccountAggregate = Ums.Domain.Identity.UserAccount.UserAccount;

public sealed class InMemoryUserAccountRepository : IUserAccountRepository, IUnitOfWork
{
    private readonly ConcurrentDictionary<Guid, UserAccountAggregate> _store = new();
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public InMemoryUserAccountRepository(IHttpContextAccessor? httpContextAccessor = null)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // REC-05: Tenant filter for dev/test isolation
    private Guid? CurrentTenantId =>
        _httpContextAccessor?.HttpContext?.RequestServices
            .GetService<ITenantContext>()?.OrganizationId;

    public IUnitOfWork UnitOfWork => this;

    public Task<UserAccountAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _store.TryGetValue(id, out var userAccount);
        userAccount?.BrokenRules.Clear();
        return Task.FromResult(userAccount);
    }

    public Task<UserAccountAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public Task<IReadOnlyList<UserAccountAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        // REC-05: filter by tenant when a request context is available
        var tid = CurrentTenantId;
        var all = (tid.HasValue
            ? _store.Values.Where(u => u.Props.TenantId.GetValue() == tid.Value)
            : _store.Values).ToList();
        all.ForEach(u => u.BrokenRules.Clear());
        return Task.FromResult<IReadOnlyList<UserAccountAggregate>>(all);
    }

    public Task<UserAccountAggregate?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var userAccount = _store.Values.FirstOrDefault(u => string.Equals(u.Props.Email.GetValue(), email.GetValue(), StringComparison.Ordinal));
        userAccount?.BrokenRules.Clear();
        return Task.FromResult<UserAccountAggregate?>(userAccount);
    }

    public Task<IReadOnlyList<UserAccountAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var filtered = _store.Values.Where(u => u.Props.TenantId.GetValue() == tenantId).ToList();
        filtered.ForEach(u => u.BrokenRules.Clear());
        return Task.FromResult<IReadOnlyList<UserAccountAggregate>>(filtered);
    }

    public Task AddAsync(UserAccountAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task UpdateAsync(UserAccountAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _store[aggregate.Props.Id.GetValue()] = aggregate;
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) => Task.FromResult(1);

    public Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default) => Task.FromResult(true);

    public void Seed(UserAccountAggregate aggregate)
    {
        aggregate.DomainEvents.MarkChangesAsCommitted();
        _store[aggregate.Props.Id.GetValue()] = aggregate;
    }

    public void Dispose() { }
}
