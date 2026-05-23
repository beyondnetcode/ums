using Ums.Application.Configuration.AppConfiguration.DTOs;

namespace Ums.Application.Configuration.AppConfiguration.Queries;

public sealed record GetAppConfigurationByIdQuery(Guid AppConfigurationId) : IQuery<AppConfigurationDto>;
