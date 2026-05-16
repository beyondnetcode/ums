namespace Ums.Domain.Authorization;

public sealed class AuthorizationGrant : Entity<AuthorizationGrant, AuthorizationGrantProps>
{
    internal AuthorizationGrant(AuthorizationGrantProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid? TemplateId => Props.TemplateId?.GetValue();
    public Guid? ProfileId => Props.ProfileId?.GetValue();
    public Guid FunctionalActionId => Props.FunctionalActionId.GetValue();
    public PermissionEffect Effect => Props.Effect;

    public Result ChangeEffect(PermissionEffect effect)
    {
        Props.Effect = effect;
        Props.Audit.Update("system");
        return Result.Success();
    }
}
