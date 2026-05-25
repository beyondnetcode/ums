namespace Ums.Infrastructure.Persistence.Seeders;

using Microsoft.Extensions.DependencyInjection;
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
        var cfg = AppConfigurationAggregate.Create(
            null,
            null,
            null,
            Code.Create("MAX_LOGIN_ATTEMPTS"),
            ConfigurationValue.Create("5"),
            Description.Create("Maximum login attempts before locking out the user"),
            true,
            false,
            actor);

        return cfg.IsSuccess ? new[] { cfg.Value } : Array.Empty<AppConfigurationAggregate>();
    }

    // FeatureFlag.Create(string flagCode, FlagType, string flagTargets, LinkedResourceType?, IdValueObject?, int? rolloutPercentage, ActorId)
    private static IReadOnlyList<FeatureFlagAggregate> BuildSeedFeatureFlags(ActorId actor)
    {
        var flag = FeatureFlagAggregate.Create(
            "ENABLE_MFA",
            FlagType.Boolean,
            "*",
            null,
            null,
            null,
            actor);

        return flag.IsSuccess ? new[] { flag.Value } : Array.Empty<FeatureFlagAggregate>();
    }

    // IdpConfiguration.Create(TenantId, SystemSuiteId, ProviderType, string[], string configPayload, string secretRef, int resolutionPriority, Guid? fallbackToId, ActorId)
    private static IReadOnlyList<IdpConfigurationAggregate> BuildSeedIdpConfigs(ActorId actor)
    {
        var ransaTenantId = TenantId.Load(Guid.Parse(CoreDevDataSeeder.RansaTenantId));

        var idp = IdpConfigurationAggregate.Create(
            ransaTenantId,
            SystemSuiteId.Load(Guid.Parse(CoreDevDataSeeder.DemoSystemSuiteId)),
            ProviderType.AzureAd,
            new[] { "ransa.pe", "ransa.com.pe" },
            "{\"clientId\":\"1234\",\"tenantId\":\"abcd-azure-tenant\"}",
            "keyvault://ums/ransa-entra-secret",
            1,
            null,
            actor);

        return idp.IsSuccess ? new[] { idp.Value } : Array.Empty<IdpConfigurationAggregate>();
    }
}
