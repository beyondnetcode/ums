using Microsoft.EntityFrameworkCore;
using Ums.Domain.IGA;
using Ums.Domain.Kernel;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.IGA.Entities;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Infrastructure.Persistence.Reflection;

namespace Ums.Infrastructure.Persistence.IGA;

using PromotionRequestAggregate = Ums.Domain.IGA.PromotionRequest.PromotionRequest;

public sealed class SqlServerPromotionRequestRepository : IPromotionRequestRepository, IUnitOfWork
{
    private readonly UmsPlatformDbContext _dbContext;
    private readonly HashSet<PromotionRequestAggregate> _trackedAggregates = [];

    public SqlServerPromotionRequestRepository(UmsPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IUnitOfWork UnitOfWork => this;

    public async Task<PromotionRequestAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.Set<PromotionRequestRecord>()
            .Include(x => x.ImpactAnalyses)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record is null ? null : IgaAggregateFactory.RehydratePromotionRequest(record, record.ImpactAnalyses);
    }

    public Task<PromotionRequestAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public async Task<IReadOnlyList<PromotionRequestAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        IQueryable<PromotionRequestRecord> query = _dbContext.Set<PromotionRequestRecord>().Include(x => x.ImpactAnalyses);

        if (tenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == tenantId.Value);
        }

        var records = await query.ToListAsync(cancellationToken);

        return records.Select(r => IgaAggregateFactory.RehydratePromotionRequest(r, r.ImpactAnalyses)).ToList();
    }

    public async Task<IReadOnlyList<PromotionRequestAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.Set<PromotionRequestRecord>()
            .Include(x => x.ImpactAnalyses)
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return records.Select(r => IgaAggregateFactory.RehydratePromotionRequest(r, r.ImpactAnalyses)).ToList();
    }

    public async Task<IReadOnlyList<PromotionRequestAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.Set<PromotionRequestRecord>()
            .Include(x => x.ImpactAnalyses)
            .Where(x => x.UserId == userId)
            .ToListAsync(cancellationToken);

        return records.Select(r => IgaAggregateFactory.RehydratePromotionRequest(r, r.ImpactAnalyses)).ToList();
    }

    public Task AddAsync(PromotionRequestAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<PromotionRequestRecord>().Add(ToRecord(aggregate));
        _trackedAggregates.Add(aggregate);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(PromotionRequestAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Set<PromotionRequestRecord>()
            .Include(x => x.ImpactAnalyses)
            .FirstOrDefaultAsync(x => x.Id == aggregate.Props.Id.GetValue(), cancellationToken)
            ?? throw new InvalidOperationException($"PromotionRequest {aggregate.Props.Id.GetValue()} does not exist.");

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

    private static PromotionRequestRecord ToRecord(PromotionRequestAggregate aggregate)
    {
        var audit = aggregate.Props.Audit.GetValue();
        return new PromotionRequestRecord
        {
            Id = aggregate.Props.Id.GetValue(),
            TenantId = aggregate.Props.TenantId.GetValue(),
            UserId = aggregate.Props.UserId.GetValue(),
            CurrentRoleId = aggregate.Props.CurrentRoleId.GetValue(),
            TargetRoleId = aggregate.Props.TargetRoleId.GetValue(),
            RequestedAt = aggregate.Props.RequestedAt,
            RequestedBy = aggregate.Props.RequestedBy.GetValue().ToString(),
            RequestReason = aggregate.Props.RequestReason?.GetValue(),
            ManagerId = aggregate.Props.ManagerId.GetValue(),
            ManagerApprovalStatusId = (int)aggregate.Props.ManagerApprovalStatus,
            ManagerDecisionAt = aggregate.Props.ManagerDecisionAt,
            SecurityApprovalStatusId = (int)aggregate.Props.SecurityApprovalStatus,
            SecurityDecisionAt = aggregate.Props.SecurityDecisionAt,
            StatusId = (int)aggregate.Props.Status,
            ExecutedAt = aggregate.Props.ExecutedAt,
            ExecutedBy = aggregate.Props.ExecutedBy?.GetValue().ToString(),
            VerifiedAt = aggregate.Props.VerifiedAt,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
            ImpactAnalyses = aggregate.ImpactAnalyses.Select(a =>
            {
                var aa = a.Props.Audit.GetValue();
                return new PromotionImpactAnalysisRecord
                {
                    Id = a.Props.Id.GetValue(),
                    PromotionRequestId = a.Props.PromotionRequestId.GetValue(),
                    RiskScore = a.Props.RiskScore,
                    RiskLevel = a.Props.RiskLevel.GetValue(),
                    NewPermissionsCount = a.Props.NewPermissionsCount,
                    RemovedPermissionsCount = a.Props.RemovedPermissionsCount,
                    AffectedSystemsCount = a.Props.AffectedSystemsCount,
                    ConflictingPermissions = a.Props.ConflictingPermissions?.GetValue(),
                    RiskFactors = a.Props.RiskFactors?.GetValue(),
                    SuggestedMitigations = a.Props.SuggestedMitigations?.GetValue(),
                    AnalyzedAt = a.Props.AnalyzedAt,
                    AnalyzedBy = a.Props.AnalyzedBy?.GetValue(),
                    CreatedBy = aa.CreatedBy,
                    CreatedAtUtc = aa.CreatedAt,
                    UpdatedBy = aa.UpdatedBy,
                    UpdatedAtUtc = aa.UpdatedAt,
                    AuditTimeSpan = aa.TimeSpan,
                };
            }).ToList()
        };
    }

    private void Apply(PromotionRequestRecord target, PromotionRequestAggregate source)
    {
        var replacement = ToRecord(source);

        target.TenantId = replacement.TenantId;
        target.UserId = replacement.UserId;
        target.CurrentRoleId = replacement.CurrentRoleId;
        target.TargetRoleId = replacement.TargetRoleId;
        target.RequestedAt = replacement.RequestedAt;
        target.RequestedBy = replacement.RequestedBy;
        target.RequestReason = replacement.RequestReason;
        target.ManagerId = replacement.ManagerId;
        target.ManagerApprovalStatusId = replacement.ManagerApprovalStatusId;
        target.ManagerDecisionAt = replacement.ManagerDecisionAt;
        target.SecurityApprovalStatusId = replacement.SecurityApprovalStatusId;
        target.SecurityDecisionAt = replacement.SecurityDecisionAt;
        target.StatusId = replacement.StatusId;
        target.ExecutedAt = replacement.ExecutedAt;
        target.ExecutedBy = replacement.ExecutedBy;
        target.VerifiedAt = replacement.VerifiedAt;
        target.CreatedBy = replacement.CreatedBy;
        target.CreatedAtUtc = replacement.CreatedAtUtc;
        target.UpdatedBy = replacement.UpdatedBy;
        target.UpdatedAtUtc = replacement.UpdatedAtUtc;
        target.AuditTimeSpan = replacement.AuditTimeSpan;

        EfChildCollectionReconciler.ReconcileById(
            _dbContext,
            target.ImpactAnalyses,
            replacement.ImpactAnalyses,
            analysis => analysis.Id,
            UpdateImpactAnalysis);
    }

    private static void UpdateImpactAnalysis(PromotionImpactAnalysisRecord target, PromotionImpactAnalysisRecord source)
    {
        target.PromotionRequestId = source.PromotionRequestId;
        target.RiskScore = source.RiskScore;
        target.RiskLevel = source.RiskLevel;
        target.NewPermissionsCount = source.NewPermissionsCount;
        target.RemovedPermissionsCount = source.RemovedPermissionsCount;
        target.AffectedSystemsCount = source.AffectedSystemsCount;
        target.ConflictingPermissions = source.ConflictingPermissions;
        target.RiskFactors = source.RiskFactors;
        target.SuggestedMitigations = source.SuggestedMitigations;
        target.AnalyzedAt = source.AnalyzedAt;
        target.AnalyzedBy = source.AnalyzedBy;
        target.CreatedBy = source.CreatedBy;
        target.CreatedAtUtc = source.CreatedAtUtc;
        target.UpdatedBy = source.UpdatedBy;
        target.UpdatedAtUtc = source.UpdatedAtUtc;
        target.AuditTimeSpan = source.AuditTimeSpan;
    }
}
