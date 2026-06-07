using MassTransit;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ums.Infrastructure.Persistence.Options;
using Ums.Presentation;
using Ums.Domain.Configuration;
using Ums.Domain.Approvals;
using Ums.Domain.Audit.AuditRecord;
using Ums.Domain.Approvals.NotificationRule;
using Ums.Domain.Enums;
using Ums.Infrastructure.Persistence.Configuration.Entities;
using Ums.Infrastructure.Persistence.Approvals.Entities;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Domain.Configuration.AppConfiguration;
using Ums.Domain.Configuration.FeatureFlag;
using Ums.Domain.Configuration.IdpConfiguration;
using Ums.Domain.Approvals.ApprovalWorkflow;
using System;

namespace Ums.Presentation.IntegrationTest.Infrastructure
{
    /// <summary>
    /// REC-15: <see cref="WebApplicationFactory{TEntryPoint}"/> that wires the UMS API
    /// to the SQL Server Testcontainer started by <see cref="PostgreSqlContainerFixture"/>.
    /// </summary>
    public sealed class PostgreSqlWebApplicationFactory(string connectionString)
        : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");

            builder.ConfigureAppConfiguration((_, cfg) =>
            {
                cfg.Sources.Clear();
                cfg.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    // Persistence — full SQL Server mode
                    ["Persistence:Provider"] = PersistenceProvider.PostgreSql.ToString(),
                    ["Persistence:AggregateStoreMode"] = AggregateStoreMode.PostgreSql.ToString(),
                    ["Persistence:UsePostgreSqlIdentityStores"] = bool.TrueString,
                    ["Persistence:UsePostgreSqlAuthorizationStores"] = bool.TrueString,
                    ["Persistence:UsePostgreSqlConfigurationStores"] = bool.TrueString,
                    ["Persistence:SeedDevData"] = bool.FalseString,
                    ["Persistence:EnableOutbox"] = bool.FalseString,
                    ["Persistence:InitializePlatformStoreOnStartup"] = bool.FalseString,
                    ["Jwt:Secret"] = "INTEGRATION_TEST_JWT_SECRET_KEY_CHANGE_ME_MIN_32_CHARS",
                    ["Jwt:Issuer"] = "ums-api",
                    ["Jwt:Audience"] = "ums-web-app",
                    ["Jwt:ExpirationMinutes"] = "60",
                    ["Jwt:RefreshTokenExpirationDays"] = "7",

                    // Connection
                    ["ConnectionStrings:DefaultConnection"] = connectionString,

                    // Misc
                    ["Secrets:Source"] = "AppSettings",
                    ["AllowedOrigins"] = "https://localhost",
                });
            });

            builder.ConfigureTestServices(services =>
            {
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("Test", options => { });
            });
        }

        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);
            // Seed the database with required configuration and approval aggregates using the real DbContext
            PostgresTestSeeder.SeedConfigurationAggregates(host.Services);
            PostgresTestSeeder.SeedApprovalAggregates(host.Services);
            return host;
        }
    }

    // Shared seeder for PostgreSQL test container – uses EfCore DbContext directly
    internal static class PostgresTestSeeder
    {
        public static void SeedConfigurationAggregates(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Ums.Infrastructure.Persistence.UmsPlatformDbContext>();

            var actor = ActorId.Create("00000000-0000-0000-0000-000000000111");
            var tenantId = TenantId.Load(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
            var systemSuiteId = SystemSuiteId.Load(Guid.Parse("11111111-1111-1111-1111-111111111111"));
            var moduleId = IdValueObject.Load(Guid.Parse("22222222-2222-2222-2222-222222222222"));

            // Feature flag
            if (!db.Set<FeatureFlagRecord>().Any(f => f.SystemSuiteId == systemSuiteId.GetValue() && f.FlagCode == "tenant_dashboard_enabled"))
            {
                var featureFlagRecord = new FeatureFlagRecord
                {
                    Id = Guid.NewGuid(),
                    FlagCode = "tenant_dashboard_enabled",
                    FlagTypeId = FlagType.Boolean.Id,
                    FlagTargets = "tenant-console",
                    StatusId = FlagStatus.Active.Id,
                    LinkedResourceTypeId = (int?)LinkedResourceType.Module.Id,
                    LinkedResourceId = moduleId.GetValue(),
                    RolloutPercentage = null,
                    CreatedBy = actor.GetValue().ToString(),
                    CreatedAtUtc = DateTime.UtcNow,
                    SystemSuiteId = systemSuiteId.GetValue(),
                    TenantId = null
                };
                db.Set<FeatureFlagRecord>().Add(featureFlagRecord);
            }

            // App configuration
            if (!db.Set<AppConfigurationRecord>().Any(a => a.TenantId == tenantId.GetValue() && a.SystemSuiteId == systemSuiteId.GetValue() && a.ModuleId == moduleId.GetValue() && a.Code == "session_timeout_minutes"))
            {
                var appConfigRecord = new AppConfigurationRecord
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId.GetValue(),
                    SystemSuiteId = systemSuiteId.GetValue(),
                    ModuleId = moduleId.GetValue(),
                    Code = "session_timeout_minutes",
                    Value = "30",
                    Description = "Session timeout in minutes for tenant console.",
                    ScopeId = ConfigurationScope.Tenant.Id,
                    IsInheritable = true,
                    IsEncrypted = false,
                    IsNonOverridable = false,
                    Version = "1.0.0",
                    StatusId = ConfigStatus.Published.Id,
                    CreatedBy = actor.GetValue().ToString(),
                    CreatedAtUtc = DateTime.UtcNow
                };
                db.Set<AppConfigurationRecord>().Add(appConfigRecord);
            }

            // Idp configuration
            if (!db.Set<IdpConfigurationRecord>().Any(i => i.TenantId == tenantId.GetValue()))
            {
                var idpConfigRecord = new IdpConfigurationRecord
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId.GetValue(),
                    SystemSuiteId = systemSuiteId.GetValue(),
                    ProviderTypeId = ProviderType.AzureAd.Id,
                    DomainHintsJson = "[\"beyondnet.com\", \"ums.local\"]",
                    ConfigPayload = "{\\\"authority\\\":\\\"https://login.microsoftonline.com/common\\\"}",
                    SecretRef = "kv/idp/azuread",
                    StatusId = 0,
                    ResolutionPriority = 10,
                    Version = 1,
                    CreatedBy = actor.GetValue().ToString(),
                    CreatedAtUtc = DateTime.UtcNow
                };
                db.Set<IdpConfigurationRecord>().Add(idpConfigRecord);
            }

            // Notification rule
            if (!db.Set<NotificationRuleRecord>().Any(n => n.TenantId == tenantId.GetValue()))
            {
                var ruleRecord = new NotificationRuleRecord
                {
                    Id = Guid.NewGuid(),
                    TenantId = tenantId.GetValue(),
                    ChannelId = NotificationChannel.Email.Id,
                    Recipient = "alerts@beyondnet.com",
                    IsActive = true,
                    CreatedBy = actor.GetValue().ToString(),
                    CreatedAtUtc = DateTime.UtcNow
                };
                db.Set<NotificationRuleRecord>().Add(ruleRecord);
            }

            db.SaveChanges();
        }

        public static void SeedApprovalAggregates(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<Ums.Infrastructure.Persistence.UmsPlatformDbContext>();
            var actor = ActorId.Create("00000000-0000-0000-0000-000000000111");
            var tenantId = TenantId.Load(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));

            if (db.Set<ApprovalWorkflow>().Any())
                return;

            var manualWorkflow = ApprovalWorkflow.Create(
                tenantId,
                Code.Create("manual-approval"),
                Name.Create("Manual Approval"),
                Description.Create("Workflow requiring manual approval."),
                UserCategory.Internal,
                true,
                null,
                actor,
                requiredDocumentCount: 1).Value;
            SetAggregateId(manualWorkflow.Props, Guid.Parse("88888888-1111-1111-1111-111111111111"));
            db.Set<ApprovalWorkflow>().Add(manualWorkflow);

            var autoWorkflow = ApprovalWorkflow.Create(
                tenantId,
                Code.Create("auto-approval"),
                Name.Create("Auto Approval"),
                Description.Create("Workflow that auto-approves requests."),
                UserCategory.Internal,
                false,
                null,
                actor).Value;
            SetAggregateId(autoWorkflow.Props, Guid.Parse("88888888-2222-2222-2222-222222222222"));
            db.Set<ApprovalWorkflow>().Add(autoWorkflow);

            db.SaveChanges();
        }

        private static void SetAggregateId(object props, Guid id)
        {
            var idProperty = props.GetType().GetProperty("Id");
            idProperty!.SetValue(props, IdValueObject.Load(id));
        }
    }
}
