namespace Ums.Infrastructure.Persistence.Seeders;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Configuration.Entities;

public static class ParameterCatalogSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var dbContext = serviceProvider.GetRequiredService<UmsPlatformDbContext>();
        await SeedDefinitionsAsync(dbContext, cancellationToken);
        await SeedGlobalValuesAsync(dbContext, cancellationToken);
        await SeedTenantValuesAsync(dbContext, cancellationToken);
    }

    private static async Task SeedDefinitionsAsync(UmsPlatformDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existingCount = await dbContext.ParameterDefinitions.CountAsync(cancellationToken);
        if (existingCount > 0)
            return;

        var systemActorId = "SYSTEM";

        var definitions = new List<ParameterDefinitionRecord>
        {
            new()
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
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
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
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
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
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
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
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
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
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
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
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
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
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
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
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
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
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            }
        };

        await dbContext.ParameterDefinitions.AddRangeAsync(definitions, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedGlobalValuesAsync(UmsPlatformDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existingCount = await dbContext.ParameterGlobalValues.CountAsync(cancellationToken);
        if (existingCount > 0)
            return;

        var systemActorId = "SYSTEM";

        var globalValues = new List<ParameterGlobalValueRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111103"),
                EffectiveValue = "3600000",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
            {
                Id = Guid.NewGuid(),
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111104"),
                EffectiveValue = "604800000",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
            {
                Id = Guid.NewGuid(),
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111108"),
                EffectiveValue = "rest",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            }
        };

        await dbContext.ParameterGlobalValues.AddRangeAsync(globalValues, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public static async Task SeedTenantValuesAsync(UmsPlatformDbContext dbContext, CancellationToken cancellationToken = default)
    {
        var existingCount = await dbContext.ParameterTenantValues.CountAsync(cancellationToken);
        if (existingCount > 0)
            return;

        var systemActorId = "SYSTEM";

        var ransaId = Guid.Parse("3FA85F64-5717-4562-B3FC-2C963F66AFA6");
        var apmId = Guid.Parse("A3F5B9D2-7C3D-4C8E-A9B0-123456789ABC");
        var neptuniaId = Guid.Parse("C9B736B4-6A84-48F8-B34D-176BC5A6D542");
        var unimarId = Guid.Parse("F3E2D1C0-B9A8-7F6E-5D4C-321098765432");
        var paitaId = Guid.Parse("9E8D7C6B-5A4F-3E2D-1C0B-9876543210FE");
        var intradevcoId = Guid.Parse("5F4E3D2C-1B0A-9F8E-7D6C-543210987654");

        var tenantValues = new List<ParameterTenantValueRecord>
        {
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = ransaId,
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                OverrideValue = "45",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = ransaId,
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111106"),
                OverrideValue = "true",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = ransaId,
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111107"),
                OverrideValue = "true",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = apmId,
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                OverrideValue = "20",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = apmId,
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                OverrideValue = "3",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = apmId,
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111106"),
                OverrideValue = "true",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = neptuniaId,
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111101"),
                OverrideValue = "60",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = neptuniaId,
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111105"),
                OverrideValue = "14",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = unimarId,
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                OverrideValue = "5",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = paitaId,
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                OverrideValue = "5",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            },
            new()
            {
                Id = Guid.NewGuid(),
                TenantId = intradevcoId,
                ParameterDefinitionId = Guid.Parse("11111111-1111-1111-1111-111111111102"),
                OverrideValue = "5",
                StatusId = 2,
                Version = "1.0.0",
                CreatedBy = systemActorId,
                CreatedAtUtc = DateTime.UtcNow,
                AuditTimeSpan = DateTime.UtcNow.ToString("O")
            }
        };

        await dbContext.ParameterTenantValues.AddRangeAsync(tenantValues, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}