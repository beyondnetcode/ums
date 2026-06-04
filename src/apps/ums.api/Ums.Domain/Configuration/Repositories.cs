namespace Ums.Domain.Configuration;

using Ums.Domain.Configuration.AppConfiguration;
using Ums.Domain.Configuration.FeatureFlag;
using Ums.Domain.Configuration.IdpConfiguration;
using Ums.Domain.Configuration.Parameter;
using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;
using FeatureFlagAggregate = Ums.Domain.Configuration.FeatureFlag.FeatureFlag;
using IdpConfigurationAggregate = Ums.Domain.Configuration.IdpConfiguration.IdpConfiguration;

public interface IAppConfigurationRepository : IAggregateRepository<AppConfigurationAggregate>
{
    Task<AppConfigurationAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<AppConfigurationAggregate?> GetByScopeAndCodeAsync(Guid? tenantId, Guid? systemSuiteId, Guid? moduleId, string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AppConfigurationAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    /// <summary>
    /// REC-10: Overload of UpdateAsync that enforces optimistic concurrency by setting
    /// the expected RowVersion (from the client's If-Match ETag).  If <paramref name="expectedRowVersion"/>
    /// is null the update proceeds without the explicit concurrency check (EF still tracks its own version).
    /// </summary>
    Task UpdateAsync(AppConfigurationAggregate aggregate, byte[]? expectedRowVersion, CancellationToken cancellationToken = default);
}

public interface IFeatureFlagRepository : IAggregateRepository<FeatureFlagAggregate>
{
    Task<FeatureFlagAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    [Obsolete("Use GetBySystemSuiteAndCodeAsync instead. Global flag codes are no longer unique.")]
    Task<FeatureFlagAggregate?> GetByCodeAsync(string flagCode, CancellationToken cancellationToken = default);
    Task<FeatureFlagAggregate?> GetBySystemSuiteAndCodeAsync(Guid systemSuiteId, string flagCode, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeatureFlagAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<FeatureFlagAggregate>> GetBySystemSuiteIdAsync(Guid systemSuiteId, CancellationToken cancellationToken = default);
}

public interface IParameterDefinitionRepository
{
    Task<ParameterDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ParameterDefinition?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ParameterDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ParameterDefinition definition, CancellationToken cancellationToken = default);
    Task UpdateAsync(ParameterDefinition definition, CancellationToken cancellationToken = default);
    Task<int> CountByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<int> CountGlobalValuesAsync(Guid definitionId, CancellationToken cancellationToken = default);
    Task<int> CountTenantValuesAsync(Guid definitionId, CancellationToken cancellationToken = default);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IParameterGlobalValueRepository
{
    Task<ParameterGlobalValue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ParameterGlobalValue?> GetByDefinitionIdAsync(Guid definitionId, CancellationToken cancellationToken = default);
    Task AddAsync(ParameterGlobalValue value, CancellationToken cancellationToken = default);
    Task UpdateAsync(ParameterGlobalValue value, CancellationToken cancellationToken = default);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IParameterTenantValueRepository
{
    Task<ParameterTenantValue?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ParameterTenantValue?> GetByTenantAndDefinitionAsync(Guid tenantId, Guid definitionId, CancellationToken cancellationToken = default);
    Task AddAsync(ParameterTenantValue value, CancellationToken cancellationToken = default);
    Task UpdateAsync(ParameterTenantValue value, CancellationToken cancellationToken = default);
    Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
}

public interface IIdpConfigurationRepository : IAggregateRepository<IdpConfigurationAggregate>
{
    Task<IdpConfigurationAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IdpConfigurationAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<IdpConfigurationAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
