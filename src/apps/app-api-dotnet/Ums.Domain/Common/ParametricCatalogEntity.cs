namespace Ums.Domain.Common;

/// <summary>
/// Base contract for all parameter/configuration/catalog entities in UMS.
/// Mandatory fields: Code, Value, Description.
/// </summary>
public abstract class ParametricCatalogEntity : Entity
{
    public string Code { get; protected set; } = string.Empty;
    public string Value { get; protected set; } = string.Empty;
    public string Description { get; protected set; } = string.Empty;

    protected Result SetCatalogFields(string code, string value, string description)
    {
        if (string.IsNullOrWhiteSpace(code))
            return Result.Failure("Code is required.");

        if (string.IsNullOrWhiteSpace(value))
            return Result.Failure("Value is required.");

        if (string.IsNullOrWhiteSpace(description))
            return Result.Failure("Description is required.");

        Code = code.Trim().ToUpperInvariant();
        Value = value.Trim();
        Description = description.Trim();
        return Result.Success();
    }
}
