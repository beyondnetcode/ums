using Ums.Infrastructure.Persistence;

namespace Ums.Infrastructure.Persistence.Identity.Entities;

public sealed class TenantParameterRecord : IAuditableRecord
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public int ValueTypeId { get; set; }
    public int CategoryId { get; set; }
    public bool IsActive { get; set; }
    public bool IsSensitive { get; set; }
    public string? DefaultValue { get; set; }
    public string? AllowedValues { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string AuditTimeSpan { get; set; } = string.Empty;
    public byte[] RowVersion { get; set; } = [];
}