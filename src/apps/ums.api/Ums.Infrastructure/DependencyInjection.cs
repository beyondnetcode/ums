namespace Ums.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Ums.Application.Common.Interfaces;
using Ums.Infrastructure.HealthChecks;
using Ums.Domain.Audit.AuditRecord;
using Ums.Domain.Approvals;
using Ums.Domain.Authorization;
using Ums.Domain.Configuration;
using Ums.Domain.IGA;
using Ums.Domain.Identity;
using Ums.Infrastructure.Persistence.Audit;
using Ums.Infrastructure.Persistence.Approvals;
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

        // OPS-01 / HARDENING-03: Token revocation store.
        // When Redis:Connection is configured → use RedisTokenRevocationStore (all pods share state).
        // Fallback → InMemoryTokenRevocationStore (fine for single-node / dev / tests).
        var redisConnection = configuration["Redis:Connection"];
        if (!string.IsNullOrWhiteSpace(redisConnection))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration         = redisConnection;
                options.InstanceName          = "ums:";   // Namespace prefix to avoid key collisions in shared Redis.
            });
            services.AddSingleton<ITokenRevocationStore, RedisTokenRevocationStore>();
        }
        else
        {
            services.AddSingleton<ITokenRevocationStore, InMemoryTokenRevocationStore>();
        }

        // HARDENING-03: Register Infrastructure MediatR notification handlers (UserDeleted, UserBlocked → revoke tokens).
        // MediatR.AddApplication() only scans Ums.Application; Infrastructure handlers must be registered here.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        services.AddHostedService<PersistenceRuntimeReporter>();
        services.AddHostedService<OutboxDispatcherBackgroundService>(); // FIX-02: dispatch domain events from outbox

        var persistence = configuration.GetSection(PersistenceOptions.SectionName).Get<PersistenceOptions>() ?? new();

        // REC-04: Cross-aggregate transaction scope
        if (persistence.Provider == PersistenceProvider.SqlServer)
            services.AddScoped<IUnitOfWorkScope, UnitOfWorkScope>();
        else
            services.AddSingleton<IUnitOfWorkScope, NoOpUnitOfWorkScope>();

        if (persistence.Provider == PersistenceProvider.SqlServer)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured for SQL Server persistence.");

            services.AddScoped<OrganizationDbContextInterceptor>();
            services.AddScoped<AuditSaveChangesInterceptor>(); // FIX-08: auto-stamp audit columns

            // REC-08: Polly resilience pipeline — circuit breaker on top of EF Core's transient retry
            services.AddResiliencePipeline("ums-sql", pipelineBuilder =>
            {
                pipelineBuilder
                    .AddRetry(new Polly.Retry.RetryStrategyOptions
                    {
                        MaxRetryAttempts = 3,
                        Delay = TimeSpan.FromMilliseconds(200),
                        BackoffType = Polly.DelayBackoffType.Exponential,
                    })
                    .AddCircuitBreaker(new Polly.CircuitBreaker.CircuitBreakerStrategyOptions
                    {
                        FailureRatio    = 0.5,
                        SamplingDuration = TimeSpan.FromSeconds(30),
                        MinimumThroughput = 10,
                        BreakDuration    = TimeSpan.FromSeconds(60),
                    });
            });

            services.AddDbContext<UmsPlatformDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(connectionString, sqlServer =>
                {
                    sqlServer.MigrationsHistoryTable("__EFMigrationsHistory", UmsPlatformDbContext.DefaultSchema);
                    sqlServer.EnableRetryOnFailure(3); // REC-08: reduced; circuit breaker handles sustained failures
                });

                options.AddInterceptors(
                    serviceProvider.GetRequiredService<OrganizationDbContextInterceptor>(),
                    serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
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
            services.AddScoped<IIdpConfigurationRepository, SqlServerIdpConfigurationRepository>();
        }
        else
        {
            services.AddSingleton<InMemoryAppConfigurationRepository>();
            services.AddSingleton<IAppConfigurationRepository>(sp => sp.GetRequiredService<InMemoryAppConfigurationRepository>());

            services.AddSingleton<InMemoryFeatureFlagRepository>();
            services.AddSingleton<IFeatureFlagRepository>(sp => sp.GetRequiredService<InMemoryFeatureFlagRepository>());

            services.AddSingleton<InMemoryIdpConfigurationRepository>();
            services.AddSingleton<IIdpConfigurationRepository>(sp => sp.GetRequiredService<InMemoryIdpConfigurationRepository>());
        }

        // TODO(api-aggregate-tracker): Add SQL Server repositories for SystemSuite and PermissionTemplate.
        services.AddSingleton<InMemorySystemSuiteRepository>();
        services.AddSingleton<ISystemSuiteRepository>(sp => sp.GetRequiredService<InMemorySystemSuiteRepository>());

        services.AddSingleton<InMemoryPermissionTemplateRepository>();
        services.AddSingleton<IPermissionTemplateRepository>(sp => sp.GetRequiredService<InMemoryPermissionTemplateRepository>());

        // TODO(api-aggregate-tracker): Migrate Audit, Approvals, and IGA aggregate repositories from in-memory to SQL Server.
        if (persistence.Provider == PersistenceProvider.SqlServer)
        {
            services.AddScoped<IAuditRecordRepository, SqlServerAuditRecordRepository>();
        }
        else
        {
            services.AddSingleton<InMemoryAuditRecordRepository>();
            services.AddSingleton<IAuditRecordRepository>(sp => sp.GetRequiredService<InMemoryAuditRecordRepository>());
        }

        if (persistence.Provider == PersistenceProvider.SqlServer)
        {
            services.AddScoped<IApprovalWorkflowRepository, SqlServerApprovalWorkflowRepository>();
            services.AddScoped<IApprovalRequestRepository, SqlServerApprovalRequestRepository>();
            services.AddScoped<INotificationRuleRepository, SqlServerNotificationRuleRepository>();
        }
        else
        {
            services.AddSingleton<InMemoryApprovalWorkflowRepository>();
            services.AddSingleton<IApprovalWorkflowRepository>(sp => sp.GetRequiredService<InMemoryApprovalWorkflowRepository>());

            services.AddSingleton<InMemoryApprovalRequestRepository>();
            services.AddSingleton<IApprovalRequestRepository>(sp => sp.GetRequiredService<InMemoryApprovalRequestRepository>());

            services.AddSingleton<InMemoryNotificationRuleRepository>();
            services.AddSingleton<INotificationRuleRepository>(sp => sp.GetRequiredService<InMemoryNotificationRuleRepository>());
        }

        services.AddSingleton<InMemoryDocumentTypeRepository>();
        services.AddSingleton<IDocumentTypeRepository>(sp => sp.GetRequiredService<InMemoryDocumentTypeRepository>());

        services.AddSingleton<InMemoryUserDocumentRepository>();
        services.AddSingleton<IUserDocumentRepository>(sp => sp.GetRequiredService<InMemoryUserDocumentRepository>());

        services.AddSingleton<InMemoryAccessEnforcementPolicyRepository>();
        services.AddSingleton<IAccessEnforcementPolicyRepository>(sp => sp.GetRequiredService<InMemoryAccessEnforcementPolicyRepository>());

        services.AddSingleton<InMemoryPromotionRequestRepository>();
        services.AddSingleton<IPromotionRequestRepository>(sp => sp.GetRequiredService<InMemoryPromotionRequestRepository>());

        services.AddSingleton<InMemoryRoleMaturityStatusRepository>();
        services.AddSingleton<IRoleMaturityStatusRepository>(sp => sp.GetRequiredService<InMemoryRoleMaturityStatusRepository>());

        // TODO(api-aggregate-tracker): Validate SQL Server runtime for Configuration context and add dev seed coverage if needed.
        return services;
    }

    /// <summary>
    /// REC-02: Registers real health checks (liveness, readiness, outbox backlog, SQL Server).
    /// Call from Program.cs: <c>builder.Services.AddInfrastructureHealthChecks(configuration);</c>
    /// Then map endpoints: <c>app.MapInfrastructureHealthCheckEndpoints();</c>
    /// </summary>
    public static IServiceCollection AddInfrastructureHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var persistence = configuration.GetSection(PersistenceOptions.SectionName).Get<PersistenceOptions>() ?? new();

        var builder = services.AddHealthChecks();

        if (persistence.Provider == PersistenceProvider.SqlServer)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (!string.IsNullOrWhiteSpace(connectionString))
            {
                builder.AddSqlServer(
                    connectionString,
                    name: "sql_server",
                    tags: ["ready", "db"]);
            }

            // Outbox backlog monitor — uses the scoped UmsPlatformDbContext
            builder.AddCheck<HealthChecks.OutboxBacklogHealthCheck>(
                "outbox_backlog",
                tags: ["ready", "outbox"]);
        }

        return services;
    }
}
