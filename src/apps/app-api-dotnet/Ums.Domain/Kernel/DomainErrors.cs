namespace Ums.Domain.Kernel;

public static class DomainErrors
{
    public const string TenantRequired = "Tenant identifier is required.";
    public const string CodeRequired = "Code is required.";
    public const string NameRequired = "Name is required.";
    public const string DescriptionRequired = "Description is required.";

    public static string Required(string fieldName) => $"{fieldName} is required.";
    public static string Invalid(string fieldName) => $"{fieldName} is invalid.";
}

