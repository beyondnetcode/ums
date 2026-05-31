namespace Ums.Infrastructure.Persistence.Seeders;

using Microsoft.Extensions.DependencyInjection;
using BeyondNetCode.Shell.Ddd;
using Ums.Domain.Configuration;
using Ums.Domain.Configuration.AppConfiguration;
using Ums.Domain.Configuration.FeatureFlag;
using Ums.Domain.Configuration.IdpConfiguration;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Domain.Authorization;
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
        var tenantConfigs = BuildTenantSpecificConfigurations(actor);
        if (inMemoryAppConfigRepository is not null)
        {
            foreach (var cfg in configs) inMemoryAppConfigRepository.Seed(cfg);
            foreach (var cfg in tenantConfigs) inMemoryAppConfigRepository.Seed(cfg);
        }
        else if (appConfigRepository is not null)
        {
            var allExisting = await appConfigRepository.GetAllAsync(null, cancellationToken);

            var existingGlobalCodes = allExisting
                .Where(c => c.Props.TenantId is null)
                .Select(c => c.Code.GetValue())
                .ToHashSet();

            foreach (var cfg in configs.Where(c => !existingGlobalCodes.Contains(c.Code.GetValue())))
            {
                await appConfigRepository.AddAsync(cfg, cancellationToken);
            }

            var tenantIds = tenantConfigs.Select(c => c.Props.TenantId!.GetValue()).Distinct();
            foreach (var tenantId in tenantIds)
            {
                var existingTenantCodes = allExisting
                    .Where(c => c.Props.TenantId?.GetValue() == tenantId)
                    .Select(c => c.Code.GetValue())
                    .ToHashSet();
                var configsForTenant = tenantConfigs.Where(c => c.Props.TenantId?.GetValue() == tenantId);

                foreach (var cfg in configsForTenant.Where(c => !existingTenantCodes.Contains(c.Code.GetValue())))
                {
                    await appConfigRepository.AddAsync(cfg, cancellationToken);
                }
            }

            await appConfigRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }

        // FeatureFlag
        var flags = await BuildSeedFeatureFlagsAsync(serviceProvider, actor, cancellationToken);
        if (inMemoryFeatureFlagRepository is not null)
            foreach (var flag in flags) inMemoryFeatureFlagRepository.Seed(flag);
        else if (featureFlagRepository is not null)
        {
            var existing = await featureFlagRepository.GetAllAsync(null, cancellationToken);
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
            var existing = await idpConfigRepository.GetAllAsync(null, cancellationToken);
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

        var accessTokenDuration = AppConfigurationAggregate.Create(
            null, null, null,
            Code.Create("ACCESS_TOKEN_DURATION_MS"),
            ConfigurationValue.Create("3600000"),
            Description.Create("Access token lifetime in milliseconds (default: 1 hour)"),
            true, false, actor);
        if (accessTokenDuration.IsSuccess) results.Add(accessTokenDuration.Value);

        var refreshTokenDuration = AppConfigurationAggregate.Create(
            null, null, null,
            Code.Create("REFRESH_TOKEN_DURATION_MS"),
            ConfigurationValue.Create("604800000"),
            Description.Create("Refresh token lifetime in milliseconds (default: 7 days)"),
            true, false, actor);
        if (refreshTokenDuration.IsSuccess) results.Add(refreshTokenDuration.Value);

        var minPasswordLength = AppConfigurationAggregate.Create(
            null, null, null,
            Code.Create("MIN_PASSWORD_LENGTH"),
            ConfigurationValue.Create("12"),
            Description.Create("Minimum required password length"),
            true, false, actor);
        if (minPasswordLength.IsSuccess) results.Add(minPasswordLength.Value);

        var maxValidityPeriod = AppConfigurationAggregate.Create(
            null, null, null,
            Code.Create("MAX_VALIDITY_PERIOD_DAYS"),
            ConfigurationValue.Create("365"),
            Description.Create("Maximum user account validity period in days"),
            true, false, actor);
        if (maxValidityPeriod.IsSuccess) results.Add(maxValidityPeriod.Value);

        var configTransport = AppConfigurationAggregate.Create(
            null, null, null,
            Code.Create("FRONTEND_CONFIG_TRANSPORT"),
            ConfigurationValue.Create("rest"),
            Description.Create("Transport mode for frontend config queries: 'graphql' or 'rest'"),
            true, false, actor);
        if (configTransport.IsSuccess) results.Add(configTransport.Value);

        var useExternalIdp = AppConfigurationAggregate.Create(
            null, null, null,
            Code.Create("AUTH_USE_EXTERNAL_IDP"),
            ConfigurationValue.Create("false"),
            Description.Create("Whether the tenant uses external Identity Providers instead of local password credentials"),
            true, false, actor);
        if (useExternalIdp.IsSuccess) results.Add(useExternalIdp.Value);

        return results;
    }

    private static IReadOnlyList<AppConfigurationAggregate> BuildTenantSpecificConfigurations(ActorId actor)
    {
        var results = new List<AppConfigurationAggregate>();

        // All tenant IDs from the database
        var tenantConfigs = new (Guid TenantId, string Code, string Value, string Description)[]
        {
            // RANSA_PERU (3fa85f64-5717-4562-b3fc-2c963f66afa6)
            (Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), "SESSION_TIMEOUT_MINUTES", "45", "Session timeout for Ransa logistics operations"),
            (Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), "MFA_REQUIRED_FOR_ADMIN", "true", "Require MFA for Ransa admin users"),
            (Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), "UI_CUSTOM_BRANDING_ENABLED", "true", "Enable custom branding for Ransa"),
            (Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"), "AUTH_USE_EXTERNAL_IDP", "true", "Ransa uses external identity providers"),

            // APM_CALLAO (A3F5B9D2-7C3D-4C8E-A9B0-123456789ABC)
            (Guid.Parse("A3F5B9D2-7C3D-4C8E-A9B0-123456789ABC"), "SESSION_TIMEOUT_MINUTES", "20", "Short session timeout for APM port security"),
            (Guid.Parse("A3F5B9D2-7C3D-4C8E-A9B0-123456789ABC"), "MAX_LOGIN_ATTEMPTS", "3", "Strict login attempt limit for APM terminals"),
            (Guid.Parse("A3F5B9D2-7C3D-4C8E-A9B0-123456789ABC"), "MFA_REQUIRED_FOR_ADMIN", "true", "Require MFA for all APM users"),
            (Guid.Parse("A3F5B9D2-7C3D-4C8E-A9B0-123456789ABC"), "AUTH_USE_EXTERNAL_IDP", "false", "APM Callao forces local authentication"),

            // NEPTUNIA (C9B736B4-6A84-48F8-B34D-176BC5A6D542)
            (Guid.Parse("C9B736B4-6A84-48F8-B34D-176BC5A6D542"), "SESSION_TIMEOUT_MINUTES", "60", "Extended session timeout for Neptunia industrial operations"),
            (Guid.Parse("C9B736B4-6A84-48F8-B34D-176BC5A6D542"), "MIN_PASSWORD_LENGTH", "14", "Extended password requirement for Neptunia"),
            (Guid.Parse("C9B736B4-6A84-48F8-B34D-176BC5A6D542"), "AUTH_USE_EXTERNAL_IDP", "true", "Neptunia uses external identity providers"),

            // UNIMAR (5F4E3D2C-1B0A-9F8E-7D6C-543210987654) - Maritime university
            (Guid.Parse("5F4E3D2C-1B0A-9F8E-7D6C-543210987654"), "SESSION_TIMEOUT_MINUTES", "30", "Standard session timeout for university users"),
            (Guid.Parse("5F4E3D2C-1B0A-9F8E-7D6C-543210987654"), "MAX_LOGIN_ATTEMPTS", "5", "Standard login attempt limit"),
            (Guid.Parse("5F4E3D2C-1B0A-9F8E-7D6C-543210987654"), "MFA_REQUIRED_FOR_ADMIN", "false", "MFA optional for UNIMAR"),
            (Guid.Parse("5F4E3D2C-1B0A-9F8E-7D6C-543210987654"), "AUTH_USE_EXTERNAL_IDP", "false", "Unimar forces local authentication"),

            // PAITA_PORT (9E8D7C6B-5A4F-3E2D-1C0B-9876543210FE) - Port authority
            (Guid.Parse("9E8D7C6B-5A4F-3E2D-1C0B-9876543210FE"), "SESSION_TIMEOUT_MINUTES", "15", "Short session for port security"),
            (Guid.Parse("9E8D7C6B-5A4F-3E2D-1C0B-9876543210FE"), "MAX_LOGIN_ATTEMPTS", "3", "Strict login attempts for port"),
            (Guid.Parse("9E8D7C6B-5A4F-3E2D-1C0B-9876543210FE"), "MFA_REQUIRED_FOR_ADMIN", "true", "Require MFA for port admins"),
            (Guid.Parse("9E8D7C6B-5A4F-3E2D-1C0B-9876543210FE"), "AUTH_USE_EXTERNAL_IDP", "true", "Paita uses external identity providers"),

            // INTRADEVCO (F3E2D1C0-B9A8-7F6E-5D4C-321098765432) - Trading company
            (Guid.Parse("F3E2D1C0-B9A8-7F6E-5D4C-321098765432"), "SESSION_TIMEOUT_MINUTES", "40", "Extended session for trading operations"),
            (Guid.Parse("F3E2D1C0-B9A8-7F6E-5D4C-321098765432"), "MAX_LOGIN_ATTEMPTS", "5", "Standard login attempt limit"),
            (Guid.Parse("F3E2D1C0-B9A8-7F6E-5D4C-321098765432"), "MIN_PASSWORD_LENGTH", "10", "Standard password length"),
            (Guid.Parse("F3E2D1C0-B9A8-7F6E-5D4C-321098765432"), "AUTH_USE_EXTERNAL_IDP", "true", "Intradevco uses external identity providers"),
        };

        foreach (var (tenantId, code, value, description) in tenantConfigs)
        {
            var config = AppConfigurationAggregate.Create(
                TenantId.Load(tenantId), null, null,
                Code.Create(code),
                ConfigurationValue.Create(value),
                Description.Create(description),
                true, false, actor);
            if (config.IsSuccess) results.Add(config.Value);
        }

        return results;
    }

    // FeatureFlag.Create(IdValueObject systemSuiteId, IdValueObject? tenantId, string flagCode, FlagType, string flagTargets, LinkedResourceType?, IdValueObject?, int? rolloutPercentage, ActorId)
    private static async Task<IReadOnlyList<FeatureFlagAggregate>> BuildSeedFeatureFlagsAsync(IServiceProvider serviceProvider, ActorId actor, CancellationToken cancellationToken)
    {
        var results = new List<FeatureFlagAggregate>();
        var suiteRepository = serviceProvider.GetService<ISystemSuiteRepository>();

        // If we have the real suite repository, create flags for actual seeded suites
        if (suiteRepository is not null)
        {
            var suites = await suiteRepository.GetAllAsync(null, cancellationToken);
            foreach (var suite in suites)
            {
                var suiteId = IdValueObject.Load(suite.GetId().GetValue());
                var suiteCode = suite.Code.GetValue();

                // Create 2-3 flags per suite based on suite code
                var flagsForSuite = suiteCode switch
                {
                    "LOGISTICS_CORE" => new (string Code, FlagType Type, string Targets, string Description, int? Rollout)[]
                    {
                        ("ENABLE_MFA", FlagType.Boolean, "*", "Multi-factor authentication for logistics users", null),
                        ("DARK_MODE", FlagType.Boolean, "*", "Dark mode UI toggle", null),
                        ("ADVANCED_REPORTING", FlagType.Boolean, "role:ADMIN,role:SUPERVISOR", "Advanced analytics dashboard", null),
                        ("ALLOW_PASSWORD_RESET_BY_ADMIN", FlagType.Boolean, "*", "Allow admin to reset user passwords", null),
                        ("ALLOW_VALIDITY_PERIOD_MODIFICATION", FlagType.Boolean, "*", "Allow admin to modify user validity periods", null),
                    },
                    "WMS" => new (string Code, FlagType Type, string Targets, string Description, int? Rollout)[]
                    {
                        ("BULK_PICKING", FlagType.Boolean, "*", "Bulk picking workflow", null),
                        ("VOICE_PICKING", FlagType.Boolean, "role:OPERATOR", "Voice-directed picking", null),
                        ("AUTO_REORDER", FlagType.Percentage, "*", "Automatic reorder point calculation", 50),
                    },
                    _ => Array.Empty<(string Code, FlagType Type, string Targets, string Description, int? Rollout)>()
                };

                foreach (var flagDef in flagsForSuite)
                {
                    var flag = FeatureFlagAggregate.Create(
                        suiteId,
                        null,
                        flagDef.Code,
                        flagDef.Type,
                        flagDef.Targets,
                        null,
                        null,
                        flagDef.Type == FlagType.Percentage ? flagDef.Rollout : null,
                        actor);

                    if (flag.IsSuccess)
                    {
                        // Activate some flags for demo purposes
                        if (flagDef.Code == "DARK_MODE" || flagDef.Code == "BULK_PICKING")
                        {
                            flag.Value.Activate(actor);
                            flag.Value.DomainEvents.MarkChangesAsCommitted();
                        }

                        // Add criteria for role-targeted flags
                        if (flagDef.Targets.Contains("role:"))
                        {
                            var roleCode = flagDef.Targets.Split(':')[1].Split(',')[0];
                            flag.Value.AddCriteria("RoleCode", "Equals", roleCode, actor);
                            flag.Value.DomainEvents.MarkChangesAsCommitted();
                        }

                        results.Add(flag.Value);
                    }
                }
            }
        }
        else
        {
            // Fallback: create a single flag with the demo system suite ID for tests
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

            if (flag.IsSuccess) results.Add(flag.Value);
        }

        return results;
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
