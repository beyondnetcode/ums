using Microsoft.EntityFrameworkCore;
using Ums.Domain.Authorization;
using Ums.Domain.Kernel;
using Ums.Infrastructure.Persistence.Authorization.Entities;
using Ums.Infrastructure.Persistence.Reflection;

namespace Ums.Infrastructure.Persistence.Authorization;

using SystemSuiteAggregate = Ums.Domain.Authorization.SystemSuite.SystemSuite;

public sealed class PostgreSqlSystemSuiteRepository(UmsPlatformDbContext dbContext) : ISystemSuiteRepository, IUnitOfWork
{
    private readonly HashSet<SystemSuiteAggregate> _trackedAggregates = [];

    public IUnitOfWork UnitOfWork => this;

    public async Task<SystemSuiteAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.SystemSuites
            .AsSplitQuery()
            .Include(x => x.Modules).ThenInclude(x => x.Menus).ThenInclude(x => x.SubMenus).ThenInclude(x => x.Options)
            .Include(x => x.AppSettings)
            .Include(x => x.Actions)
            .Include(x => x.DomainResources)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public async Task<SystemSuiteAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.SystemSuites
            .AsSplitQuery()
            .Include(x => x.Modules).ThenInclude(x => x.Menus).ThenInclude(x => x.SubMenus).ThenInclude(x => x.Options)
            .Include(x => x.AppSettings)
            .Include(x => x.Actions)
            .Include(x => x.DomainResources)
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Id == id, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public async Task<SystemSuiteAggregate?> GetByCodeAsync(Code code, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.SystemSuites
            .AsSplitQuery()
            .Include(x => x.Modules).ThenInclude(x => x.Menus).ThenInclude(x => x.SubMenus).ThenInclude(x => x.Options)
            .Include(x => x.AppSettings)
            .Include(x => x.Actions)
            .Include(x => x.DomainResources)
            .FirstOrDefaultAsync(x => x.Code == code.GetValue(), cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public async Task<IReadOnlyList<SystemSuiteAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        IQueryable<SystemSuiteRecord> query = dbContext.SystemSuites.AsSplitQuery()
            .Include(x => x.Modules).ThenInclude(x => x.Menus).ThenInclude(x => x.SubMenus).ThenInclude(x => x.Options)
            .Include(x => x.AppSettings)
            .Include(x => x.Actions)
            .Include(x => x.DomainResources);

        if (tenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == tenantId.Value);
        }

        var records = await query.OrderBy(x => x.Code).ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public async Task<IReadOnlyList<SystemSuiteAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.SystemSuites
            .AsSplitQuery()
            .Include(x => x.Modules).ThenInclude(x => x.Menus).ThenInclude(x => x.SubMenus).ThenInclude(x => x.Options)
            .Include(x => x.AppSettings)
            .Include(x => x.Actions)
            .Include(x => x.DomainResources)
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public Task AddAsync(SystemSuiteAggregate aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.SystemSuites.Add(ToRecord(aggregate));
        _trackedAggregates.Add(aggregate);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(SystemSuiteAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var id = aggregate.Props.Id.GetValue();

        // Prefer the already-tracked entity loaded by GetByIdAsync in the same request scope.
        // Issuing a second query returns the same instances via EF Core's identity map, but
        // the Clear()+Add(same-PK) pattern in Apply then creates a Deleted↔Added conflict in
        // the change tracker that causes a false DbUpdateConcurrencyException.
        var existing =
            dbContext.ChangeTracker
                .Entries<SystemSuiteRecord>()
                .FirstOrDefault(e => e.Entity.Id == id)
                ?.Entity
            ?? await dbContext.SystemSuites
                .Include(x => x.Modules).ThenInclude(x => x.Menus).ThenInclude(x => x.SubMenus).ThenInclude(x => x.Options)
                .Include(x => x.AppSettings)
                .Include(x => x.Actions)
                .Include(x => x.DomainResources)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new InvalidOperationException($"System suite {id} does not exist.");

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

    public void Dispose() => dbContext.Dispose();

    private static SystemSuiteAggregate Rehydrate(SystemSuiteRecord record)
        => AuthorizationAggregateFactory.RehydrateSystemSuite(record, record.Modules, record.AppSettings, record.Actions, record.DomainResources);

    private static SystemSuiteRecord ToRecord(SystemSuiteAggregate aggregate)
    {
        var audit = aggregate.Props.Audit.GetValue();
        return new SystemSuiteRecord
        {
            Id = aggregate.Props.Id.GetValue(),
            TenantId = aggregate.Props.TenantId.GetValue(),
            Code = aggregate.Props.Code.GetValue(),
            Name = aggregate.Props.Name.GetValue(),
            Description = aggregate.Props.Description.GetValue(),
            StatusId = aggregate.Props.Status.Id,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
            Modules = aggregate.Modules.Select(ToRecord).ToList(),
            AppSettings = aggregate.AppSettings.Select(x => new SystemSuiteAppSettingRecord
            {
                Id = Guid.NewGuid(),
                SystemSuiteId = aggregate.Props.Id.GetValue(),
                ConfigKey = x.Key.GetValue(),
                ConfigValue = x.Value.GetValue(),
                ScopeId = x.Scope.Id,
            }).ToList(),
            Actions = aggregate.Actions.Select(x =>
            {
                var a = x.Props.Audit.GetValue();
                return new SystemSuiteActionRecord
                {
                    Id = x.Props.Id.GetValue(),
                    TenantId = x.Props.TenantId.GetValue(),
                    SystemSuiteId = x.Props.SystemSuiteId!.GetValue(),
                    ModuleId = x.Props.ModuleId?.GetValue(),
                    Code = x.Props.Code.GetValue(),
                    Name = x.Props.Name.GetValue(),
                    CreatedBy = a.CreatedBy,
                    CreatedAtUtc = a.CreatedAt,
                    UpdatedBy = a.UpdatedBy,
                    UpdatedAtUtc = a.UpdatedAt,
                    AuditTimeSpan = a.TimeSpan,
                };
            }).ToList(),
            DomainResources = aggregate.DomainResources.Select(x =>
            {
                var a = x.Props.Audit.GetValue();
                return new SystemSuiteDomainResourceRecord
                {
                    Id = x.Props.Id.GetValue(),
                    SystemSuiteId = x.Props.SystemSuiteId.GetValue(),
                    ModuleId = x.Props.ModuleId?.GetValue(),
                    ParentResourceId = x.Props.ParentResourceId?.GetValue(),
                    Type = x.Props.Type.Name,
                    Code = x.Props.Code.GetValue(),
                    Name = x.Props.Name.GetValue(),
                    Description = x.Props.Description.GetValue(),
                    CreatedBy = a.CreatedBy,
                    CreatedAtUtc = a.CreatedAt,
                    UpdatedBy = a.UpdatedBy,
                    UpdatedAtUtc = a.UpdatedAt,
                    AuditTimeSpan = a.TimeSpan,
                };
            }).ToList(),
        };
    }

    private static SystemSuiteModuleRecord ToRecord(Ums.Domain.Authorization.SystemSuite.Module.Module module)
    {
        var audit = module.Props.Audit.GetValue();
        return new SystemSuiteModuleRecord
        {
            Id = module.Props.Id.GetValue(),
            SystemSuiteId = module.Props.SystemId.GetValue(),
            Code = module.Props.Code.GetValue(),
            Name = module.Props.Name.GetValue(),
            Description = module.Props.Description.GetValue(),
            StatusId = module.Props.Status.Id,
            SortOrder = module.Props.SortOrder,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
            Menus = module.Menus.Select(ToRecord).ToList(),
        };
    }

    private static SystemSuiteMenuRecord ToRecord(Ums.Domain.Authorization.SystemSuite.Menu.Menu menu)
    {
        var audit = menu.Props.Audit.GetValue();
        return new SystemSuiteMenuRecord
        {
            Id = menu.Props.Id.GetValue(),
            ModuleId = menu.Props.ModuleId.GetValue(),
            Code = menu.Props.Code.GetValue(),
            Label = menu.Props.Label.GetValue(),
            Description = menu.Props.Description.GetValue(),
            SortOrder = menu.Props.SortOrder,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
            SubMenus = menu.SubMenus.Select(ToRecord).ToList(),
        };
    }

    private static SystemSuiteSubMenuRecord ToRecord(Ums.Domain.Authorization.SystemSuite.SubMenu.SubMenu subMenu)
    {
        var audit = subMenu.Props.Audit.GetValue();
        return new SystemSuiteSubMenuRecord
        {
            Id = subMenu.Props.Id.GetValue(),
            MenuId = subMenu.Props.MenuId.GetValue(),
            Code = subMenu.Props.Code.GetValue(),
            Label = subMenu.Props.Label.GetValue(),
            Description = subMenu.Props.Description.GetValue(),
            SortOrder = subMenu.Props.SortOrder,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
            Options = subMenu.Options.Select(ToRecord).ToList(),
        };
    }

    private static SystemSuiteOptionRecord ToRecord(Ums.Domain.Authorization.SystemSuite.Option.Option option)
    {
        var audit = option.Props.Audit.GetValue();
        return new SystemSuiteOptionRecord
        {
            Id = option.Props.Id.GetValue(),
            SubMenuId = option.Props.SubMenuId.GetValue(),
            Code = option.Props.Code.GetValue(),
            Label = option.Props.Label.GetValue(),
            Description = option.Props.Description.GetValue(),
            ActionCode = option.Props.ActionCode.GetValue(),
            SortOrder = option.Props.SortOrder,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
        };
    }

    private void Apply(SystemSuiteRecord target, SystemSuiteAggregate source)
    {
        var replacement = ToRecord(source);

        // ── Scalar properties ────────────────────────────────────────────────
        target.TenantId    = replacement.TenantId;
        target.Code        = replacement.Code;
        target.Name        = replacement.Name;
        target.Description = replacement.Description;
        target.StatusId    = replacement.StatusId;
        target.CreatedBy   = replacement.CreatedBy;
        target.CreatedAtUtc = replacement.CreatedAtUtc;
        target.UpdatedBy   = replacement.UpdatedBy;
        target.UpdatedAtUtc = replacement.UpdatedAtUtc;
        target.AuditTimeSpan = replacement.AuditTimeSpan;

        // ── Modules ───────────────────────────────────────────────────────────
        ReconcileModules(target.Modules, replacement.Modules);

        // ── AppSettings ───────────────────────────────────────────────────────
        // Key: (ConfigKey, ScopeId) — NOT Id.
        // ToRecord() generates Id = Guid.NewGuid() on every call, so using Id
        // as the key would always treat every existing setting as Deleted and every
        // replacement as a new INSERT, causing DbUpdateConcurrencyException when
        // EF Core finds a non-default GUID that doesn't exist in the database.
        ReconcileByKey(
            target.AppSettings,
            replacement.AppSettings,
            s => (s.ConfigKey, s.ScopeId),
            (existing, rep) =>
            {
                existing.ConfigKey   = rep.ConfigKey;
                existing.ConfigValue = rep.ConfigValue;
                existing.ScopeId     = rep.ScopeId;
            });

        // ── Actions ───────────────────────────────────────────────────────────
        ReconcileByKey(
            target.Actions,
            replacement.Actions,
            a => a.Id,
            (existing, rep) =>
            {
                existing.Code        = rep.Code;
                existing.Name        = rep.Name;
                existing.ModuleId    = rep.ModuleId;
                existing.UpdatedBy   = rep.UpdatedBy;
                existing.UpdatedAtUtc = rep.UpdatedAtUtc;
                existing.AuditTimeSpan = rep.AuditTimeSpan;
            });

        // ── DomainResources ───────────────────────────────────────────────────
        ReconcileByKey(
            target.DomainResources,
            replacement.DomainResources,
            d => d.Id,
            (existing, rep) =>
            {
                existing.ModuleId    = rep.ModuleId;
                existing.Type        = rep.Type;
                existing.Code        = rep.Code;
                existing.Name        = rep.Name;
                existing.Description = rep.Description;
                existing.UpdatedBy   = rep.UpdatedBy;
                existing.UpdatedAtUtc = rep.UpdatedAtUtc;
                existing.AuditTimeSpan = rep.AuditTimeSpan;
            });
    }

    /// <summary>
    /// Reconciles a tracked EF Core child collection against a replacement set.
    ///
    /// EF Core change-tracking subtlety (FIX-09):
    /// When a new entity with a manually-set non-default GUID is added to a tracked
    /// navigation collection, EF Core's <c>DetectChanges()</c> may assign
    /// <c>EntityState.Modified</c> (assuming the GUID already exists in the database)
    /// instead of <c>EntityState.Added</c>. This causes an UPDATE on a non-existent row
    /// → 0 rows affected → false <c>DbUpdateConcurrencyException</c>.
    ///
    /// The fix: explicitly set <c>EntityState.Added</c> on every genuinely new entity
    /// so EF Core always generates INSERT, never UPDATE.
    /// </summary>
    private void ReconcileByKey<TEntity, TKey>(
        IList<TEntity> tracked,
        IList<TEntity> replacement,
        Func<TEntity, TKey> keySelector,
        Action<TEntity, TEntity> update)
        where TKey : notnull
    {
        var replacementByKey = replacement.ToDictionary(keySelector);
        var trackedByKey     = tracked.ToDictionary(keySelector);

        // Remove entities no longer present in the aggregate.
        foreach (var (key, entity) in trackedByKey)
        {
            if (!replacementByKey.ContainsKey(key))
                tracked.Remove(entity);
        }

        // Update existing in-place or explicitly INSERT new.
        foreach (var (key, repEntity) in replacementByKey)
        {
            if (trackedByKey.TryGetValue(key, out var existingEntity))
            {
                update(existingEntity, repEntity);   // update in-place — no PK conflict
            }
            else
            {
                tracked.Add(repEntity);

                // FIX-09: Force EntityState.Added so EF Core generates INSERT.
                // DetectChanges() alone would assign Modified for entities with a
                // pre-set non-default GUID, trying to UPDATE a row that does not exist.
                dbContext.Entry(repEntity).State = EntityState.Added;
            }
        }
    }

    /// <summary>
    /// Reconciles the Modules collection and recursively reconciles all
    /// Menus → SubMenus → Options within each module.
    /// </summary>
    private void ReconcileModules(
        IList<SystemSuiteModuleRecord> tracked,
        IList<SystemSuiteModuleRecord> replacement)
    {
        ReconcileByKey(
            tracked,
            replacement,
            m => m.Id,
            (existing, rep) =>
            {
                existing.Code          = rep.Code;
                existing.Name          = rep.Name;
                existing.Description   = rep.Description;
                existing.StatusId      = rep.StatusId;
                existing.SortOrder     = rep.SortOrder;
                existing.UpdatedBy     = rep.UpdatedBy;
                existing.UpdatedAtUtc  = rep.UpdatedAtUtc;
                existing.AuditTimeSpan = rep.AuditTimeSpan;

                ReconcileMenus(existing.Menus, rep.Menus);
            });
    }

    private void ReconcileMenus(
        IList<SystemSuiteMenuRecord> tracked,
        IList<SystemSuiteMenuRecord> replacement)
    {
        ReconcileByKey(
            tracked,
            replacement,
            m => m.Id,
            (existing, rep) =>
            {
                existing.Code          = rep.Code;
                existing.Label         = rep.Label;
                existing.Description   = rep.Description;
                existing.SortOrder     = rep.SortOrder;
                existing.UpdatedBy     = rep.UpdatedBy;
                existing.UpdatedAtUtc  = rep.UpdatedAtUtc;
                existing.AuditTimeSpan = rep.AuditTimeSpan;

                ReconcileSubMenus(existing.SubMenus, rep.SubMenus);
            });
    }

    private void ReconcileSubMenus(
        IList<SystemSuiteSubMenuRecord> tracked,
        IList<SystemSuiteSubMenuRecord> replacement)
    {
        ReconcileByKey(
            tracked,
            replacement,
            sm => sm.Id,
            (existing, rep) =>
            {
                existing.Code          = rep.Code;
                existing.Label         = rep.Label;
                existing.Description   = rep.Description;
                existing.SortOrder     = rep.SortOrder;
                existing.UpdatedBy     = rep.UpdatedBy;
                existing.UpdatedAtUtc  = rep.UpdatedAtUtc;
                existing.AuditTimeSpan = rep.AuditTimeSpan;

                ReconcileOptions(existing.Options, rep.Options);
            });
    }

    private void ReconcileOptions(
        IList<SystemSuiteOptionRecord> tracked,
        IList<SystemSuiteOptionRecord> replacement)
    {
        ReconcileByKey(
            tracked,
            replacement,
            o => o.Id,
            (existing, rep) =>
            {
                existing.Code          = rep.Code;
                existing.Label         = rep.Label;
                existing.Description   = rep.Description;
                existing.ActionCode    = rep.ActionCode;
                existing.SortOrder     = rep.SortOrder;
                existing.UpdatedBy     = rep.UpdatedBy;
                existing.UpdatedAtUtc  = rep.UpdatedAtUtc;
                existing.AuditTimeSpan = rep.AuditTimeSpan;
            });
    }
}
