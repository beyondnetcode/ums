namespace Ums.Infrastructure.Configuration;

using Microsoft.EntityFrameworkCore;
using Ums.Application.Common;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Configuration.Entities;

public sealed record ResolvedParameter(
    Guid DefinitionId,
    string Code,
    string Name,
    string Description,
    int DataTypeId,
    string EffectiveValue,
    string DefaultValue,
    int ScopeId,
    bool IsOverride,
    string Status);

public interface IParameterResolutionService
{
    Task<IReadOnlyList<ResolvedParameter>> GetGlobalParametersAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ResolvedParameter>> GetTenantParametersAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<string> GetEffectiveValueAsync(Guid? tenantId, string code, CancellationToken cancellationToken = default);
}

public sealed class ParameterResolutionService : IParameterResolutionService
{
    private readonly UmsPlatformDbContext _dbContext;
    private readonly ITenantContext _tenantContext;

    public ParameterResolutionService(UmsPlatformDbContext dbContext, ITenantContext tenantContext)
    {
        _dbContext = dbContext;
        _tenantContext = tenantContext;
    }

    public async Task<IReadOnlyList<ResolvedParameter>> GetGlobalParametersAsync(CancellationToken cancellationToken = default)
    {
        var definitions = await _dbContext.ParameterDefinitions
            .Where(d => d.IsActive && (d.ScopeId == 1 || d.ScopeId == 3))
            .OrderBy(d => d.DisplayOrder)
            .ThenBy(d => d.Code)
            .ToListAsync(cancellationToken);

        var globalValues = await _dbContext.ParameterGlobalValues
            .IgnoreQueryFilters()
            .ToDictionaryAsync(v => v.ParameterDefinitionId, cancellationToken);

        var results = new List<ResolvedParameter>();
        foreach (var def in definitions)
        {
            var hasOverride = globalValues.TryGetValue(def.Id, out var globalValue);
            var effectiveValue = hasOverride ? globalValue.EffectiveValue : def.DefaultValue;
            var status = hasOverride ? globalValue.StatusId.ToString() : "Default";

            results.Add(new ResolvedParameter(
                def.Id,
                def.Code,
                def.Name,
                def.Description,
                def.DataTypeId,
                effectiveValue,
                def.DefaultValue,
                def.ScopeId,
                hasOverride,
                status));
        }

        return results;
    }

    public async Task<IReadOnlyList<ResolvedParameter>> GetTenantParametersAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var definitions = await _dbContext.ParameterDefinitions
            .Where(d => d.IsActive && (d.ScopeId == 2 || d.ScopeId == 3))
            .OrderBy(d => d.DisplayOrder)
            .ThenBy(d => d.Code)
            .ToListAsync(cancellationToken);

        var globalValues = await _dbContext.ParameterGlobalValues
            .IgnoreQueryFilters()
            .ToDictionaryAsync(v => v.ParameterDefinitionId, cancellationToken);

        var tenantValues = await _dbContext.ParameterTenantValues
            .IgnoreQueryFilters()
            .Where(v => v.TenantId == tenantId)
            .ToDictionaryAsync(v => v.ParameterDefinitionId, cancellationToken);

        var results = new List<ResolvedParameter>();
        foreach (var def in definitions)
        {
            var hasTenantOverride = tenantValues.TryGetValue(def.Id, out var tenantValue);
            var effectiveValue = hasTenantOverride
                ? tenantValue.OverrideValue
                : def.ScopeId == 3 && globalValues.TryGetValue(def.Id, out var globalValue)
                    ? globalValue.EffectiveValue
                    : def.DefaultValue;

            string status;
            if (hasTenantOverride)
                status = tenantValue.StatusId.ToString();
            else if (def.ScopeId == 3 && globalValues.ContainsKey(def.Id))
                status = "GlobalDefault";
            else
                status = "Default";

            results.Add(new ResolvedParameter(
                def.Id,
                def.Code,
                def.Name,
                def.Description,
                def.DataTypeId,
                effectiveValue,
                def.DefaultValue,
                def.ScopeId,
                hasTenantOverride,
                status));
        }

        return results;
    }

    public async Task<string> GetEffectiveValueAsync(Guid? tenantId, string code, CancellationToken cancellationToken = default)
    {
        var definition = await _dbContext.ParameterDefinitions
            .FirstOrDefaultAsync(d => d.Code == code && d.IsActive, cancellationToken);

        if (definition is null)
            return string.Empty;

        if (tenantId.HasValue && (definition.ScopeId == 2 || definition.ScopeId == 3))
        {
            var tenantValue = await _dbContext.ParameterTenantValues
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.TenantId == tenantId.Value && v.ParameterDefinitionId == definition.Id, cancellationToken);

            if (tenantValue is not null)
                return tenantValue.OverrideValue;
        }

        if (definition.ScopeId == 1 || definition.ScopeId == 3)
        {
            var globalValue = await _dbContext.ParameterGlobalValues
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(v => v.ParameterDefinitionId == definition.Id, cancellationToken);

            if (globalValue is not null)
                return globalValue.EffectiveValue;
        }

        return definition.DefaultValue;
    }
}