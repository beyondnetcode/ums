using System;
using Ums.Infrastructure.Persistence;

namespace Ums.Infrastructure.Persistence.Approvals.Entities;

public sealed class ApprovalRequiredDocumentRecord : IAuditableRecord
{
    public Guid Id { get; set; }
    public Guid WorkflowId { get; set; }
    public Guid DocumentTypeId { get; set; }
    public bool IsMandatory { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string AuditTimeSpan { get; set; } = string.Empty;
}
