namespace Ums.Domain.Authorization.Template;

using Ums.Domain.Events;
using Ums.Domain.Authorization.Template.PermissionTemplateItem;
using PermissionTemplateItemEntity = Ums.Domain.Authorization.Template.PermissionTemplateItem.PermissionTemplateItem;

public sealed class PermissionTemplate : AggregateRoot<PermissionTemplate, PermissionTemplateProps>
{
    private readonly List<PermissionTemplateItemEntity> _items = new();

    public new PermissionTemplateDomainEventsManager DomainEvents { get; }

    private PermissionTemplate(PermissionTemplateProps props) : base(props)
    {
        DomainEvents = new PermissionTemplateDomainEventsManager(this);

        if (TrackingState.IsNew)
        {
            DomainEvents.RaiseEvent(new PermissionTemplateCreatedEvent(
                Props.Id.GetValue(),
                Props.TenantId.GetValue(),
                Props.RoleId.GetValue(),
                Props.SystemSuiteId.GetValue(),
                Props.Version.GetValue()));
        }
    }

    public TenantId TenantId => Props.TenantId;
    public RoleId RoleId => Props.RoleId;
    public SystemSuiteId SystemSuiteId => Props.SystemSuiteId;
    public new TemplateVersion Version => Props.Version;
    public TemplateStatus Status => Props.Status;

    public IReadOnlyCollection<PermissionTemplateItemEntity> Items => _items.AsReadOnly();

    public TemplateId GetId() => TemplateId.Load(Props.Id.GetValue());

    public static Result<PermissionTemplate> Create(
        TenantId tenantId,
        RoleId roleId,
        SystemSuiteId systemSuiteId,
        ActorId createdBy)
    {
        var props = new PermissionTemplateProps(
            IdValueObject.Create(),
            tenantId,
            roleId,
            systemSuiteId,
            TemplateVersion.Initial(),
            TemplateStatus.Draft,
            createdBy);

        var template = new PermissionTemplate(props);

        if (!template.IsValid())
        {
            return Result<PermissionTemplate>.Failure(template.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<PermissionTemplate>.Success(template);
    }

    public Result Publish(ActorId updatedBy)
    {
        if (Status != TemplateStatus.Draft)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Authorization.TemplateNotDraft));
        }

        if (!Items.Any())
        {
            BrokenRules.Add(new BrokenRule(nameof(Items), DomainErrors.Authorization.TemplateItemsRequired));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = TemplateStatus.Published;
        DomainEvents.RaiseEvent(new PermissionTemplatePublishedEvent(Props.Id.GetValue(), Props.Version.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Deprecate(ActorId updatedBy)
    {
        if (Status != TemplateStatus.Published)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Authorization.TemplateNotPublished));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = TemplateStatus.Deprecated;
        DomainEvents.RaiseEvent(new PermissionTemplateDeprecatedEvent(Props.Id.GetValue(), Props.Version.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Delete(ActorId deletedBy, int activeProfileCount = 0)
    {
        if (Status != TemplateStatus.Draft && Status != TemplateStatus.Deprecated)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Authorization.TemplateNotDeletable));
        }

        if (activeProfileCount > 0)
        {
            BrokenRules.Add(new BrokenRule(nameof(Items), DomainErrors.Authorization.TemplateHasActiveProfiles));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        DomainEvents.RaiseEvent(new PermissionTemplateDeletedEvent(Props.Id.GetValue(), Props.Version.GetValue()));
        Props.Audit.Update(deletedBy.GetValue());
        return Result.Success();
    }

    public Result AddItem(ExclusiveArcTarget targetType, IdValueObject targetId, ActionId actionId, bool isAllowed, bool isDenied, ActorId createdBy)
    {
        if (Status != TemplateStatus.Draft)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Authorization.TemplateNotDraft));
        }

        if (_items.Any(i => i.TargetType == targetType && i.TargetId == targetId && i.ActionId == actionId))
        {
            BrokenRules.Add(new BrokenRule(nameof(Items), DomainErrors.Authorization.TemplateItemTargetAlreadyExists));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var itemResult = PermissionTemplateItemEntity.Create(GetId(), targetType, targetId, actionId, isAllowed, isDenied, createdBy);
        if (itemResult.IsFailure)
        {
            return Result.Failure(itemResult.Error);
        }

        _items.Add(itemResult.Value);
        TrackingState.MarkAsDirty();
        DomainEvents.RaiseEvent(new PermissionTemplateMutatedEvent(Props.Id.GetValue(), Props.Version.GetValue()));
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result SetItemAllow(IdValueObject itemId, ActorId updatedBy)
    {
        return ExecuteOnItem(itemId, item => item.SetAllow(updatedBy), updatedBy);
    }

    public Result SetItemDeny(IdValueObject itemId, ActorId updatedBy)
    {
        return ExecuteOnItem(itemId, item => item.SetDeny(updatedBy), updatedBy);
    }

    public Result SetItemNeutral(IdValueObject itemId, ActorId updatedBy)
    {
        return ExecuteOnItem(itemId, item => item.SetNeutral(updatedBy), updatedBy);
    }

    public Result ActivateItem(IdValueObject itemId, ActorId updatedBy)
    {
        return ExecuteOnItem(itemId, item => item.Activate(updatedBy), updatedBy);
    }

    public Result DeactivateItem(IdValueObject itemId, ActorId updatedBy)
    {
        return ExecuteOnItem(itemId, item => item.Deactivate(updatedBy), updatedBy);
    }

    public Result RemoveItem(IdValueObject itemId, ActorId updatedBy)
    {
        if (Status != TemplateStatus.Draft)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Authorization.TemplateNotDraft));
        }

        var item = FindItem(itemId);
        if (item.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Items), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _items.Remove(item.Value);
        TrackingState.MarkAsDirty();
        DomainEvents.RaiseEvent(new PermissionTemplateMutatedEvent(Props.Id.GetValue(), Props.Version.GetValue()));
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    private Result ExecuteOnItem(IdValueObject itemId, Func<PermissionTemplateItemEntity, Result> action, ActorId updatedBy)
    {
        if (Status != TemplateStatus.Draft)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Authorization.TemplateNotDraft));
        }

        var item = FindItem(itemId);
        if (item.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Items), DomainErrors.Common.NotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var result = action(item.Value);
        if (result.IsFailure)
        {
            return result;
        }

        TrackingState.MarkAsDirty();
        DomainEvents.RaiseEvent(new PermissionTemplateMutatedEvent(Props.Id.GetValue(), Props.Version.GetValue()));
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    private Result<PermissionTemplateItemEntity> FindItem(IdValueObject itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id.GetValue() == itemId.GetValue());
        return item is null
            ? Result<PermissionTemplateItemEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<PermissionTemplateItemEntity>.Success(item);
    }

}
