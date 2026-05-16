namespace Ums.Domain.Authorization;

public sealed class FunctionalModule : Entity<FunctionalModule, FunctionalModuleProps>
{
    private readonly List<FunctionalSubmodule> _menus = new();

    private FunctionalModule(FunctionalModuleProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid SystemSuiteId => Props.SystemSuiteId.GetValue();
    public string Code => Props.Code.GetValue();
    public string Name => Props.Name.GetValue();
    public string Description => Props.Description.GetValue();
    public bool IsActive => Props.IsActive;
    public IReadOnlyCollection<FunctionalSubmodule> Menus => _menus.AsReadOnly();

    public static Result<FunctionalModule> Create(Guid tenantId, Guid systemSuiteId, string code, string name, string description)
    {
        if (tenantId == Guid.Empty || systemSuiteId == Guid.Empty)
            return Result<FunctionalModule>.Failure("Tenant and system identifiers are required.");

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(name))
            return Result<FunctionalModule>.Failure(DomainErrors.NameRequired);

        if (string.IsNullOrWhiteSpace(description))
            return Result<FunctionalModule>.Failure(DomainErrors.DescriptionRequired);

        var props = new FunctionalModuleProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId.Load(systemSuiteId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            global::Ums.Domain.Kernel.ValueObjects.Description.Create(description.Trim()));

        return Result<FunctionalModule>.Success(new FunctionalModule(props));
    }

    public Result AddMenu(string code, string label, int displayOrder, string? iconCode = null)
    {
        var menuResult = FunctionalSubmodule.Create(Props.TenantId.GetValue(), Props.SystemSuiteId.GetValue(), Props.Id.GetValue(), code, label, displayOrder, iconCode);
        if (menuResult.IsFailure)
            return Result.Failure(menuResult.Error);

        if (_menus.Any(menu => menu.Code == menuResult.Value.Code))
            return Result.Failure("Menu code must be unique inside the module.");

        _menus.Add(menuResult.Value);
        Props.Audit.Update("system");
        return Result.Success();
    }
}
