using Ums.Application.Configuration.IdpConfiguration.DTOs;

namespace Ums.Infrastructure.Configuration.IdpResolution;

internal interface IIdpResolutionStrategy
{
    ResolvedIdpConfigurationDto Resolve(IdpResolutionContext context);
}
