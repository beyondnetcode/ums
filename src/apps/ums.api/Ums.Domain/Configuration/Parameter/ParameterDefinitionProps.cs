namespace Ums.Domain.Configuration.Parameter;

using Ums.Domain.Configuration.Parameter.ValueObjects;
using Ums.Domain.Kernel;

public sealed class ParameterDefinitionProps : IProps
{
    public IdValueObject Id { get; private set; }
    public Code Code { get; private set; }
    public ParameterName Name { get; private set; }
    public Description Description { get; private set; }
    public ParameterDataType DataType { get; private set; }
    public DefaultValue DefaultValue { get; private set; }
    public ParameterScope Scope { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsMandatory { get; private set; }
    public int DisplayOrder { get; private set; }
    public string Version { get; private set; }
    public AuditValueObject Audit { get; private set; }

    public ParameterDefinitionProps(
        IdValueObject id,
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
        Id = id;
        Code = code;
        Name = name;
        Description = description;
        DataType = dataType;
        DefaultValue = defaultValue;
        Scope = scope;
        IsActive = isActive;
        IsMandatory = isMandatory;
        DisplayOrder = displayOrder;
        Version = "1.0.0";
        Audit = AuditValueObject.Create(createdBy.GetValue());
    }

    private ParameterDefinitionProps(
        IdValueObject id,
        Code code,
        ParameterName name,
        Description description,
        ParameterDataType dataType,
        DefaultValue defaultValue,
        ParameterScope scope,
        bool isActive,
        bool isMandatory,
        int displayOrder,
        string version,
        AuditValueObject audit)
    {
        Id = id;
        Code = code;
        Name = name;
        Description = description;
        DataType = dataType;
        DefaultValue = defaultValue;
        Scope = scope;
        IsActive = isActive;
        IsMandatory = isMandatory;
        DisplayOrder = displayOrder;
        Version = version;
        Audit = audit;
    }

    public ParameterDefinitionProps WithName(ParameterName name) => new(Id, Code, name, Description, DataType, DefaultValue, Scope, IsActive, IsMandatory, DisplayOrder, Version, Audit);
    public ParameterDefinitionProps WithDescription(Description description) => new(Id, Code, Name, description, DataType, DefaultValue, Scope, IsActive, IsMandatory, DisplayOrder, Version, Audit);
    public ParameterDefinitionProps WithDefaultValue(DefaultValue defaultValue) => new(Id, Code, Name, Description, DataType, defaultValue, Scope, IsActive, IsMandatory, DisplayOrder, Version, Audit);
    public ParameterDefinitionProps WithScope(ParameterScope scope) => new(Id, Code, Name, Description, DataType, DefaultValue, scope, IsActive, IsMandatory, DisplayOrder, Version, Audit);
    public ParameterDefinitionProps WithIsActive(bool isActive) => new(Id, Code, Name, Description, DataType, DefaultValue, Scope, isActive, IsMandatory, DisplayOrder, Version, Audit);
    public ParameterDefinitionProps WithIsMandatory(bool isMandatory) => new(Id, Code, Name, Description, DataType, DefaultValue, Scope, IsActive, isMandatory, DisplayOrder, Version, Audit);
    public ParameterDefinitionProps WithDisplayOrder(int displayOrder) => new(Id, Code, Name, Description, DataType, DefaultValue, Scope, IsActive, IsMandatory, displayOrder, Version, Audit);
    public ParameterDefinitionProps WithVersion(string version) => new(Id, Code, Name, Description, DataType, DefaultValue, Scope, IsActive, IsMandatory, DisplayOrder, version, Audit);

    public object Clone() => MemberwiseClone();
}

public sealed record ParameterName
{
    public string Value { get; }

    private ParameterName(string value) => Value = value;

    public static ParameterName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Parameter name cannot be empty", nameof(value));
        if (value.Length > 200)
            throw new ArgumentException("Parameter name cannot exceed 200 characters", nameof(value));
        return new ParameterName(value.Trim());
    }

    public static ParameterName? TryCreate(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : Create(value);
}

public sealed record DefaultValue
{
    public string Value { get; }

    private DefaultValue(string value) => Value = value;

    public static DefaultValue Create(string value) => new(value ?? string.Empty);

    public static DefaultValue? TryCreate(string? value) =>
        value is null ? null : Create(value);
}