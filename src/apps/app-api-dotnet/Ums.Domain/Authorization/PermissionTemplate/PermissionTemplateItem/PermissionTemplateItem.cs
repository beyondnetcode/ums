namespace Ums.Domain.Authorization.PermissionTemplate.PermissionTemplateItem;

public sealed class PermissionTemplateItem : Entity<PermissionTemplateItem, PermissionTemplateItemProps>
{
    private PermissionTemplateItem(PermissionTemplateItemProps props) : base(props)
    {
    }

    public PermissionTemplateId TemplateId => Props.TemplateId;
    public ExclusiveArcTarget TargetType => Props.TargetType;
    public IdValueObject TargetId => Props.TargetId;
    public ActionId? ActionId => Props.ActionId;
    public PermissionEffect Effect => Props.Effect;
    public PermissionState ItemStatus => Props.ItemStatus;

    public PermissionTemplateItemId GetId() => PermissionTemplateItemId.Load(Props.Id.GetValue());

    public static Result<PermissionTemplateItem> Create(
        PermissionTemplateId templateId,
        ExclusiveArcTarget targetType,
        IdValueObject targetId,
        ActionId? actionId,
        PermissionEffect effect,
        ActorId createdBy)
    {
        var props = new PermissionTemplateItemProps(IdValueObject.Create(), templateId, targetType, targetId, actionId, effect, PermissionState.Enabled, createdBy);
        var item = new PermissionTemplateItem(props);

        if (!item.IsValid())
        {
            return Result<PermissionTemplateItem>.Failure(item.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<PermissionTemplateItem>.Success(item);
    }

    public Result UpdateEffect(PermissionEffect newEffect, ActorId updatedBy)
    {
        Props.Effect = newEffect;
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result UpdateItemStatus(PermissionState newStatus, ActorId updatedBy)
    {
        Props.ItemStatus = newStatus;
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }
}
