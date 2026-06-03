namespace Ums.Infrastructure.Persistence.Seeders;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ums.Domain.Identity.Tenant.TenantParameter;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Configuration.Entities;

public static class ParameterCatalogSeeder
{
    private static readonly Guid[] TenantIds =
    [
        Guid.Parse(CoreDevDataSeeder.InternalAdminTenantId),
        Guid.Parse(CoreDevDataSeeder.RansaTenantId),
        Guid.Parse("A3F5B9D2-7C3D-4C8E-A9B0-123456789ABC"),
        Guid.Parse("C9B736B4-6A84-48F8-B34D-176BC5A6D542"),
        Guid.Parse("5F4E3D2C-1B0A-9F8E-7D6C-543210987654"),
        Guid.Parse("9E8D7C6B-5A4F-3E2D-1C0B-9876543210FE"),
        Guid.Parse("F3E2D1C0-B9A8-7F6E-5D4C-321098765432"),
    ];

    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var dbContext = serviceProvider.GetRequiredService<UmsPlatformDbContext>();
        await SeedDefinitionsAsync(dbContext, cancellationToken);
        await SeedGlobalValuesAsync(dbContext, cancellationToken);
        await SeedTenantValuesAsync(dbContext, cancellationToken);
    }

    private static async Task SeedDefinitionsAsync(UmsPlatformDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existingCodes = (await dbContext.ParameterDefinitions
                .Select(definition => definition.Code)
                .ToListAsync(cancellationToken))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var definitions = BuildDefinitions()
            .Where(definition => !existingCodes.Contains(definition.Code))
            .ToList();

        if (definitions.Count == 0)
        {
            return;
        }

        await dbContext.ParameterDefinitions.AddRangeAsync(definitions, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedGlobalValuesAsync(UmsPlatformDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existingDefinitionIds = await dbContext.ParameterGlobalValues
            .Select(value => value.ParameterDefinitionId)
            .ToHashSetAsync(cancellationToken);

        var now = DateTime.UtcNow;
        var systemActorId = "SYSTEM";
        var globalValues = new List<ParameterGlobalValueRecord>();

        AddGlobalValue(globalValues, existingDefinitionIds, "11111111-1111-1111-1111-111111111103", "3600000", systemActorId, now);
        AddGlobalValue(globalValues, existingDefinitionIds, "11111111-1111-1111-1111-111111111104", "604800000", systemActorId, now);
        AddGlobalValue(globalValues, existingDefinitionIds, "11111111-1111-1111-1111-111111111108", "rest", systemActorId, now);

        if (globalValues.Count == 0)
        {
            return;
        }

        await dbContext.ParameterGlobalValues.AddRangeAsync(globalValues, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedTenantValuesAsync(UmsPlatformDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var definitionsByCode = (await dbContext.ParameterDefinitions.ToListAsync(cancellationToken))
            .ToDictionary(definition => definition.Code, StringComparer.OrdinalIgnoreCase);

        var existingKeys = await dbContext.ParameterTenantValues
            .Select(value => new { value.TenantId, value.ParameterDefinitionId })
            .ToListAsync(cancellationToken);

        var existingKeySet = existingKeys
            .Select(value => (value.TenantId, value.ParameterDefinitionId))
            .ToHashSet();

        var now = DateTime.UtcNow;
        var systemActorId = "SYSTEM";
        var tenantValues = new List<ParameterTenantValueRecord>();

        foreach (var tenantId in TenantIds)
        {
            foreach (var seedValue in BuildTenantValues())
            {
                if (!definitionsByCode.TryGetValue(seedValue.Code, out var definition))
                {
                    continue;
                }

                var key = (tenantId, definition.Id);
                if (existingKeySet.Contains(key))
                {
                    continue;
                }

                tenantValues.Add(new ParameterTenantValueRecord
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId,
                    ParameterDefinitionId = definition.Id,
                    OverrideValue = seedValue.Value,
                    StatusId = 2,
                    Version = "1.0.0",
                    CreatedBy = systemActorId,
                    CreatedAtUtc = now,
                    AuditTimeSpan = now.ToString("O")
                });
            }
        }

        if (tenantValues.Count == 0)
        {
            return;
        }

        await dbContext.ParameterTenantValues.AddRangeAsync(tenantValues, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IReadOnlyList<ParameterDefinitionRecord> BuildDefinitions()
    {
        var now = DateTime.UtcNow;
        var systemActorId = "SYSTEM";

        return
        [
            new ParameterDefinitionRecord
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                Code = "SESSION_TIMEOUT_MINUTES",
                Name = "Session Timeout",
                Description = "Idle session timeout in minutes",
                DataTypeId = 2,
                DefaultValue = "30",
                ScopeId = 3,
                IsActive = true,
                IsMandatory = true,
                DisplayOrder = 1,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = now,
                AuditTimeSpan = now.ToString("O")
            },
            new ParameterDefinitionRecord
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                Code = "MAX_LOGIN_ATTEMPTS",
                Name = "Max Login Attempts",
                Description = "Maximum login attempts before lockout",
                DataTypeId = 2,
                DefaultValue = "5",
                ScopeId = 3,
                IsActive = true,
                IsMandatory = true,
                DisplayOrder = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = now,
                AuditTimeSpan = now.ToString("O")
            },
            new ParameterDefinitionRecord
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111103"),
                Code = "ACCESS_TOKEN_DURATION_MS",
                Name = "Access Token Duration",
                Description = "Access token lifetime in milliseconds",
                DataTypeId = 2,
                DefaultValue = "3600000",
                ScopeId = 1,
                IsActive = true,
                IsMandatory = true,
                DisplayOrder = 3,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = now,
                AuditTimeSpan = now.ToString("O")
            },
            new ParameterDefinitionRecord
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111104"),
                Code = "REFRESH_TOKEN_DURATION_MS",
                Name = "Refresh Token Duration",
                Description = "Refresh token lifetime in milliseconds (7 days)",
                DataTypeId = 2,
                DefaultValue = "604800000",
                ScopeId = 1,
                IsActive = true,
                IsMandatory = true,
                DisplayOrder = 4,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = now,
                AuditTimeSpan = now.ToString("O")
            },
            new ParameterDefinitionRecord
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111105"),
                Code = "MIN_PASSWORD_LENGTH",
                Name = "Min Password Length",
                Description = "Minimum required password length",
                DataTypeId = 2,
                DefaultValue = "12",
                ScopeId = 3,
                IsActive = true,
                IsMandatory = true,
                DisplayOrder = 5,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = now,
                AuditTimeSpan = now.ToString("O")
            },
            new ParameterDefinitionRecord
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111106"),
                Code = "MFA_REQUIRED_FOR_ADMIN",
                Name = "MFA Required for Admin",
                Description = "Require MFA for admin users",
                DataTypeId = 3,
                DefaultValue = "false",
                ScopeId = 3,
                IsActive = true,
                IsMandatory = false,
                DisplayOrder = 6,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = now,
                AuditTimeSpan = now.ToString("O")
            },
            new ParameterDefinitionRecord
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111107"),
                Code = "UI_CUSTOM_BRANDING_ENABLED",
                Name = "Custom Branding Enabled",
                Description = "Enable custom tenant branding",
                DataTypeId = 3,
                DefaultValue = "false",
                ScopeId = 3,
                IsActive = true,
                IsMandatory = false,
                DisplayOrder = 7,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = now,
                AuditTimeSpan = now.ToString("O")
            },
            new ParameterDefinitionRecord
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111108"),
                Code = "FRONTEND_CONFIG_TRANSPORT",
                Name = "Frontend Config Transport",
                Description = "Transport mode for frontend config: graphql or rest",
                DataTypeId = 1,
                DefaultValue = "rest",
                ScopeId = 1,
                IsActive = true,
                IsMandatory = false,
                DisplayOrder = 8,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = now,
                AuditTimeSpan = now.ToString("O")
            },
            new ParameterDefinitionRecord
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111109"),
                Code = "MAX_VALIDITY_PERIOD_DAYS",
                Name = "Max Validity Period Days",
                Description = "Maximum user account validity period in days",
                DataTypeId = 2,
                DefaultValue = "365",
                ScopeId = 3,
                IsActive = true,
                IsMandatory = false,
                DisplayOrder = 9,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = now,
                AuditTimeSpan = now.ToString("O")
            },
            new ParameterDefinitionRecord
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111110"),
                Code = "AUTH_USE_EXTERNAL_IDP",
                Name = "Use External IDP",
                Description = "Whether the tenant uses external Identity Providers instead of local password credentials",
                DataTypeId = 3,
                DefaultValue = "false",
                ScopeId = 3,
                IsActive = true,
                IsMandatory = false,
                DisplayOrder = 10,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = now,
                AuditTimeSpan = now.ToString("O")
            }
        ];
    }

    private static IReadOnlyList<(string Code, string Value)> BuildTenantValues()
    {
        return
        [
            (TenantParameterCodes.ExportProfilePermissionGraphAllowedFormats, "JSON,XML,YAML,CSV"),
            (TenantParameterCodes.ExportProfilePermissionGraphDefaultFormat, "JSON"),
            (TenantParameterCodes.ExportProfilePermissionGraphIncludeTechnicalMetadata, "true"),
            (TenantParameterCodes.ExportProfilePermissionGraphMaskGuids, "false"),
            (TenantParameterCodes.ExportProfilePermissionGraphIncludeFeatureFlags, "true"),
            (TenantParameterCodes.ExportProfilePermissionGraphIncludeEffectivePermissionsSummary, "true"),
            (TenantParameterCodes.ExportProfilePermissionGraphMaxItems, "10000"),
            (TenantParameterCodes.AuthGraphDefaultFormat, "JSON"),
            (TenantParameterCodes.AuthGraphAllowedFormats, "JSON,XML,YAML,CSV"),
            (TenantParameterCodes.AuthGraphIncludeTechnicalMetadata, "true"),
        ];
    }

    private static void AddGlobalValue(
        ICollection<ParameterGlobalValueRecord> values,
        ISet<Guid> existingDefinitionIds,
        string definitionId,
        string effectiveValue,
        string createdBy,
        DateTime now)
    {
        var parsedDefinitionId = Guid.Parse(definitionId);
        if (existingDefinitionIds.Contains(parsedDefinitionId))
        {
            return;
        }

        values.Add(new ParameterGlobalValueRecord
        {
            Id = Guid.NewGuid(),
            ParameterDefinitionId = parsedDefinitionId,
            EffectiveValue = effectiveValue,
            StatusId = 2,
            Version = "1.0.0",
            CreatedBy = createdBy,
            CreatedAtUtc = now,
            AuditTimeSpan = now.ToString("O")
        });
    }
}
