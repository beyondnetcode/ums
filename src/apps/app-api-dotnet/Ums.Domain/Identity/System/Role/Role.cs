namespace Ums.Domain.Identity.System.Role;

public sealed class Role : Entity<Role, RoleProps>
{
    private readonly List<ActionCode> _allowedActions = new();

    private Role(RoleProps props) : base(props)
    {
    }

    public SystemId SystemId => Props.SystemId;
    public Code Code => Props.Code;
    public Name Name => Props.Name;
    public Description Description => Props.Description;
    public bool IsActive => Props.IsActive;

    public IReadOnlyCollection<ActionCode> AllowedActions => _allowedActions.AsReadOnly();

    public RoleId GetId() => RoleId.Load(Props.Id.GetValue());

    public static Result<Role> Create(
        SystemId systemId,
        Code code,
        Name name,
        Description description,
        ActorId createdBy)
    {
        var props = new RoleProps(IdValueObject.Create(), systemId, code, name, description, createdBy);
        var role = new Role(props);

        if (!role.IsValid())
        {
            return Result<Role>.Failure(role.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<Role>.Success(role);
    }

    public Result Update(Name name, Description description, ActorId updatedBy)
    {
        Props.Name = name;
        Props.Description = description;

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Activate(ActorId updatedBy)
    {
        if (IsActive)
        {
            BrokenRules.Add(new BrokenRule(nameof(IsActive), DomainErrors.System.RoleAlreadyActive));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.IsActive = true;
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result Deactivate(ActorId updatedBy)
    {
        if (!IsActive)
        {
            BrokenRules.Add(new BrokenRule(nameof(IsActive), DomainErrors.System.RoleAlreadyInactive));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.IsActive = false;
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result GrantAction(ActionCode actionCode, ActorId updatedBy)
    {
        if (_allowedActions.Contains(actionCode))
        {
            BrokenRules.Add(new BrokenRule(nameof(AllowedActions), DomainErrors.System.ActionAlreadyGranted));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _allowedActions.Add(actionCode);
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    public Result RevokeAction(ActionCode actionCode, ActorId updatedBy)
    {
        if (!_allowedActions.Contains(actionCode))
        {
            BrokenRules.Add(new BrokenRule(nameof(AllowedActions), DomainErrors.System.ActionNotGranted));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        _allowedActions.Remove(actionCode);
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }
}
