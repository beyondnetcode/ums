namespace Ums.Domain.Authorization;

public sealed class Role : AggregateRoot<Role, RoleProps>
{
    private Role(RoleProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new RoleCreatedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.Code.GetValue()), true);
        }
    }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid SystemSuiteId => Props.SystemSuiteId.GetValue();
    public Guid? ParentRoleId => Props.ParentRoleId?.GetValue();
    public string Code => Props.Code.GetValue();
    public string Name => Props.Name.GetValue();
    public string Description => Props.Description.GetValue();
    public bool IsActive => Props.IsActive;

    public static Result<Role> Create(Guid tenantId, Guid systemSuiteId, string code, string name, string description, Guid? parentRoleId = null)
    {
        if (tenantId == Guid.Empty || systemSuiteId == Guid.Empty)
            return Result<Role>.Failure(DomainErrors.Role.TenantAndSystemRequired);

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(name))
            return Result<Role>.Failure(DomainErrors.Common.Required);

        if (string.IsNullOrWhiteSpace(description))
            return Result<Role>.Failure(DomainErrors.Common.Required);

        var props = new RoleProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId.Load(systemSuiteId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.Description.Create(description.Trim()),
            parentRoleId.HasValue ? IdValueObject.Load(parentRoleId.Value) : null);

        var role = new Role(props);
        return Result<Role>.Success(role);
    }
}
