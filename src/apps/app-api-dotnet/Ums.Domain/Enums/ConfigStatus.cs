namespace Ums.Domain.Enums;

public class ConfigStatus : DomainEnumeration
{
    public static readonly ConfigStatus Draft     = new(1, nameof(Draft));
    public static readonly ConfigStatus Published = new(2, nameof(Published));
    public static readonly ConfigStatus Archived  = new(3, nameof(Archived));

    private ConfigStatus(int id, string name) : base(id, name) { }
}
