namespace Ums.Infrastructure;

using MediatR;
using MassTransit;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using BeyondNetCode.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer;
using Ums.Domain.Audit.AuditRecord;
using Ums.Domain.Approvals;
using Ums.Domain.Authorization;
using Ums.Domain.Configuration;
using Ums.Domain.Identity;
using Ums.Domain.Identity.Repositories.TenantParameter;
using Ums.Infrastructure.Persistence.Audit;
using Ums.Infrastructure.Persistence.Approvals;
using Ums.Infrastructure.Persistence.Authorization;
using Ums.Infrastructure.Persistence.Configuration;
using Ums.Domain.Configuration;
using Ums.Infrastructure.Hosting;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Identity;
using Ums.Infrastructure.Persistence.Identity.TenantParameter;
using Ums.Infrastructure.Persistence.Interceptors;
using Ums.Infrastructure.Persistence.Options;
using Ums.Infrastructure.Services;
using Ums.Application.Approvals.ApprovalRequest.Services;
using Ums.Application.Identity.Tenant.TenantParameter.Services;
using Ums.Application.Authorization.Profile.Exporters;
using Ums.Infrastructure.Approvals.ApprovalRequest;
using Ums.Application.Approvals.NotificationRule.Services;
using Ums.Infrastructure.Approvals.NotificationRule;
using Ums.Application.Configuration.IdpConfiguration.Services;
using Ums.Application.Configuration.Services;
using Ums.Domain.Configuration.FeatureFlag;
using Ums.Infrastructure.Configuration;
using Ums.Infrastructure.Configuration.IdpResolution;
using BeyondNetCode.Shell.Factory.Installer.Extensions;
using Ums.Infrastructure.Persistence.Authorization.Exporters;
using Ums.Application.Common.Interfaces;
using Ums.Infrastructure.Services.Notifications;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment? environment = null)
    {
        services
            .AddOptions<PersistenceOptions>()
            .Bind(configuration.GetSection(PersistenceOptions.SectionName))
            .Validate(
                options => options.Provider == PersistenceProvider.InMemory
                    || !string.IsNullOrWhiteSpace(configuration.GetConnectionString("DefaultConnection")),
                "ConnectionStrings:DefaultConnection is required when Persistence.Provider is SqlServer, Sqlite or PostgreSql.")
            .ValidateOnStart();

        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ITenantContext, TenantContext>();
        services.AddSingleton<IPasswordHashingService, BcryptPasswordHashingService>();
        services.AddScoped<RequestContextAccessor>();
        services.AddScoped<IRequestContext>(sp => sp.GetRequiredService<RequestContextAccessor>());
        services.AddScoped<IExecutionContextAccessor>(sp => sp.GetRequiredService<RequestContextAccessor>());
        services.AddSingleton<IValueEncryptionService, AesValueEncryptionService>();
        services.AddScoped<IFeatureFlagEvaluator, FeatureFlagEvaluator>();
        services.AddScoped<IIdpConfigurationResolver, IdpConfigurationResolver>();
        services.AddScoped<INotificationRecipientResolver, NotificationRecipientResolver>();
        services.AddScoped<INotificationService, SimulatedNotificationAdapter>();
        services.AddScoped<IApprovalRequestCreationPolicyResolver, ApprovalRequestCreationPolicyResolver>();
        services.AddSingleton(Channel.CreateUnbounded<AuditTrailEntry>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false,
        }));
        services.AddSingleton<IAuditTrailSink, AuditTrailChannelSink>();

        services.AddFactory(builder => builder
            .AddTransient<EmailNotificationRecipientStrategy, EmailNotificationRecipientStrategy>()
            .AddTransient<SmsNotificationRecipientStrategy, SmsNotificationRecipientStrategy>()
            .AddTransient<InAppNotificationRecipientStrategy, InAppNotificationRecipientStrategy>()
            .AddTransient<ManualApprovalRequestCreationStrategy, ManualApprovalRequestCreationStrategy>()
            .AddTransient<AutoApproveApprovalRequestCreationStrategy, AutoApproveApprovalRequestCreationStrategy>()
            .AddTransient<InternalBcryptIdpResolutionStrategy, InternalBcryptIdpResolutionStrategy>()
            .AddTransient<ZitadelIdpResolutionStrategy, ZitadelIdpResolutionStrategy>()
            .AddTransient<AzureAdIdpResolutionStrategy, AzureAdIdpResolutionStrategy>()
            .AddTransient<OktaIdpResolutionStrategy, OktaIdpResolutionStrategy>()
            .AddTransient<KeycloakIdpResolutionStrategy, KeycloakIdpResolutionStrategy>()
            .AddTransient<Auth0IdpResolutionStrategy, Auth0IdpResolutionStrategy>()
            .AddTransient<GoogleIdpResolutionStrategy, GoogleIdpResolutionStrategy>()
            .AddTransient<LdapIdpResolutionStrategy, LdapIdpResolutionStrategy>()
            .AddTransient<Saml2IdpResolutionStrategy, Saml2IdpResolutionStrategy>()
            .AddTransient<GenericOidcIdpResolutionStrategy, GenericOidcIdpResolutionStrategy>()
            .AddTransient<ProfileJsonExporter, ProfileJsonExporter>()
            .AddTransient<ProfileXmlExporter, ProfileXmlExporter>()
            .AddTransient<ProfileYamlExporter, ProfileYamlExporter>()
            .AddTransient<ProfileCsvExporter, ProfileCsvExporter>()
            // Auth Graph serializers (JSON/XML/YAML/CSV)
            .AddTransient<Authorization.Graph.JsonAuthorizationGraphSerializer, Authorization.Graph.JsonAuthorizationGraphSerializer>()
            .AddTransient<Authorization.Graph.XmlAuthorizationGraphSerializer,  Authorization.Graph.XmlAuthorizationGraphSerializer>()
            .AddTransient<Authorization.Graph.YamlAuthorizationGraphSerializer, Authorization.Graph.YamlAuthorizationGraphSerializer>()
            .AddTransient<Authorization.Graph.CsvAuthorizationGraphSerializer,  Authorization.Graph.CsvAuthorizationGraphSerializer>()
            .AddSource<NotificationRecipientStrategyFactorySetup>()
            .AddSource<ApprovalRequestCreationStrategyFactorySetup>()
            .AddSource<IdpResolutionStrategyFactorySetup>()
            .AddSource<ProfileExportFactorySetup>()
            .AddSource<Authorization.Graph.AuthorizationGraphSerializerFactorySetup>()
            // Production IDP adapters (empty until real adapters are implemented)
            .AddSource<Identity.Auth.IdpAuthAdapterFactorySetup>());

        // Stub IDP adapters — registered per-strategy for non-Production only.
        // In Production, real adapters are registered in IdpAuthAdapterFactorySetup.
        var isNotProduction = environment is null || !environment.IsProduction();
        if (isNotProduction)
        {
            services.AddFactory(b => b
                .AddTransient<Identity.Auth.StubIdpAuthAdapter, Identity.Auth.StubIdpAuthAdapter>()
                .AddSource<Identity.Auth.IdpAuthAdapterStubFactorySetup>());
        }

        services.AddScoped<ITenantParameterProvider, TenantParameterProvider>();
        services.AddScoped<ITenantExportConfigurationProvider, TenantExportConfigurationProvider>();

        // Auth Graph Engine services
        services.AddScoped<Ums.Domain.Authorization.Graph.IAuthorizationGraphBuilder,
                           Ums.Application.Authorization.Graph.AuthorizationGraphBuilderService>();
        services.AddScoped<Ums.Application.Authorization.Graph.IAuthGraphFormatProvider,
                           Ums.Application.Authorization.Graph.AuthGraphFormatProvider>();
        services.AddScoped<Ums.Domain.Identity.Auth.IAuthMethodResolver,
                           Ums.Application.Identity.Auth.AuthMethodResolverService>();
        services.AddScoped<Ums.Domain.Identity.Auth.ILocalAuthStrategy,
                           Ums.Application.Identity.Auth.LocalAuthStrategyService>();
        services.AddScoped<Ums.Domain.Identity.Auth.IIdpAuthStrategy,
                           Ums.Infrastructure.Identity.Auth.IdpAuthStrategyDispatcher>();
        services.AddScoped<Ums.Application.Common.Interfaces.IAuthAuditService,
                           Ums.Infrastructure.Identity.Auth.AuthAuditService>();
        // Default serializer (JSON) — injected directly into CommandHandler
        services.AddTransient<Ums.Application.Authorization.Graph.Serializers.IAuthorizationGraphSerializer,
                              Ums.Infrastructure.Authorization.Graph.JsonAuthorizationGraphSerializer>();

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
        // Register read‑model projection handlers (in‑process MediatR for Phase 1)
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Ums.ReadModels.Projections.PermissionTemplateProjectionHandler).Assembly));

        services.AddHostedService<PersistenceRuntimeReporter>();
        services.AddHostedService<AuditTrailPersistenceBackgroundService>();

        var persistence = configuration.GetSection(PersistenceOptions.SectionName).Get<PersistenceOptions>() ?? new();

        services.AddMassTransit(x =>
        {
            x.SetKebabCaseEndpointNameFormatter();

            x.AddConsumers(Assembly.GetExecutingAssembly());

            x.UsingInMemory((context, cfg) =>
            {
                cfg.ConfigureEndpoints(context);
            });
        });

        // REC-04: Cross-aggregate transaction scope
        if (persistence.Provider == PersistenceProvider.SqlServer || persistence.Provider == PersistenceProvider.Sqlite || persistence.Provider == PersistenceProvider.PostgreSql)
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
        else if (persistence.Provider == PersistenceProvider.Sqlite)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured for SQLite persistence.");

            if (connectionString.Contains("Server=", StringComparison.OrdinalIgnoreCase))
            {
                connectionString = "Data Source=umsdev.db";
            }

            services.AddScoped<OrganizationDbContextInterceptor>();
            services.AddScoped<AuditSaveChangesInterceptor>(); // FIX-08: auto-stamp audit columns

            services.AddDbContext<UmsPlatformDbContext>((serviceProvider, options) =>
            {
                options.UseSqlite(connectionString);

                options.AddInterceptors(
                    serviceProvider.GetRequiredService<OrganizationDbContextInterceptor>(),
                    serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
            });
        }

        else if (persistence.Provider == PersistenceProvider.PostgreSql)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured for PostgreSQL persistence.");

            services.AddScoped<OrganizationDbContextInterceptor>();
            services.AddScoped<AuditSaveChangesInterceptor>();

            services.AddResiliencePipeline("ums-postgres", pipelineBuilder =>
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
                options.UseNpgsql(connectionString, pgOptions =>
                {
                    pgOptions.MigrationsHistoryTable("__EFMigrationsHistory", UmsPlatformDbContext.DefaultSchema);
                    pgOptions.EnableRetryOnFailure(3);
                });

                options.AddInterceptors(
                    serviceProvider.GetRequiredService<OrganizationDbContextInterceptor>(),
                    serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
            });
        }

        if ((persistence.Provider == PersistenceProvider.SqlServer && persistence.UseSqlServerIdentityStores) ||
            (persistence.Provider == PersistenceProvider.Sqlite && persistence.UseSqliteIdentityStores) ||
            (persistence.Provider == PersistenceProvider.PostgreSql && persistence.UsePostgreSqlIdentityStores))
        {
            services.AddScoped<ITenantRepository, SqlServerTenantRepository>();
            services.AddScoped<ITenantParameterRepository, SqlServerTenantParameterRepository>();
            services.AddScoped<ITenantSignupRequestRepository, SqlServerTenantSignupRequestRepository>();
            services.AddScoped<IUserAccountRepository, SqlServerUserAccountRepository>();
            services.AddScoped<IUserManagementDelegationRepository, SqlServerUserManagementDelegationRepository>();
        }
        else
        {
            services.AddSingleton<InMemoryTenantRepository>();
            services.AddSingleton<ITenantRepository>(sp => sp.GetRequiredService<InMemoryTenantRepository>());

            services.AddSingleton<InMemoryTenantParameterRepository>();
            services.AddSingleton<ITenantParameterRepository>(sp => sp.GetRequiredService<InMemoryTenantParameterRepository>());

            services.AddSingleton<InMemoryTenantSignupRequestRepository>();
            services.AddSingleton<ITenantSignupRequestRepository>(sp => sp.GetRequiredService<InMemoryTenantSignupRequestRepository>());

            services.AddSingleton<InMemoryUserAccountRepository>();
            services.AddSingleton<IUserAccountRepository>(sp => sp.GetRequiredService<InMemoryUserAccountRepository>());

            services.AddSingleton<InMemoryUserManagementDelegationRepository>();
            services.AddSingleton<IUserManagementDelegationRepository>(sp => sp.GetRequiredService<InMemoryUserManagementDelegationRepository>());
        }

        if ((persistence.Provider == PersistenceProvider.SqlServer && persistence.UseSqlServerAuthorizationStores) ||
            (persistence.Provider == PersistenceProvider.Sqlite && persistence.UseSqliteAuthorizationStores) ||
            (persistence.Provider == PersistenceProvider.PostgreSql && persistence.UsePostgreSqlAuthorizationStores))
        {
            services.AddScoped<IProfileRepository, SqlServerProfileRepository>();
            services.AddScoped<ISystemSuiteRepository, SqlServerSystemSuiteRepository>();
            services.AddScoped<IPermissionTemplateRepository, SqlServerPermissionTemplateRepository>();
            services.AddScoped<IRoleRepository, SqlServerRoleRepository>();
            services.AddScoped<ITemplateAssignmentRuleRepository, SqlServerTemplateAssignmentRuleRepository>();
        }
        else
        {
            services.AddSingleton<InMemoryProfileRepository>();
            services.AddSingleton<IProfileRepository>(sp => sp.GetRequiredService<InMemoryProfileRepository>());

            services.AddSingleton<InMemorySystemSuiteRepository>();
            services.AddSingleton<ISystemSuiteRepository>(sp => sp.GetRequiredService<InMemorySystemSuiteRepository>());

            services.AddSingleton<InMemoryPermissionTemplateRepository>();
            services.AddSingleton<IPermissionTemplateRepository>(sp => sp.GetRequiredService<InMemoryPermissionTemplateRepository>());

            services.AddSingleton<InMemoryRoleRepository>();
            services.AddSingleton<IRoleRepository>(sp => sp.GetRequiredService<InMemoryRoleRepository>());

            services.AddSingleton<InMemoryTemplateAssignmentRuleRepository>();
            services.AddSingleton<ITemplateAssignmentRuleRepository>(sp => sp.GetRequiredService<InMemoryTemplateAssignmentRuleRepository>());
        }

        if ((persistence.Provider == PersistenceProvider.SqlServer && persistence.UseSqlServerConfigurationStores) ||
            (persistence.Provider == PersistenceProvider.Sqlite && persistence.UseSqliteConfigurationStores) ||
            (persistence.Provider == PersistenceProvider.PostgreSql && persistence.UsePostgreSqlConfigurationStores))
        {
            services.AddScoped<IAppConfigurationRepository, SqlServerAppConfigurationRepository>();
            services.AddScoped<IFeatureFlagRepository, SqlServerFeatureFlagRepository>();
            services.AddScoped<IIdpConfigurationRepository, SqlServerIdpConfigurationRepository>();
            services.AddScoped<IParameterDefinitionRepository, SqlServerParameterDefinitionRepository>();
            services.AddScoped<IParameterGlobalValueRepository, SqlServerParameterGlobalValueRepository>();
            services.AddScoped<IParameterTenantValueRepository, SqlServerParameterTenantValueRepository>();
        }
        else
        {
            services.AddSingleton<InMemoryAppConfigurationRepository>();
            services.AddSingleton<IAppConfigurationRepository>(sp => sp.GetRequiredService<InMemoryAppConfigurationRepository>());

            services.AddSingleton<InMemoryFeatureFlagRepository>();
            services.AddSingleton<IFeatureFlagRepository>(sp => sp.GetRequiredService<InMemoryFeatureFlagRepository>());

            services.AddSingleton<InMemoryIdpConfigurationRepository>();
            services.AddSingleton<IIdpConfigurationRepository>(sp => sp.GetRequiredService<InMemoryIdpConfigurationRepository>());

            services.AddSingleton<InMemoryParameterRepositories>();
            services.AddSingleton<IParameterDefinitionRepository>(sp => sp.GetRequiredService<InMemoryParameterRepositories>());
            services.AddSingleton<IParameterGlobalValueRepository>(sp => sp.GetRequiredService<InMemoryParameterRepositories>());
            services.AddSingleton<IParameterTenantValueRepository>(sp => sp.GetRequiredService<InMemoryParameterRepositories>());
        }

        if (persistence.Provider == PersistenceProvider.SqlServer || persistence.Provider == PersistenceProvider.Sqlite || persistence.Provider == PersistenceProvider.PostgreSql)
        {
            services.AddScoped<IAuditRecordRepository, SqlServerAuditRecordRepository>();
        }
        else
        {
            services.AddSingleton<InMemoryAuditRecordRepository>();
            services.AddSingleton<IAuditRecordRepository>(sp => sp.GetRequiredService<InMemoryAuditRecordRepository>());
        }

        if ((persistence.Provider == PersistenceProvider.SqlServer && persistence.UseSqlServerApprovalsStores) ||
            (persistence.Provider == PersistenceProvider.Sqlite && persistence.UseSqliteApprovalsStores) ||
            (persistence.Provider == PersistenceProvider.PostgreSql && persistence.UsePostgreSqlApprovalsStores))
        {
            services.AddScoped<IApprovalWorkflowRepository, SqlServerApprovalWorkflowRepository>();
            services.AddScoped<IApprovalRequestRepository, SqlServerApprovalRequestRepository>();
            services.AddScoped<INotificationRuleRepository, SqlServerNotificationRuleRepository>();
            services.AddScoped<IDocumentTypeRepository, SqlServerDocumentTypeRepository>();
            services.AddScoped<IUserDocumentRepository, SqlServerUserDocumentRepository>();
            services.AddScoped<IAccessEnforcementPolicyRepository, SqlServerAccessEnforcementPolicyRepository>();
        }
        else
        {
            services.AddSingleton<InMemoryApprovalWorkflowRepository>();
            services.AddSingleton<IApprovalWorkflowRepository>(sp => sp.GetRequiredService<InMemoryApprovalWorkflowRepository>());

            services.AddSingleton<InMemoryApprovalRequestRepository>();
            services.AddSingleton<IApprovalRequestRepository>(sp => sp.GetRequiredService<InMemoryApprovalRequestRepository>());

            services.AddSingleton<InMemoryNotificationRuleRepository>();
            services.AddSingleton<INotificationRuleRepository>(sp => sp.GetRequiredService<InMemoryNotificationRuleRepository>());

            services.AddSingleton<InMemoryDocumentTypeRepository>();
            services.AddSingleton<IDocumentTypeRepository>(sp => sp.GetRequiredService<InMemoryDocumentTypeRepository>());

            services.AddSingleton<InMemoryUserDocumentRepository>();
            services.AddSingleton<IUserDocumentRepository>(sp => sp.GetRequiredService<InMemoryUserDocumentRepository>());

            services.AddSingleton<InMemoryAccessEnforcementPolicyRepository>();
            services.AddSingleton<IAccessEnforcementPolicyRepository>(sp => sp.GetRequiredService<InMemoryAccessEnforcementPolicyRepository>());
        }

        // ── AOP: DispatchProxy aspect-oriented infrastructure ──────────────────────
        // AddAop() registers the built-in aspects (LoggerAspect, AdviceAspect, RetryAspect),
        // the PointCut, AspectExecutor and IFactory<ILogger> / IFactory<IAdvice> singletons.
        //
        // Two ILogger adapters are available — select via [LoggerAspect(Type = typeof(...))] :
        //
        //   IMelLogger       → MelLogger (MEL, Debug level, PII-safe)
        //                      Lightweight; ideal for dev and low-noise production paths.
        //
        //   IUmsLogger       → UmsSerilogLogger (Serilog, Information level, observability-aware)
        //                      Enriches every line with TenantId, CorrelationId, TraceId, SpanId
        //                      and BoundedContext; ships to OTel → Loki via Serilog OTel sink.
        //                      Requires an active IUserContext (scoped) — use on command handlers.
        //
        // AddAopProxy<TService, TImpl>() wraps the concrete handler with a DispatchProxy.
        // It registers TImpl as itself AND replaces the TService registration; MediatR
        // resolves the proxy (last registration wins) and delegates to the concrete handler.
        services.AddAop(builder => 
        {
            builder.AddAspect<AuditTrailAspect>();
            builder.AddAspect<TransactionAspect>();
            builder.AddAspect<TenantValidationAspect>();
            builder.AddAspect<AuthorizationAspect>();
        });

        services.AddKeyedTransient<BeyondNetCode.Shell.Aop.Aspects.ILogger, Ums.Infrastructure.Aop.MelLogger>(
            typeof(Ums.Application.Common.Aop.IMelLogger));

        services.AddKeyedTransient<BeyondNetCode.Shell.Aop.Aspects.ILogger, Ums.Infrastructure.Aop.UmsSerilogLogger>(
            typeof(Ums.Application.Common.Aop.IUmsLogger));

        RegisterMediatRAopProxies(services, typeof(Ums.Application.DependencyInjection).Assembly);

        return services;
    }

    private static void RegisterMediatRAopProxies(IServiceCollection services, Assembly applicationAssembly)
    {
        var addAopProxyMethod = typeof(BeyondNetCode.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer.ServiceCollectionExtension)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .Single(method =>
                method.Name == nameof(BeyondNetCode.Shell.Aop.Microsoft.Extensions.DependencyInjection.Aspects.Installer.ServiceCollectionExtension.AddAopProxy)
                && method.IsGenericMethodDefinition
                && method.GetGenericArguments().Length == 2);

        var handlerContracts = applicationAssembly
            .DefinedTypes
            .Where(type => type is { IsClass: true, IsAbstract: false })
            .SelectMany(type => type.ImplementedInterfaces
                .Where(implemented =>
                    implemented.IsGenericType
                    && implemented.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(implemented => new
                {
                    ServiceType = implemented,
                    ImplementationType = type.AsType(),
                }))
            .ToList();

        foreach (var contract in handlerContracts)
        {
            addAopProxyMethod
                .MakeGenericMethod(contract.ServiceType, contract.ImplementationType)
                .Invoke(null, [services, ServiceLifetime.Scoped]);
        }
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

        if (persistence.Provider == PersistenceProvider.SqlServer || persistence.Provider == PersistenceProvider.Sqlite || persistence.Provider == PersistenceProvider.PostgreSql)
        {
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
            }

                        if (persistence.Provider == PersistenceProvider.PostgreSql)
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (!string.IsNullOrWhiteSpace(connectionString))
                {
                    builder.AddNpgSql(
                        connectionString,
                        name: "postgresql",
                        tags: ["ready", "db"]);
                }
            }


        }

        return services;
    }
}
