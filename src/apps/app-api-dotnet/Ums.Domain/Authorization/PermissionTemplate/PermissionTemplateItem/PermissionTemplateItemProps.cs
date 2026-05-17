namespace Ums.Domain.Authorization.PermissionTemplate.PermissionTemplateItem;

public class PermissionTemplateItemProps : IProps
{
    public IdValueObject Id { get; set; }
    public PermissionTemplateId TemplateId { get; set; }
    public ExclusiveArcTarget TargetType { get; set; }
    public IdValueObject TargetId { get; set; }
    public ActionId? ActionId { get; set; }
    public PermissionEffect Effect { get; set; }
    public PermissionState ItemStatus { get; set; }
    public AuditValueObject Audit { get; private set; }

    public PermissionTemplateItemProps(
        IdValueObject id,
        PermissionTemplateId templateId,
        ExclusiveArcTarget targetType,
        IdValueObject targetId,
        ActionId? actionId,
        PermissionEffect effect,
        PermissionState itemStatus,
        ActorId createdBy)
    {
        Id = id;
        TemplateId = templateId;
        TargetType = targetType;
        TargetId = targetId;
        ActionId = actionId;
        Effect = effect;
        ItemStatus = itemStatus;
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
