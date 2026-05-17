namespace Ums.Domain.Authorization.PermissionTemplate;

using Ums.Domain.Events;
using Ums.Domain.Authorization.PermissionTemplate.PermissionTemplateItem;
using PermissionTemplateItemEntity = Ums.Domain.Authorization.PermissionTemplate.PermissionTemplateItem.PermissionTemplateItem;

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
                Props.RoleId.GetValue(),
                Props.SystemSuiteId.GetValue(),
                Props.Version.GetValue()));
        }
    }

    public RoleId RoleId => Props.RoleId;
    public SystemSuiteId SystemSuiteId => Props.SystemSuiteId;
    public Code Code => Props.Code;
    public Name Name => Props.Name;
    public Description Description => Props.Description;
    public new TemplateVersion Version => Props.Version;
    public TemplateStatus Status => Props.Status;

    public IReadOnlyCollection<PermissionTemplateItemEntity> Items => _items.AsReadOnly();

    public PermissionTemplateId GetId() => PermissionTemplateId.Load(Props.Id.GetValue());

    public static Result<PermissionTemplate> Create(
        RoleId roleId,
        SystemSuiteId systemSuiteId,
        Code code,
        Name name,
        Description description,
        ActorId createdBy)
    {
        var props = new PermissionTemplateProps(
            IdValueObject.Create(),
            roleId,
            systemSuiteId,
            code,
            name,
            description,
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

        if (Status == TemplateStatus.Deprecated)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Authorization.TemplateAlreadyDeprecated));
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

    public Result AddItem(ExclusiveArcTarget targetType, IdValueObject targetId, ActionId? actionId, PermissionEffect effect, ActorId createdBy)
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

        var itemResult = PermissionTemplateItemEntity.Create(GetId(), targetType, targetId, actionId, effect, createdBy);
        if (itemResult.IsFailure)
        {
            return Result.Failure(itemResult.Error);
        }

        _items.Add(itemResult.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(createdBy.GetValue());
        return Result.Success();
    }

    public Result UpdateItemEffect(IdValueObject itemId, PermissionEffect newEffect, ActorId updatedBy)
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

        var updateResult = item.Value.UpdateEffect(newEffect, updatedBy);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        TrackingState.MarkAsDirty();
        DomainEvents.RaiseEvent(new PermissionTemplateMutatedEvent(Props.Id.GetValue(), Props.Version.GetValue()));
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result UpdateItemStatus(IdValueObject itemId, PermissionState newStatus, ActorId updatedBy)
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

        var updateResult = item.Value.UpdateItemStatus(newStatus, updatedBy);
        if (updateResult.IsFailure)
        {
            return Result.Failure(updateResult.Error);
        }

        TrackingState.MarkAsDirty();
        DomainEvents.RaiseEvent(new PermissionTemplateMutatedEvent(Props.Id.GetValue(), Props.Version.GetValue()));
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
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

    private Result<PermissionTemplateItemEntity> FindItem(IdValueObject itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id.GetValue() == itemId.GetValue());
        return item is null
            ? Result<PermissionTemplateItemEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<PermissionTemplateItemEntity>.Success(item);
    }
}
