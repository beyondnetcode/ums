using Microsoft.EntityFrameworkCore;
using Ums.Domain.Configuration;
using Ums.Domain.Kernel;
using Ums.Infrastructure.Persistence.Configuration.Entities;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Infrastructure.Persistence.Reflection;

namespace Ums.Infrastructure.Persistence.Configuration;

using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;

public sealed class SqlServerAppConfigurationRepository(UmsPlatformDbContext dbContext) : IAppConfigurationRepository, IUnitOfWork
{
    private readonly HashSet<AppConfigurationAggregate> _trackedAggregates = [];

    public IUnitOfWork UnitOfWork => this;

    public async Task<AppConfigurationAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.AppConfigurations
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public Task<AppConfigurationAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public async Task<AppConfigurationAggregate?> GetByScopeAndCodeAsync(Guid? tenantId, Guid? systemSuiteId, Guid? moduleId, string code, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.AppConfigurations
            .FirstOrDefaultAsync(x =>
                x.TenantId == tenantId
                && x.SystemSuiteId == systemSuiteId
                && x.ModuleId == moduleId
                && x.Code == code, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public async Task<IReadOnlyList<AppConfigurationAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await dbContext.AppConfigurations
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public Task AddAsync(AppConfigurationAggregate aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.AppConfigurations.Add(ToRecord(aggregate));
        _trackedAggregates.Add(aggregate);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(AppConfigurationAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.AppConfigurations
            .FirstOrDefaultAsync(x => x.Id == aggregate.Props.Id.GetValue(), cancellationToken)
            ?? throw new InvalidOperationException($"AppConfiguration {aggregate.Props.Id.GetValue()} does not exist.");

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

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var aggregate in _trackedAggregates)
        {
            aggregate.DomainEvents.MarkChangesAsCommitted();
        }

        _trackedAggregates.Clear();
        return true;
    }

    public void Dispose() => dbContext.Dispose();

    private static AppConfigurationAggregate Rehydrate(AppConfigurationRecord record)
        => ConfigurationAggregateFactory.RehydrateAppConfiguration(record);

    private static AppConfigurationRecord ToRecord(AppConfigurationAggregate aggregate)
    {
        var audit = aggregate.Props.Audit.GetValue();
        return new AppConfigurationRecord
        {
            Id = aggregate.Props.Id.GetValue(),
            TenantId = aggregate.Props.TenantId?.GetValue(),
            SystemSuiteId = aggregate.Props.SystemSuiteId?.GetValue(),
            ModuleId = aggregate.Props.ModuleId?.GetValue(),
            Code = aggregate.Props.Code.GetValue(),
            Value = aggregate.Props.Value.GetValue(),
            Description = aggregate.Props.Description.GetValue(),
            ScopeId = aggregate.Props.Scope.Id,
            IsInheritable = aggregate.Props.IsInheritable,
            IsEncrypted = aggregate.Props.IsEncrypted,
            Version = aggregate.Props.Version,
            StatusId = aggregate.Props.Status.Id,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
        };
    }

    private static void Apply(AppConfigurationRecord target, AppConfigurationAggregate source)
    {
        var replacement = ToRecord(source);

        target.TenantId = replacement.TenantId;
        target.SystemSuiteId = replacement.SystemSuiteId;
        target.ModuleId = replacement.ModuleId;
        target.Code = replacement.Code;
        target.Value = replacement.Value;
        target.Description = replacement.Description;
        target.ScopeId = replacement.ScopeId;
        target.IsInheritable = replacement.IsInheritable;
        target.IsEncrypted = replacement.IsEncrypted;
        target.Version = replacement.Version;
        target.StatusId = replacement.StatusId;
        target.CreatedBy = replacement.CreatedBy;
        target.CreatedAtUtc = replacement.CreatedAtUtc;
        target.UpdatedBy = replacement.UpdatedBy;
        target.UpdatedAtUtc = replacement.UpdatedAtUtc;
        target.AuditTimeSpan = replacement.AuditTimeSpan;
    }
}
