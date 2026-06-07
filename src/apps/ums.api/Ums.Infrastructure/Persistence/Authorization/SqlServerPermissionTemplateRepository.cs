using Microsoft.EntityFrameworkCore;
using Ums.Domain.Authorization;
using Ums.Domain.Kernel;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Authorization.Entities;
using Ums.Infrastructure.Persistence.Reflection;

namespace Ums.Infrastructure.Persistence.Authorization;

using PermissionTemplateAggregate = Ums.Domain.Authorization.Template.PermissionTemplate;

public sealed class SqlServerPermissionTemplateRepository(UmsPlatformDbContext dbContext) : IPermissionTemplateRepository, IUnitOfWork
{
    private readonly HashSet<PermissionTemplateAggregate> _trackedAggregates = [];

    public IUnitOfWork UnitOfWork => this;

    public async Task<PermissionTemplateAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.PermissionTemplates
            .AsSplitQuery()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public async Task<PermissionTemplateAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.PermissionTemplates
            .AsSplitQuery()
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public async Task<IReadOnlyList<PermissionTemplateAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        IQueryable<PermissionTemplateRecord> query = dbContext.PermissionTemplates.AsSplitQuery().Include(x => x.Items);

        if (tenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == tenantId.Value);
        }

        var records = await query.OrderBy(x => x.RoleId).ThenBy(x => x.Version).ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public async Task<IReadOnlyList<PermissionTemplateAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.PermissionTemplates
            .AsSplitQuery()
            .Include(x => x.Items)
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.RoleId)
            .ThenBy(x => x.Version)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public Task AddAsync(PermissionTemplateAggregate aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.PermissionTemplates.Add(ToRecord(aggregate));
        _trackedAggregates.Add(aggregate);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(PermissionTemplateAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.PermissionTemplates
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == aggregate.Props.Id.GetValue(), cancellationToken)
            ?? throw new InvalidOperationException($"Permission template {aggregate.Props.Id.GetValue()} does not exist.");

        Apply(existing, aggregate);
        _trackedAggregates.Add(aggregate);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);

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
        catch (DbUpdateConcurrencyException ex)
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

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.PermissionTemplates
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (record is null) return false;

        dbContext.PermissionTemplates.Remove(record);
        return true;
    }

    // ── Dependency guard queries ────────────────────────────────────────────

    public Task<int> CountPublishedByRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
        => dbContext.PermissionTemplates.CountAsync(
            t => t.RoleId == roleId && t.StatusId == 2 /* Published */,
            cancellationToken);

    public Task<int> CountItemsByTargetAsync(Guid targetId, CancellationToken cancellationToken = default)
        => dbContext.PermissionTemplateItems.CountAsync(
            i => i.TargetId == targetId && i.IsActive,
            cancellationToken);

    public void Dispose() => dbContext.Dispose();

    private static PermissionTemplateAggregate Rehydrate(PermissionTemplateRecord record)
        => AuthorizationAggregateFactory.RehydratePermissionTemplate(record, record.Items);

    private static PermissionTemplateRecord ToRecord(PermissionTemplateAggregate aggregate)
    {
        var audit = aggregate.Props.Audit.GetValue();
        return new PermissionTemplateRecord
        {
            Id = aggregate.Props.Id.GetValue(),
            TenantId = aggregate.Props.TenantId.GetValue(),
            RoleId = aggregate.Props.RoleId.GetValue(),
            SystemSuiteId = aggregate.Props.SystemSuiteId.GetValue(),
            Version = aggregate.Props.Version.GetValue(),
            StatusId = aggregate.Props.Status.Id,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
            Items = aggregate.Items.Select(x =>
            {
                var a = x.Props.Audit.GetValue();
                return new PermissionTemplateItemRecord
                {
                    Id = x.Props.Id.GetValue(),
                    TemplateId = x.Props.TemplateId.GetValue(),
                    TargetTypeId = x.Props.TargetType.Id,
                    TargetId = x.Props.TargetId.GetValue(),
                    ActionId = x.Props.ActionId.GetValue(),
                    IsAllowed = x.Props.IsAllowed,
                    IsDenied = x.Props.IsDenied,
                    IsActive = x.Props.IsActive,
                    CreatedBy = a.CreatedBy,
                    CreatedAtUtc = a.CreatedAt,
                    UpdatedBy = a.UpdatedBy,
                    UpdatedAtUtc = a.UpdatedAt,
                    AuditTimeSpan = a.TimeSpan,
                };
            }).ToList(),
        };
    }

    private void Apply(PermissionTemplateRecord target, PermissionTemplateAggregate source)
    {
        var replacement = ToRecord(source);

        target.TenantId = replacement.TenantId;
        target.RoleId = replacement.RoleId;
        target.SystemSuiteId = replacement.SystemSuiteId;
        target.Version = replacement.Version;
        target.StatusId = replacement.StatusId;
        target.CreatedBy = replacement.CreatedBy;
        target.CreatedAtUtc = replacement.CreatedAtUtc;
        target.UpdatedBy = replacement.UpdatedBy;
        target.UpdatedAtUtc = replacement.UpdatedAtUtc;
        target.AuditTimeSpan = replacement.AuditTimeSpan;

        EfChildCollectionReconciler.ReconcileById(
            dbContext,
            target.Items,
            replacement.Items,
            item => item.Id,
            UpdateItem);
    }

    private static void UpdateItem(PermissionTemplateItemRecord target, PermissionTemplateItemRecord source)
    {
        target.TemplateId = source.TemplateId;
        target.TargetTypeId = source.TargetTypeId;
        target.TargetId = source.TargetId;
        target.ActionId = source.ActionId;
        target.IsAllowed = source.IsAllowed;
        target.IsDenied = source.IsDenied;
        target.IsActive = source.IsActive;
        target.CreatedBy = source.CreatedBy;
        target.CreatedAtUtc = source.CreatedAtUtc;
        target.UpdatedBy = source.UpdatedBy;
        target.UpdatedAtUtc = source.UpdatedAtUtc;
        target.AuditTimeSpan = source.AuditTimeSpan;
    }
}
