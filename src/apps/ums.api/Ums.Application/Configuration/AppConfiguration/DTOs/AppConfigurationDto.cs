namespace Ums.Application.Configuration.AppConfiguration.DTOs;

public sealed record AppConfigurationDto(
    Guid AppConfigurationId,
    Guid? TenantId,
    Guid? SystemSuiteId,
    Guid? ModuleId,
    string Code,
    string Value,
    string Description,
    string Scope,
    bool IsInheritable,
    bool IsEncrypted,
    string Version,
    string Status);
