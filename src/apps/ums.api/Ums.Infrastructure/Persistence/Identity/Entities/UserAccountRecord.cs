using Ums.Infrastructure.Persistence;

namespace Ums.Infrastructure.Persistence.Identity.Entities;

public sealed class UserAccountRecord : IAuditableRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? BranchId { get; set; }
    public string Email { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int StatusId { get; set; }
    public string? IdentityReference { get; set; }
    public int? IdentityReferenceTypeId { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string AuditTimeSpan { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = [];

    // REC-16: Soft-delete + GDPR anonymization
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAtUtc { get; set; }
    public string? DeletedBy { get; set; }
    /// <summary>Timestamp set when email and IdentityReference are replaced with anonymized values (GDPR).</summary>
    public DateTime? AnonymizedAtUtc { get; set; }

    public List<UserAccountMfaEnrollmentRecord> MfaEnrollments { get; set; } = [];
    public List<UserAccountPasswordCredentialRecord> PasswordCredentials { get; set; } = [];
}
