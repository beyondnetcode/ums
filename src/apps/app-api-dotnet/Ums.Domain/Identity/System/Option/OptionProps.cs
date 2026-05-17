namespace Ums.Domain.Identity.System.Option;

public class OptionProps : IProps
{
    public IdValueObject Id { get; set; }
    public SubMenuId SubMenuId { get; set; }
    public Code Code { get; set; }
    public Name Label { get; set; }
    public Description Description { get; set; }
    public ActionCode ActionCode { get; set; }
    public int SortOrder { get; set; }
    public AuditValueObject Audit { get; private set; }

    public OptionProps(
        IdValueObject id,
        SubMenuId subMenuId,
        Code code,
        Name label,
        Description description,
        ActionCode actionCode,
        int sortOrder,
        ActorId createdBy)
    {
        Id = id;
        SubMenuId = subMenuId;
        Code = code;
        Label = label;
        Description = description;
        ActionCode = actionCode;
        SortOrder = sortOrder;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
