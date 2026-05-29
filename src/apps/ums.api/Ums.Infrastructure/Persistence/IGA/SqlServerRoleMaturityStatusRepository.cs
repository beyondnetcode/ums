using Microsoft.EntityFrameworkCore;
using Ums.Domain.IGA;
using Ums.Domain.Kernel;
using Ums.Infrastructure.Persistence.IGA.Entities;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Infrastructure.Persistence.Reflection;

namespace Ums.Infrastructure.Persistence.IGA;

using RoleMaturityStatusAggregate = Ums.Domain.IGA.RoleMaturityStatus.RoleMaturityStatus;

public sealed class SqlServerRoleMaturityStatusRepository : IRoleMaturityStatusRepository, IUnitOfWork
{
    private readonly UmsPlatformDbContext _dbContext;
    private readonly HashSet<RoleMaturityStatusAggregate> _trackedAggregates = [];

    public SqlServerRoleMaturityStatusRepository(UmsPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IUnitOfWork UnitOfWork => this;

    public async Task<RoleMaturityStatusAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.Set<RoleMaturityStatusRecord>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record is null ? null : IgaAggregateFactory.RehydrateRoleMaturityStatus(record);
    }

    public Task<RoleMaturityStatusAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public async Task<IReadOnlyList<RoleMaturityStatusAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        IQueryable<RoleMaturityStatusRecord> query = _dbContext.Set<RoleMaturityStatusRecord>();

        if (tenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == tenantId.Value);
        }

        var records = await query.ToListAsync(cancellationToken);

        return records.Select(IgaAggregateFactory.RehydrateRoleMaturityStatus).ToList();
    }

    public async Task<IReadOnlyList<RoleMaturityStatusAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.Set<RoleMaturityStatusRecord>()
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return records.Select(IgaAggregateFactory.RehydrateRoleMaturityStatus).ToList();
    }

    public async Task<IReadOnlyList<RoleMaturityStatusAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.Set<RoleMaturityStatusRecord>()
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        return records.Select(IgaAggregateFactory.RehydrateRoleMaturityStatus).ToList();
    }

    public Task AddAsync(RoleMaturityStatusAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<RoleMaturityStatusRecord>().Add(ToRecord(aggregate));
        _trackedAggregates.Add(aggregate);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(RoleMaturityStatusAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Set<RoleMaturityStatusRecord>()
            .FirstOrDefaultAsync(x => x.Id == aggregate.Props.Id.GetValue(), cancellationToken)
            ?? throw new InvalidOperationException($"RoleMaturityStatus {aggregate.Props.Id.GetValue()} does not exist.");

        Apply(existing, aggregate);
        _trackedAggregates.Add(aggregate);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var aggregate in _trackedAggregates)
        {
            _dbContext.OutboxMessages.AddRange(OutboxMessageFactory.CreateFromAggregate(aggregate));
        }

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
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

    public void Dispose() { }

    private static RoleMaturityStatusRecord ToRecord(RoleMaturityStatusAggregate aggregate)
    {
        var audit = aggregate.Props.Audit.GetValue();
        return new RoleMaturityStatusRecord
        {
            Id = aggregate.Props.Id.GetValue(),
            TenantId = aggregate.Props.TenantId.GetValue(),
            UserId = aggregate.Props.UserId.GetValue(),
            RoleId = aggregate.Props.RoleId.GetValue(),
            CurrentMaturityLevelId = (int)aggregate.Props.CurrentMaturityLevel,
            NextEligibleMaturityLevelId = aggregate.Props.NextEligibleMaturityLevel.HasValue
                ? (int)aggregate.Props.NextEligibleMaturityLevel.Value
                : null,
            AssignedAt = aggregate.Props.AssignedAt,
            CurrentLevelSince = aggregate.Props.CurrentLevelSince,
            EligibleForPromotionAt = aggregate.Props.EligibleForPromotionAt,
            CompletedCertificationsCount = aggregate.Props.CompletedCertificationsCount,
            CompletedTrainingsCount = aggregate.Props.CompletedTrainingsCount,
            PerformanceScore = aggregate.Props.PerformanceScore,
            HasNoComplianceIssues = aggregate.Props.HasNoComplianceIssues,
            BlockingFactor = aggregate.Props.BlockingFactor?.GetValue(),
            LastReviewedAt = aggregate.Props.LastReviewedAt,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
        };
    }

    private static void Apply(RoleMaturityStatusRecord target, RoleMaturityStatusAggregate source)
    {
        var replacement = ToRecord(source);

        target.TenantId = replacement.TenantId;
        target.UserId = replacement.UserId;
        target.RoleId = replacement.RoleId;
        target.CurrentMaturityLevelId = replacement.CurrentMaturityLevelId;
        target.NextEligibleMaturityLevelId = replacement.NextEligibleMaturityLevelId;
        target.AssignedAt = replacement.AssignedAt;
        target.CurrentLevelSince = replacement.CurrentLevelSince;
        target.EligibleForPromotionAt = replacement.EligibleForPromotionAt;
        target.CompletedCertificationsCount = replacement.CompletedCertificationsCount;
        target.CompletedTrainingsCount = replacement.CompletedTrainingsCount;
        target.PerformanceScore = replacement.PerformanceScore;
        target.HasNoComplianceIssues = replacement.HasNoComplianceIssues;
        target.BlockingFactor = replacement.BlockingFactor;
        target.LastReviewedAt = replacement.LastReviewedAt;
        target.CreatedBy = replacement.CreatedBy;
        target.CreatedAtUtc = replacement.CreatedAtUtc;
        target.UpdatedBy = replacement.UpdatedBy;
        target.UpdatedAtUtc = replacement.UpdatedAtUtc;
        target.AuditTimeSpan = replacement.AuditTimeSpan;
    }
}
