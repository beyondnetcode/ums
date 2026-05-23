using Ums.Application.Configuration.AppConfiguration.DTOs;

namespace Ums.Application.Configuration.AppConfiguration.Commands;

public sealed record CreateAppConfigurationCommand(
    Guid? TenantId,
    Guid? SystemSuiteId,
    Guid? ModuleId,
    string Code,
    string Value,
    string Description,
    bool IsInheritable,
    bool IsEncrypted) : ICommand<CreateAppConfigurationResponse>;
