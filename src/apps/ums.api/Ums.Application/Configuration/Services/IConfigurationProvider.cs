namespace Ums.Application.Configuration.Services;

using Ums.Domain.Configuration;
using Ums.Domain.Configuration.AppConfiguration;
using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;

public interface IConfigurationProvider
{
    AppConfigurationAggregate? GetGlobal(string code);

    AppConfigurationAggregate? GetForTenant(Guid tenantId, string code);

    AppConfigurationAggregate? GetWithPrecedence(string code, Guid? tenantId = null);

    string GetValue(string code, Guid? tenantId = null, string? defaultValue = null);

    T GetValueAs<T>(string code, Guid? tenantId = null, T? defaultValue = default);

    bool HasOverride(string code, Guid tenantId);

    IReadOnlyList<AppConfigurationAggregate> GetAllGlobal();

    IReadOnlyList<AppConfigurationAggregate> GetAllForTenant(Guid tenantId);

    void Set(string code, string value, Guid? tenantId = null);

    Task LoadAsync(CancellationToken cancellationToken = default);

    Task ReloadAsync(CancellationToken cancellationToken = default);

    Task ReloadTenantAsync(Guid tenantId, CancellationToken cancellationToken = default);

    event EventHandler<ConfigurationChangedEventArgs>? ConfigurationChanged;
}

public sealed class ConfigurationChangedEventArgs : EventArgs
{
    public string Code { get; }
    public Guid? TenantId { get; }
    public string? OldValue { get; }
    public string? NewValue { get; }
    public DateTime Timestamp { get; }

    public ConfigurationChangedEventArgs(string code, Guid? tenantId, string? oldValue, string? newValue)
    {
        Code = code;
        TenantId = tenantId;
        OldValue = oldValue;
        NewValue = newValue;
        Timestamp = DateTime.UtcNow;
    }
}