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
        var existing = await dbContext.SystemSuites
            .Include(x => x.Modules).ThenInclude(x => x.Menus).ThenInclude(x => x.SubMenus).ThenInclude(x => x.Options)
            .Include(x => x.AppSettings)
            .Include(x => x.Actions)
            .FirstOrDefaultAsync(x => x.Id == aggregate.Props.Id.GetValue(), cancellationToken)
            ?? throw new InvalidOperationException($"System suite {aggregate.Props.Id.GetValue()} does not exist.");

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

        target.TenantId = replacement.TenantId;
        target.Code = replacement.Code;
        target.Name = replacement.Name;
        target.Description = replacement.Description;
        target.StatusId = replacement.StatusId;
        target.CreatedBy = replacement.CreatedBy;
        target.CreatedAtUtc = replacement.CreatedAtUtc;
        target.UpdatedBy = replacement.UpdatedBy;
        target.UpdatedAtUtc = replacement.UpdatedAtUtc;
        target.AuditTimeSpan = replacement.AuditTimeSpan;

        target.Modules.Clear();
        foreach (var module in replacement.Modules)
        {
            target.Modules.Add(module);
        }

        target.AppSettings.Clear();
        foreach (var setting in replacement.AppSettings)
        {
            target.AppSettings.Add(setting);
        }

        target.Actions.Clear();
        foreach (var action in replacement.Actions)
        {
            target.Actions.Add(action);
        }
    }
}
