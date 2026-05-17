namespace Ums.Domain.Identity.System.Configuration;

public class Configuration
{
    public ConfigurationKey Key { get; }
    public ConfigurationValue Value { get; }
    public ConfigurationScope Scope { get; }

    private Configuration(ConfigurationKey key, ConfigurationValue value, ConfigurationScope scope)
    {
        Key = key;
        Value = value;
        Scope = scope;
    }

    public static Result<Configuration> Create(ConfigurationKey key, ConfigurationValue value, ConfigurationScope scope)
    {
        if (string.IsNullOrWhiteSpace(key.GetValue()))
        {
            return Result<Configuration>.Failure(DomainErrors.ValueObject.PropertyRequired);
        }

        if (string.IsNullOrWhiteSpace(value.GetValue()))
        {
            return Result<Configuration>.Failure(DomainErrors.ValueObject.PropertyRequired);
        }

        var configuration = new Configuration(key, value, scope);
        return Result<Configuration>.Success(configuration);
    }

    public Configuration WithValue(ConfigurationValue newValue)
    {
        return new Configuration(Key, newValue, Scope);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Configuration other) return false;
        return Key.GetValue() == other.Key.GetValue() && Scope.Id == other.Scope.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Key.GetValue(), Scope.Id);
    }
}
