using Microsoft.EntityFrameworkCore;
using Ums.Domain.Configuration;
using Ums.Domain.Kernel;
using Ums.Infrastructure.Persistence.Configuration.Entities;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Infrastructure.Persistence.Reflection;

namespace Ums.Infrastructure.Persistence.Configuration;

using FeatureFlagAggregate = Ums.Domain.Configuration.FeatureFlag.FeatureFlag;

public sealed class SqlServerFeatureFlagRepository(UmsPlatformDbContext dbContext) : IFeatureFlagRepository, IUnitOfWork
{
    private readonly HashSet<FeatureFlagAggregate> _trackedAggregates = [];

    public IUnitOfWork UnitOfWork => this;

    public async Task<FeatureFlagAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.FeatureFlags
            .AsSplitQuery()
            .Include(x => x.EvaluationLogs)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public Task<FeatureFlagAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public async Task<FeatureFlagAggregate?> GetByCodeAsync(string flagCode, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.FeatureFlags
            .AsSplitQuery()
            .Include(x => x.EvaluationLogs)
            .FirstOrDefaultAsync(x => x.FlagCode == flagCode, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public async Task<IReadOnlyList<FeatureFlagAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await dbContext.FeatureFlags
            .AsSplitQuery()
            .Include(x => x.EvaluationLogs)
            .OrderBy(x => x.FlagCode)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public Task AddAsync(FeatureFlagAggregate aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.FeatureFlags.Add(ToRecord(aggregate));
        _trackedAggregates.Add(aggregate);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(FeatureFlagAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.FeatureFlags
            .Include(x => x.EvaluationLogs)
            .FirstOrDefaultAsync(x => x.Id == aggregate.Props.Id.GetValue(), cancellationToken)
            ?? throw new InvalidOperationException($"FeatureFlag {aggregate.Props.Id.GetValue()} does not exist.");

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

    private static FeatureFlagAggregate Rehydrate(FeatureFlagRecord record)
        => ConfigurationAggregateFactory.RehydrateFeatureFlag(record, record.EvaluationLogs);

    private static FeatureFlagRecord ToRecord(FeatureFlagAggregate aggregate)
    {
        var audit = aggregate.Props.Audit.GetValue();
        return new FeatureFlagRecord
        {
            Id = aggregate.Props.Id.GetValue(),
            FlagCode = aggregate.Props.FlagCode,
            FlagTypeId = aggregate.Props.FlagType.Id,
            FlagTargets = aggregate.Props.FlagTargets,
            StatusId = aggregate.Props.Status.Id,
            LinkedResourceTypeId = aggregate.Props.LinkedResourceType?.Id,
            LinkedResourceId = aggregate.Props.LinkedResourceId?.GetValue(),
            RolloutPercentage = aggregate.Props.RolloutPercentage,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
            EvaluationLogs = aggregate.EvaluationLog.Select(log => new FeatureFlagEvaluationLogRecord
            {
                Id = log.Props.Id.GetValue(),
                FeatureFlagId = aggregate.Props.Id.GetValue(),
                EvaluatedBy = log.Props.EvaluatedBy,
                Result = log.Props.Result,
                Context = log.Props.Context,
                EvaluatedAtUtc = log.Props.EvaluatedAt,
            }).ToList(),
        };
    }

    private static void Apply(FeatureFlagRecord target, FeatureFlagAggregate source)
    {
        var replacement = ToRecord(source);

        target.FlagCode = replacement.FlagCode;
        target.FlagTypeId = replacement.FlagTypeId;
        target.FlagTargets = replacement.FlagTargets;
        target.StatusId = replacement.StatusId;
        target.LinkedResourceTypeId = replacement.LinkedResourceTypeId;
        target.LinkedResourceId = replacement.LinkedResourceId;
        target.RolloutPercentage = replacement.RolloutPercentage;
        target.CreatedBy = replacement.CreatedBy;
        target.CreatedAtUtc = replacement.CreatedAtUtc;
        target.UpdatedBy = replacement.UpdatedBy;
        target.UpdatedAtUtc = replacement.UpdatedAtUtc;
        target.AuditTimeSpan = replacement.AuditTimeSpan;

        target.EvaluationLogs.Clear();
        foreach (var log in replacement.EvaluationLogs)
        {
            target.EvaluationLogs.Add(log);
        }
    }
}
