namespace Ums.Domain.Authorization;

public sealed class FunctionalAction : Entity<FunctionalAction, FunctionalActionProps>
{
    private FunctionalAction(FunctionalActionProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid SystemSuiteId => Props.SystemSuiteId.GetValue();
    public string Code => Props.Code.GetValue();
    public string Name => Props.Name.GetValue();
    public FunctionalActionLevel Level => Props.Level;
    public Guid? LevelId => Props.LevelId?.GetValue();
    public bool IsActive => Props.IsActive;

    public static Result<FunctionalAction> Create(Guid tenantId, Guid systemSuiteId, string code, string name, FunctionalActionLevel level, Guid? levelId = null)
    {
        if (tenantId == Guid.Empty || systemSuiteId == Guid.Empty)
            return Result<FunctionalAction>.Failure(DomainErrors.FunctionalAction.TenantAndSystemRequired);

        if (level != FunctionalActionLevel.System && (!levelId.HasValue || levelId.Value == Guid.Empty))
            return Result<FunctionalAction>.Failure(DomainErrors.FunctionalAction.LevelRequired);

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(name))
            return Result<FunctionalAction>.Failure(DomainErrors.Common.Required);

        var props = new FunctionalActionProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId.Load(systemSuiteId),
            codeValue,
            global::Ums.Domain.Kernel.ValueObjects.Name.Create(name.Trim()),
            level,
            levelId.HasValue ? IdValueObject.Load(levelId.Value) : null);

        return Result<FunctionalAction>.Success(new FunctionalAction(props));
    }
}
