namespace Ums.Application.Common;

using Ums.Shell.Ddd;

internal static class DomainEnumerationParser
{
    public static T? FromName<T>(string? name)
        where T : DomainEnumeration
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        return DomainEnumeration
            .GetAll<T>()
            .FirstOrDefault(value => string.Equals(value.Name, name.Trim(), StringComparison.OrdinalIgnoreCase));
    }
}
