using System;
using Ums.Infrastructure.Persistence;

namespace Ums.Infrastructure.Persistence.Approvals.Entities;

public sealed class AccessEnforcementPolicyRecord : IAuditableRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid? ProfileId { get; set; }
    public Guid? RoleId { get; set; }
    public int EnforcementActionId { get; set; }
    public bool IsActive { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string AuditTimeSpan { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = [];
}
