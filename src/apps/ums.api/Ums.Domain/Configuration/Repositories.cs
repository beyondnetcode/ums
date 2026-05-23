namespace Ums.Domain.Configuration;

using Ums.Domain.Configuration.AppConfiguration;
using Ums.Domain.Configuration.FeatureFlag;
using Ums.Domain.Configuration.IdpConfiguration;
using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;
using FeatureFlagAggregate = Ums.Domain.Configuration.FeatureFlag.FeatureFlag;
using IdpConfigurationAggregate = Ums.Domain.Configuration.IdpConfiguration.IdpConfiguration;

public interface IAppConfigurationRepository : IAggregateRepository<AppConfigurationAggregate>
{
    Task<AppConfigurationAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppConfigurationAggregate?> GetByScopeAndCodeAsync(Guid? tenantId, Guid? systemSuiteId, Guid? moduleId, string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppConfigurationAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IFeatureFlagRepository : IAggregateRepository<FeatureFlagAggregate>
{
    Task<FeatureFlagAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<FeatureFlagAggregate?> GetByCodeAsync(string flagCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeatureFlagAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
}

public interface IIdpConfigurationRepository : IAggregateRepository<IdpConfigurationAggregate>
{
    Task<IdpConfigurationAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IdpConfigurationAggregate>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IdpConfigurationAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
