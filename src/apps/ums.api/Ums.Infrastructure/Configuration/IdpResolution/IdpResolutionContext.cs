using IdpConfigurationAggregate = Ums.Domain.Configuration.IdpConfiguration.IdpConfiguration;

namespace Ums.Infrastructure.Configuration.IdpResolution;

internal sealed record IdpResolutionContext(IdpConfigurationAggregate Configuration, bool DomainMatched);
