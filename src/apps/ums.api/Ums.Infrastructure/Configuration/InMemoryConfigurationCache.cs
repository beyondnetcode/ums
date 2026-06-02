namespace Ums.Infrastructure.Configuration;

using System.Collections.Concurrent;
using Ums.Application.Configuration.Services;
using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;

/// <summary>
/// In-process, thread-safe implementation of <see cref="IConfigurationCache"/>.
///
/// TD-003: This is the Phase-1 implementation. When Redis infrastructure is available,
/// replace this registration with a RedisConfigurationCache that wraps IDistributedCache
/// (already used for token revocation). No other code changes are required because all
/// consumers depend on IConfigurationCache, not on this concrete class.
///
/// The three dictionaries mirror the three access patterns:
///   _global    → fast O(1) lookup by code for global parameters
///   _tenant    → per-tenant dictionary for tenant-specific overrides
///   _precedence → merged view (tenant override wins over global) keyed as "tenantId:code"
/// </summary>
public sealed class InMemoryConfigurationCache : IConfigurationCache
{
    private readonly ConcurrentDictionary<string, AppConfigurationAggregate> _global = new(StringComparer.OrdinalIgnoreCase);
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, AppConfigurationAggregate>> _tenant = new();
    private readonly ConcurrentDictionary<string, AppConfigurationAggregate> _precedence = new(StringComparer.OrdinalIgnoreCase);

    // ── Read ────────────────────────────────────────────────────────────────

    public AppConfigurationAggregate? GetGlobal(string code)
    {
        _global.TryGetValue(code, out var config);
        return config;
    }

    public AppConfigurationAggregate? GetForTenant(Guid tenantId, string code)
    {
        if (_tenant.TryGetValue(tenantId, out var tenantCache) &&
            tenantCache.TryGetValue(code, out var config))
        {
            return config;
        }
        return null;
    }

    public AppConfigurationAggregate? GetWithPrecedence(string code, Guid? tenantId)
    {
        if (tenantId.HasValue)
        {
            var key = PrecedenceKey(tenantId.Value, code);
            if (_precedence.TryGetValue(key, out var tenantConfig))
                return tenantConfig;
        }

        _global.TryGetValue(code, out var globalConfig);
        return globalConfig;
    }

    public IReadOnlyList<AppConfigurationAggregate> GetAllGlobal()
        => _global.Values.ToList();

    public IReadOnlyList<AppConfigurationAggregate> GetAllForTenant(Guid tenantId)
        => _tenant.TryGetValue(tenantId, out var cache)
            ? cache.Values.ToList()
            : [];

    public bool HasTenantOverride(string code, Guid tenantId)
        => _precedence.ContainsKey(PrecedenceKey(tenantId, code));

    // ── Write ────────────────────────────────────────────────────────────────

    public void PopulateGlobal(IEnumerable<AppConfigurationAggregate> configs)
    {
        foreach (var config in configs)
        {
            var code = config.Code.GetValue();
            _global[code] = config;
            _precedence.TryAdd(code, config);
        }
    }

    public void PopulateTenant(Guid tenantId, IEnumerable<AppConfigurationAggregate> configs)
    {
        var cache = _tenant.GetOrAdd(tenantId, _ => new ConcurrentDictionary<string, AppConfigurationAggregate>(StringComparer.OrdinalIgnoreCase));

        foreach (var config in configs)
        {
            var code = config.Code.GetValue();
            cache[code] = config;
            _precedence[PrecedenceKey(tenantId, code)] = config;
        }
    }

    public void InvalidateTenant(Guid tenantId)
    {
        if (_tenant.TryRemove(tenantId, out var removed))
        {
            foreach (var code in removed.Keys)
                _precedence.TryRemove(PrecedenceKey(tenantId, code), out _);
        }
    }

    public void InvalidateAll()
    {
        _global.Clear();
        _tenant.Clear();
        _precedence.Clear();
    }

    private static string PrecedenceKey(Guid tenantId, string code) => $"{tenantId}:{code}";
}
