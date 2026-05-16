namespace Ums.Domain.Authorization;

public sealed class FunctionalOption : Entity<FunctionalOption, FunctionalOptionProps>
{
    private FunctionalOption(FunctionalOptionProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid SystemSuiteId => Props.SystemSuiteId.GetValue();
    public Guid ModuleId => Props.ModuleId.GetValue();
    public Guid MenuId => Props.MenuId.GetValue();
    public string Code => Props.Code.GetValue();
    public string Label => Props.Label.GetValue();
    public string RoutePath => Props.RoutePath.GetValue();
    public bool IsActive => Props.IsActive;

    public static Result<FunctionalOption> Create(Guid tenantId, Guid systemSuiteId, Guid moduleId, Guid menuId, string code, string label, string routePath)
    {
        if (tenantId == Guid.Empty || systemSuiteId == Guid.Empty || moduleId == Guid.Empty || menuId == Guid.Empty)
            return Result<FunctionalOption>.Failure("Tenant, system, module, and menu identifiers are required.");

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(label))
            return Result<FunctionalOption>.Failure("Option label is required.");

        if (string.IsNullOrWhiteSpace(routePath))
            return Result<FunctionalOption>.Failure("Route path is required.");

        var props = new FunctionalOptionProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId.Load(systemSuiteId),
            IdValueObject.Load(moduleId),
            IdValueObject.Load(menuId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(label.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(routePath.Trim()));

        return Result<FunctionalOption>.Success(new FunctionalOption(props));
    }
}
