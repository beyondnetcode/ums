using Ums.Application.Configuration.IdpConfiguration.DTOs;

namespace Ums.Application.Configuration.IdpConfiguration.Queries;

public sealed record GetIdpConfigurationByIdQuery(Guid IdpConfigurationId) : IQuery<IdpConfigurationDto>;
