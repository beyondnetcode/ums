namespace Ums.Domain.Authorization;

public sealed class Profile : AggregateRoot<Profile, ProfileProps>
{
    private readonly List<AuthorizationGrant> _grants = new();
    private readonly List<Guid> _roleIds = new();

    private Profile(ProfileProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new ProfileCreatedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.Name.GetValue()), true);
        }
    }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid? BranchId => Props.BranchId?.GetValue();
    public string Name => Props.Name.GetValue();
    public Guid? TemplateId => Props.TemplateId?.GetValue();
    public bool AutoAssigned => Props.AutoAssigned;
    public bool IsActive => Props.IsActive;
    public IReadOnlyCollection<AuthorizationGrant> Grants => _grants.AsReadOnly();
    public IReadOnlyCollection<Guid> RoleIds => _roleIds.AsReadOnly();

    public static Result<Profile> Create(Guid tenantId, string name, Guid? branchId = null, Guid? templateId = null, bool autoAssigned = false)
    {
        if (tenantId == Guid.Empty)
            return Result<Profile>.Failure(DomainErrors.TenantRequired);

        if (string.IsNullOrWhiteSpace(name))
            return Result<Profile>.Failure(DomainErrors.NameRequired);

        var props = new ProfileProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            branchId.HasValue ? global::Ums.Domain.Kernel.ValueObjects.BranchId.Load(branchId.Value) : null,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            templateId.HasValue ? global::Ums.Domain.Authorization.ValueObjects.TemplateId.Load(templateId.Value) : null,
            autoAssigned);

        var profile = new Profile(props);
        return Result<Profile>.Success(profile);
    }

    public Result AddRole(Guid roleId)
    {
        if (roleId == Guid.Empty)
            return Result.Failure("Role identifier is required.");

        if (_roleIds.Contains(roleId))
            return Result.Failure("Role is already assigned to profile.");

        _roleIds.Add(roleId);
        Props.Audit.Update("system");
        return Result.Success();
    }

    public Result AddGrant(Guid functionalActionId, PermissionEffect effect)
    {
        if (functionalActionId == Guid.Empty)
            return Result.Failure("Functional action identifier is required.");

        var existingGrant = _grants.FirstOrDefault(grant => grant.FunctionalActionId == functionalActionId);
        if (existingGrant is not null)
            return existingGrant.ChangeEffect(effect);

        var grantProps = new AuthorizationGrantProps(
            IdValueObject.Create(),
            Props.TenantId,
            null,
            global::Ums.Domain.Authorization.ValueObjects.ProfileId.Load(Props.Id.GetValue()),
            global::Ums.Domain.Authorization.ValueObjects.FunctionalActionId.Load(functionalActionId),
            effect);

        _grants.Add(new AuthorizationGrant(grantProps));
        DomainEvents.ApplyChange(new ProfilePermissionChangedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), functionalActionId), true);
        Props.Audit.Update("system");
        return Result.Success();
    }
}
