namespace Ums.Domain.Configuration.Parameter;

using Ums.Domain.Configuration.Parameter.ValueObjects;

public sealed class ParameterDefinition : AggregateRoot<ParameterDefinition, ParameterDefinitionProps>
{
    private ParameterDefinition(ParameterDefinitionProps props) : base(props)
    {
    }

    public Code Code => Props.Code;
    public ParameterName Name => Props.Name;
    public Description Description => Props.Description;
    public ParameterDataType DataType => Props.DataType;
    public DefaultValue DefaultValue => Props.DefaultValue;
    public ParameterScope Scope => Props.Scope;
    public bool IsActive => Props.IsActive;
    public bool IsMandatory => Props.IsMandatory;
    public int DisplayOrder => Props.DisplayOrder;
    public new string Version => Props.Version;

    public ParameterDefinitionId GetId() => ParameterDefinitionId.Load(Props.Id.GetValue());

    public static Result<ParameterDefinition> Create(
        Code code,
        ParameterName name,
        Description description,
        ParameterDataType dataType,
        DefaultValue defaultValue,
        ParameterScope scope,
        bool isActive,
        bool isMandatory,
        int displayOrder,
        ActorId createdBy)
    {
        if (code is null)
            return Result<ParameterDefinition>.Failure("Parameter code is required");

        if (name is null)
            return Result<ParameterDefinition>.Failure("Parameter name is required");

        if (defaultValue is null)
            return Result<ParameterDefinition>.Failure("Default value is required");

        var props = new ParameterDefinitionProps(
            IdValueObject.Create(), code, name, description,
            dataType, defaultValue, scope, isActive, isMandatory, displayOrder, createdBy);

        var entity = new ParameterDefinition(props);

        if (!entity.IsValid())
        {
            return Result<ParameterDefinition>.Failure(entity.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<ParameterDefinition>.Success(entity);
    }

    public Result Update(
        ParameterName name,
        Description description,
        DefaultValue defaultValue,
        ParameterScope scope,
        bool isActive,
        bool isMandatory,
        int displayOrder,
        ActorId updatedBy)
    {
        SetProps(Props
            .WithName(name)
            .WithDescription(description)
            .WithDefaultValue(defaultValue)
            .WithScope(scope)
            .WithIsActive(isActive)
            .WithIsMandatory(isMandatory)
            .WithDisplayOrder(displayOrder)
            .WithVersion(BumpMinorVersion(Props.Version)));

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy.GetValue());
        return Result.Success();
    }

    private static string BumpMinorVersion(string semver)
    {
        if (System.Version.TryParse(semver, out var v))
            return $"{v.Major}.{v.Minor + 1}.0";
        return semver;
    }
}

public sealed class ParameterDefinitionId
{
    public Guid Value { get; }

    private ParameterDefinitionId(Guid value) => Value = value;

    public static ParameterDefinitionId Create(Guid value) => new(value);
    public static ParameterDefinitionId Load(Guid value) => new(value);
    public static ParameterDefinitionId New() => new(Guid.NewGuid());
}