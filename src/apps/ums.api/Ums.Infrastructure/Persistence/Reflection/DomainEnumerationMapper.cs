using Ums.Shell.Ddd;

namespace Ums.Infrastructure.Persistence.Reflection;

internal static class DomainEnumerationMapper
{
    public static TEnum FromValue<TEnum>(int value) where TEnum : DomainEnumeration
    {
        return DomainEnumeration.FromValue<TEnum>(value)
            ?? throw new InvalidOperationException($"Enumeration {typeof(TEnum).Name} does not define value {value}.");
    }
}
