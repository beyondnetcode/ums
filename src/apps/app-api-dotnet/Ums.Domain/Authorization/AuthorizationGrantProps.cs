namespace Ums.Domain.Authorization;

public class AuthorizationGrantProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public IdValueObject? TemplateId { get; private set; }
    public IdValueObject? ProfileId { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.FunctionalActionId FunctionalActionId { get; private set; }
    public PermissionEffect Effect { get; set; }
    public AuditValueObject Audit { get; private set; }

    public AuthorizationGrantProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Authorization.ValueObjects.TemplateId? templateId, global::Ums.Domain.Authorization.ValueObjects.ProfileId? profileId, global::Ums.Domain.Authorization.ValueObjects.FunctionalActionId functionalActionId, PermissionEffect effect)
    {
        Id = id;
        TenantId = tenantId;
        TemplateId = templateId;
        ProfileId = profileId;
        FunctionalActionId = functionalActionId;
        Effect = effect;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
