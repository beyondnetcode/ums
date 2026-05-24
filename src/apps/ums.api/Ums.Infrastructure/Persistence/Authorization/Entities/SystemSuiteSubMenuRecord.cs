namespace Ums.Infrastructure.Persistence.Authorization.Entities;

public sealed class SystemSuiteSubMenuRecord : IAuditableRecord
{
    public Guid Id { get; set; }
    public Guid MenuId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string AuditTimeSpan { get; set; } = string.Empty;

    public SystemSuiteMenuRecord Menu { get; set; } = null!;
    public List<SystemSuiteOptionRecord> Options { get; set; } = [];
}
