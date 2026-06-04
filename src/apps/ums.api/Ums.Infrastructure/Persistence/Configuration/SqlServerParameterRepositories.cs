namespace Ums.Infrastructure.Persistence.Configuration;

using Microsoft.EntityFrameworkCore;
using Ums.Domain.Configuration;
using Ums.Domain.Configuration.Parameter;
using Ums.Domain.Kernel;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Infrastructure.Persistence.Configuration.Entities;
using Ums.Infrastructure.Persistence.Reflection;

public sealed class SqlServerParameterDefinitionRepository(UmsPlatformDbContext db)
    : IParameterDefinitionRepository
{
    public async Task<ParameterDefinition?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await db.ParameterDefinitions.FirstOrDefaultAsync(x => x.Id == id, ct);
        return r is null ? null : ConfigurationAggregateFactory.RehydrateParameterDefinition(r);
    }

    public async Task<ParameterDefinition?> GetByCodeAsync(string code, CancellationToken ct = default)
    {
        var upper = code.ToUpperInvariant();
        var r = await db.ParameterDefinitions.FirstOrDefaultAsync(x => x.Code == upper, ct);
        return r is null ? null : ConfigurationAggregateFactory.RehydrateParameterDefinition(r);
    }

    public async Task<IReadOnlyList<ParameterDefinition>> GetAllAsync(CancellationToken ct = default)
    {
        var records = await db.ParameterDefinitions.OrderBy(x => x.DisplayOrder).ToListAsync(ct);
        return records.Select(ConfigurationAggregateFactory.RehydrateParameterDefinition).ToList();
    }

    public async Task AddAsync(ParameterDefinition d, CancellationToken ct = default)
        => await db.ParameterDefinitions.AddAsync(ToRecord(d), ct);

    public async Task UpdateAsync(ParameterDefinition d, CancellationToken ct = default)
    {
        var existing = await db.ParameterDefinitions
            .FirstOrDefaultAsync(x => x.Id == d.Props.Id.GetValue(), ct)
            ?? throw new InvalidOperationException($"ParameterDefinition {d.Props.Id.GetValue()} not found.");
        Apply(existing, d);
    }

    public Task<int> CountByCodeAsync(string code, CancellationToken ct = default)
        => db.ParameterDefinitions.CountAsync(x => x.Code == code.ToUpperInvariant(), ct);

    public Task<int> CountGlobalValuesAsync(Guid definitionId, CancellationToken ct = default)
        => db.ParameterGlobalValues.CountAsync(x => x.ParameterDefinitionId == definitionId, ct);

    public Task<int> CountTenantValuesAsync(Guid definitionId, CancellationToken ct = default)
        => db.ParameterTenantValues.CountAsync(x => x.ParameterDefinitionId == definitionId, ct);

    public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static ParameterDefinitionRecord ToRecord(ParameterDefinition d)
    {
        var audit = d.Props.Audit.GetValue();
        return new ParameterDefinitionRecord
        {
            Id            = d.Props.Id.GetValue(),
            Code          = d.Props.Code.GetValue(),
            Name          = d.Props.Name.Value,
            Description   = d.Props.Description.GetValue(),
            DataTypeId    = d.Props.DataType.Id,
            DefaultValue  = d.Props.DefaultValue.Value,
            ScopeId       = d.Props.Scope.Id,
            IsActive      = d.Props.IsActive,
            IsMandatory   = d.Props.IsMandatory,
            DisplayOrder  = d.Props.DisplayOrder,
            Version       = d.Props.Version,
            CreatedBy     = audit.CreatedBy,
            CreatedAtUtc  = audit.CreatedAt,
            UpdatedBy     = audit.UpdatedBy,
            UpdatedAtUtc  = audit.UpdatedAt,
            AuditTimeSpan = audit.TimeSpan,
        };
    }

    private static void Apply(ParameterDefinitionRecord t, ParameterDefinition d)
    {
        var audit = d.Props.Audit.GetValue();
        t.Name = d.Props.Name.Value;
        t.Description = d.Props.Description.GetValue();
        t.DefaultValue = d.Props.DefaultValue.Value;
        t.ScopeId = d.Props.Scope.Id;
        t.IsActive = d.Props.IsActive;
        t.IsMandatory = d.Props.IsMandatory;
        t.DisplayOrder = d.Props.DisplayOrder;
        t.Version = d.Props.Version;
        t.UpdatedBy = audit.UpdatedBy;
        t.UpdatedAtUtc = audit.UpdatedAt;
        t.AuditTimeSpan = audit.TimeSpan;
    }
}

public sealed class SqlServerParameterGlobalValueRepository(UmsPlatformDbContext db)
    : IParameterGlobalValueRepository
{
    public async Task<ParameterGlobalValue?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await db.ParameterGlobalValues.FirstOrDefaultAsync(x => x.Id == id, ct);
        return r is null ? null : ConfigurationAggregateFactory.RehydrateParameterGlobalValue(r);
    }

    public async Task<ParameterGlobalValue?> GetByDefinitionIdAsync(Guid definitionId, CancellationToken ct = default)
    {
        var r = await db.ParameterGlobalValues
            .FirstOrDefaultAsync(x => x.ParameterDefinitionId == definitionId, ct);
        return r is null ? null : ConfigurationAggregateFactory.RehydrateParameterGlobalValue(r);
    }

    public async Task AddAsync(ParameterGlobalValue v, CancellationToken ct = default)
        => await db.ParameterGlobalValues.AddAsync(ToRecord(v), ct);

    public async Task UpdateAsync(ParameterGlobalValue v, CancellationToken ct = default)
    {
        var existing = await db.ParameterGlobalValues
            .FirstOrDefaultAsync(x => x.Id == v.Props.Id.GetValue(), ct)
            ?? throw new InvalidOperationException($"ParameterGlobalValue {v.Props.Id.GetValue()} not found.");
        Apply(existing, v);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static ParameterGlobalValueRecord ToRecord(ParameterGlobalValue v)
    {
        var audit = v.Props.Audit.GetValue();
        return new ParameterGlobalValueRecord
        {
            Id                    = v.Props.Id.GetValue(),
            ParameterDefinitionId = v.Props.ParameterDefinitionId.GetValue(),
            EffectiveValue        = v.Props.Value.Value,
            StatusId              = v.Props.Status.Id,
            Version               = v.Props.Version,
            CreatedBy             = audit.CreatedBy,
            CreatedAtUtc          = audit.CreatedAt,
            UpdatedBy             = audit.UpdatedBy,
            UpdatedAtUtc          = audit.UpdatedAt,
            AuditTimeSpan         = audit.TimeSpan,
        };
    }

    private static void Apply(ParameterGlobalValueRecord t, ParameterGlobalValue v)
    {
        var audit = v.Props.Audit.GetValue();
        t.EffectiveValue = v.Props.Value.Value;
        t.StatusId       = v.Props.Status.Id;
        t.Version        = v.Props.Version;
        t.UpdatedBy      = audit.UpdatedBy;
        t.UpdatedAtUtc   = audit.UpdatedAt;
        t.AuditTimeSpan  = audit.TimeSpan;
    }
}

public sealed class SqlServerParameterTenantValueRepository(UmsPlatformDbContext db)
    : IParameterTenantValueRepository
{
    public async Task<ParameterTenantValue?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var r = await db.ParameterTenantValues.FirstOrDefaultAsync(x => x.Id == id, ct);
        return r is null ? null : ConfigurationAggregateFactory.RehydrateParameterTenantValue(r);
    }

    public async Task<ParameterTenantValue?> GetByTenantAndDefinitionAsync(
        Guid tenantId, Guid definitionId, CancellationToken ct = default)
    {
        var r = await db.ParameterTenantValues
            .FirstOrDefaultAsync(x => x.TenantId == tenantId && x.ParameterDefinitionId == definitionId, ct);
        return r is null ? null : ConfigurationAggregateFactory.RehydrateParameterTenantValue(r);
    }

    public async Task AddAsync(ParameterTenantValue v, CancellationToken ct = default)
        => await db.ParameterTenantValues.AddAsync(ToRecord(v), ct);

    public async Task UpdateAsync(ParameterTenantValue v, CancellationToken ct = default)
    {
        var existing = await db.ParameterTenantValues
            .FirstOrDefaultAsync(x => x.Id == v.Props.Id.GetValue(), ct)
            ?? throw new InvalidOperationException($"ParameterTenantValue {v.Props.Id.GetValue()} not found.");
        Apply(existing, v);
    }

    public async Task<bool> SaveChangesAsync(CancellationToken ct = default)
    {
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static ParameterTenantValueRecord ToRecord(ParameterTenantValue v)
    {
        var audit = v.Props.Audit.GetValue();
        return new ParameterTenantValueRecord
        {
            Id                    = v.Props.Id.GetValue(),
            TenantId              = v.Props.TenantId.GetValue(),
            ParameterDefinitionId = v.Props.ParameterDefinitionId.GetValue(),
            OverrideValue         = v.Props.Value.Value,
            StatusId              = v.Props.Status.Id,
            Version               = v.Props.Version,
            CreatedBy             = audit.CreatedBy,
            CreatedAtUtc          = audit.CreatedAt,
            UpdatedBy             = audit.UpdatedBy,
            UpdatedAtUtc          = audit.UpdatedAt,
            AuditTimeSpan         = audit.TimeSpan,
        };
    }

    private static void Apply(ParameterTenantValueRecord t, ParameterTenantValue v)
    {
        var audit = v.Props.Audit.GetValue();
        t.OverrideValue = v.Props.Value.Value;
        t.StatusId      = v.Props.Status.Id;
        t.Version       = v.Props.Version;
        t.UpdatedBy     = audit.UpdatedBy;
        t.UpdatedAtUtc  = audit.UpdatedAt;
        t.AuditTimeSpan = audit.TimeSpan;
    }
}
