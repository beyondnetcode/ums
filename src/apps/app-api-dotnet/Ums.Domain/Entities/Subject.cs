using Ums.Domain.Common;
using Ums.Domain.Enums;
using System.Text.RegularExpressions;

namespace Ums.Domain.Entities;

/// <summary>
/// Subject acts as the agnostic Persona/Identity container, tied strictly to an Organization.
/// </summary>
public class Subject : Entity
{
    public Guid OrganizationId { get; private set; }
    public string Email { get; private set; } = string.Empty;
    public string IdentityReference { get; private set; } = string.Empty;
    public ReferenceType ReferenceType { get; private set; }
    public bool IsActive { get; private set; } = true;

    // Navigation reference
    public Organization Organization { get; private set; } = null!;

    private Subject() { }

    private Subject(Guid id, Guid organizationId, string email, string identityReference, ReferenceType referenceType)
    {
        Id = id;
        OrganizationId = organizationId;
        Email = email.ToLowerInvariant();
        IdentityReference = identityReference;
        ReferenceType = referenceType;
    }

    private static readonly Regex EmailRegex = new(
        @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    /// <summary>
    /// Factory pattern enforcing creation invariants for Subjects.
    /// </summary>
    public static Result<Subject> Create(
        Guid organizationId, 
        string email, 
        string identityReference, 
        ReferenceType referenceType)
    {
        if (organizationId == Guid.Empty)
            return Result<Subject>.Failure("Invalid Organization identifier.");

        if (string.IsNullOrWhiteSpace(email) || !EmailRegex.IsMatch(email.Trim()))
            return Result<Subject>.Failure("A valid email address is required.");

        if (string.IsNullOrWhiteSpace(identityReference))
            return Result<Subject>.Failure("Identity external reference cannot be empty.");

        var subject = new Subject(
            Guid.NewGuid(), 
            organizationId, 
            email.Trim(), 
            identityReference.Trim(), 
            referenceType);

        return Result<Subject>.Success(subject);
    }

    public Result Deactivate()
    {
        if (!IsActive)
            return Result.Failure("Subject is already inactive.");

        IsActive = false;
        return Result.Success();
    }

    public Result Activate()
    {
        if (IsActive)
            return Result.Failure("Subject is already active.");

        IsActive = true;
        return Result.Success();
    }
}
