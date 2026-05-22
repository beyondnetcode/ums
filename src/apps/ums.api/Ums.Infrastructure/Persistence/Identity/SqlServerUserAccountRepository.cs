using Microsoft.EntityFrameworkCore;
using Ums.Domain.Identity;
using Ums.Domain.Kernel;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Infrastructure.Persistence.Identity.Entities;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Infrastructure.Persistence.Reflection;

namespace Ums.Infrastructure.Persistence.Identity;

using UserAccountAggregate = Ums.Domain.Identity.UserAccount.UserAccount;

public sealed class SqlServerUserAccountRepository(UmsPlatformDbContext dbContext) : IUserAccountRepository, IUnitOfWork
{
    private readonly HashSet<UserAccountAggregate> _trackedAggregates = [];

    public IUnitOfWork UnitOfWork => this;

    public async Task<UserAccountAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.UserAccounts
            .AsSplitQuery()
            .Include(x => x.MfaEnrollments)
            .Include(x => x.PasswordCredentials)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public Task<UserAccountAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public async Task<UserAccountAggregate?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.UserAccounts
            .AsSplitQuery()
            .Include(x => x.MfaEnrollments)
            .Include(x => x.PasswordCredentials)
            .FirstOrDefaultAsync(x => x.Email == email.GetValue(), cancellationToken);

        return record is null ? null : Rehydrate(record);
    }

    public async Task<IReadOnlyList<UserAccountAggregate>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var records = await dbContext.UserAccounts
            .AsSplitQuery()
            .Include(x => x.MfaEnrollments)
            .Include(x => x.PasswordCredentials)
            .OrderBy(x => x.Email)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public async Task<IReadOnlyList<UserAccountAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var records = await dbContext.UserAccounts
            .AsSplitQuery()
            .Include(x => x.MfaEnrollments)
            .Include(x => x.PasswordCredentials)
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Email)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    public Task AddAsync(UserAccountAggregate aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.UserAccounts.Add(ToRecord(aggregate));
        _trackedAggregates.Add(aggregate);
        return Task.CompletedTask;
    }

    public async Task UpdateAsync(UserAccountAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.UserAccounts
            .Include(x => x.MfaEnrollments)
            .Include(x => x.PasswordCredentials)
            .FirstOrDefaultAsync(x => x.Id == aggregate.Props.Id.GetValue(), cancellationToken)
            ?? throw new InvalidOperationException($"User account {aggregate.Props.Id.GetValue()} does not exist.");

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
            aggregate.DomainEvents.MarkChangesAsCommitted();
        }

        _trackedAggregates.Clear();
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public void Dispose() => dbContext.Dispose();

    private static UserAccountAggregate Rehydrate(UserAccountRecord record)
        => IdentityAggregateFactory.RehydrateUserAccount(record, record.MfaEnrollments, record.PasswordCredentials);

    private static UserAccountRecord ToRecord(UserAccountAggregate aggregate)
    {
        var audit = aggregate.Props.Audit.GetValue();
        return new UserAccountRecord
        {
            Id = aggregate.Props.Id.GetValue(),
            TenantId = aggregate.Props.TenantId.GetValue(),
            BranchId = aggregate.Props.BranchId?.GetValue(),
            Email = aggregate.Email.GetValue(),
            CategoryId = aggregate.Category.Id,
            StatusId = aggregate.Status.Id,
            IdentityReference = aggregate.IdentityReference?.GetValue(),
            IdentityReferenceTypeId = aggregate.IdentityReferenceType?.Id,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
            MfaEnrollments = aggregate.MfaEnrollments.Select(x =>
            {
                var a = x.Props.Audit.GetValue();
                return new UserAccountMfaEnrollmentRecord
                {
                    Id = x.Props.Id.GetValue(),
                    UserAccountId = x.Props.UserAccountId.GetValue(),
                    MethodId = x.Method.Id,
                    StatusId = x.Status.Id,
                    CreatedBy = a.CreatedBy,
                    CreatedAtUtc = a.CreatedAt,
                    UpdatedBy = a.UpdatedBy,
                    UpdatedAtUtc = a.UpdatedAt,
                    AuditTimeSpan = a.TimeSpan,
                };
            }).ToList(),
            PasswordCredentials = aggregate.PasswordCredentials.Select(x =>
            {
                var a = x.Props.Audit.GetValue();
                return new UserAccountPasswordCredentialRecord
                {
                    Id = x.Props.Id.GetValue(),
                    UserAccountId = x.Props.UserAccountId.GetValue(),
                    PasswordHash = x.PasswordHash.GetValue(),
                    IsActive = x.IsActive,
                    CreatedBy = a.CreatedBy,
                    CreatedAtUtc = a.CreatedAt,
                    UpdatedBy = a.UpdatedBy,
                    UpdatedAtUtc = a.UpdatedAt,
                    AuditTimeSpan = a.TimeSpan,
                };
            }).ToList(),
        };
    }

    private static void Apply(UserAccountRecord target, UserAccountAggregate source)
    {
        var replacement = ToRecord(source);

        target.TenantId = replacement.TenantId;
        target.BranchId = replacement.BranchId;
        target.Email = replacement.Email;
        target.CategoryId = replacement.CategoryId;
        target.StatusId = replacement.StatusId;
        target.IdentityReference = replacement.IdentityReference;
        target.IdentityReferenceTypeId = replacement.IdentityReferenceTypeId;
        target.CreatedBy = replacement.CreatedBy;
        target.CreatedAtUtc = replacement.CreatedAtUtc;
        target.UpdatedBy = replacement.UpdatedBy;
        target.UpdatedAtUtc = replacement.UpdatedAtUtc;
        target.AuditTimeSpan = replacement.AuditTimeSpan;

        target.MfaEnrollments.Clear();
        foreach (var enrollment in replacement.MfaEnrollments)
        {
            target.MfaEnrollments.Add(enrollment);
        }

        target.PasswordCredentials.Clear();
        foreach (var credential in replacement.PasswordCredentials)
        {
            target.PasswordCredentials.Add(credential);
        }
    }
}
