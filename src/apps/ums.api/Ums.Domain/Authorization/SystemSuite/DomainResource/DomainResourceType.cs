namespace Ums.Domain.Authorization.SystemSuite.DomainResource;

public class DomainResourceType : DomainEnumeration
{
    public static readonly DomainResourceType Aggregate = new(1, nameof(Aggregate));
    public static readonly DomainResourceType Entity = new(2, nameof(Entity));

    private DomainResourceType(int id, string name) : base(id, name) { }
}
