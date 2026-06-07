using Microsoft.EntityFrameworkCore;
using Ums.Domain.Identity.Repositories.TenantParameter;
using Ums.Domain.Identity.Tenant.TenantParameter;
using Ums.Domain.Kernel;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Identity.Entities;
using Ums.Infrastructure.Persistence.Reflection;

namespace Ums.Infrastructure.Persistence.Identity.TenantParameter;

using TenantParameterAggregate = Ums.Domain.Identity.Tenant.TenantParameter.TenantParameter;

public sealed class PostgreSqlTenantParameterRepository : ITenantParameterRepository, IUnitOfWork
{
    private readonly UmsPlatformDbContext dbContext;
    private readonly HashSet<TenantParameterAggregate> _trackedAggregates = [];

    public PostgreSqlTenantParameterRepository(UmsPlatformDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public IUnitOfWork UnitOfWork => this;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var aggregate in _trackedAggregates)
        {
            await dbContext.PublishDomainEventsAsync(aggregate.DomainEvents.GetUncommittedChanges(), cancellationToken);
        }

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            var entry = ex.Entries.FirstOrDefault();
            var id = (Guid)(entry?.Property("Id").CurrentValue ?? Guid.Empty);
            throw new ConcurrencyConflictException(entry?.Metadata.Name ?? "Unknown", id);
        }

        foreach (var aggregate in _trackedAggregates)
        {
            aggregate.DomainEvents.MarkChangesAsCommitted();
        }

        _trackedAggregates.Clear();
        return true;
    }

    public void Dispose()
    {
    }

    public async Task<TenantParameterAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.TenantParameters
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    Task<TenantParameterAggregate?> IAggregateRepository<TenantParameterAggregate>.GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken)
    {
        return GetByIdAsync(tenantId, id, cancellationToken);
    }

    public async Task<TenantParameterAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.TenantParameters
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public async Task<TenantParameterAggregate?> GetByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.TenantParameters
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Code == code, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public async Task<IReadOnlyList<TenantParameterAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.TenantParameters
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.CategoryId)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public async Task<IReadOnlyList<TenantParameterAggregate>> GetActiveByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.TenantParameters
            .Where(x => x.TenantId == tenantId && x.IsActive)
            .OrderBy(x => x.CategoryId)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public async Task<IReadOnlyList<TenantParameterAggregate>> GetByCategoryAsync(Guid tenantId, string category, CancellationToken cancellationToken = default)
    {
        var categoryEnum = TenantParameterCategory.TryFromString(category);
        if (categoryEnum is null)
        {
            return [];
        }

        var records = await dbContext.TenantParameters
            .Where(x => x.TenantId == tenantId && x.IsActive && x.CategoryId == categoryEnum.Id)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public async Task<bool> ExistsActiveCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default)
    {
        return await dbContext.TenantParameters
            .AnyAsync(x => x.TenantId == tenantId && x.Code == code && x.IsActive, cancellationToken);
    }

    public Task AddAsync(TenantParameterAggregate aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.TenantParameters.Add(ToRecord(aggregate));
        _trackedAggregates.Add(aggregate);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(TenantParameterAggregate aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.TenantParameters.Update(ToRecord(aggregate));
        _trackedAggregates.Add(aggregate);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(TenantParameterAggregate aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.TenantParameters.Remove(ToRecord(aggregate));
        _trackedAggregates.Remove(aggregate);
        return Task.CompletedTask;
    }

    private static TenantParameterAggregate Rehydrate(TenantParameterRecord record)
        => IdentityAggregateFactory.RehydrateTenantParameter(record);

    private static TenantParameterRecord ToRecord(TenantParameterAggregate aggregate)
    {
        return new TenantParameterRecord
        {
            Id = aggregate.GetId().GetValue(),
            TenantId = aggregate.TenantId.GetValue(),
            Code = aggregate.Code.GetValue(),
            Description = aggregate.Description.GetValue(),
            Value = aggregate.Value,
            ValueTypeId = aggregate.ValueType.Id,
            CategoryId = aggregate.Category.Id,
            IsActive = aggregate.IsActive,
            IsSensitive = aggregate.IsSensitive,
            DefaultValue = aggregate.DefaultValue,
            AllowedValues = aggregate.AllowedValues,
            CreatedBy = aggregate.Props.Audit.GetValue().CreatedBy,
            CreatedAtUtc = aggregate.Props.Audit.GetValue().CreatedAt,
            UpdatedBy = aggregate.Props.Audit.GetValue().UpdatedBy,
            UpdatedAtUtc = aggregate.Props.Audit.GetValue().UpdatedAt,
            AuditTimeSpan = aggregate.Props.Audit.GetValue().TimeSpan
        };
    }
}