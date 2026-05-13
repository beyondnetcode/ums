using Ums.Domain.Common;
using Ums.Domain.Enums;

namespace Ums.Domain.Entities;

/// <summary>
/// Organization acts as the strategic Boundary and Aggregate Root of the domain.
/// </summary>
public class Organization : Entity
{
    public string Name { get; private set; } = string.Empty;
    public OrganizationType Type { get; private set; }
    public string? ErpCode { get; private set; }

    // Encapsulated Navigation property
    private readonly List<Subject> _subjects = new();
    public IReadOnlyCollection<Subject> Subjects => _subjects.AsReadOnly();

    // Private constructor for EF Core and Factory
    private Organization() { }

    private Organization(Guid id, string name, OrganizationType type, string? erpCode)
    {
        Id = id;
        Name = name;
        Type = type;
        ErpCode = erpCode;
    }

    /// <summary>
    /// Factory method to enforce business invariants upon creation.
    /// </summary>
    public static Result<Organization> Create(string name, OrganizationType type, string? erpCode = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            return Result<Organization>.Failure("Organization name cannot be empty.");

        if (name.Length < 3)
            return Result<Organization>.Failure("Organization name must be at least 3 characters.");

        var organization = new Organization(Guid.NewGuid(), name.Trim(), type, erpCode?.Trim());
        
        return Result<Organization>.Success(organization);
    }

    public Result UpdateName(string newName)
    {
        if (string.IsNullOrWhiteSpace(newName))
            return Result.Failure("New name cannot be empty.");

        Name = newName.Trim();
        return Result.Success();
    }
}
