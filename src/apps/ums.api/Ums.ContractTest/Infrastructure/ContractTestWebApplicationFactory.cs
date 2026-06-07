using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Approvals;
using Ums.Domain.Audit.AuditRecord;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Audit;
using Ums.Infrastructure.Persistence.Options;
using Ums.ReadModels;
using Ums.Presentation;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Ums.Domain.Identity;
using Ums.Domain.Authorization;
using Ums.Domain.Audit;
using Ums.Domain.Configuration;
using Microsoft.Extensions.Hosting;

namespace Ums.ContractTest.Infrastructure;

/// <summary>
/// WebApplicationFactory for provider verification tests.
/// Starts the UMS API with InMemory stores so PactNet consumer tests can verify 
/// contracts against a live server.
/// </summary>
public sealed class ContractTestWebApplicationFactory : WebApplicationFactory<Program>
{
    static ContractTestWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT",              "Development");
        Environment.SetEnvironmentVariable("Persistence__Provider",               "InMemory");
        Environment.SetEnvironmentVariable("Persistence__UseSqlServerIdentityStores",      "false");
        Environment.SetEnvironmentVariable("Persistence__UseSqlServerAuthorizationStores", "false");
        Environment.SetEnvironmentVariable("Persistence__UseSqlServerConfigurationStores", "false");
        Environment.SetEnvironmentVariable("Persistence__SeedDevData",                     "true");
        Environment.SetEnvironmentVariable("Persistence__EnableOutbox",                    "false");
        Environment.SetEnvironmentVariable("Persistence__InitializePlatformStoreOnStartup","false");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.Sources.Clear();
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Persistence:Provider"]                        = PersistenceProvider.InMemory.ToString(),
                ["Persistence:UseSqlServerIdentityStores"]      = bool.FalseString,
                ["Persistence:UseSqlServerAuthorizationStores"] = bool.FalseString,
                ["Persistence:UseSqlServerConfigurationStores"] = bool.FalseString,
                ["Persistence:SeedDevData"]                     = bool.TrueString,
                ["Persistence:EnableOutbox"]                    = bool.FalseString,
                ["Persistence:InitializePlatformStoreOnStartup"]= bool.FalseString,
                ["Secrets:Source"]                              = "AppSettings",
                ["AllowedOrigins"]                              = "https://localhost",
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<UmsPlatformDbContext>();
            services.AddDbContext<UmsPlatformDbContext>((_, options) =>
                options.UseInMemoryDatabase("ContractTestDb"));

            // Register ReadModelDbContext with InMemory so projection handlers can be resolved.
            services.RemoveAll<ReadModelDbContext>();
            services.AddDbContext<ReadModelDbContext>((_, options) =>
                options.UseInMemoryDatabase("ContractTestReadModelDb"));

            // Swap all SQL Server stores for their InMemory equivalents.
            foreach (var t in new[]
            {
                typeof(ITenantRepository), typeof(IUserAccountRepository),
                typeof(IUserManagementDelegationRepository), typeof(IProfileRepository),
                typeof(IAppConfigurationRepository), typeof(IFeatureFlagRepository),
                typeof(IIdpConfigurationRepository), typeof(IAuditRecordRepository),
                typeof(IApprovalWorkflowRepository), typeof(IApprovalRequestRepository),
                typeof(INotificationRuleRepository), typeof(IUnitOfWorkScope),
            }) services.RemoveAll(t);

            services.AddSingleton<InMemoryTenantRepository>();
            services.AddSingleton<ITenantRepository>(sp => sp.GetRequiredService<InMemoryTenantRepository>());

            services.AddSingleton<InMemoryUserAccountRepository>();
            services.AddSingleton<IUserAccountRepository>(sp => sp.GetRequiredService<InMemoryUserAccountRepository>());

            services.AddSingleton<InMemoryUserManagementDelegationRepository>();
            services.AddSingleton<IUserManagementDelegationRepository>(sp =>
                sp.GetRequiredService<InMemoryUserManagementDelegationRepository>());

            services.AddSingleton<InMemoryProfileRepository>();
            services.AddSingleton<IProfileRepository>(sp => sp.GetRequiredService<InMemoryProfileRepository>());

            services.AddSingleton<InMemoryAppConfigurationRepository>();
            services.AddSingleton<IAppConfigurationRepository>(sp =>
                sp.GetRequiredService<InMemoryAppConfigurationRepository>());

            services.AddSingleton<InMemoryFeatureFlagRepository>();
            services.AddSingleton<IFeatureFlagRepository>(sp =>
                sp.GetRequiredService<InMemoryFeatureFlagRepository>());

            services.AddSingleton<InMemoryIdpConfigurationRepository>();
            services.AddSingleton<IIdpConfigurationRepository>(sp =>
                sp.GetRequiredService<InMemoryIdpConfigurationRepository>());

            services.AddSingleton<InMemoryAuditRecordRepository>();
            services.AddSingleton<IAuditRecordRepository>(sp =>
                sp.GetRequiredService<InMemoryAuditRecordRepository>());

            services.AddSingleton<InMemoryApprovalWorkflowRepository>();
            services.AddSingleton<IApprovalWorkflowRepository>(sp =>
                sp.GetRequiredService<InMemoryApprovalWorkflowRepository>());

            services.AddSingleton<InMemoryApprovalRequestRepository>();
            services.AddSingleton<IApprovalRequestRepository>(sp =>
                sp.GetRequiredService<InMemoryApprovalRequestRepository>());

            services.AddSingleton<InMemoryNotificationRuleRepository>();
            services.AddSingleton<INotificationRuleRepository>(sp =>
                sp.GetRequiredService<InMemoryNotificationRuleRepository>());

            services.AddSingleton<IUnitOfWorkScope, NoOpUnitOfWorkScope>();
        });
    }
}
