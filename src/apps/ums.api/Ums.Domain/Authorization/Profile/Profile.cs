namespace Ums.Domain.Authorization.Profile;

using Ums.Domain.Events;
using Ums.Domain.Authorization.Profile.ProfilePermission;
using Ums.Domain.Authorization.Template;
using ProfilePermissionEntity = Ums.Domain.Authorization.Profile.ProfilePermission.ProfilePermission;
using PermissionTemplateEntity = Ums.Domain.Authorization.Template.PermissionTemplate;
using EffectConst = Ums.Domain.Authorization.Profile.PermissionEffectConstants;

public sealed class Profile : AggregateRoot<Profile, ProfileProps>
{
    private readonly List<ProfilePermissionEntity> _permissions = new();

    public new ProfileDomainEventsManager DomainEvents { get; }

    private Profile(ProfileProps props) : base(props)
    {
        DomainEvents = new ProfileDomainEventsManager(this);

        if (TrackingState.IsNew)
        {
            DomainEvents.RaiseEvent(new ProfileCreatedEvent(
                Props.Id.GetValue(),
                Props.TenantId.GetValue(),
                Props.UserId.GetValue(),
                Props.RoleId.GetValue(),
                Props.BranchId?.GetValue()));
        }
    }

    public TenantId TenantId => Props.TenantId;
    public UserId UserId => Props.UserId;
    public RoleId RoleId => Props.RoleId;
    public BranchId? BranchId => Props.BranchId;
    public ProfileScope Scope => Props.Scope;
    public bool IsActive => Props.IsActive;

    public IReadOnlyCollection<ProfilePermissionEntity> Permissions => _permissions.AsReadOnly();

    public ProfileId GetId() => ProfileId.Load(Props.Id.GetValue());

    public static Result<Profile> Create(
        TenantId tenantId,
        UserId userId,
        RoleId roleId,
        BranchId? branchId,
        ActorId createdBy)
    {
        var scope = branchId is null ? ProfileScope.OrgWide : ProfileScope.BranchScoped;
        var props = new ProfileProps(IdValueObject.Create(), tenantId, userId, roleId, branchId, scope, createdBy);
        var profile = new Profile(props);

        if (!profile.IsValid())
        {
            return Result<Profile>.Failure(profile.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<Profile>.Success(profile);
    }

    public Result AssignTemplate(PermissionTemplateEntity template, ActorId assignedBy)
    {
        if (!IsActive)
        {
            BrokenRules.Add(new BrokenRule(nameof(IsActive), DomainErrors.Authorization.ProfileAlreadyInactive));
        }

        if (template.TenantId != TenantId)
        {
            BrokenRules.Add(new BrokenRule(nameof(Template), DomainErrors.Authorization.TemplateTenantMismatch));
        }

        if (template.Status != TemplateStatus.Published)
        {
            BrokenRules.Add(new BrokenRule(nameof(Template), DomainErrors.Authorization.TemplateNotPublishedForProfile));
        }

        var templateId = TemplateId.Load(template.GetId().GetValue());

        if (_permissions.Any(p => p.TemplateId.Equals(templateId)))
        {
            BrokenRules.Add(new BrokenRule(nameof(Permissions), DomainErrors.Authorization.ProfileTemplateAlreadyLinked));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        foreach (var templateItem in template.Items)
        {
            var targetId = IdValueObject.Load(templateItem.TargetId.GetValue());
            var actionId = ActionId.Load(templateItem.ActionId.GetValue());
            var settings = ProfilePermissionSettings.Create(
                templateItem.TargetType,
                targetId,
                actionId,
                templateItem.IsAllowed,
                templateItem.IsDenied);

            var permissionResult = ProfilePermissionEntity.Create(
                GetId(),
                templateId,
                settings,
                assignedBy);

            if (permissionResult.IsFailure)
            {
                return Result.Failure(permissionResult.Error);
            }

            _permissions.Add(permissionResult.Value);
        }

        DomainEvents.RaiseEvent(new TemplateLinkedToProfileEvent(Props.Id.GetValue(), templateId.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(assignedBy.GetValue());
        return Result.Success();
    }

    public Result OverridePermissionAllow(IdValueObject permissionId, ActorId updatedBy)
    {
        return ExecuteOnPermission(permissionId, p => p.OverrideAllow(updatedBy), updatedBy, EffectConst.Allow);
    }

    public Result OverridePermissionDeny(IdValueObject permissionId, ActorId updatedBy)
    {
        return ExecuteOnPermission(permissionId, p => p.OverrideDeny(updatedBy), updatedBy, EffectConst.Deny);
    }

    public Result OverridePermissionNeutral(IdValueObject permissionId, ActorId updatedBy)
    {
        return ExecuteOnPermission(permissionId, p => p.OverrideNeutral(updatedBy), updatedBy, EffectConst.Neutral);
    }

    public Result DeactivatePermission(IdValueObject permissionId, ActorId updatedBy)
    {
        return ExecuteOnPermission(permissionId, p => p.Deactivate(updatedBy), updatedBy, EffectConst.Deactivated);
    }

    public Result ActivatePermission(IdValueObject permissionId, ActorId updatedBy)
    {
        return ExecuteOnPermission(permissionId, p => p.Activate(updatedBy), updatedBy, EffectConst.Activated);
    }

    private Result ExecuteOnPermission(IdValueObject permissionId, Func<ProfilePermissionEntity, Result> action, ActorId updatedBy, string eventType)
    {
        if (!IsActive)
        {
            BrokenRules.Add(new BrokenRule(nameof(IsActive), DomainErrors.Authorization.ProfileAlreadyInactive));
        }

        var permission = FindPermission(permissionId);
        if (permission.IsFailure)
        {
            BrokenRules.Add(new BrokenRule(nameof(Permissions), DomainErrors.Authorization.PermissionNotFound));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var result = action(permission.Value);
        if (result.IsFailure)
        {
            return result;
        }

        DomainEvents.RaiseEvent(new PermissionOverriddenEvent(
            Props.Id.GetValue(),
            permission.Value.GetId().GetValue(),
            eventType));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Deactivate(ActorId updatedBy)
    {
        if (!IsActive)
        {
            BrokenRules.Add(new BrokenRule(nameof(IsActive), DomainErrors.Authorization.ProfileAlreadyInactive));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        SetProps(Props.WithIsActive(false));
        DomainEvents.RaiseEvent(new ProfileDeactivatedEvent(Props.Id.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Activate(ActorId updatedBy)
    {
        if (IsActive)
        {
            BrokenRules.Add(new BrokenRule(nameof(IsActive), DomainErrors.Authorization.ProfileAlreadyActive));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        SetProps(Props.WithIsActive(true));
        DomainEvents.RaiseEvent(new ProfileActivatedEvent(Props.Id.GetValue()));
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    private Result<ProfilePermissionEntity> FindPermission(IdValueObject permissionId)
    {
        var permission = _permissions.FirstOrDefault(p => p.Id.GetValue() == permissionId.GetValue());
        return permission is null
            ? Result<ProfilePermissionEntity>.Failure(DomainErrors.Common.NotFound)
            : Result<ProfilePermissionEntity>.Success(permission);
    }
}
