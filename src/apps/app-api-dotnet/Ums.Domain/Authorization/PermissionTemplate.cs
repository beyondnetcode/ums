namespace Ums.Domain.Authorization;

public sealed class PermissionTemplate : AggregateRoot<PermissionTemplate, PermissionTemplateProps>
{
    private readonly List<AuthorizationGrant> _grants = new();

    private PermissionTemplate(PermissionTemplateProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid SystemSuiteId => Props.SystemSuiteId.GetValue();
    public string Code => Props.Code.GetValue();
    public string Name => Props.Name.GetValue();
    public string TemplateVersion => Props.TemplateVersion.GetValue();
    public string Description => Props.Description.GetValue();
    public Guid CreatedBy => Props.CreatedBy.GetValue();
    public LifecycleStatus Status => Props.Status;
    public IReadOnlyCollection<AuthorizationGrant> Grants => _grants.AsReadOnly();

    public static Result<PermissionTemplate> Create(Guid tenantId, Guid systemSuiteId, string code, string name, string version, string description, Guid createdBy)
    {
        if (tenantId == Guid.Empty || systemSuiteId == Guid.Empty || createdBy == Guid.Empty)
            return Result<PermissionTemplate>.Failure("Tenant, system, and creator identifiers are required.");

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(name))
            return Result<PermissionTemplate>.Failure(DomainErrors.NameRequired);

        if (string.IsNullOrWhiteSpace(description))
            return Result<PermissionTemplate>.Failure(DomainErrors.DescriptionRequired);

        var props = new PermissionTemplateProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId.Load(systemSuiteId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.Version.Create(version.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.Description.Create(description.Trim()),
            IdValueObject.Load(createdBy));

        return Result<PermissionTemplate>.Success(new PermissionTemplate(props));
    }

    public Result AddGrant(Guid functionalActionId, PermissionEffect effect)
    {
        if (functionalActionId == Guid.Empty)
            return Result.Failure("Functional action identifier is required.");

        if (_grants.Any(grant => grant.FunctionalActionId == functionalActionId))
            return Result.Failure("Template already declares a grant for this action.");

        var grantProps = new AuthorizationGrantProps(
            IdValueObject.Create(),
            Props.TenantId,
            global::Ums.Domain.Authorization.ValueObjects.TemplateId.Load(Props.Id.GetValue()),
            null,
            global::Ums.Domain.Authorization.ValueObjects.FunctionalActionId.Load(functionalActionId),
            effect);

        _grants.Add(new AuthorizationGrant(grantProps));
        Props.Audit.Update("system");
        return Result.Success();
    }

    public Result Publish()
    {
        if (!_grants.Any())
            return Result.Failure("Permission template cannot be published without grants.");

        Props.Status = LifecycleStatus.Published;
        DomainEvents.ApplyChange(new PermissionTemplatePublishedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.Code.GetValue(), Props.TemplateVersion.GetValue()), true);
        Props.Audit.Update("system");
        return Result.Success();
    }
}
