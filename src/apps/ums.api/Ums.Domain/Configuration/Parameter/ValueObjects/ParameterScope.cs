namespace Ums.Domain.Configuration.Parameter.ValueObjects;

public sealed class ParameterScope : DomainEnumeration
{
    public static readonly ParameterScope GlobalOnly = new(1, nameof(GlobalOnly));
    public static readonly ParameterScope TenantOnly = new(2, nameof(TenantOnly));
    public static readonly ParameterScope GlobalAndTenant = new(3, nameof(GlobalAndTenant));

    public bool SupportsGlobal => Id == 1 || Id == 3;
    public bool SupportsTenant => Id == 2 || Id == 3;

    private ParameterScope(int value, string name) : base(value, name) { }

    public static IEnumerable<ParameterScope> AllScopes => [GlobalOnly, TenantOnly, GlobalAndTenant];

    public static ParameterScope FromValue(int value) =>
        AllScopes.FirstOrDefault(s => s.Id == value)
        ?? throw new ArgumentOutOfRangeException(nameof(value), $"Invalid scope value: {value}");
}