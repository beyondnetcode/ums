namespace Ums.Domain.Kernel;

using Ums.Shell.Ddd;
using Ums.Shell.Ddd.Interfaces;
using Ums.Shell.Ddd.ValueObjects.Common;
using Ums.Domain.Kernel.ValueObjects;

public abstract class ParametricCatalogProps : IProps
{
    public IdValueObject Id { get; protected set; } = default!;
    public TenantId TenantId { get; protected set; } = default!;
    public SystemSuiteId? SystemSuiteId { get; protected set; }
    public Code Code { get; protected set; } = default!;
    public Value Value { get; protected set; } = default!;
    public Description Description { get; protected set; } = default!;
    public Version Version { get; protected set; } = default!;
    public bool IsCacheable { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public AuditValueObject Audit { get; protected set; } = default!;

    public virtual object Clone()
    {
        return this.MemberwiseClone();
    }
}

public abstract class ParametricCatalogEntity<TEntity, TProps> : AggregateRoot<TEntity, TProps>
    where TEntity : ParametricCatalogEntity<TEntity, TProps>
    where TProps : ParametricCatalogProps
{
    protected ParametricCatalogEntity(TProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid? SystemSuiteId => Props.SystemSuiteId?.GetValue();
    public string Code => Props.Code.GetValue();
    public string Value => Props.Value.GetValue();
    public string Description => Props.Description.GetValue();
    public new string Version => Props.Version.GetValue();
    public bool IsCacheable => Props.IsCacheable;
    public bool IsActive => Props.IsActive;

    public Guid GetId() => Props.Id.GetValue();

    public Result SetCatalogFields(
        Guid tenantId,
        string code,
        string value,
        string description,
        string updatedBy,
        string version = "1.0.0",
        Guid? systemSuiteId = null)
    {
        if (tenantId == Guid.Empty)
            return Result.Failure("TenantId is required.");

        var codeValue = global::Ums.Domain.Kernel.ValueObjects.Code.Create(code);

        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure("Value is required.");

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure("Description is required.");

        if (string.IsNullOrWhiteSpace(version))
            return Result.Failure("Version is required.");

        Props.GetType().GetProperty(nameof(ParametricCatalogProps.TenantId))?.SetValue(Props, global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId));
        Props.GetType().GetProperty(nameof(ParametricCatalogProps.SystemSuiteId))?.SetValue(Props, systemSuiteId.HasValue ? global::Ums.Domain.Kernel.ValueObjects.SystemSuiteId.Load(systemSuiteId.Value) : null);
        Props.GetType().GetProperty(nameof(ParametricCatalogProps.Code))?.SetValue(Props, codeValue);
        Props.GetType().GetProperty(nameof(ParametricCatalogProps.Value))?.SetValue(Props, global::Ums.Domain.Kernel.ValueObjects.Value.Create(value));
        Props.GetType().GetProperty(nameof(ParametricCatalogProps.Description))?.SetValue(Props, global::Ums.Domain.Kernel.ValueObjects.Description.Create(description));
        Props.GetType().GetProperty(nameof(ParametricCatalogProps.Version))?.SetValue(Props, global::Ums.Domain.Kernel.ValueObjects.Version.Create(version));
        
        Props.Audit.Update(updatedBy);

        return Result.Success();
    }

    public Result UpdateValue(string value, string description, string updatedBy, string version)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure("Value is required.");

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure("Description is required.");

        if (string.IsNullOrWhiteSpace(version))
            return Result.Failure("Version is required.");

        Props.GetType().GetProperty(nameof(ParametricCatalogProps.Value))?.SetValue(Props, global::Ums.Domain.Kernel.ValueObjects.Value.Create(value));
        Props.GetType().GetProperty(nameof(ParametricCatalogProps.Description))?.SetValue(Props, global::Ums.Domain.Kernel.ValueObjects.Description.Create(description));
        Props.GetType().GetProperty(nameof(ParametricCatalogProps.Version))?.SetValue(Props, global::Ums.Domain.Kernel.ValueObjects.Version.Create(version));
        
        Props.Audit.Update(updatedBy);

        return Result.Success();
    }

    public void Deactivate(string updatedBy)
    {
        Props.IsActive = false;
        Props.Audit.Update(updatedBy);
    }
}
