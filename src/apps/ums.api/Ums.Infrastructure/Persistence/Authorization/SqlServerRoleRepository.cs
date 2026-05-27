using Microsoft.EntityFrameworkCore;
using Ums.Domain.Authorization;
using Ums.Domain.Kernel;
using Ums.Infrastructure.Persistence.Authorization.Entities;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Infrastructure.Persistence.Reflection;

namespace Ums.Infrastructure.Persistence.Authorization;

using RoleAggregate = Ums.Domain.Authorization.Role.Role;

public sealed class SqlServerRoleRepository(UmsPlatformDbContext dbContext) : IRoleRepository, IUnitOfWork
{
    private readonly HashSet<RoleAggregate> _trackedAggregates = [];

    public IUnitOfWork UnitOfWork => this;

    public async Task<RoleAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Roles.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        return record is null ? null : AuthorizationAggregateFactory.RehydrateRole(record);
    }

    public Task<RoleAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public async Task<RoleAggregate?> GetByCodeAsync(Guid systemSuiteId, Code code, CancellationToken cancellationToken = default)
    {
        var codeValue = code.GetValue();
        var record = await dbContext.Roles.FirstOrDefaultAsync(
            x => x.SystemSuiteId == systemSuiteId && x.Code == codeValue,
            cancellationToken);
        return record is null ? null : AuthorizationAggregateFactory.RehydrateRole(record);
    }

    public async Task<IReadOnlyList<RoleAggregate>> GetBySystemSuiteIdAsync(Guid systemSuiteId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.Roles
            .Where(x => x.SystemSuiteId == systemSuiteId)
            .OrderBy(x => x.HierarchyLevel)
            .ThenBy(x => x.PromotionOrder)
            .ThenBy(x => x.Value)
            .ToListAsync(cancellationToken);
        return records.Select(AuthorizationAggregateFactory.RehydrateRole).ToList();
    }

    public async Task<IReadOnlyList<RoleAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.Roles
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Value)
            .ToListAsync(cancellationToken);
        return records.Select(AuthorizationAggregateFactory.RehydrateRole).ToList();
    }

    public Task AddAsync(RoleAggregate aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.Roles.Add(ToRecord(aggregate));
        _trackedAggregates.Add(aggregate);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(RoleAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var target = await dbContext.Roles.FirstOrDefaultAsync(
            x => x.Id == aggregate.Props.Id.GetValue(),
            cancellationToken)
            ?? throw new InvalidOperationException($"Role {aggregate.Props.Id.GetValue()} does not exist.");
        Apply(target, aggregate);
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

    private static RoleRecord ToRecord(RoleAggregate aggregate)
    {
        var audit = aggregate.Props.Audit.GetValue();
        return new RoleRecord
        {
            Id = aggregate.Props.Id.GetValue(),
            TenantId = aggregate.TenantId.GetValue(),
            SystemSuiteId = aggregate.SystemSuiteId.GetValue(),
            ParentRoleId = aggregate.ParentRoleId?.GetValue(),
            Code = aggregate.Code.GetValue(),
            Value = aggregate.Value.GetValue(),
            Description = aggregate.Description.GetValue(),
            HierarchyLevel = aggregate.HierarchyLevel,
            PromotionOrder = aggregate.PromotionOrder,
            IsActive = aggregate.IsActive,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
        };
    }

    private static void Apply(RoleRecord target, RoleAggregate aggregate)
    {
        var source = ToRecord(aggregate);
        target.ParentRoleId = source.ParentRoleId;
        target.Value = source.Value;
        target.Description = source.Description;
        target.HierarchyLevel = source.HierarchyLevel;
        target.PromotionOrder = source.PromotionOrder;
        target.IsActive = source.IsActive;
        target.UpdatedBy = source.UpdatedBy;
        target.UpdatedAtUtc = source.UpdatedAtUtc;
        target.AuditTimeSpan = source.AuditTimeSpan;
    }
}
