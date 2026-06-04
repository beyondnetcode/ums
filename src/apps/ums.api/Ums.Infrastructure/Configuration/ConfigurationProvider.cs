namespace Ums.Infrastructure.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Ums.Application.Configuration.Services;
using Ums.Domain.Configuration;
using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;

/// <summary>
/// Singleton implementation of <see cref="IConfigurationProvider"/>.
///
/// Delegates all storage to <see cref="IConfigurationCache"/> (TD-003: today in-memory,
/// future Redis). ConfigurationProvider handles load/reload orchestration and precedence
/// resolution; the cache handles the actual storage.
/// </summary>
public sealed class ConfigurationProvider : IConfigurationProvider, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfigurationCache _cache;
    private bool _isLoaded;
    private readonly object _loadLock = new();

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public ConfigurationProvider(IServiceScopeFactory scopeFactory, IConfigurationCache cache)
    {
        _scopeFactory = scopeFactory;
        _cache = cache;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        lock (_loadLock)
        {
            if (_isLoaded) return;
        }

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAppConfigurationRepository>();

        var allConfigs = await repository.GetAllAsync(null, cancellationToken);

        // Populate global entries (Scope.Id == 1 = GlobalOnly)
        _cache.PopulateGlobal(allConfigs.Where(c => c.Scope.Id == 1));

        // Populate per-tenant entries
        var tenantIds = allConfigs
            .Where(c => c.Props.TenantId is not null)
            .Select(c => c.Props.TenantId!.GetValue())
            .Distinct();

        foreach (var tenantId in tenantIds)
        {
            var tenantConfigs = await repository.GetAllAsync(tenantId, cancellationToken);

            // Bucket each config into the correct scope cache (BR-1: Module > Suite > Tenant > Global).
            _cache.PopulateTenant(tenantId, tenantConfigs.Where(c => c.Scope.Id == 2));

            foreach (var grp in tenantConfigs.Where(c => c.Scope.Id == 4 && c.Props.SystemSuiteId is not null)
                                             .GroupBy(c => c.Props.SystemSuiteId!.GetValue()))
                _cache.PopulateSuite(grp.Key, grp);

            foreach (var grp in tenantConfigs.Where(c => c.Scope.Id == 5 && c.Props.ModuleId is not null)
                                             .GroupBy(c => c.Props.ModuleId!.GetValue()))
                _cache.PopulateModule(grp.Key, grp);
        }

        lock (_loadLock)
        {
            _isLoaded = true;
        }
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        _cache.InvalidateAll();

        lock (_loadLock)
        {
            _isLoaded = false;
        }

        await LoadAsync(cancellationToken);
    }

    public async Task ReloadTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        _cache.InvalidateTenant(tenantId);

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAppConfigurationRepository>();

        var tenantConfigs = await repository.GetAllAsync(tenantId, cancellationToken);

        _cache.PopulateTenant(tenantId, tenantConfigs.Where(c => c.Scope.Id == 2));

        foreach (var grp in tenantConfigs.Where(c => c.Scope.Id == 4 && c.Props.SystemSuiteId is not null)
                                         .GroupBy(c => c.Props.SystemSuiteId!.GetValue()))
            _cache.PopulateSuite(grp.Key, grp);

        foreach (var grp in tenantConfigs.Where(c => c.Scope.Id == 5 && c.Props.ModuleId is not null)
                                         .GroupBy(c => c.Props.ModuleId!.GetValue()))
            _cache.PopulateModule(grp.Key, grp);
    }

    // ── Reads (delegate to cache) ─────────────────────────────────────────────

    public AppConfigurationAggregate? GetGlobal(string code)
        => _cache.GetGlobal(code);

    public AppConfigurationAggregate? GetForTenant(Guid tenantId, string code)
        => _cache.GetForTenant(tenantId, code);

    public AppConfigurationAggregate? GetWithPrecedence(string code, Guid? tenantId = null, Guid? suiteId = null, Guid? moduleId = null)
        => _cache.GetWithPrecedence(code, tenantId, suiteId, moduleId);

    public string GetValue(string code, Guid? tenantId = null, string? defaultValue = null)
    {
        var config = GetWithPrecedence(code, tenantId);
        return config?.Props.Value.GetValue() ?? defaultValue ?? string.Empty;
    }

    public string GetValue(string code, Guid? tenantId, Guid? suiteId, Guid? moduleId, string? defaultValue = null)
    {
        var config = GetWithPrecedence(code, tenantId, suiteId, moduleId);
        return config?.Props.Value.GetValue() ?? defaultValue ?? string.Empty;
    }

    public T GetValueAs<T>(string code, Guid? tenantId = null, T? defaultValue = default)
    {
        var value = GetValue(code, tenantId);
        if (string.IsNullOrEmpty(value) && defaultValue is not null)
            return defaultValue;

        try
        {
            if (typeof(T) == typeof(int))    return (T)(object)int.Parse(value);
            if (typeof(T) == typeof(bool))   return (T)(object)bool.Parse(value);
            if (typeof(T) == typeof(double)) return (T)(object)double.Parse(value);
            if (typeof(T) == typeof(Guid))   return (T)(object)Guid.Parse(value);
            if (typeof(T) == typeof(string)) return (T)(object)value;
            return defaultValue ?? throw new NotSupportedException($"Type {typeof(T)} is not supported");
        }
        catch
        {
            if (defaultValue is not null) return defaultValue;
            throw;
        }
    }

    public T GetValueAs<T>(string code, Guid? tenantId, Guid? suiteId, Guid? moduleId, T? defaultValue = default)
    {
        var value = GetValue(code, tenantId, suiteId, moduleId);
        if (string.IsNullOrEmpty(value) && defaultValue is not null)
            return defaultValue;

        try
        {
            if (typeof(T) == typeof(int))    return (T)(object)int.Parse(value);
            if (typeof(T) == typeof(bool))   return (T)(object)bool.Parse(value);
            if (typeof(T) == typeof(double)) return (T)(object)double.Parse(value);
            if (typeof(T) == typeof(Guid))   return (T)(object)Guid.Parse(value);
            if (typeof(T) == typeof(string)) return (T)(object)value;
            return defaultValue ?? throw new NotSupportedException($"Type {typeof(T)} is not supported");
        }
        catch
        {
            if (defaultValue is not null) return defaultValue;
            throw;
        }
    }

    public bool HasOverride(string code, Guid tenantId)
        => _cache.HasTenantOverride(code, tenantId);

    public IReadOnlyList<AppConfigurationAggregate> GetAllGlobal()
        => _cache.GetAllGlobal();

    public IReadOnlyList<AppConfigurationAggregate> GetAllForTenant(Guid tenantId)
        => _cache.GetAllForTenant(tenantId);

    public void Set(string code, string value, Guid? tenantId = null)
    {
        var oldValue = GetValue(code, tenantId);
        ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(code, tenantId, oldValue, value));
    }

    public void Dispose() => _cache.InvalidateAll();
}
