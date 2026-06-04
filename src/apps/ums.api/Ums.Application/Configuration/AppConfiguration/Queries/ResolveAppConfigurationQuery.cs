namespace Ums.Application.Configuration.AppConfiguration.Queries;

using Ums.Application.Configuration.AppConfiguration.DTOs;

public sealed record ResolveAppConfigurationQuery(
    string Code,
    Guid?  TenantId,
    Guid?  SuiteId,
    Guid?  ModuleId) : IQuery<ResolvedAppConfigurationDto>;
