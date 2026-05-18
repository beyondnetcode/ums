namespace Ums.Domain.Authorization.SystemSuite.AppSetting;

public class AppSetting
{
    public ConfigurationKey Key { get; }
    public ConfigurationValue Value { get; }
    public ConfigurationScope Scope { get; }

    private AppSetting(ConfigurationKey key, ConfigurationValue value, ConfigurationScope scope)
    {
        Key = key;
        Value = value;
        Scope = scope;
    }

    public static Result<AppSetting> Create(ConfigurationKey key, ConfigurationValue value, ConfigurationScope scope)
    {
        if (string.IsNullOrWhiteSpace(key.GetValue()))
        {
            return Result<AppSetting>.Failure(DomainErrors.ValueObject.PropertyRequired);
        }

        if (string.IsNullOrWhiteSpace(value.GetValue()))
        {
            return Result<AppSetting>.Failure(DomainErrors.ValueObject.PropertyRequired);
        }

        return Result<AppSetting>.Success(new AppSetting(key, value, scope));
    }

    public AppSetting WithValue(ConfigurationValue newValue) => new(Key, newValue, Scope);

    public override bool Equals(object? obj)
    {
        if (obj is not AppSetting other) return false;
        return Key.GetValue() == other.Key.GetValue() && Scope.Id == other.Scope.Id;
    }

    public override int GetHashCode() => HashCode.Combine(Key.GetValue(), Scope.Id);
}
