namespace Ums.Infrastructure.Persistence;

using System.Collections.Concurrent;
using Ums.Domain.Configuration;
using Ums.Domain.Configuration.Parameter;

/// <summary>
/// In-memory implementation of all three Parameter repositories for dev/test environments.
/// Uses explicit interface implementation to resolve GetByIdAsync / SaveChangesAsync
/// signature collisions between the three interfaces.
/// </summary>
public sealed class InMemoryParameterRepositories
    : IParameterDefinitionRepository,
      IParameterGlobalValueRepository,
      IParameterTenantValueRepository
{
    private readonly ConcurrentDictionary<Guid, ParameterDefinition>  _defs   = new();
    private readonly ConcurrentDictionary<Guid, ParameterGlobalValue> _gv     = new();
    private readonly ConcurrentDictionary<Guid, ParameterTenantValue> _tv     = new();

    // ── IParameterDefinitionRepository ───────────────────────────────────────

    Task<ParameterDefinition?> IParameterDefinitionRepository.GetByIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_defs.GetValueOrDefault(id));

    Task<ParameterDefinition?> IParameterDefinitionRepository.GetByCodeAsync(string code, CancellationToken ct)
        => Task.FromResult(_defs.Values.FirstOrDefault(d =>
            string.Equals(d.Props.Code.GetValue(), code, StringComparison.OrdinalIgnoreCase)));

    Task<IReadOnlyList<ParameterDefinition>> IParameterDefinitionRepository.GetAllAsync(CancellationToken ct)
        => Task.FromResult<IReadOnlyList<ParameterDefinition>>(
            _defs.Values.OrderBy(d => d.Props.DisplayOrder).ToList());

    Task IParameterDefinitionRepository.AddAsync(ParameterDefinition d, CancellationToken ct)
    {
        _defs[d.Props.Id.GetValue()] = d;
        return Task.CompletedTask;
    }

    Task IParameterDefinitionRepository.UpdateAsync(ParameterDefinition d, CancellationToken ct)
    {
        _defs[d.Props.Id.GetValue()] = d;
        return Task.CompletedTask;
    }

    Task<int> IParameterDefinitionRepository.CountByCodeAsync(string code, CancellationToken ct)
        => Task.FromResult(_defs.Values.Count(d =>
            string.Equals(d.Props.Code.GetValue(), code, StringComparison.OrdinalIgnoreCase)));

    Task<int> IParameterDefinitionRepository.CountGlobalValuesAsync(Guid defId, CancellationToken ct)
        => Task.FromResult(_gv.Values.Count(v => v.Props.ParameterDefinitionId.GetValue() == defId));

    Task<int> IParameterDefinitionRepository.CountTenantValuesAsync(Guid defId, CancellationToken ct)
        => Task.FromResult(_tv.Values.Count(v => v.Props.ParameterDefinitionId.GetValue() == defId));

    Task<bool> IParameterDefinitionRepository.SaveChangesAsync(CancellationToken ct) => Task.FromResult(true);

    // ── IParameterGlobalValueRepository ──────────────────────────────────────

    Task<ParameterGlobalValue?> IParameterGlobalValueRepository.GetByIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_gv.GetValueOrDefault(id));

    Task<ParameterGlobalValue?> IParameterGlobalValueRepository.GetByDefinitionIdAsync(Guid defId, CancellationToken ct)
        => Task.FromResult(_gv.Values.FirstOrDefault(v => v.Props.ParameterDefinitionId.GetValue() == defId));

    Task IParameterGlobalValueRepository.AddAsync(ParameterGlobalValue v, CancellationToken ct)
    {
        _gv[v.Props.Id.GetValue()] = v;
        return Task.CompletedTask;
    }

    Task IParameterGlobalValueRepository.UpdateAsync(ParameterGlobalValue v, CancellationToken ct)
    {
        _gv[v.Props.Id.GetValue()] = v;
        return Task.CompletedTask;
    }

    Task<bool> IParameterGlobalValueRepository.SaveChangesAsync(CancellationToken ct) => Task.FromResult(true);

    // ── IParameterTenantValueRepository ──────────────────────────────────────

    Task<ParameterTenantValue?> IParameterTenantValueRepository.GetByIdAsync(Guid id, CancellationToken ct)
        => Task.FromResult(_tv.GetValueOrDefault(id));

    Task<ParameterTenantValue?> IParameterTenantValueRepository.GetByTenantAndDefinitionAsync(
        Guid tenantId, Guid defId, CancellationToken ct)
        => Task.FromResult(_tv.Values.FirstOrDefault(v =>
            v.Props.TenantId.GetValue() == tenantId &&
            v.Props.ParameterDefinitionId.GetValue() == defId));

    Task IParameterTenantValueRepository.AddAsync(ParameterTenantValue v, CancellationToken ct)
    {
        _tv[v.Props.Id.GetValue()] = v;
        return Task.CompletedTask;
    }

    Task IParameterTenantValueRepository.UpdateAsync(ParameterTenantValue v, CancellationToken ct)
    {
        _tv[v.Props.Id.GetValue()] = v;
        return Task.CompletedTask;
    }

    Task<bool> IParameterTenantValueRepository.SaveChangesAsync(CancellationToken ct) => Task.FromResult(true);
}
