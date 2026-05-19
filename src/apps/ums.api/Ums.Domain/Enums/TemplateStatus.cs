namespace Ums.Domain.Enums;

public class TemplateStatus : DomainEnumeration
{
    public static readonly TemplateStatus Draft = new(1, nameof(Draft));
    public static readonly TemplateStatus Published = new(2, nameof(Published));
    public static readonly TemplateStatus Deprecated = new(3, nameof(Deprecated));

    private TemplateStatus(int id, string name) : base(id, name) { }
}
