namespace Ums.Infrastructure.Configuration;

using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using Ums.Application.Configuration.Services;
using Ums.Domain.Configuration;
using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;

public sealed class ConfigurationProvider : IConfigurationProvider, IDisposable
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ConcurrentDictionary<string, AppConfigurationAggregate> _globalCache = new();
    private readonly ConcurrentDictionary<Guid, ConcurrentDictionary<string, AppConfigurationAggregate>> _tenantCaches = new();
    private readonly ConcurrentDictionary<string, AppConfigurationAggregate> _precedenceCache = new();
    private bool _isLoaded;
    private readonly object _loadLock = new();

    public event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;

    public ConfigurationProvider(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task LoadAsync(CancellationToken cancellationToken = default)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAppConfigurationRepository>();

        lock (_loadLock)
        {
            if (_isLoaded) return;
        }

        var globalConfigs = await repository.GetAllAsync(null, cancellationToken);
        foreach (var config in globalConfigs.Where(c => c.Scope.Id == 1))
        {
            _globalCache.TryAdd(config.Code.GetValue(), config);
            _precedenceCache.TryAdd(config.Code.GetValue(), config);
        }

        var tenantIds = globalConfigs
            .Where(c => c.Props.TenantId is not null)
            .Select(c => c.Props.TenantId!.GetValue())
            .Distinct();

        foreach (var tenantId in tenantIds)
        {
            var tenantConfigs = await repository.GetAllAsync(tenantId, cancellationToken);
            var tenantCache = new ConcurrentDictionary<string, AppConfigurationAggregate>();
            foreach (var config in tenantConfigs)
            {
                var code = config.Code.GetValue();
                tenantCache.TryAdd(code, config);
                var precedenceKey = $"{tenantId}:{code}";
                _precedenceCache.TryAdd(precedenceKey, config);
            }
            _tenantCaches.TryAdd(tenantId, tenantCache);
        }

        lock (_loadLock)
        {
            _isLoaded = true;
        }
    }

    public async Task ReloadAsync(CancellationToken cancellationToken = default)
    {
        _globalCache.Clear();
        _tenantCaches.Clear();
        _precedenceCache.Clear();

        lock (_loadLock)
        {
            _isLoaded = false;
        }

        await LoadAsync(cancellationToken);
    }

    public async Task ReloadTenantAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (_tenantCaches.TryRemove(tenantId, out var oldCache))
        {
            foreach (var code in oldCache.Keys)
            {
                var precedenceKey = $"{tenantId}:{code}";
                _precedenceCache.TryRemove(precedenceKey, out _);
            }
        }

        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAppConfigurationRepository>();

        var tenantConfigs = await repository.GetAllAsync(tenantId, cancellationToken);
        var tenantCache = new ConcurrentDictionary<string, AppConfigurationAggregate>();
        foreach (var config in tenantConfigs)
        {
            var code = config.Code.GetValue();
            tenantCache.TryAdd(code, config);
            var precedenceKey = $"{tenantId}:{code}";
            _precedenceCache.TryAdd(precedenceKey, config);
        }
        _tenantCaches.TryAdd(tenantId, tenantCache);
    }

    public AppConfigurationAggregate? GetGlobal(string code)
    {
        _globalCache.TryGetValue(code, out var config);
        return config;
    }

    public AppConfigurationAggregate? GetForTenant(Guid tenantId, string code)
    {
        if (_tenantCaches.TryGetValue(tenantId, out var tenantCache))
        {
            if (tenantCache.TryGetValue(code, out var config))
            {
                return config;
            }
        }
        return null;
    }

    public AppConfigurationAggregate? GetWithPrecedence(string code, Guid? tenantId = null)
    {
        if (tenantId.HasValue)
        {
            var precedenceKey = $"{tenantId}:{code}";
            if (_precedenceCache.TryGetValue(precedenceKey, out var tenantConfig))
            {
                return tenantConfig;
            }
        }

        _globalCache.TryGetValue(code, out var globalConfig);
        return globalConfig;
    }

    public string GetValue(string code, Guid? tenantId = null, string? defaultValue = null)
    {
        var config = GetWithPrecedence(code, tenantId);
        return config?.Props.Value.GetValue() ?? defaultValue ?? string.Empty;
    }

    public T GetValueAs<T>(string code, Guid? tenantId = null, T? defaultValue = default)
    {
        var value = GetValue(code, tenantId);
        if (string.IsNullOrEmpty(value) && defaultValue is not null)
        {
            return defaultValue;
        }

        try
        {
            if (typeof(T) == typeof(int))
                return (T)(object)int.Parse(value);
            if (typeof(T) == typeof(bool))
                return (T)(object)bool.Parse(value);
            if (typeof(T) == typeof(double))
                return (T)(object)double.Parse(value);
            if (typeof(T) == typeof(Guid))
                return (T)(object)Guid.Parse(value);
            if (typeof(T) == typeof(string))
                return (T)(object)value;

            return defaultValue ?? throw new NotSupportedException($"Type {typeof(T)} is not supported");
        }
        catch
        {
            if (defaultValue is not null)
                return defaultValue;
            throw;
        }
    }

    public bool HasOverride(string code, Guid tenantId)
    {
        var precedenceKey = $"{tenantId}:{code}";
        return _precedenceCache.ContainsKey(precedenceKey);
    }

    public IReadOnlyList<AppConfigurationAggregate> GetAllGlobal()
    {
        return _globalCache.Values.ToList();
    }

    public IReadOnlyList<AppConfigurationAggregate> GetAllForTenant(Guid tenantId)
    {
        if (_tenantCaches.TryGetValue(tenantId, out var cache))
        {
            return cache.Values.ToList();
        }
        return Array.Empty<AppConfigurationAggregate>();
    }

    public void Set(string code, string value, Guid? tenantId = null)
    {
        var oldValue = GetValue(code, tenantId);

        var config = tenantId.HasValue
            ? GetForTenant(tenantId.Value, code)
            : GetGlobal(code);

        if (config is null)
        {
            return;
        }

        ConfigurationChanged?.Invoke(this, new ConfigurationChangedEventArgs(code, tenantId, oldValue, value));
    }

    public void Dispose()
    {
        _globalCache.Clear();
        _tenantCaches.Clear();
        _precedenceCache.Clear();
    }
}