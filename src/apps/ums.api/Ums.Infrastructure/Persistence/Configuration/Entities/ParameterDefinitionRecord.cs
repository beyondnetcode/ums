using Ums.Infrastructure.Persistence;

namespace Ums.Infrastructure.Persistence.Configuration.Entities;

public sealed class ParameterDefinitionRecord : IAuditableRecord
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int DataTypeId { get; set; }
    public string DefaultValue { get; set; } = string.Empty;
    public int ScopeId { get; set; }
    public bool IsActive { get; set; }
    public bool IsMandatory { get; set; }
    public int DisplayOrder { get; set; }
    public string Version { get; set; } = string.Empty;
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public string? UpdatedBy { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string AuditTimeSpan { get; set; } = string.Empty;
}