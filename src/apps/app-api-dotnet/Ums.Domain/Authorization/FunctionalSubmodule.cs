namespace Ums.Domain.Authorization;

public sealed class FunctionalSubmodule : Entity<FunctionalSubmodule, FunctionalSubmoduleProps>
{
    private readonly List<FunctionalOption> _options = new();

    private FunctionalSubmodule(FunctionalSubmoduleProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid SystemSuiteId => Props.SystemSuiteId.GetValue();
    public Guid ModuleId => Props.ModuleId.GetValue();
    public string Code => Props.Code.GetValue();
    public string Label => Props.Label.GetValue();
    public int DisplayOrder => Props.DisplayOrder;
    public string? IconCode => Props.IconCode?.GetValue();
    public bool IsActive => Props.IsActive;
    public IReadOnlyCollection<FunctionalOption> Options => _options.AsReadOnly();

    public static Result<FunctionalSubmodule> Create(Guid tenantId, Guid systemSuiteId, Guid moduleId, string code, string label, int displayOrder, string? iconCode = null)
    {
        if (tenantId == Guid.Empty || systemSuiteId == Guid.Empty || moduleId == Guid.Empty)
            return Result<FunctionalSubmodule>.Failure("Tenant, system, and module identifiers are required.");

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(label))
            return Result<FunctionalSubmodule>.Failure("Menu label is required.");

        var props = new FunctionalSubmoduleProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId.Load(systemSuiteId),
            IdValueObject.Load(moduleId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(label.Trim()),
            displayOrder,
            iconCode != null ? global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(iconCode) : null);

        return Result<FunctionalSubmodule>.Success(new FunctionalSubmodule(props));
    }

    public Result AddOption(string code, string label, string routePath)
    {
        var optionResult = FunctionalOption.Create(Props.TenantId.GetValue(), Props.SystemSuiteId.GetValue(), Props.ModuleId.GetValue(), Props.Id.GetValue(), code, label, routePath);
        if (optionResult.IsFailure)
            return Result.Failure(optionResult.Error);

        if (_options.Any(option => option.Code == optionResult.Value.Code))
            return Result.Failure("Option code must be unique inside the menu.");

        _options.Add(optionResult.Value);
        Props.Audit.Update("system");
        return Result.Success();
    }
}
