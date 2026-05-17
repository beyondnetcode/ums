namespace Ums.Domain.Enums;

public class ConfigurationScope : DomainEnumeration
{
    public static readonly ConfigurationScope Global = new(1, nameof(Global));
    public static readonly ConfigurationScope Tenant = new(2, nameof(Tenant));
    public static readonly ConfigurationScope User = new(3, nameof(User));

    private ConfigurationScope(int id, string name) : base(id, name) { }
}
