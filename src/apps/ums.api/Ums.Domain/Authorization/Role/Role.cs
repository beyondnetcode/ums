namespace Ums.Domain.Authorization.Role;

using Ums.Domain.Events;

public sealed class Role : AggregateRoot<Role, RoleProps>
{
    public new RoleDomainEventsManager DomainEvents { get; }

    private Role(RoleProps props) : base(props)
    {
        DomainEvents = new RoleDomainEventsManager(this);

        if (TrackingState.IsNew)
        {
            DomainEvents.RaiseEvent(new RoleCreatedEvent(
                Props.Id.GetValue(),
                Props.TenantId.GetValue(),
                Props.SystemSuiteId.GetValue(),
                Props.Code.GetValue(),
                Props.ParentRoleId?.GetValue()));
        }
    }

    public TenantId TenantId => Props.TenantId;
    public SystemSuiteId SystemSuiteId => Props.SystemSuiteId;
    public RoleId? ParentRoleId => Props.ParentRoleId;
    public Code Code => Props.Code;
    public Name Value => Props.Value;
    public Description Description => Props.Description;
    public int HierarchyLevel => Props.HierarchyLevel;
    public int PromotionOrder => Props.PromotionOrder;
    public bool IsActive => Props.IsActive;

    public RoleId GetId() => RoleId.Load(Props.Id.GetValue());

    public static Result<Role> Create(
        TenantId tenantId,
        SystemSuiteId systemSuiteId,
        Code code,
        Name value,
        Description description,
        RoleId? parentRoleId,
        int hierarchyLevel,
        int promotionOrder,
        ActorId createdBy)
    {
        var props = new RoleProps(
            IdValueObject.Create(),
            tenantId,
            systemSuiteId,
            parentRoleId,
            code,
            value,
            description,
            hierarchyLevel,
            promotionOrder,
            createdBy);
        var role = new Role(props);
        role.ValidateHierarchy();

        return role.IsValid()
            ? Result<Role>.Success(role)
            : Result<Role>.Failure(role.BrokenRules.GetBrokenRulesAsString());
    }

    public Result Update(
        Name value,
        Description description,
        RoleId? parentRoleId,
        int hierarchyLevel,
        int promotionOrder,
        ActorId updatedBy)
    {
        Props.Value = value;
        Props.Description = description;
        Props.ParentRoleId = parentRoleId;
        Props.HierarchyLevel = hierarchyLevel;
        Props.PromotionOrder = promotionOrder;
        ValidateHierarchy();

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        DomainEvents.RaiseEvent(new RoleUpdatedEvent(Props.Id.GetValue()));
        return Result.Success();
    }

    public Result Activate(ActorId updatedBy)
    {
        if (IsActive)
        {
            BrokenRules.Add(new BrokenRule(nameof(IsActive), DomainErrors.Authorization.RoleAlreadyActive));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.IsActive = true;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        DomainEvents.RaiseEvent(new RoleActivatedEvent(Props.Id.GetValue()));
        return Result.Success();
    }

    public Result Deactivate(ActorId updatedBy)
    {
        if (!IsActive)
        {
            BrokenRules.Add(new BrokenRule(nameof(IsActive), DomainErrors.Authorization.RoleAlreadyInactive));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.IsActive = false;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        DomainEvents.RaiseEvent(new RoleDeactivatedEvent(Props.Id.GetValue()));
        return Result.Success();
    }

    private void ValidateHierarchy()
    {
        if (Props.HierarchyLevel < 0)
        {
            BrokenRules.Add(new BrokenRule(nameof(HierarchyLevel), DomainErrors.Authorization.RoleHierarchyLevelInvalid));
        }

        if (Props.PromotionOrder < 0)
        {
            BrokenRules.Add(new BrokenRule(nameof(PromotionOrder), DomainErrors.Authorization.RolePromotionOrderInvalid));
        }

        if (Props.ParentRoleId is null && Props.HierarchyLevel != 0)
        {
            BrokenRules.Add(new BrokenRule(nameof(HierarchyLevel), DomainErrors.Authorization.RootRoleHierarchyLevelInvalid));
        }

        if (Props.ParentRoleId is not null && Props.HierarchyLevel == 0)
        {
            BrokenRules.Add(new BrokenRule(nameof(HierarchyLevel), DomainErrors.Authorization.ChildRoleHierarchyLevelInvalid));
        }
    }
}
