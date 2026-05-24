namespace Ums.Infrastructure.Persistence.Authorization.Entities;

public sealed class SystemSuiteAppSettingRecord
{
    public Guid Id { get; set; }
    public Guid SystemSuiteId { get; set; }
    public string ConfigKey { get; set; } = string.Empty;
    public string ConfigValue { get; set; } = string.Empty;
    public int ScopeId { get; set; }

    public SystemSuiteRecord SystemSuite { get; set; } = null!;
}
