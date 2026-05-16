namespace Ums.Domain.Configuration;

using Ums.Domain.Kernel;

public interface IIdpConfigurationRepository : ITenantScopedRepository<IdpConfiguration>
{
    Task<IdpConfiguration?> GetEffectiveAsync(Guid tenantId, Guid? systemSuiteId, string emailDomain, CancellationToken cancellationToken = default);
}

public interface IAppConfigurationRepository : ITenantScopedRepository<AppConfiguration>
{
    Task<AppConfiguration?> GetByCodeAsync(Guid tenantId, Guid systemSuiteId, string code, CancellationToken cancellationToken = default);
}

public interface IFeatureFlagRepository : ITenantScopedRepository<FeatureFlag>
{
    Task<FeatureFlag?> GetByCodeAsync(Guid tenantId, string code, CancellationToken cancellationToken = default);
}

