using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Ums.Domain.Audit.AuditRecord;
using Ums.Infrastructure.Persistence.Audit.Entities;
using Ums.Infrastructure.Persistence.Reflection;
using BeyondNetCode.Shell.Ddd.Interfaces;

namespace Ums.Infrastructure.Persistence.Audit;

using AuditRecordAggregate = Ums.Domain.Audit.AuditRecord.AuditRecord;

public sealed class PostgreSqlAuditRecordRepository : IAuditRecordRepository, BeyondNetCode.Shell.Ddd.Interfaces.IUnitOfWork
{
    private readonly UmsPlatformDbContext _dbContext;
    private readonly HashSet<AuditRecordAggregate> _trackedAggregates = [];

    public PostgreSqlAuditRecordRepository(UmsPlatformDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    BeyondNetCode.Shell.Ddd.Interfaces.IUnitOfWork IRepository<AuditRecord>.UnitOfWork => this;

    public async Task<AuditRecordAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await _dbContext.Set<AuditRecordRecord>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public Task AppendAsync(AuditRecordAggregate record, CancellationToken ct = default)
    {
        _dbContext.Set<AuditRecordRecord>().Add(ToRecord(record));
        _trackedAggregates.Add(record);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<AuditRecordAggregate>> QueryByEntityAsync(
        Guid entityId, string entityType, Guid rootTenantId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var records = await _dbContext.Set<AuditRecordRecord>()
            .Where(r => r.AffectedEntityId == entityId &&
                        r.AffectedEntityType == entityType &&
                        r.RootTenantId == rootTenantId &&
                        r.WhenOccurred >= from &&
                        r.WhenOccurred <= to)
            .OrderByDescending(r => r.WhenOccurred)
            .ToListAsync(ct);

        return records.Select(Rehydrate).ToList();
    }

    public async Task<IReadOnlyList<AuditRecordAggregate>> QueryByActorAsync(
        Guid actorId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var records = await _dbContext.Set<AuditRecordRecord>()
            .Where(r => r.WhoActed == actorId &&
                        r.WhenOccurred >= from &&
                        r.WhenOccurred <= to)
            .OrderByDescending(r => r.WhenOccurred)
            .ToListAsync(ct);

        return records.Select(Rehydrate).ToList();
    }

    public async Task<IReadOnlyList<AuditRecordAggregate>> QueryByEventTypeAsync(
        string eventType, Guid rootTenantId, DateTime from, DateTime to, CancellationToken ct = default)
    {
        var query = _dbContext.Set<AuditRecordRecord>()
            .Where(r => r.RootTenantId == rootTenantId &&
                        r.WhenOccurred >= from &&
                        r.WhenOccurred <= to);

        if (!string.Equals(eventType, "*", StringComparison.Ordinal))
        {
            query = query.Where(r => r.EventType == eventType);
        }

        var records = await query.OrderByDescending(r => r.WhenOccurred).ToListAsync(ct);
        return records.Select(Rehydrate).ToList();
    }

    // IUnitOfWork Implementation
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);

    public async Task<bool> SaveEntitiesAsync(object entity, CancellationToken cancellationToken = default)
    {
        foreach (var aggregate in _trackedAggregates)
        {
            aggregate.DomainEvents.MarkChangesAsCommitted();
        }
        await _dbContext.SaveChangesAsync(cancellationToken);
        _trackedAggregates.Clear();
        return true;
    }

    public void Dispose()
    {
        // Managed by EF Core lifecycle
    }

    private static AuditRecordAggregate Rehydrate(AuditRecordRecord record)
        => AuditAggregateFactory.RehydrateAuditRecord(record);

    private static AuditRecordRecord ToRecord(AuditRecordAggregate aggregate)
    {
        return new AuditRecordRecord
        {
            Id = aggregate.Props.Id.GetValue(),
            WhoActed = aggregate.WhoActed,
            SubjectTypeId = aggregate.SubjectType.Id,
            WhenOccurred = aggregate.WhenOccurred,
            WhatChanged = aggregate.WhatChanged,
            EventType = aggregate.EventType,
            AuditResultId = aggregate.AuditResult.Id,
            AffectedEntityId = aggregate.AffectedEntityId,
            AffectedEntityType = aggregate.AffectedEntityType,
            RootTenantId = aggregate.RootTenantId,
            Metadata = aggregate.Metadata
        };
    }
}
