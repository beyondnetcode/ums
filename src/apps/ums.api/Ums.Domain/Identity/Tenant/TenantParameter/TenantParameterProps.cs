namespace Ums.Domain.Identity.Tenant.TenantParameter;

using Ums.Domain.Kernel.ValueObjects;
using CodeEntity = Ums.Domain.Kernel.ValueObjects.Code;
using DescriptionEntity = Ums.Domain.Kernel.ValueObjects.Description;

public class TenantParameterProps : IProps
{
    public TenantParameterProps(
        IdValueObject id,
        TenantId tenantId,
        CodeEntity code,
        DescriptionEntity description,
        string value,
        TenantParameterValueType valueType,
        TenantParameterCategory category,
        bool isActive,
        bool isSensitive,
        string? defaultValue,
        string? allowedValues,
        AuditValueObject audit)
    {
        Id = id;
        TenantId = tenantId;
        Code = code;
        Description = description;
        Value = value;
        ValueType = valueType;
        Category = category;
        IsActive = isActive;
        IsSensitive = isSensitive;
        DefaultValue = defaultValue;
        AllowedValues = allowedValues;
        Audit = audit;
    }

    public IdValueObject Id { get; private set; }
    public TenantId TenantId { get; private set; }
    public CodeEntity Code { get; private set; }
    public DescriptionEntity Description { get; private set; }
    public string Value { get; private set; }
    public TenantParameterValueType ValueType { get; private set; }
    public TenantParameterCategory Category { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsSensitive { get; private set; }
    public string? DefaultValue { get; private set; }
    public string? AllowedValues { get; private set; }
    public AuditValueObject Audit { get; private set; }

    public TenantParameterProps WithValue(string value)
    {
        var clone = (TenantParameterProps)MemberwiseClone();
        clone.Value = value;
        return clone;
    }

    public TenantParameterProps WithIsActive(bool isActive)
    {
        var clone = (TenantParameterProps)MemberwiseClone();
        clone.IsActive = isActive;
        return clone;
    }

    public object Clone()
    {
        return MemberwiseClone();
    }
}