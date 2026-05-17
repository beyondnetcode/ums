namespace Ums.Domain.Enums;

public class SystemStatus : DomainEnumeration
{
    public static readonly SystemStatus Active = new(1, nameof(Active));
    public static readonly SystemStatus Maintenance = new(2, nameof(Maintenance));
    public static readonly SystemStatus Deprecated = new(3, nameof(Deprecated));

    private SystemStatus(int id, string name) : base(id, name) { }
}
