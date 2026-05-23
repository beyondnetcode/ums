using Microsoft.EntityFrameworkCore;
using Ums.Domain.Authorization;
using Ums.Domain.Kernel;
using Ums.Infrastructure.Persistence.Authorization.Entities;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Infrastructure.Persistence.Reflection;

namespace Ums.Infrastructure.Persistence.Authorization;

using ProfileAggregate = Ums.Domain.Authorization.Profile.Profile;

public sealed class SqlServerProfileRepository(UmsPlatformDbContext dbContext) : IProfileRepository, IUnitOfWork
{
    private readonly HashSet<ProfileAggregate> _trackedAggregates = [];

    public IUnitOfWork UnitOfWork => this;

    public async Task<ProfileAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Profiles
            .AsSplitQuery()
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public Task<ProfileAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public async Task<IReadOnlyList<ProfileAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await dbContext.Profiles
            .AsSplitQuery()
            .Include(x => x.Permissions)
            .OrderBy(x => x.UserId)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public async Task<IReadOnlyList<ProfileAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.Profiles
            .AsSplitQuery()
            .Include(x => x.Permissions)
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.UserId)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public async Task<IReadOnlyList<ProfileAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.Profiles
            .AsSplitQuery()
            .Include(x => x.Permissions)
            .Where(x => x.UserId == userId)
            .OrderBy(x => x.RoleId)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public Task AddAsync(ProfileAggregate aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.Profiles.Add(ToRecord(aggregate));
        _trackedAggregates.Add(aggregate);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(ProfileAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Profiles
            .Include(x => x.Permissions)
            .FirstOrDefaultAsync(x => x.Id == aggregate.Props.Id.GetValue(), cancellationToken)
            ?? throw new InvalidOperationException($"Profile {aggregate.Props.Id.GetValue()} does not exist.");

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
        catch (Microsoft.EntityFrameworkCore.DbUpdateConcurrencyException ex)
        {
            // FIX-03: surface optimistic concurrency failures as 409 Conflict
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

    private static ProfileAggregate Rehydrate(ProfileRecord record)
        => AuthorizationAggregateFactory.RehydrateProfile(record, record.Permissions);

    private static ProfileRecord ToRecord(ProfileAggregate aggregate)
    {
        var audit = aggregate.Props.Audit.GetValue();
        return new ProfileRecord
        {
            Id = aggregate.Props.Id.GetValue(),
            TenantId = aggregate.Props.TenantId.GetValue(),
            UserId = aggregate.Props.UserId.GetValue(),
            RoleId = aggregate.Props.RoleId.GetValue(),
            BranchId = aggregate.Props.BranchId?.GetValue(),
            ScopeId = aggregate.Scope.Id,
            IsActive = aggregate.IsActive,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
            Permissions = aggregate.Permissions.Select(permission =>
            {
                var a = permission.Props.Audit.GetValue();
                return new ProfilePermissionRecord
                {
                    Id = permission.Props.Id.GetValue(),
                    ProfileId = permission.Props.ProfileId.GetValue(),
                    TemplateId = permission.Props.TemplateId.GetValue(),
                    TargetTypeId = permission.TargetType.Id,
                    TargetId = permission.TargetId.GetValue(),
                    ActionId = permission.ActionId.GetValue(),
                    IsAllowed = permission.IsAllowed,
                    IsDenied = permission.IsDenied,
                    IsActive = permission.IsActive,
                    IsOverride = permission.IsOverride,
                    CreatedBy = a.CreatedBy,
                    CreatedAtUtc = a.CreatedAt,
                    UpdatedBy = a.UpdatedBy,
                    UpdatedAtUtc = a.UpdatedAt,
                    AuditTimeSpan = a.TimeSpan,
                };
            }).ToList(),
        };
    }

    private static void Apply(ProfileRecord target, ProfileAggregate source)
    {
        var replacement = ToRecord(source);

        target.TenantId = replacement.TenantId;
        target.UserId = replacement.UserId;
        target.RoleId = replacement.RoleId;
        target.BranchId = replacement.BranchId;
        target.ScopeId = replacement.ScopeId;
        target.IsActive = replacement.IsActive;
        target.CreatedBy = replacement.CreatedBy;
        target.CreatedAtUtc = replacement.CreatedAtUtc;
        target.UpdatedBy = replacement.UpdatedBy;
        target.UpdatedAtUtc = replacement.UpdatedAtUtc;
        target.AuditTimeSpan = replacement.AuditTimeSpan;

        target.Permissions.Clear();
        foreach (var permission in replacement.Permissions)
        {
            target.Permissions.Add(permission);
        }
    }
}
