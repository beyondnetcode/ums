namespace Ums.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Audit.AuditRecord;
using Ums.Domain.Approvals;
using Ums.Domain.Authorization;
using Ums.Domain.Configuration;
using Ums.Domain.IGA;
using Ums.Domain.Identity;
using Ums.Infrastructure.Persistence.Authorization;
using Ums.Infrastructure.Persistence.Configuration;
using Ums.Infrastructure.Hosting;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Identity;
using Ums.Infrastructure.Persistence.Interceptors;
using Ums.Infrastructure.Persistence.Options;
using Ums.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddOptions<PersistenceOptions>()
            .Bind(configuration.GetSection(PersistenceOptions.SectionName))
            .Validate(
                options => options.Provider == PersistenceProvider.InMemory
                    || !string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection")),
                "ConnectionStrings:DefaultConnection is required when Persistence.Provider is SqlServer.")
            .ValidateOnStart();

        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddHostedService<PersistenceRuntimeReporter>();

        var persistence = configuration.GetSection(PersistenceOptions.SectionName).Get<PersistenceOptions>() ?? new();

        if (persistence.Provider == PersistenceProvider.SqlServer)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured for SQL Server persistence.");

            services.AddScoped<OrganizationDbContextInterceptor>();

            services.AddDbContext<UmsPlatformDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(connectionString, sqlServer =>
                {
                    sqlServer.MigrationsHistoryTable("__EFMigrationsHistory", UmsPlatformDbContext.DefaultSchema);
                    sqlServer.EnableRetryOnFailure(5);
                });

                options.AddInterceptors(serviceProvider.GetRequiredService<OrganizationDbContextInterceptor>());
            });
        }

        if (persistence.Provider == PersistenceProvider.SqlServer && persistence.UseSqlServerIdentityStores)
        {
            services.AddScoped<ITenantRepository, SqlServerTenantRepository>();
            services.AddScoped<IUserAccountRepository, SqlServerUserAccountRepository>();
            services.AddScoped<IUserManagementDelegationRepository, SqlServerUserManagementDelegationRepository>();
        }
        else
        {
            services.AddSingleton<InMemoryTenantRepository>();
            services.AddSingleton<ITenantRepository>(sp => sp.GetRequiredService<InMemoryTenantRepository>());

            services.AddSingleton<InMemoryUserAccountRepository>();
            services.AddSingleton<IUserAccountRepository>(sp => sp.GetRequiredService<InMemoryUserAccountRepository>());

            services.AddSingleton<InMemoryUserManagementDelegationRepository>();
            services.AddSingleton<IUserManagementDelegationRepository>(sp => sp.GetRequiredService<InMemoryUserManagementDelegationRepository>());
        }

        if (persistence.Provider == PersistenceProvider.SqlServer && persistence.UseSqlServerAuthorizationStores)
        {
            services.AddScoped<IProfileRepository, SqlServerProfileRepository>();
        }
        else
        {
            services.AddSingleton<InMemoryProfileRepository>();
            services.AddSingleton<IProfileRepository>(sp => sp.GetRequiredService<InMemoryProfileRepository>());
        }

        if (persistence.Provider == PersistenceProvider.SqlServer && persistence.UseSqlServerConfigurationStores)
        {
            services.AddScoped<IAppConfigurationRepository, SqlServerAppConfigurationRepository>();
            services.AddScoped<IFeatureFlagRepository, SqlServerFeatureFlagRepository>();
        }
        else
        {
            services.AddSingleton<InMemoryAppConfigurationRepository>();
            services.AddSingleton<IAppConfigurationRepository>(sp => sp.GetRequiredService<InMemoryAppConfigurationRepository>());

            services.AddSingleton<InMemoryFeatureFlagRepository>();
            services.AddSingleton<IFeatureFlagRepository>(sp => sp.GetRequiredService<InMemoryFeatureFlagRepository>());
        }

        // TODO(api-aggregate-tracker): Add SQL Server repositories for SystemSuite and PermissionTemplate.
        services.AddSingleton<InMemorySystemSuiteRepository>();
        services.AddSingleton<ISystemSuiteRepository>(sp => sp.GetRequiredService<InMemorySystemSuiteRepository>());

        services.AddSingleton<InMemoryPermissionTemplateRepository>();
        services.AddSingleton<IPermissionTemplateRepository>(sp => sp.GetRequiredService<InMemoryPermissionTemplateRepository>());

        // TODO(api-aggregate-tracker): Migrate Audit, Approvals, and IGA aggregate repositories from in-memory to SQL Server.
        services.AddSingleton<InMemoryAuditRecordRepository>();
        services.AddSingleton<IAuditRecordRepository>(sp => sp.GetRequiredService<InMemoryAuditRecordRepository>());

        services.AddSingleton<InMemoryApprovalWorkflowRepository>();
        services.AddSingleton<IApprovalWorkflowRepository>(sp => sp.GetRequiredService<InMemoryApprovalWorkflowRepository>());

        services.AddSingleton<InMemoryApprovalRequestRepository>();
        services.AddSingleton<IApprovalRequestRepository>(sp => sp.GetRequiredService<InMemoryApprovalRequestRepository>());

        services.AddSingleton<InMemoryDocumentTypeRepository>();
        services.AddSingleton<IDocumentTypeRepository>(sp => sp.GetRequiredService<InMemoryDocumentTypeRepository>());

        services.AddSingleton<InMemoryUserDocumentRepository>();
        services.AddSingleton<IUserDocumentRepository>(sp => sp.GetRequiredService<InMemoryUserDocumentRepository>());

        services.AddSingleton<InMemoryAccessEnforcementPolicyRepository>();
        services.AddSingleton<IAccessEnforcementPolicyRepository>(sp => sp.GetRequiredService<InMemoryAccessEnforcementPolicyRepository>());

        services.AddSingleton<InMemoryNotificationRuleRepository>();
        services.AddSingleton<INotificationRuleRepository>(sp => sp.GetRequiredService<InMemoryNotificationRuleRepository>());

        services.AddSingleton<InMemoryPromotionRequestRepository>();
        services.AddSingleton<IPromotionRequestRepository>(sp => sp.GetRequiredService<InMemoryPromotionRequestRepository>());

        services.AddSingleton<InMemoryRoleMaturityStatusRepository>();
        services.AddSingleton<IRoleMaturityStatusRepository>(sp => sp.GetRequiredService<InMemoryRoleMaturityStatusRepository>());

        services.AddSingleton<InMemoryIdpConfigurationRepository>();
        services.AddSingleton<IIdpConfigurationRepository>(sp => sp.GetRequiredService<InMemoryIdpConfigurationRepository>());

        // TODO(api-aggregate-tracker): Add SQL Server repository for IdpConfiguration and enable full Configuration context persistence.
        return services;
    }
}
