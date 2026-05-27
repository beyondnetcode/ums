namespace Ums.Infrastructure.Persistence.Authorization.Entities;

using System;

public sealed class SystemSuiteDomainResourceRecord : IAuditableRecord
{
    public Guid Id { get; set; }
    public Guid SystemSuiteId { get; set; }
    public Guid? ModuleId { get; set; }
    public string Type { get; set; } = null!; // Aggregate or Entity
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string AuditTimeSpan { get; set; } = string.Empty;

    public SystemSuiteRecord SystemSuite { get; set; } = null!;
}
