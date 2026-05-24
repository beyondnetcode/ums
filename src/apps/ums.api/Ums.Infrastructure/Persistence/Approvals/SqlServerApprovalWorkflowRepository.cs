using Microsoft.EntityFrameworkCore;
using Ums.Domain.Approvals;
using Ums.Domain.Kernel;
using Ums.Infrastructure.Persistence.Approvals.Entities;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Infrastructure.Persistence.Reflection;

namespace Ums.Infrastructure.Persistence.Approvals;

using ApprovalWorkflowAggregate = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalWorkflow;

public sealed class SqlServerApprovalWorkflowRepository : IApprovalWorkflowRepository, IUnitOfWork
{
    private readonly UmsPlatformDbContext _dbContext;
    private readonly HashSet<ApprovalWorkflowAggregate> _trackedAggregates = [];

    public SqlServerApprovalWorkflowRepository(UmsPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IUnitOfWork UnitOfWork => this;

    public async Task<ApprovalWorkflowAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.Set<ApprovalWorkflowRecord>()
            .Include(x => x.RequiredDocuments)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public Task<ApprovalWorkflowAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public async Task<IReadOnlyList<ApprovalWorkflowAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.Set<ApprovalWorkflowRecord>()
            .Include(x => x.RequiredDocuments)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public async Task<IReadOnlyList<ApprovalWorkflowAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var records = await _dbContext.Set<ApprovalWorkflowRecord>()
            .Include(x => x.RequiredDocuments)
            .Where(x => x.TenantId == tenantId)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public Task AddAsync(ApprovalWorkflowAggregate aggregate, CancellationToken cancellationToken = default)
    {
        _dbContext.Set<ApprovalWorkflowRecord>().Add(ToRecord(aggregate));
        _trackedAggregates.Add(aggregate);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(ApprovalWorkflowAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await _dbContext.Set<ApprovalWorkflowRecord>()
            .Include(x => x.RequiredDocuments)
            .FirstOrDefaultAsync(x => x.Id == aggregate.Props.Id.GetValue(), cancellationToken)
            ?? throw new InvalidOperationException($"Approval workflow {aggregate.Props.Id.GetValue()} does not exist.");

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

    public void Dispose()
    {
    }

    private static ApprovalWorkflowAggregate Rehydrate(ApprovalWorkflowRecord record)
        => ApprovalsAggregateFactory.RehydrateWorkflow(record, record.RequiredDocuments);

    private static ApprovalWorkflowRecord ToRecord(ApprovalWorkflowAggregate aggregate)
    {
        var audit = aggregate.Props.Audit.GetValue();
        return new ApprovalWorkflowRecord
        {
            Id = aggregate.Props.Id.GetValue(),
            TenantId = aggregate.Props.TenantId.GetValue(),
            SystemSuiteId = aggregate.Props.SystemSuiteId?.GetValue(),
            Code = aggregate.Code.GetValue(),
            Name = aggregate.Name.GetValue(),
            Description = aggregate.Description.GetValue(),
            TargetUserCategoryId = aggregate.TargetUserCategory.Id,
            RequiresApproval = aggregate.RequiresApproval,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
            RequiredDocuments = aggregate.RequiredDocuments.Select(x =>
            {
                var a = x.Props.Audit.GetValue();
                return new ApprovalRequiredDocumentRecord
                {
                    Id = x.Props.Id.GetValue(),
                    WorkflowId = x.WorkflowId.GetValue(),
                    DocumentTypeId = x.DocumentTypeId.GetValue(),
                    IsMandatory = x.IsMandatory,
                    CreatedBy = a.CreatedBy,
                    CreatedAtUtc = a.CreatedAt,
                    UpdatedBy = a.UpdatedBy,
                    UpdatedAtUtc = a.UpdatedAt,
                    AuditTimeSpan = a.TimeSpan
                };
            }).ToList()
        };
    }

    private static void Apply(ApprovalWorkflowRecord target, ApprovalWorkflowAggregate source)
    {
        var replacement = ToRecord(source);

        target.TenantId = replacement.TenantId;
        target.SystemSuiteId = replacement.SystemSuiteId;
        target.Code = replacement.Code;
        target.Name = replacement.Name;
        target.Description = replacement.Description;
        target.TargetUserCategoryId = replacement.TargetUserCategoryId;
        target.RequiresApproval = replacement.RequiresApproval;
        target.CreatedBy = replacement.CreatedBy;
        target.CreatedAtUtc = replacement.CreatedAtUtc;
        target.UpdatedBy = replacement.UpdatedBy;
        target.UpdatedAtUtc = replacement.UpdatedAtUtc;
        target.AuditTimeSpan = replacement.AuditTimeSpan;

        target.RequiredDocuments.Clear();
        foreach (var doc in replacement.RequiredDocuments)
        {
            target.RequiredDocuments.Add(doc);
        }
    }
}
