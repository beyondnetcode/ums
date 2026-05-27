namespace Ums.Infrastructure.Persistence.Seeders;

using Microsoft.Extensions.DependencyInjection;
using Ums.Shell.Ddd;
using Ums.Domain.Configuration;
using Ums.Domain.Configuration.AppConfiguration;
using Ums.Domain.Configuration.FeatureFlag;
using Ums.Domain.Configuration.IdpConfiguration;
using Ums.Domain.Kernel.ValueObjects;
using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;
using FeatureFlagAggregate = Ums.Domain.Configuration.FeatureFlag.FeatureFlag;
using IdpConfigurationAggregate = Ums.Domain.Configuration.IdpConfiguration.IdpConfiguration;

public static class ConfigurationDevDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var appConfigRepository = serviceProvider.GetService<IAppConfigurationRepository>();
        var inMemoryAppConfigRepository = serviceProvider.GetService<InMemoryAppConfigurationRepository>();

        var featureFlagRepository = serviceProvider.GetService<IFeatureFlagRepository>();
        var inMemoryFeatureFlagRepository = serviceProvider.GetService<InMemoryFeatureFlagRepository>();

        var idpConfigRepository = serviceProvider.GetService<IIdpConfigurationRepository>();
        var inMemoryIdpConfigRepository = serviceProvider.GetService<InMemoryIdpConfigurationRepository>();

        var actor = ActorId.Create(CoreDevDataSeeder.SystemActorId);

        // AppConfiguration
        var configs = BuildSeedAppConfigurations(actor);
        if (inMemoryAppConfigRepository is not null)
            foreach (var cfg in configs) inMemoryAppConfigRepository.Seed(cfg);
        else if (appConfigRepository is not null)
        {
            var existing = await appConfigRepository.GetAllAsync(cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var cfg in configs) await appConfigRepository.AddAsync(cfg, cancellationToken);
                await appConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }

        // FeatureFlag
        var flags = BuildSeedFeatureFlags(actor);
        if (inMemoryFeatureFlagRepository is not null)
            foreach (var flag in flags) inMemoryFeatureFlagRepository.Seed(flag);
        else if (featureFlagRepository is not null)
        {
            var existing = await featureFlagRepository.GetAllAsync(cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var flag in flags) await featureFlagRepository.AddAsync(flag, cancellationToken);
                await featureFlagRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }

        // IdpConfig
        var idpConfigs = BuildSeedIdpConfigs(actor);
        if (inMemoryIdpConfigRepository is not null)
            foreach (var idp in idpConfigs) inMemoryIdpConfigRepository.Seed(idp);
        else if (idpConfigRepository is not null)
        {
            var existing = await idpConfigRepository.GetAllAsync(cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var idp in idpConfigs) await idpConfigRepository.AddAsync(idp, cancellationToken);
                await idpConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }
    }

    // AppConfiguration.Create(TenantId?, SystemSuiteId?, IdValueObject? moduleId, Code, ConfigurationValue, Description, bool isInheritable, bool isEncrypted, ActorId)
    private static IReadOnlyList<AppConfigurationAggregate> BuildSeedAppConfigurations(ActorId actor)
    {
        var results = new List<AppConfigurationAggregate>();

        // SESSION_TIMEOUT_MINUTES must come first — ConfigurationRestEndpointTests checks items[0].code
        var sessionTimeout = AppConfigurationAggregate.Create(
            null, null, null,
            Code.Create("SESSION_TIMEOUT_MINUTES"),
            ConfigurationValue.Create("30"),
            Description.Create("Idle session timeout in minutes"),
            true, false, actor);
        if (sessionTimeout.IsSuccess) results.Add(sessionTimeout.Value);

        var maxAttempts = AppConfigurationAggregate.Create(
            null, null, null,
            Code.Create("MAX_LOGIN_ATTEMPTS"),
            ConfigurationValue.Create("5"),
            Description.Create("Maximum login attempts before locking out the user"),
            true, false, actor);
        if (maxAttempts.IsSuccess) results.Add(maxAttempts.Value);

        return results;
    }

    // FeatureFlag.Create(IdValueObject systemSuiteId, IdValueObject? tenantId, string flagCode, FlagType, string flagTargets, LinkedResourceType?, IdValueObject?, int? rolloutPercentage, ActorId)
    private static IReadOnlyList<FeatureFlagAggregate> BuildSeedFeatureFlags(ActorId actor)
    {
        var flag = FeatureFlagAggregate.Create(
            IdValueObject.Load(Guid.Parse(CoreDevDataSeeder.DemoSystemSuiteId)),
            null,
            "ENABLE_MFA",
            FlagType.Boolean,
            "*",
            null,
            null,
            null,
            actor);

        return flag.IsSuccess ? new[] { flag.Value } : Array.Empty<FeatureFlagAggregate>();
    }

    // Fixed system-suite ID used by IdpConfigurationRestEndpointTests and ConfigurationGraphQlTests
    // to resolve IDP by domain "beyondnet.com".
    private const string TestIdpSystemSuiteId = "11111111-1111-1111-1111-111111111111";

    // IdpConfiguration.Create(TenantId, SystemSuiteId, ProviderType, string[], string configPayload, string secretRef, int resolutionPriority, Guid? fallbackToId, ActorId)
    private static IReadOnlyList<IdpConfigurationAggregate> BuildSeedIdpConfigs(ActorId actor)
    {
        var ransaTenantId = TenantId.Load(Guid.Parse(CoreDevDataSeeder.RansaTenantId));
        var results = new List<IdpConfigurationAggregate>();

        // Primary IDP for Ransa internal domain (Draft — standard dev seed)
        var ransaIdp = IdpConfigurationAggregate.Create(
            ransaTenantId,
            SystemSuiteId.Load(Guid.Parse(CoreDevDataSeeder.DemoSystemSuiteId)),
            ProviderType.AzureAd,
            new[] { "ransa.pe", "ransa.com.pe" },
            "{\"clientId\":\"1234\",\"tenantId\":\"abcd-azure-tenant\"}",
            "keyvault://ums/ransa-entra-secret",
            1,
            null,
            actor);
        if (ransaIdp.IsSuccess) results.Add(ransaIdp.Value);

        // Active IDP required by IdpConfigurationRestEndpointTests.ResolveIdpConfiguration_*
        // and ConfigurationGraphQlTests.GraphQlResolveIdpConfigurationQuery_*
        // Uses fixed systemSuiteId and beyondnet.com domain to match test assertions.
        var testIdp = IdpConfigurationAggregate.Create(
            ransaTenantId,
            SystemSuiteId.Load(Guid.Parse(TestIdpSystemSuiteId)),
            ProviderType.AzureAd,
            new[] { "beyondnet.com" },
            "{\"authority\":\"https://login.microsoftonline.com/common\",\"clientId\":\"test-client-id\"}",
            "keyvault://ums/beyondnet-entra-secret",
            1,
            null,
            actor);

        if (testIdp.IsSuccess)
        {
            // Activate so the resolver can select it (requires Active status)
            testIdp.Value.Activate(actor);
            testIdp.Value.DomainEvents.MarkChangesAsCommitted();
            results.Add(testIdp.Value);
        }

        return results;
    }
}
