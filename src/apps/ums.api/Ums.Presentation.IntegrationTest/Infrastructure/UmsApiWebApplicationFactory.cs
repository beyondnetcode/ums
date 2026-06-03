using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Ums.Domain.Audit.AuditRecord;
using Ums.Domain.Approvals;
using Ums.Domain.Enums;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Audit;
using Ums.Infrastructure.Persistence.Options;
using Ums.Presentation;

namespace Ums.Presentation.IntegrationTest.Infrastructure;

public sealed class UmsApiWebApplicationFactory : WebApplicationFactory<Program>
{
    static UmsApiWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("Persistence__Provider", "InMemory");
        Environment.SetEnvironmentVariable("Persistence__AggregateStoreMode", "InMemory");
        Environment.SetEnvironmentVariable("Persistence__UseSqlServerIdentityStores", "false");
        Environment.SetEnvironmentVariable("Persistence__UseSqlServerAuthorizationStores", "false");
        Environment.SetEnvironmentVariable("Persistence__UseSqlServerConfigurationStores", "false");
        Environment.SetEnvironmentVariable("Persistence__SeedDevData", "true");
        Environment.SetEnvironmentVariable("Persistence__EnableOutbox", "false");
        Environment.SetEnvironmentVariable("Persistence__InitializePlatformStoreOnStartup", "false");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.Sources.Clear();
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Provider"] = PersistenceProvider.InMemory.ToString(),
                ["Persistence:AggregateStoreMode"] = AggregateStoreMode.InMemory.ToString(),
                ["Persistence:UseSqlServerIdentityStores"] = bool.FalseString,
                ["Persistence:UseSqlServerAuthorizationStores"] = bool.FalseString,
                ["Persistence:UseSqlServerConfigurationStores"] = bool.FalseString,
                ["Persistence:SeedDevData"] = bool.TrueString,
                ["Persistence:EnableOutbox"] = bool.FalseString,
                ["Persistence:InitializePlatformStoreOnStartup"] = bool.FalseString,
                ["Jwt:Secret"] = "INTEGRATION_TEST_JWT_SECRET_KEY_CHANGE_ME_MIN_32_CHARS",
                ["Jwt:Issuer"] = "ums-api",
                ["Jwt:Audience"] = "ums-web-app",
                ["Jwt:ExpirationMinutes"] = "60",
                ["Jwt:RefreshTokenExpirationDays"] = "7",
                ["Secrets:Source"] = "AppSettings",
                ["AllowedOrigins"] = "https://localhost",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<UmsPlatformDbContext>();
            services.AddDbContext<UmsPlatformDbContext>((serviceProvider, options) =>
            {
                options.UseInMemoryDatabase("IntegrationTestDb");
            });

            services.RemoveAll<ITenantRepository>();
            services.RemoveAll<IUserAccountRepository>();
            services.RemoveAll<IUserManagementDelegationRepository>();
            services.RemoveAll<IProfileRepository>();
            services.RemoveAll<IAppConfigurationRepository>();
            services.RemoveAll<IFeatureFlagRepository>();
            services.RemoveAll<IIdpConfigurationRepository>();
            services.RemoveAll<IAuditRecordRepository>();
            services.RemoveAll<IApprovalWorkflowRepository>();
            services.RemoveAll<IApprovalRequestRepository>();
            services.RemoveAll<INotificationRuleRepository>();
            services.RemoveAll<IUnitOfWorkScope>();

            services.RemoveAll<InMemoryTenantRepository>();
            services.RemoveAll<InMemoryUserAccountRepository>();
            services.RemoveAll<InMemoryUserManagementDelegationRepository>();
            services.RemoveAll<InMemoryProfileRepository>();
            services.RemoveAll<InMemoryAppConfigurationRepository>();
            services.RemoveAll<InMemoryFeatureFlagRepository>();
            services.RemoveAll<InMemoryIdpConfigurationRepository>();
            services.RemoveAll<InMemoryAuditRecordRepository>();
            services.RemoveAll<InMemoryApprovalWorkflowRepository>();
            services.RemoveAll<InMemoryApprovalRequestRepository>();
            services.RemoveAll<InMemoryNotificationRuleRepository>();

            services.AddSingleton<InMemoryTenantRepository>();
            services.AddSingleton<ITenantRepository>(sp => sp.GetRequiredService<InMemoryTenantRepository>());

            services.AddSingleton<InMemoryUserAccountRepository>();
            services.AddSingleton<IUserAccountRepository>(sp => sp.GetRequiredService<InMemoryUserAccountRepository>());

            services.AddSingleton<InMemoryUserManagementDelegationRepository>();
            services.AddSingleton<IUserManagementDelegationRepository>(sp => sp.GetRequiredService<InMemoryUserManagementDelegationRepository>());

            services.AddSingleton<InMemoryProfileRepository>();
            services.AddSingleton<IProfileRepository>(sp => sp.GetRequiredService<InMemoryProfileRepository>());

            services.AddSingleton<InMemoryAppConfigurationRepository>();
            services.AddSingleton<IAppConfigurationRepository>(sp => sp.GetRequiredService<InMemoryAppConfigurationRepository>());

            services.AddSingleton<InMemoryFeatureFlagRepository>();
            services.AddSingleton<IFeatureFlagRepository>(sp => sp.GetRequiredService<InMemoryFeatureFlagRepository>());

            services.AddSingleton<InMemoryIdpConfigurationRepository>();
            services.AddSingleton<IIdpConfigurationRepository>(sp => sp.GetRequiredService<InMemoryIdpConfigurationRepository>());

            services.AddSingleton<InMemoryAuditRecordRepository>();
            services.AddSingleton<IAuditRecordRepository>(sp => sp.GetRequiredService<InMemoryAuditRecordRepository>());

            services.AddSingleton<InMemoryApprovalWorkflowRepository>();
            services.AddSingleton<IApprovalWorkflowRepository>(sp => sp.GetRequiredService<InMemoryApprovalWorkflowRepository>());

            services.AddSingleton<InMemoryApprovalRequestRepository>();
            services.AddSingleton<IApprovalRequestRepository>(sp => sp.GetRequiredService<InMemoryApprovalRequestRepository>());

            services.AddSingleton<InMemoryNotificationRuleRepository>();
            services.AddSingleton<INotificationRuleRepository>(sp => sp.GetRequiredService<InMemoryNotificationRuleRepository>());

            services.AddSingleton<IUnitOfWorkScope, NoOpUnitOfWorkScope>();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        SeedConfigurationAggregates(host.Services);
        SeedApprovalAggregates(host.Services);
        return host;
    }

    private static void SeedConfigurationAggregates(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var appConfigurationRepository = scope.ServiceProvider.GetRequiredService<InMemoryAppConfigurationRepository>();
        var featureFlagRepository = scope.ServiceProvider.GetRequiredService<InMemoryFeatureFlagRepository>();
        var idpConfigurationRepository = scope.ServiceProvider.GetRequiredService<InMemoryIdpConfigurationRepository>();

        var actor = ActorId.Create("00000000-0000-0000-0000-000000000111");
        var tenantId = TenantId.Load(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));
        var systemSuiteId = SystemSuiteId.Load(Guid.Parse("11111111-1111-1111-1111-111111111111"));
        var moduleId = IdValueObject.Load(Guid.Parse("22222222-2222-2222-2222-222222222222"));

        if (featureFlagRepository.GetBySystemSuiteAndCodeAsync(systemSuiteId.GetValue(), "tenant_dashboard_enabled").GetAwaiter().GetResult() is null)
        {
            var featureFlag = FeatureFlag.Create(
                systemSuiteId,
                null,
                "tenant_dashboard_enabled",
                FlagType.Boolean,
                "tenant-console",
                LinkedResourceType.Module,
                moduleId,
                null,
                actor).Value;

            featureFlag.Activate(actor);
            featureFlagRepository.Seed(featureFlag);
        }

        if (appConfigurationRepository.GetByScopeAndCodeAsync(tenantId.GetValue(), systemSuiteId.GetValue(), moduleId.GetValue(), "session_timeout_minutes").GetAwaiter().GetResult() is null)
        {
            var appConfiguration = AppConfiguration.Create(
                tenantId,
                systemSuiteId,
                moduleId,
                Code.Create("session_timeout_minutes"),
                ConfigurationValue.Create("30"),
                Description.Create("Session timeout in minutes for tenant console."),
                true,
                false,
                actor).Value;

            appConfiguration.Publish(actor);
            appConfigurationRepository.Seed(appConfiguration);
        }

        if (idpConfigurationRepository.GetByTenantIdAsync(tenantId.GetValue()).GetAwaiter().GetResult().Count == 0)
        {
            var idpConfiguration = IdpConfiguration.Create(
                tenantId,
                systemSuiteId,
                ProviderType.AzureAd,
                ["beyondnet.com", "ums.local"],
                "{\"authority\":\"https://login.microsoftonline.com/common\"}",
                "kv/idp/azuread",
                10,
                null,
                actor).Value;

            idpConfiguration.Activate(actor);
            idpConfigurationRepository.Seed(idpConfiguration);
        }

        var notificationRuleRepository = scope.ServiceProvider.GetRequiredService<InMemoryNotificationRuleRepository>();
        if (notificationRuleRepository.GetByTenantIdAsync(tenantId.GetValue()).GetAwaiter().GetResult().Count == 0)
        {
            var notificationRule = Ums.Domain.Approvals.NotificationRule.NotificationRule.Create(
                tenantId,
                NotificationChannel.Email,
                TextValueObject.Create("alerts@beyondnet.com"),
                actor).Value;

            notificationRuleRepository.Seed(notificationRule);
        }
    }

    private static void SeedApprovalAggregates(IServiceProvider services)
    {
        using var scope = services.CreateScope();

        var workflowRepository = scope.ServiceProvider.GetRequiredService<InMemoryApprovalWorkflowRepository>();
        var actor = ActorId.Create("00000000-0000-0000-0000-000000000111");
        var tenantId = TenantId.Load(Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"));

        if (workflowRepository.GetAllAsync().GetAwaiter().GetResult().Count > 0)
        {
            return;
        }

        var manualWorkflow = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalWorkflow.Create(
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
        workflowRepository.Seed(manualWorkflow);

        var autoWorkflow = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalWorkflow.Create(
            tenantId,
            Code.Create("auto-approval"),
            Name.Create("Auto Approval"),
            Description.Create("Workflow that auto-approves requests."),
            UserCategory.Internal,
            false,
            null,
            actor).Value;
        SetAggregateId(autoWorkflow.Props, Guid.Parse("88888888-2222-2222-2222-222222222222"));
        workflowRepository.Seed(autoWorkflow);
    }

    private static void SetAggregateId(object props, Guid id)
    {
        var idProperty = props.GetType().GetProperty("Id");
        idProperty!.SetValue(props, IdValueObject.Load(id));
    }
}
