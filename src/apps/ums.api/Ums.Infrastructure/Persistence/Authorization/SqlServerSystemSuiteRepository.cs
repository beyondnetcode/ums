using Microsoft.EntityFrameworkCore;
using Ums.Domain.Authorization;
using Ums.Domain.Kernel;
using Ums.Infrastructure.Persistence.Authorization.Entities;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Infrastructure.Persistence.Reflection;

namespace Ums.Infrastructure.Persistence.Authorization;

using SystemSuiteAggregate = Ums.Domain.Authorization.SystemSuite.SystemSuite;

public sealed class SqlServerSystemSuiteRepository(UmsPlatformDbContext dbContext) : ISystemSuiteRepository, IUnitOfWork
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
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public Task<SystemSuiteAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public async Task<SystemSuiteAggregate?> GetByCodeAsync(Code code, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.SystemSuites
            .AsSplitQuery()
            .Include(x => x.Modules).ThenInclude(x => x.Menus).ThenInclude(x => x.SubMenus).ThenInclude(x => x.Options)
            .Include(x => x.AppSettings)
            .Include(x => x.Actions)
            .FirstOrDefaultAsync(x => x.Code == code.GetValue(), cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public async Task<IReadOnlyList<SystemSuiteAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await dbContext.SystemSuites
            .AsSplitQuery()
            .Include(x => x.Modules).ThenInclude(x => x.Menus).ThenInclude(x => x.SubMenus).ThenInclude(x => x.Options)
            .Include(x => x.AppSettings)
            .Include(x => x.Actions)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public async Task<IReadOnlyList<SystemSuiteAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.SystemSuites
            .AsSplitQuery()
            .Include(x => x.Modules).ThenInclude(x => x.Menus).ThenInclude(x => x.SubMenus).ThenInclude(x => x.Options)
            .Include(x => x.AppSettings)
            .Include(x => x.Actions)
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
            dbContext.OutboxMessages.AddRange(OutboxMessageFactory.CreateFromAggregate(aggregate));
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
        => AuthorizationAggregateFactory.RehydrateSystemSuite(record, record.Modules, record.AppSettings, record.Actions);

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

    private static void Apply(SystemSuiteRecord target, SystemSuiteAggregate source)
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
        // Reconcile by Id to avoid the Clear()+Add(same-PK) anti-pattern.
        // Clear() marks existing tracked entities as Deleted; adding new POCOs
        // with the same PKs then creates a Deleted↔Added conflict in the EF
        // change tracker, causing a false DbUpdateConcurrencyException.
        ReconcileModules(target.Modules, replacement.Modules);

        // ── AppSettings ───────────────────────────────────────────────────────
        ReconcileById(
            target.AppSettings,
            replacement.AppSettings,
            s => s.Id,
            (existing, rep) =>
            {
                existing.ConfigKey   = rep.ConfigKey;
                existing.ConfigValue = rep.ConfigValue;
                existing.ScopeId     = rep.ScopeId;
            });

        // ── Actions ───────────────────────────────────────────────────────────
        ReconcileById(
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
    }

    /// <summary>
    /// Reconciles a tracked EF Core child collection against a replacement set using
    /// <typeparamref name="TKey"/> as the identity. Existing children are updated
    /// in-place (no Delete+Insert cycle); removed children are evicted; new children
    /// are appended.
    /// </summary>
    private static void ReconcileById<TEntity, TKey>(
        IList<TEntity> tracked,
        IList<TEntity> replacement,
        Func<TEntity, TKey> keySelector,
        Action<TEntity, TEntity> update)
        where TKey : notnull
    {
        var replacementById = replacement.ToDictionary(keySelector);
        var trackedById     = tracked.ToDictionary(keySelector);

        // Remove entities no longer present in the aggregate.
        foreach (var (key, entity) in trackedById)
        {
            if (!replacementById.ContainsKey(key))
                tracked.Remove(entity);
        }

        // Update existing or add new.
        foreach (var (key, repEntity) in replacementById)
        {
            if (trackedById.TryGetValue(key, out var existingEntity))
                update(existingEntity, repEntity);   // update in-place — no PK conflict
            else
                tracked.Add(repEntity);              // genuinely new — safe INSERT
        }
    }

    /// <summary>
    /// Reconciles the Modules collection, including in-place property updates.
    /// Menu-level reconciliation is intentionally deferred to the AddMenu/RemoveMenu
    /// command handlers which operate on an individual module scope.
    /// </summary>
    private static void ReconcileModules(
        IList<SystemSuiteModuleRecord> tracked,
        IList<SystemSuiteModuleRecord> replacement)
    {
        ReconcileById(
            tracked,
            replacement,
            m => m.Id,
            (existing, rep) =>
            {
                existing.Code        = rep.Code;
                existing.Name        = rep.Name;
                existing.Description = rep.Description;
                existing.StatusId    = rep.StatusId;
                existing.SortOrder   = rep.SortOrder;
                existing.UpdatedBy   = rep.UpdatedBy;
                existing.UpdatedAtUtc = rep.UpdatedAtUtc;
                existing.AuditTimeSpan = rep.AuditTimeSpan;
                // Menus are NOT touched here: they belong to the existing tracked
                // collection and are managed by dedicated menu commands.
            });
    }
}
