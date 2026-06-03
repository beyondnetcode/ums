namespace Ums.Infrastructure.Persistence.Seeders;

using System.Reflection;
using BeyondNetCode.Shell.Ddd;
using Microsoft.Extensions.DependencyInjection;
using Ums.Domain.Authorization;
using Ums.Domain.Configuration;
using Ums.Domain.Configuration.AppConfiguration;
using Ums.Domain.Configuration.FeatureFlag;
using Ums.Domain.Configuration.IdpConfiguration;
using Ums.Domain.Identity.Repositories.TenantParameter;
using Ums.Domain.Identity.Tenant.TenantParameter;
using Ums.Domain.Kernel.ValueObjects;
using AppConfigurationAggregate = Ums.Domain.Configuration.AppConfiguration.AppConfiguration;
using FeatureFlagAggregate = Ums.Domain.Configuration.FeatureFlag.FeatureFlag;
using IdpConfigurationAggregate = Ums.Domain.Configuration.IdpConfiguration.IdpConfiguration;
using TenantParameterAggregate = Ums.Domain.Identity.Tenant.TenantParameter.TenantParameter;

public static class ConfigurationDevDataSeeder
{
    private const string TestIdpSystemSuiteId = "11111111-1111-1111-1111-111111111111";
    private static readonly BindingFlags PrivateInstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly Guid DemoSystemSuiteGuid = Guid.Parse(CoreDevDataSeeder.DemoSystemSuiteId);
    private static readonly Guid RansaTenantGuid = Guid.Parse(CoreDevDataSeeder.RansaTenantId);
    private static readonly Guid InternalAdminTenantGuid = Guid.Parse(CoreDevDataSeeder.InternalAdminTenantId);
    private static readonly Guid ApmTenantGuid = Guid.Parse("A3F5B9D2-7C3D-4C8E-A9B0-123456789ABC");
    private static readonly Guid NeptuniaTenantGuid = Guid.Parse("C9B736B4-6A84-48F8-B34D-176BC5A6D542");
    private static readonly Guid UnimarTenantGuid = Guid.Parse("5F4E3D2C-1B0A-9F8E-7D6C-543210987654");
    private static readonly Guid PaitaTenantGuid = Guid.Parse("9E8D7C6B-5A4F-3E2D-1C0B-9876543210FE");
    private static readonly Guid IntradevcoTenantGuid = Guid.Parse("F3E2D1C0-B9A8-7F6E-5D4C-321098765432");

    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var appConfigRepository = serviceProvider.GetService<IAppConfigurationRepository>();
        var featureFlagRepository = serviceProvider.GetService<IFeatureFlagRepository>();
        var idpConfigRepository = serviceProvider.GetService<IIdpConfigurationRepository>();
        var tenantParameterRepository = serviceProvider.GetService<ITenantParameterRepository>();
        var suiteRepository = serviceProvider.GetService<ISystemSuiteRepository>();

        var actor = ActorId.Create(CoreDevDataSeeder.SystemActorId);

        await SeedAppConfigurationsAsync(appConfigRepository, BuildSeedAppConfigurations(actor).Concat(BuildTenantSpecificConfigurations(actor)).ToList(), cancellationToken);
        await SeedFeatureFlagsAsync(featureFlagRepository, suiteRepository, actor, cancellationToken);
        await SeedIdpConfigsAsync(idpConfigRepository, suiteRepository, actor, cancellationToken);
        await SeedTenantParametersAsync(tenantParameterRepository, actor, cancellationToken);
    }

    private static async Task SeedAppConfigurationsAsync(
        IAppConfigurationRepository? repository,
        IReadOnlyList<AppConfigurationAggregate> desiredConfigs,
        CancellationToken cancellationToken)
    {
        if (repository is null)
        {
            return;
        }

        var needsSave = false;

        foreach (var config in desiredConfigs)
        {
            var existing = await repository.GetByScopeAndCodeAsync(
                config.Props.TenantId?.GetValue(),
                config.Props.SystemSuiteId?.GetValue(),
                config.Props.ModuleId?.GetValue(),
                config.Code.GetValue(),
                cancellationToken);

            if (existing is not null)
            {
                continue;
            }

            await repository.AddAsync(config, cancellationToken);
            needsSave = true;
        }

        if (needsSave)
        {
            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
    }

    // AppConfiguration.Create(TenantId?, SystemSuiteId?, IdValueObject? moduleId, Code, ConfigurationValue, Description, bool isInheritable, bool isEncrypted, ActorId)
    private static IReadOnlyList<AppConfigurationAggregate> BuildSeedAppConfigurations(ActorId actor)
    {
        var results = new List<AppConfigurationAggregate>();

        // SESSION_TIMEOUT_MINUTES must come first — ConfigurationRestEndpointTests checks items[0].code
        AddPublishedConfiguration(results,
            null,
            Code.Create("SESSION_TIMEOUT_MINUTES"),
            ConfigurationValue.Create("30"),
            Description.Create("Idle session timeout in minutes"),
            actor);

        AddPublishedConfiguration(results,
            null,
            Code.Create("MAX_LOGIN_ATTEMPTS"),
            ConfigurationValue.Create("5"),
            Description.Create("Maximum login attempts before lockout"),
            actor);

        AddPublishedConfiguration(results,
            null,
            Code.Create("ACCESS_TOKEN_DURATION_MS"),
            ConfigurationValue.Create("3600000"),
            Description.Create("Access token lifetime in milliseconds (default: 1 hour)"),
            actor);

        AddPublishedConfiguration(results,
            null,
            Code.Create("REFRESH_TOKEN_DURATION_MS"),
            ConfigurationValue.Create("604800000"),
            Description.Create("Refresh token lifetime in milliseconds (default: 7 days)"),
            actor);

        AddPublishedConfiguration(results,
            null,
            Code.Create("MIN_PASSWORD_LENGTH"),
            ConfigurationValue.Create("12"),
            Description.Create("Minimum required password length"),
            actor);

        AddPublishedConfiguration(results,
            null,
            Code.Create("MAX_VALIDITY_PERIOD_DAYS"),
            ConfigurationValue.Create("365"),
            Description.Create("Maximum user account validity period in days"),
            actor);

        AddPublishedConfiguration(results,
            null,
            Code.Create("FRONTEND_CONFIG_TRANSPORT"),
            ConfigurationValue.Create("rest"),
            Description.Create("Transport mode for frontend config queries: 'graphql' or 'rest'"),
            actor);

        AddPublishedConfiguration(results,
            null,
            Code.Create("AUTH_USE_EXTERNAL_IDP"),
            ConfigurationValue.Create("false"),
            Description.Create("Whether the tenant uses external Identity Providers instead of local password credentials"),
            actor);

        AddPublishedConfiguration(results,
            null,
            Code.Create("UI_LANGUAGE_DEFAULT"),
            ConfigurationValue.Create("es"),
            Description.Create("Default UI language for the platform"),
            actor);

        AddPublishedConfiguration(results,
            null,
            Code.Create("UI_TIMEZONE_DEFAULT"),
            ConfigurationValue.Create("America/Lima"),
            Description.Create("Default UI timezone for the platform"),
            actor);

        return results;
    }

    private static IReadOnlyList<AppConfigurationAggregate> BuildTenantSpecificConfigurations(ActorId actor)
    {
        var results = new List<AppConfigurationAggregate>();

        foreach (var profile in GetTenantSeedProfiles())
        {
            var tenantId = TenantId.Load(profile.TenantId);

            AddPublishedConfiguration(results,
                tenantId,
                Code.Create("SESSION_TIMEOUT_MINUTES"),
                ConfigurationValue.Create(profile.SessionTimeoutMinutes),
                Description.Create($"Session timeout for {profile.Name}"),
                actor);

            AddPublishedConfiguration(results,
                tenantId,
                Code.Create("MAX_LOGIN_ATTEMPTS"),
                ConfigurationValue.Create(profile.MaxLoginAttempts),
                Description.Create($"Login attempt limit for {profile.Name}"),
                actor);

            AddPublishedConfiguration(results,
                tenantId,
                Code.Create("MIN_PASSWORD_LENGTH"),
                ConfigurationValue.Create(profile.MinPasswordLength),
                Description.Create($"Minimum password length for {profile.Name}"),
                actor);

            AddPublishedConfiguration(results,
                tenantId,
                Code.Create("MAX_VALIDITY_PERIOD_DAYS"),
                ConfigurationValue.Create(profile.MaxValidityPeriodDays),
                Description.Create($"Maximum user validity period for {profile.Name}"),
                actor);

            AddPublishedConfiguration(results,
                tenantId,
                Code.Create("MFA_REQUIRED_FOR_ADMIN"),
                ConfigurationValue.Create(profile.MfaRequiredForAdmin),
                Description.Create($"Whether MFA is required for {profile.Name} administrators"),
                actor);

            AddPublishedConfiguration(results,
                tenantId,
                Code.Create("UI_CUSTOM_BRANDING_ENABLED"),
                ConfigurationValue.Create(profile.CustomBrandingEnabled),
                Description.Create($"Whether custom branding is enabled for {profile.Name}"),
                actor);

            AddPublishedConfiguration(results,
                tenantId,
                Code.Create("AUTH_USE_EXTERNAL_IDP"),
                ConfigurationValue.Create(profile.AuthUseExternalIdp),
                Description.Create($"Whether {profile.Name} uses an external Identity Provider"),
                actor);

            AddPublishedConfiguration(results,
                tenantId,
                Code.Create("UI_LANGUAGE_DEFAULT"),
                ConfigurationValue.Create(profile.DefaultLanguage),
                Description.Create($"Default UI language for {profile.Name}"),
                actor);

            AddPublishedConfiguration(results,
                tenantId,
                Code.Create("UI_TIMEZONE_DEFAULT"),
                ConfigurationValue.Create(profile.DefaultTimezone),
                Description.Create($"Default UI timezone for {profile.Name}"),
                actor);
        }

        return results;
    }

    private static void AddPublishedConfiguration(
        ICollection<AppConfigurationAggregate> results,
        TenantId? tenantId,
        Code code,
        ConfigurationValue value,
        Description description,
        ActorId actor)
    {
        var configResult = AppConfigurationAggregate.Create(
            tenantId,
            null,
            null,
            code,
            value,
            description,
            true,
            false,
            actor);

        if (configResult.IsFailure)
        {
            return;
        }

        var config = configResult.Value;
        var publishResult = config.Publish(actor);
        if (publishResult.IsSuccess)
        {
            config.DomainEvents.MarkChangesAsCommitted();
        }

        results.Add(config);
    }

    private static async Task SeedFeatureFlagsAsync(
        IFeatureFlagRepository? repository,
        ISystemSuiteRepository? suiteRepository,
        ActorId actor,
        CancellationToken cancellationToken)
    {
        if (repository is null || suiteRepository is null)
        {
            return;
        }

        var suites = await suiteRepository.GetAllAsync(null, cancellationToken);
        if (suites.Count == 0)
        {
            return;
        }

        var existingFlags = await repository.GetAllAsync(null, cancellationToken);
        var existingByKey = existingFlags
            .GroupBy(flag => (flag.SystemSuiteId, flag.FlagCode))
            .ToDictionary(group => group.Key, group => group.First());

        var needsSave = false;

        foreach (var suite in suites)
        {
            var suiteId = suite.GetId().GetValue();
            var tenantId = suite.TenantId.GetValue();

            foreach (var definition in GetFeatureFlagDefinitions(suite.Code.GetValue()))
            {
                if (existingByKey.TryGetValue((suiteId, definition.Code), out var existing))
                {
                    if (existing.Props.TenantId?.GetValue() != tenantId)
                    {
                        SetFeatureFlagTenantId(existing, tenantId);
                        await repository.UpdateAsync(existing, cancellationToken);
                        needsSave = true;
                    }
                    continue;
                }

                var flagResult = FeatureFlagAggregate.Create(
                    IdValueObject.Load(suiteId),
                    IdValueObject.Load(tenantId),
                    definition.Code,
                    definition.Type,
                    definition.Targets,
                    null,
                    null,
                    definition.RolloutPercentage,
                    actor);

                if (flagResult.IsFailure)
                {
                    continue;
                }

                ApplyFeatureFlagDemoRules(flagResult.Value, definition, actor);
                await repository.AddAsync(flagResult.Value, cancellationToken);
                existingByKey[(suiteId, definition.Code)] = flagResult.Value;
                needsSave = true;
            }
        }

        if (needsSave)
        {
            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
    }

    private static void ApplyFeatureFlagDemoRules(
        FeatureFlagAggregate flag,
        FeatureFlagSeedDefinition definition,
        ActorId actor)
    {
        if (definition.Code is "DARK_MODE" or "BULK_PICKING")
        {
            flag.Activate(actor);
            flag.DomainEvents.MarkChangesAsCommitted();
        }

        if (definition.Targets.Contains("role:", StringComparison.OrdinalIgnoreCase))
        {
            var roleCode = definition.Targets.Split(':')[1].Split(',')[0];
            flag.AddCriteria("RoleCode", "Equals", roleCode, actor);
            flag.DomainEvents.MarkChangesAsCommitted();
        }

        if (definition.Code == "ENABLE_MFA")
        {
            flag.AddCriteria("TenantId", "Equals", CoreDevDataSeeder.RansaTenantId, actor);
            flag.DomainEvents.MarkChangesAsCommitted();
            flag.Activate(actor);
            flag.DomainEvents.MarkChangesAsCommitted();
        }

        if (definition.Code == "NEW_AUDIT_DASHBOARD")
        {
            flag.AddCriteria("DateRange", "Between",
                "{\"from\":\"2026-01-01T00:00:00Z\",\"to\":\"2026-12-31T23:59:59Z\"}", actor);
            flag.DomainEvents.MarkChangesAsCommitted();
            flag.Activate(actor);
            flag.DomainEvents.MarkChangesAsCommitted();
        }

        if (definition.Code == "PREMIUM_REPORTS")
        {
            flag.AddCriteria("TenantId", "Equals", CoreDevDataSeeder.RansaTenantId, actor);
            flag.AddCriteria("RoleCode", "In", "[\"ADMIN\",\"SUPERVISOR\"]", actor);
            flag.DomainEvents.MarkChangesAsCommitted();
            flag.Activate(actor);
            flag.DomainEvents.MarkChangesAsCommitted();
        }
    }

    private static IReadOnlyList<FeatureFlagSeedDefinition> GetFeatureFlagDefinitions(string suiteCode)
    {
        return suiteCode switch
        {
            "UMS" => new[]
            {
                new FeatureFlagSeedDefinition("ENABLE_MFA", FlagType.Boolean, "*", "Multi-factor authentication for logistics users", null),
                new FeatureFlagSeedDefinition("DARK_MODE", FlagType.Boolean, "*", "Dark mode UI toggle", null),
                new FeatureFlagSeedDefinition("ADVANCED_REPORTING", FlagType.Boolean, "role:ADMIN,role:SUPERVISOR", "Advanced analytics dashboard", null),
                new FeatureFlagSeedDefinition("ALLOW_PASSWORD_RESET_BY_ADMIN", FlagType.Boolean, "*", "Allow admin to reset user passwords", null),
                new FeatureFlagSeedDefinition("ALLOW_VALIDITY_PERIOD_MODIFICATION", FlagType.Boolean, "*", "Allow admin to modify user validity periods", null),
                new FeatureFlagSeedDefinition("NEW_AUDIT_DASHBOARD", FlagType.Boolean, "*", "New audit trail visualization (2026 rollout)", null),
                new FeatureFlagSeedDefinition("PREMIUM_REPORTS", FlagType.Boolean, "*", "Premium analytics reports — Ransa admins only", null),
            },
            "WMS" => new[]
            {
                new FeatureFlagSeedDefinition("BULK_PICKING", FlagType.Boolean, "*", "Bulk picking workflow", null),
                new FeatureFlagSeedDefinition("VOICE_PICKING", FlagType.Boolean, "role:OPERATOR", "Voice-directed picking", null),
                new FeatureFlagSeedDefinition("AUTO_REORDER", FlagType.Percentage, "*", "Automatic reorder point calculation", 50),
            },
            _ => Array.Empty<FeatureFlagSeedDefinition>()
        };
    }

    private static void SetFeatureFlagTenantId(FeatureFlagAggregate featureFlag, Guid tenantId)
    {
        var propsField = typeof(FeatureFlagAggregate).GetField("_props", PrivateInstanceFlags);
        var props = propsField?.GetValue(featureFlag);
        var tenantIdProperty = props?.GetType().GetProperty("TenantId", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        tenantIdProperty?.SetValue(props, IdValueObject.Load(tenantId));
    }

    private static async Task SeedIdpConfigsAsync(
        IIdpConfigurationRepository? repository,
        ISystemSuiteRepository? suiteRepository,
        ActorId actor,
        CancellationToken cancellationToken)
    {
        if (repository is null)
        {
            return;
        }

        var needsSave = false;

        foreach (var profile in GetTenantSeedProfiles().Where(profile => profile.SeedExternalIdp))
        {
            var tenantId = profile.TenantId;
            var existing = await repository.GetByTenantIdAsync(tenantId, cancellationToken);
            var desiredDefinitions = await BuildIdpSeedDefinitions(profile, suiteRepository, actor, cancellationToken);

            foreach (var definition in desiredDefinitions)
            {
                var found = existing.FirstOrDefault(item =>
                    item.SystemSuiteId.GetValue() == definition.SystemSuiteId &&
                    item.ProviderType == definition.ProviderType &&
                    item.Props.DomainHints.SequenceEqual(definition.DomainHints));

                if (found is not null)
                {
                    continue;
                }

                await repository.AddAsync(definition.Configuration, cancellationToken);
                needsSave = true;
            }
        }

        if (needsSave)
        {
            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
    }

    private static async Task<IReadOnlyList<IdpSeedDefinition>> BuildIdpSeedDefinitions(
        TenantSeedProfile profile,
        ISystemSuiteRepository? suiteRepository,
        ActorId actor,
        CancellationToken cancellationToken)
    {
        var results = new List<IdpSeedDefinition>();
        var suiteId = await ResolvePrimarySuiteIdAsync(profile.TenantId, suiteRepository, cancellationToken);

        if (profile.TenantId == RansaTenantGuid)
        {
            AddIdpSeedDefinition(
                results,
                profile.TenantId,
                suiteId,
                ProviderType.AzureAd,
                new[] { "ransa.pe", "ransa.com.pe" },
                "{\"clientId\":\"1234\",\"tenantId\":\"abcd-azure-tenant\"}",
                "keyvault://ums/ransa-entra-secret",
                1,
                null,
                actor);

            AddIdpSeedDefinition(
                results,
                profile.TenantId,
                Guid.Parse(TestIdpSystemSuiteId),
                ProviderType.AzureAd,
                new[] { "beyondnet.com" },
                "{\"authority\":\"https://login.microsoftonline.com/common\",\"clientId\":\"test-client-id\"}",
                "keyvault://ums/beyondnet-entra-secret",
                1,
                null,
                actor);

            return results;
        }

        AddIdpSeedDefinition(
            results,
            profile.TenantId,
            suiteId,
            ProviderType.AzureAd,
            profile.ExternalIdpDomainHints,
            profile.ExternalIdpPayload,
            profile.ExternalIdpSecretRef,
            profile.ExternalIdpResolutionPriority,
            null,
            actor);

        return results;
    }

    private static async Task<Guid> ResolvePrimarySuiteIdAsync(
        Guid tenantId,
        ISystemSuiteRepository? suiteRepository,
        CancellationToken cancellationToken)
    {
        if (suiteRepository is null)
        {
            return DemoSystemSuiteGuid;
        }

        var suites = await suiteRepository.GetByTenantIdAsync(tenantId, cancellationToken);
        return suites.FirstOrDefault()?.GetId().GetValue() ?? DemoSystemSuiteGuid;
    }

    private static void AddIdpSeedDefinition(
        ICollection<IdpSeedDefinition> results,
        Guid tenantId,
        Guid systemSuiteId,
        ProviderType providerType,
        string[] domainHints,
        string configPayload,
        string secretRef,
        int resolutionPriority,
        Guid? fallbackToId,
        ActorId actor)
    {
        var idpResult = IdpConfigurationAggregate.Create(
            TenantId.Load(tenantId),
            SystemSuiteId.Load(systemSuiteId),
            providerType,
            domainHints,
            configPayload,
            secretRef,
            resolutionPriority,
            fallbackToId,
            actor);

        if (idpResult.IsFailure)
        {
            return;
        }

        idpResult.Value.Activate(actor);
        idpResult.Value.DomainEvents.MarkChangesAsCommitted();
        results.Add(new IdpSeedDefinition(idpResult.Value, systemSuiteId, providerType, domainHints));
    }

    private static async Task SeedTenantParametersAsync(
        ITenantParameterRepository? repository,
        ActorId actor,
        CancellationToken cancellationToken)
    {
        if (repository is null)
        {
            return;
        }

        var needsSave = false;
        var desired = BuildSeedTenantParameters(actor);

        foreach (var tenantGroup in desired.GroupBy(parameter => parameter.TenantId.GetValue()))
        {
            var existing = await repository.GetByTenantIdAsync(tenantGroup.Key, cancellationToken);
            var existingCodes = existing.Select(parameter => parameter.Code.GetValue()).ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var parameter in tenantGroup.Where(parameter => !existingCodes.Contains(parameter.Code.GetValue())))
            {
                await repository.AddAsync(parameter, cancellationToken);
                needsSave = true;
            }
        }

        if (needsSave)
        {
            await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
        }
    }

    private static IReadOnlyList<TenantParameterAggregate> BuildSeedTenantParameters(ActorId actor)
    {
        var results = new List<TenantParameterAggregate>();

        foreach (var profile in GetTenantSeedProfiles())
        {
            var tenantId = TenantId.Load(profile.TenantId);

            foreach (var definition in GetTenantParameterDefinitions())
            {
                var parameterResult = TenantParameterAggregate.Create(
                    tenantId,
                    definition.Code,
                    definition.Description,
                    definition.Value,
                    definition.ValueType,
                    definition.Category,
                    definition.IsSensitive,
                    definition.DefaultValue,
                    definition.AllowedValues,
                    actor);

                if (parameterResult.IsSuccess)
                {
                    results.Add(parameterResult.Value);
                }
            }
        }

        return results;
    }

    private static IReadOnlyList<TenantParameterSeedDefinition> GetTenantParameterDefinitions()
    {
        return
        [
            new TenantParameterSeedDefinition(
                TenantParameterCodes.ExportProfilePermissionGraphAllowedFormats,
                "Allowed formats for permission graph exports",
                "JSON,XML,YAML,CSV",
                TenantParameterValueType.StringList,
                TenantParameterCategory.Export,
                false,
                "JSON,XML,YAML,CSV",
                "JSON,XML,YAML,CSV"),
            new TenantParameterSeedDefinition(
                TenantParameterCodes.ExportProfilePermissionGraphDefaultFormat,
                "Default format for permission graph exports",
                "JSON",
                TenantParameterValueType.String,
                TenantParameterCategory.Export,
                false,
                "JSON",
                "JSON,XML,YAML,CSV"),
            new TenantParameterSeedDefinition(
                TenantParameterCodes.ExportProfilePermissionGraphIncludeTechnicalMetadata,
                "Include technical metadata in permission graph exports",
                "true",
                TenantParameterValueType.Boolean,
                TenantParameterCategory.Export,
                false,
                "true",
                null),
            new TenantParameterSeedDefinition(
                TenantParameterCodes.ExportProfilePermissionGraphMaskGuids,
                "Mask GUIDs in permission graph exports",
                "false",
                TenantParameterValueType.Boolean,
                TenantParameterCategory.Export,
                false,
                "false",
                null),
            new TenantParameterSeedDefinition(
                TenantParameterCodes.ExportProfilePermissionGraphIncludeFeatureFlags,
                "Include feature flags in permission graph exports",
                "true",
                TenantParameterValueType.Boolean,
                TenantParameterCategory.Export,
                false,
                "true",
                null),
            new TenantParameterSeedDefinition(
                TenantParameterCodes.ExportProfilePermissionGraphIncludeEffectivePermissionsSummary,
                "Include effective permissions summary in permission graph exports",
                "true",
                TenantParameterValueType.Boolean,
                TenantParameterCategory.Export,
                false,
                "true",
                null),
            new TenantParameterSeedDefinition(
                TenantParameterCodes.ExportProfilePermissionGraphMaxItems,
                "Maximum permission graph export items",
                "10000",
                TenantParameterValueType.Integer,
                TenantParameterCategory.Export,
                false,
                "10000",
                null),
            new TenantParameterSeedDefinition(
                TenantParameterCodes.AuthGraphDefaultFormat,
                "Default serialization format for the authorization graph",
                "JSON",
                TenantParameterValueType.String,
                TenantParameterCategory.Security,
                false,
                "JSON",
                "JSON,XML,YAML,CSV"),
            new TenantParameterSeedDefinition(
                TenantParameterCodes.AuthGraphAllowedFormats,
                "Allowed formats for the authorization graph",
                "JSON,XML,YAML,CSV",
                TenantParameterValueType.StringList,
                TenantParameterCategory.Security,
                false,
                "JSON,XML,YAML,CSV",
                "JSON,XML,YAML,CSV"),
            new TenantParameterSeedDefinition(
                TenantParameterCodes.AuthGraphIncludeTechnicalMetadata,
                "Include technical metadata in the authorization graph",
                "true",
                TenantParameterValueType.Boolean,
                TenantParameterCategory.Security,
                false,
                "true",
                null),
        ];
    }

    private static IReadOnlyList<TenantSeedProfile> GetTenantSeedProfiles()
    {
        return
        [
            new TenantSeedProfile(
                InternalAdminTenantGuid,
                "INTERNAL_ADMIN",
                "30",
                "5",
                "12",
                "365",
                "false",
                "false",
                "false",
                "es",
                "America/Lima",
                false,
                [],
                string.Empty,
                string.Empty,
                1),
            new TenantSeedProfile(
                RansaTenantGuid,
                "RANSA_PERU",
                "45",
                "5",
                "12",
                "365",
                "true",
                "true",
                "true",
                "es",
                "America/Lima",
                true,
                ["ransa.pe", "ransa.com.pe"],
                "{\"authority\":\"https://login.microsoftonline.com/common\",\"clientId\":\"1234\",\"tenantId\":\"abcd-azure-tenant\"}",
                "keyvault://ums/ransa-entra-secret",
                1),
            new TenantSeedProfile(
                ApmTenantGuid,
                "APM_CALLAO",
                "20",
                "3",
                "12",
                "365",
                "true",
                "false",
                "false",
                "es",
                "America/Lima",
                false,
                [],
                string.Empty,
                string.Empty,
                1),
            new TenantSeedProfile(
                NeptuniaTenantGuid,
                "NEPTUNIA",
                "60",
                "5",
                "14",
                "365",
                "true",
                "false",
                "true",
                "es",
                "America/Lima",
                true,
                ["neptunia.com.pe", "neptunia.pe"],
                "{\"authority\":\"https://login.microsoftonline.com/neptunia\",\"clientId\":\"neptunia-client\"}",
                "keyvault://ums/neptunia-entra-secret",
                1),
            new TenantSeedProfile(
                UnimarTenantGuid,
                "UNIMAR",
                "30",
                "5",
                "12",
                "365",
                "false",
                "false",
                "false",
                "es",
                "America/Lima",
                false,
                [],
                string.Empty,
                string.Empty,
                1),
            new TenantSeedProfile(
                PaitaTenantGuid,
                "PAITA_PORT",
                "15",
                "3",
                "12",
                "365",
                "true",
                "false",
                "true",
                "es",
                "America/Lima",
                true,
                ["paita.gob.pe", "paita-port.pe"],
                "{\"authority\":\"https://login.microsoftonline.com/paita\",\"clientId\":\"paita-client\"}",
                "keyvault://ums/paita-entra-secret",
                1),
            new TenantSeedProfile(
                IntradevcoTenantGuid,
                "INTRADEVCO",
                "40",
                "5",
                "10",
                "365",
                "true",
                "false",
                "true",
                "es",
                "America/Lima",
                true,
                ["intradevco.com", "intradevco.pe"],
                "{\"authority\":\"https://login.microsoftonline.com/intradevco\",\"clientId\":\"intradevco-client\"}",
                "keyvault://ums/intradevco-entra-secret",
                1),
        ];
    }

    private sealed record TenantSeedProfile(
        Guid TenantId,
        string Name,
        string SessionTimeoutMinutes,
        string MaxLoginAttempts,
        string MinPasswordLength,
        string MaxValidityPeriodDays,
        string MfaRequiredForAdmin,
        string CustomBrandingEnabled,
        string AuthUseExternalIdp,
        string DefaultLanguage,
        string DefaultTimezone,
        bool SeedExternalIdp,
        string[] ExternalIdpDomainHints,
        string ExternalIdpPayload,
        string ExternalIdpSecretRef,
        int ExternalIdpResolutionPriority);

    private sealed record FeatureFlagSeedDefinition(
        string Code,
        FlagType Type,
        string Targets,
        string Description,
        int? RolloutPercentage);

    private sealed record TenantParameterSeedDefinition(
        string Code,
        string Description,
        string Value,
        TenantParameterValueType ValueType,
        TenantParameterCategory Category,
        bool IsSensitive,
        string? DefaultValue,
        string? AllowedValues);

    private sealed record IdpSeedDefinition(
        IdpConfigurationAggregate Configuration,
        Guid SystemSuiteId,
        ProviderType ProviderType,
        string[] DomainHints);

}
