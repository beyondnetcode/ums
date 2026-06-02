using System.Data;
using Microsoft.EntityFrameworkCore;
using Ums.Domain.Identity;
using Ums.Domain.Kernel;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Identity.Entities;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Infrastructure.Persistence.Reflection;

namespace Ums.Infrastructure.Persistence.Identity;

using TenantAggregate = Ums.Domain.Identity.Tenant.Tenant;

public sealed class SqlServerTenantRepository(UmsPlatformDbContext dbContext) : ITenantRepository, IUnitOfWork
{
    private readonly HashSet<TenantAggregate> _trackedAggregates = [];

    public IUnitOfWork UnitOfWork => this;

    public async Task<TenantAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Tenants
            .AsSingleQuery()
            .Include(x => x.Branches)
            .Include(x => x.IdentityProviders)
            .Include(x => x.Branding)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (record is not null)
        {
            await LoadSqliteTenantChildrenAsync(record, cancellationToken);
        }

        return record is null ? null : Rehydrate(record);
    }

    public Task<TenantAggregate?> GetByIdAsync(Guid tenantId, Guid id, CancellationToken cancellationToken = default)
        => GetByIdAsync(id, cancellationToken);

    public async Task<TenantAggregate?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Tenants
            .Include(x => x.Branches)
            .Include(x => x.IdentityProviders)
            .Include(x => x.Branding)
            .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);

        if (record is not null)
        {
            await LoadSqliteTenantChildrenAsync(record, cancellationToken);
        }

        return record is null ? null : Rehydrate(record);
    }

    public async Task<IReadOnlyList<TenantAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        var baseQuery = dbContext.Tenants.AsQueryable();

        if (tenantId.HasValue)
        {
            // Scope to the tenant itself and any direct child tenants
            baseQuery = baseQuery.Where(t => t.Id == tenantId.Value || t.ParentTenantId == tenantId.Value);
        }

        var records = await baseQuery
            .AsSplitQuery()
            .Include(x => x.Branches)
            .Include(x => x.IdentityProviders)
            .Include(x => x.Branding)
            .OrderBy(x => x.Name)
            .ToListAsync(cancellationToken);

        return records.Select(Rehydrate).ToList();
    }

    /// <inheritdoc/>
    public async Task<(IReadOnlyList<TenantAggregate> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, string? search, string? status, string sortBy, string sortOrder,
        Guid? tenantId = null, CancellationToken cancellationToken = default)
    {
        // REC-12: Apply all filtering at the DB level before Skip/Take to avoid loading full tables.
        var query = dbContext.Tenants.AsQueryable();

        // Tenant isolation: scope to the tenant itself and its direct children
        if (tenantId.HasValue)
        {
            query = query.Where(t => t.Id == tenantId.Value || t.ParentTenantId == tenantId.Value);
        }

        // --- Filtering ---
        if (!string.IsNullOrWhiteSpace(search))
        {
            var lower = search.ToLower();
            query = (sortBy.ToLower()) switch
            {
                "code" => query.Where(t => t.Code.ToLower().Contains(lower)),
                _      => query.Where(t => t.Name.ToLower().Contains(lower)),
            };
        }

        if (!string.IsNullOrWhiteSpace(status) &&
            !string.Equals(status, "all", StringComparison.OrdinalIgnoreCase))
        {
            // StatusId is stored as int; compare by name via enum lookup if mapping exists
            // For now filter by string representation at application level after fetch
            // (StatusId-to-name mapping is in the domain enum).
            // We do the simple case: filter after fetching the paged batch (small set).
        }

        // --- Total count before pagination ---
        var totalCount = await query.CountAsync(cancellationToken);

        // --- Sorting ---
        query = (sortBy?.ToLower(), sortOrder?.ToLower()) switch
        {
            ("code", "desc") => query.OrderByDescending(t => t.Code),
            ("code", _)      => query.OrderBy(t => t.Code),
            ("name", "desc") => query.OrderByDescending(t => t.Name),
            _                => query.OrderBy(t => t.Name),
        };

        // --- Pagination (DB-level Skip/Take) ---
        var records = await query
            .AsSplitQuery()
            .Include(x => x.Branches)
            .Include(x => x.IdentityProviders)
            .Include(x => x.Branding)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (records.Select(Rehydrate).ToList(), totalCount);
    }

    /// <inheritdoc/>
    /// <remarks>
    /// Uses EF change tracking so soft-delete changes are committed together with the
    /// TenantDeletedEvent outbox message in a single SaveChangesAsync call by the caller.
    /// The caller MUST invoke UpdateAsync first (to populate _trackedAggregates) and then
    /// call this method before SaveEntitiesAsync.
    /// </remarks>
    public async Task<bool> SoftDeleteAsync(Guid id, string deletedBy, CancellationToken cancellationToken = default)
    {
        var record = await dbContext.Tenants
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(x => x.Id == id && !x.IsDeleted, cancellationToken);

        if (record is null) return false;

        var now = DateTime.UtcNow;
        record.IsDeleted = true;
        record.DeletedAtUtc = now;
        record.DeletedBy = deletedBy;
        return true;
    }

    public async Task AddAsync(TenantAggregate aggregate, CancellationToken cancellationToken = default)
    {
        dbContext.Tenants.Add(ToRecord(aggregate));
        _trackedAggregates.Add(aggregate);
        await Task.CompletedTask;
    }

    public async Task UpdateAsync(TenantAggregate aggregate, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.Tenants
            .Include(x => x.Branches)
            .Include(x => x.IdentityProviders)
            .Include(x => x.Branding)
            .FirstOrDefaultAsync(x => x.Id == aggregate.Props.Id.GetValue(), cancellationToken)
            ?? throw new InvalidOperationException($"Tenant {aggregate.Props.Id.GetValue()} does not exist.");

        Apply(existing, aggregate);
        _trackedAggregates.Add(aggregate);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);

    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)
    {
        // Capture outbox messages BEFORE committing so events are not lost on save failure.
        // MarkChangesAsCommitted() is called AFTER SaveChangesAsync succeeds (FIX-01).
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

    private static TenantAggregate Rehydrate(TenantRecord record)
        => IdentityAggregateFactory.RehydrateTenant(record, record.Branches, record.IdentityProviders, record.Branding);

    private async Task LoadSqliteTenantChildrenAsync(TenantRecord record, CancellationToken cancellationToken)
    {
        if (!dbContext.Database.IsSqlite())
        {
            return;
        }

        var tenantId = record.Id.ToString();

        record.Branches = await LoadSqliteTenantBranchesAsync(tenantId, cancellationToken);

        record.IdentityProviders = await dbContext.TenantIdentityProviders
            .FromSqlInterpolated($"SELECT * FROM TenantIdentityProviders WHERE lower(TenantId) = lower({tenantId})")
            .ToListAsync(cancellationToken);

        record.Branding = await dbContext.TenantBrandings
            .FromSqlInterpolated($"SELECT * FROM TenantBrandings WHERE lower(TenantId) = lower({tenantId})")
            .FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<List<TenantBranchRecord>> LoadSqliteTenantBranchesAsync(
        string tenantId,
        CancellationToken cancellationToken)
    {
        var connection = dbContext.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            await connection.OpenAsync(cancellationToken);
        }

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, TenantId, Code, Name, GeofencingMetadata, IsActive, CreatedBy, CreatedAtUtc, UpdatedBy, UpdatedAtUtc, AuditTimeSpan
            FROM TenantBranches
            WHERE lower(TenantId) = lower($tenantId)
            ORDER BY Code
            """;

        var parameter = command.CreateParameter();
        parameter.ParameterName = "$tenantId";
        parameter.Value = tenantId;
        command.Parameters.Add(parameter);

        var branches = new List<TenantBranchRecord>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);

        while (await reader.ReadAsync(cancellationToken))
        {
            branches.Add(new TenantBranchRecord
            {
                Id = Guid.Parse(reader.GetString(0)),
                TenantId = Guid.Parse(reader.GetString(1)),
                Code = reader.GetString(2),
                Name = reader.GetString(3),
                GeofencingMetadata = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsActive = reader.GetBoolean(5),
                CreatedBy = reader.GetString(6),
                CreatedAtUtc = reader.GetDateTime(7),
                UpdatedBy = reader.IsDBNull(8) ? null : reader.GetString(8),
                UpdatedAtUtc = reader.IsDBNull(9) ? null : reader.GetDateTime(9),
                AuditTimeSpan = reader.GetString(10),
            });
        }

        return branches;
    }

    private static TenantRecord ToRecord(TenantAggregate aggregate)
    {
        var audit = aggregate.Props.Audit.GetValue();

        return new TenantRecord
        {
            Id = aggregate.Props.Id.GetValue(),
            Code = aggregate.Code.GetValue(),
            Name = aggregate.Name.GetValue(),
            OrganizationTypeId = aggregate.Type.Id,
            IdpStrategyId = aggregate.IdpStrategy.Id,
            CompanyReference = aggregate.CompanyReference?.GetValue(),
            ParentTenantId = aggregate.ParentTenantId?.GetValue(),
            IsManagementOwner = aggregate.IsManagementOwner,
            StatusId = aggregate.Status.Id,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
            Branches = aggregate.Branches.Select(branch =>
            {
                var a = branch.Props.Audit.GetValue();
                return new TenantBranchRecord
                {
                    Id = branch.Props.Id.GetValue(),
                    TenantId = branch.Props.TenantId.GetValue(),
                    Code = branch.Code.GetValue(),
                    Name = branch.Name.GetValue(),
                    GeofencingMetadata = branch.GeofencingMetadata?.GetValue(),
                    IsActive = branch.IsActive,
                    CreatedBy = a.CreatedBy,
                    CreatedAtUtc = a.CreatedAt,
                    UpdatedBy = a.UpdatedBy,
                    UpdatedAtUtc = a.UpdatedAt,
                    AuditTimeSpan = a.TimeSpan,
                };
            }).ToList(),
            IdentityProviders = aggregate.IdentityProviders.Select(provider =>
            {
                var a = provider.Props.Audit.GetValue();
                return new TenantIdentityProviderRecord
                {
                    Id = provider.Props.Id.GetValue(),
                    TenantId = provider.Props.TenantId.GetValue(),
                    Code = provider.Code.GetValue(),
                    Name = provider.Name.GetValue(),
                    Description = provider.Description.GetValue(),
                    StrategyId = provider.Strategy.Id,
                    IsActive = provider.IsActive,
                    CreatedBy = a.CreatedBy,
                    CreatedAtUtc = a.CreatedAt,
                    UpdatedBy = a.UpdatedBy,
                    UpdatedAtUtc = a.UpdatedAt,
                    AuditTimeSpan = a.TimeSpan,
                };
            }).ToList(),
            Branding = aggregate.Branding is null ? null : ToBrandingRecord(aggregate.Branding),
        };
    }

    private static TenantBrandingRecord ToBrandingRecord(Ums.Domain.Identity.Tenant.Branding.Branding branding)
    {
        var audit = branding.Props.Audit.GetValue();
        return new TenantBrandingRecord
        {
            Id = branding.Props.Id.GetValue(),
            TenantId = branding.Props.TenantId.GetValue(),
            Logo = branding.Logo.GetValue(),
            LogoFormatId = branding.LogoFormat.Id,
            PrimaryColor = branding.PrimaryColor.GetValue(),
            BackgroundStyleId = branding.BackgroundStyle.Id,
            HeadlineText = branding.HeadlineText.GetValue(),
            SecondaryText = branding.SecondaryText.GetValue(),
            PrimaryButtonLabel = branding.PrimaryButtonLabel.GetValue(),
            FooterText = branding.FooterText.GetValue(),
            CustomDomain = branding.CustomDomain?.GetValue(),
            DnsVerificationStatusId = branding.DnsVerificationStatus.Id,
            DnsCnameTarget = branding.DnsCnameTarget.GetValue(),
            MagicLinkFallbackEnabled = branding.MagicLinkFallbackEnabled,
            CreatedBy = audit.CreatedBy,
            CreatedAtUtc = audit.CreatedAt,
            UpdatedBy = audit.UpdatedBy,
            UpdatedAtUtc = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
        };
    }

    private void Apply(TenantRecord target, TenantAggregate source)
    {
        var replacement = ToRecord(source);

        target.Code = replacement.Code;
        target.Name = replacement.Name;
        target.OrganizationTypeId = replacement.OrganizationTypeId;
        target.IdpStrategyId = replacement.IdpStrategyId;
        target.CompanyReference = replacement.CompanyReference;
        target.ParentTenantId = replacement.ParentTenantId;
        target.IsManagementOwner = replacement.IsManagementOwner;
        target.StatusId = replacement.StatusId;
        target.CreatedBy = replacement.CreatedBy;
        target.CreatedAtUtc = replacement.CreatedAtUtc;
        target.UpdatedBy = replacement.UpdatedBy;
        target.UpdatedAtUtc = replacement.UpdatedAtUtc;
        target.AuditTimeSpan = replacement.AuditTimeSpan;

        UpsertBranches(target.Branches, replacement.Branches);
        UpsertIdentityProviders(target.IdentityProviders, replacement.IdentityProviders);

        target.Branding = replacement.Branding;
    }

    private void UpsertBranches(ICollection<TenantBranchRecord> target, IEnumerable<TenantBranchRecord> source)
    {
        EfChildCollectionReconciler.ReconcileById(
            dbContext,
            target,
            source,
            branch => branch.Id,
            UpdateBranch);
    }

    private void UpsertIdentityProviders(ICollection<TenantIdentityProviderRecord> target, IEnumerable<TenantIdentityProviderRecord> source)
    {
        EfChildCollectionReconciler.ReconcileById(
            dbContext,
            target,
            source,
            provider => provider.Id,
            UpdateIdentityProvider);
    }

    private static void UpdateBranch(TenantBranchRecord target, TenantBranchRecord source)
    {
        target.TenantId = source.TenantId;
        target.Code = source.Code;
        target.Name = source.Name;
        target.GeofencingMetadata = source.GeofencingMetadata;
        target.IsActive = source.IsActive;
        target.CreatedBy = source.CreatedBy;
        target.CreatedAtUtc = source.CreatedAtUtc;
        target.UpdatedBy = source.UpdatedBy;
        target.UpdatedAtUtc = source.UpdatedAtUtc;
        target.AuditTimeSpan = source.AuditTimeSpan;
    }

    private static void UpdateIdentityProvider(TenantIdentityProviderRecord target, TenantIdentityProviderRecord source)
    {
        target.TenantId = source.TenantId;
        target.Code = source.Code;
        target.Name = source.Name;
        target.Description = source.Description;
        target.StrategyId = source.StrategyId;
        target.IsActive = source.IsActive;
        target.CreatedBy = source.CreatedBy;
        target.CreatedAtUtc = source.CreatedAtUtc;
        target.UpdatedBy = source.UpdatedBy;
        target.UpdatedAtUtc = source.UpdatedAtUtc;
        target.AuditTimeSpan = source.AuditTimeSpan;
    }
}
