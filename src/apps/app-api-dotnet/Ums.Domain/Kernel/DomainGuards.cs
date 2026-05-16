namespace Ums.Domain.Kernel;

public static class DomainGuards
{
    public static Result Required(Guid value, string fieldName)
    {
        return value == Guid.Empty
            ? Result.Failure(DomainErrors.Required(fieldName))
            : Result.Success();
    }

    public static Result Required(string? value, string fieldName)
    {
        return string.IsNullOrWhiteSpace(value)
            ? Result.Failure(DomainErrors.Required(fieldName))
            : Result.Success();
    }

    public static string NormalizeCode(string value)
    {
        return value.Trim().ToUpperInvariant().Replace(' ', '_');
    }
}

