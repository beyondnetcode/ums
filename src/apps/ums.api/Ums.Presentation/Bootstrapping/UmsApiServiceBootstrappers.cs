using Asp.Versioning;
using Asp.Versioning.Builder;
using System.Diagnostics;
using System.Threading.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Ums.Application;
using Ums.Domain.Identity;
using Ums.Globalization;
using Ums.Globalization.Access;
using Ums.Infrastructure;
using Ums.Infrastructure.HealthChecks;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Options;
using Ums.Infrastructure.Persistence.Seeders;
using Ums.Presentation.Endpoints;
using Ums.Presentation.Services;
using Ums.Presentation.Endpoints.Approvals.AccessEnforcementPolicy;
using Ums.Presentation.Endpoints.Approvals.AccessEnforcementPolicy.Queries;
using Ums.Presentation.Endpoints.Approvals.ApprovalRequest;
using Ums.Presentation.Endpoints.Approvals.ApprovalRequest.Queries;
using Ums.Presentation.Endpoints.Approvals.ApprovalWorkflow;
using Ums.Presentation.Endpoints.Approvals.ApprovalWorkflow.Queries;
using Ums.Presentation.Endpoints.Approvals.DocumentType;
using Ums.Presentation.Endpoints.Approvals.DocumentType.Queries;
using Ums.Presentation.Endpoints.Approvals.NotificationRule;
using Ums.Presentation.Endpoints.Approvals.NotificationRule.Queries;
using Ums.Presentation.Endpoints.Approvals.UserDocument;
using Ums.Presentation.Endpoints.Approvals.UserDocument.Queries;
using Ums.Presentation.Endpoints.Audit.AuditRecord;
using Ums.Presentation.Endpoints.Audit.AuditRecord.Queries;
using Ums.Presentation.Endpoints.Authorization.Profile;
using Ums.Presentation.Endpoints.Authorization.Profile.Queries;
using Ums.Presentation.Endpoints.Authorization.SystemSuite;
using Ums.Presentation.Extensions;
using Ums.Presentation.Endpoints.Authorization.SystemSuite.Queries;
using Ums.Presentation.Endpoints.Authorization.Template;
using Ums.Presentation.Endpoints.Authorization.Template.Queries;
using Ums.Presentation.Endpoints.Authorization.AssignmentRule;
using Ums.Presentation.Endpoints.Authorization.Role;
using Ums.Presentation.Endpoints.Configuration.AppConfiguration;
using Ums.Presentation.Endpoints.Configuration.AppConfiguration.Queries;
using Ums.Presentation.Endpoints.Configuration.FeatureFlag;
using Ums.Presentation.Endpoints.Configuration.FeatureFlag.Queries;
using Ums.Presentation.Endpoints.Configuration.IdpConfiguration;
using Ums.Presentation.Endpoints.Configuration.IdpConfiguration.Queries;
using Ums.Presentation.Endpoints.Configuration.Parameter;
using Ums.Presentation.Endpoints.Configuration.ParameterCatalog.Queries;
using Ums.Presentation.Endpoints.Identity.Auth;
using Ums.Presentation.Endpoints.Identity.Tenant;
using Ums.Presentation.Endpoints.Identity.Tenant.Queries;
using Ums.Presentation.Endpoints.Identity.UserAccount;
using Ums.Presentation.Endpoints.Identity.UserAccount.Queries;
using Ums.Presentation.Endpoints.Identity.UserManagementDelegation;
using Ums.Presentation.Endpoints.Identity.UserManagementDelegation.Queries;
using Ums.Presentation.Endpoints.Identity.Onboarding;
using Ums.Presentation.Endpoints.IGA.PromotionRequest;
using Ums.Presentation.Endpoints.IGA.PromotionRequest.Queries;
using Ums.Presentation.Endpoints.IGA.RoleMaturityStatus;
using Ums.Presentation.Endpoints.IGA.RoleMaturityStatus.Queries;
using Ums.Presentation.GraphQL;
using Ums.Presentation.Middleware;
using Ums.Presentation.Bootstrapping.Bootstrappers;
using BeyondNetCode.Shell.Bootstrapper.Impl;
using BeyondNetCode.Shell.Bootstrapper.Interface;
using Serilog;

namespace Ums.Presentation.Bootstrapping;

public static class UmsApiServiceBootstrappers
{
    public static IServiceCollection AddUmsApiServiceBootstrappers(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        new CompositeBootstrapper()
            .Add(new UmsCoreApplicationBootstrapper(services, configuration, environment))
            .Add(new UmsApiPlatformBootstrapper(services, configuration))
            .Add(new UmsApiRateLimitingBootstrapper(services, configuration))
            .Add(new UmsApiDocumentationBootstrapper(services, configuration))
            .Add(new ConfigurationBootstrapper(services))
            .Run();

        return services;
    }
}

internal sealed class UmsCoreApplicationBootstrapper : IBootstrapper<IServiceCollection>
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _environment;

    public UmsCoreApplicationBootstrapper(
        IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        Result = services;
        _configuration = configuration;
        _environment = environment;
    }

    public IServiceCollection? Result { get; private set; }

    public void Run()
    {
        ArgumentNullException.ThrowIfNull(Result);

        Result.AddApplication();
        Result.AddInfrastructure(_configuration, _environment);
        Result.AddScoped<ILocalizationService, LocalizationService>();
        Result.AddUmsGraphQl(_environment);
        Result.AddMemoryCache(); // required by IdempotencyMiddleware (FIX-06)

        // HARDENING-02: JWT Bearer authentication. Disabled in dev (DevAuthMiddleware handles it).
        // Production: set Authentication:Enabled=true and configure Authority + Audience.
        Result.AddUmsAuthentication(_configuration);

        // JWT Token Service for session management
        Result.AddSingleton<IJwtTokenService, JwtTokenService>();
    }
}

internal sealed class UmsApiPlatformBootstrapper : IBootstrapper<IServiceCollection>
{
    private readonly IConfiguration _configuration;

    public UmsApiPlatformBootstrapper(IServiceCollection services, IConfiguration configuration)
    {
        Result = services;
        _configuration = configuration;
    }

    public IServiceCollection? Result { get; private set; }

    public void Run()
    {
        ArgumentNullException.ThrowIfNull(Result);

        Result.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        });

        Result.AddProblemDetails();
        Result.AddEndpointsApiExplorer();

        // REC-02: Real health checks — liveness + readiness + outbox backlog
        Result.AddInfrastructureHealthChecks(_configuration);

        // REC-06: OpenTelemetry — distributed tracing + metrics
        Result.AddUmsObservability(_configuration);
    }
}

internal sealed class UmsApiRateLimitingBootstrapper : IBootstrapper<IServiceCollection>
{
    private readonly IConfiguration _configuration;

    public UmsApiRateLimitingBootstrapper(IServiceCollection services, IConfiguration configuration)
    {
        Result = services;
        _configuration = configuration;
    }

    public IServiceCollection? Result { get; private set; }

    public void Run()
    {
        ArgumentNullException.ThrowIfNull(Result);

        var rateLimit = _configuration.GetSection("ApiSettings:RateLimiting");
        var permitLimit = rateLimit.GetValue<int>("PermitLimit", 100);
        var windowMinutes = rateLimit.GetValue<int>("WindowMinutes", 1);

        Result.AddRateLimiter(options =>
        {
            static string ResolvePartitionKey(HttpContext ctx, string prefix = "")
            {
                var tenantId = ctx.User.FindFirst("tenant_id")?.Value
                            ?? ctx.User.FindFirst("org_id")?.Value;

                var sub = ctx.User.FindFirst("sub")?.Value
                       ?? ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(sub))
                    return $"{prefix}tenant:{tenantId}:user:{sub}";

                if (!string.IsNullOrEmpty(sub))
                    return $"{prefix}user:{sub}";

                var apiKey = ctx.Request.Headers["X-Api-Key"].FirstOrDefault();
                if (!string.IsNullOrEmpty(apiKey))
                    return $"{prefix}apikey:{apiKey}";

                var ip = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return $"{prefix}ip:{ip}";
            }

            options.AddPolicy("graphql", context =>
            {
                var key = ResolvePartitionKey(context, "gql:");
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: key,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = Math.Max(1, permitLimit / 2),
                        Window = TimeSpan.FromMinutes(windowMinutes),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    });
            });

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
            {
                var key = ResolvePartitionKey(context);
                return RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: key,
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = permitLimit,
                        Window = TimeSpan.FromMinutes(windowMinutes),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0,
                    });
            });

            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.OnRejected = async (context, token) =>
            {
                var problemDetails = new ProblemDetails
                {
                    Title = "Too Many Requests",
                    Detail = "Rate limit exceeded. Please try again later.",
                    Status = StatusCodes.Status429TooManyRequests,
                    Type = "https://httpstatuses.io/429",
                    Extensions =
                    {
                        ["retryAfter"] = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry)
                            ? retry.ToString()
                            : "60",
                    },
                };

                context.HttpContext.Response.ContentType = "application/problem+json";
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, token);
            };
        });
    }
}

public static class UmsApiApplicationBuilderExtensions
{
    public static async Task<WebApplication> InitializeUmsPlatformAsync(this WebApplication app)
    {
        var persistenceOptions = app.Services.GetRequiredService<IOptions<PersistenceOptions>>().Value;

        if (persistenceOptions.Provider == PersistenceProvider.SqlServer && persistenceOptions.InitializePlatformStoreOnStartup)
        {
            using var scope = app.Services.CreateScope();
            var platformDbContext = scope.ServiceProvider.GetRequiredService<UmsPlatformDbContext>();
            await SqlServerSchemaBootstrapper.InitializeAsync(platformDbContext);
        }
        else if (persistenceOptions.Provider == PersistenceProvider.Sqlite)
        {
            using var scope = app.Services.CreateScope();
            var platformDbContext = scope.ServiceProvider.GetRequiredService<UmsPlatformDbContext>();
            await SqliteSchemaBootstrapper.InitializeAsync(platformDbContext);
        }

        if (app.Environment.IsDevelopment() && persistenceOptions.SeedDevData)
        {
            await app.SeedDevelopmentDataAsync();
        }

        return app;
    }

    public static WebApplication UseUmsApiPipeline(this WebApplication app)
    {
        app.UseCorrelationId();
        app.UseSessionTracking();
        app.UseSerilogRequestLogging(opts =>
        {
            opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                var requestContext = httpContext.RequestServices.GetRequiredService<IRequestContext>();
                diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
                diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
                diagnosticContext.Set("SessionTrackingId", requestContext.SessionTrackingId ?? string.Empty);
                diagnosticContext.Set("TraceId", requestContext.TraceId ?? Activity.Current?.TraceId.ToString() ?? string.Empty);
                diagnosticContext.Set("SpanId", requestContext.SpanId ?? Activity.Current?.SpanId.ToString() ?? string.Empty);
                diagnosticContext.Set("ErrorId", UserFacingErrorContext.GetErrorId(httpContext) ?? string.Empty);
            };
            opts.GetLevel = (ctx, _, _) =>
                ctx.Request.Path.StartsWithSegments("/health")
                    ? Serilog.Events.LogEventLevel.Verbose
                    : Serilog.Events.LogEventLevel.Information;
        });

        app.UseCulture();
        app.UseGlobalExceptionHandler();
        app.UseRateLimiter();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "UMS Tenant API v1");
                c.RoutePrefix = "swagger";
            });
        }

        app.UseCors("DefaultPolicy");
        app.UseSecurityHeaders();
        if (app.Environment.IsDevelopment())
        {
            app.UseDevAuth();
        }
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseIdempotency(); // FIX-07: Must run after auth so cached responses are never served to unauthenticated callers.
        app.UseTenantContext();
        app.UseTokenRevocation();
        if (!app.Environment.IsDevelopment())
            app.UseHttpsRedirection();

        return app;
    }

    private static async Task SeedDevelopmentDataAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        await CoreDevDataSeeder.SeedAllAsync(scope.ServiceProvider);
    }

    public static WebApplication MapUmsApiSurface(this WebApplication app)
    {
        var versionedGroup = app.CreateVersionedApiGroup();

        app.MapGraphQlSurface(versionedGroup);
        app.MapHealthSurface();
        app.MapAuthEndpoints();
        app.MapClientAuthEndpoints();

        if (app.Environment.IsDevelopment())
        {
            app.MapPactProviderStateEndpoints();
        }

        versionedGroup.MapUmsCommandEndpoints();
        versionedGroup.MapUmsQueryEndpoints();

        return app;
    }

    internal static RouteGroupBuilder CreateVersionedApiGroup(this WebApplication app)
    {
        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1))
            .ReportApiVersions()
            .Build();

        return app.MapGroup("api/v{apiVersion:apiVersion}")
            .WithApiVersionSet(versionSet);
    }

    internal static IEndpointRouteBuilder MapGraphQlSurface(
        this IEndpointRouteBuilder endpoints,
        RouteGroupBuilder versionedGroup)
    {
        endpoints.MapGraphQL("/graphql")
            .WithTags("GraphQL - Queries")
            .RequireRateLimiting("graphql");

        versionedGroup.MapGraphQL("/graphql")
            .WithTags("GraphQL - Queries")
            .RequireRateLimiting("graphql");

        return endpoints;
    }

    internal static IEndpointRouteBuilder MapHealthSurface(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/health/live", () => Results.Ok(new
        {
            Status = "Alive",
            Service = "UMS API",
            Timestamp = DateTimeOffset.UtcNow,
        }))
        .WithName("GetLiveness")
        .WithTags("Platform")
        .ExcludeFromDescription();

        endpoints.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            Predicate = hc => hc.Tags.Contains("ready"),
            ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync,
        })
        .WithName("GetReadiness")
        .WithTags("Platform");

        endpoints.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
        {
            ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync,
        })
        .WithName("GetHealth")
        .WithTags("Platform");

        return endpoints;
    }

    internal static RouteGroupBuilder MapUmsCommandEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup
            .MapIdentityCommandEndpoints()
            .MapAuthorizationCommandEndpoints()
            .MapAuditCommandEndpoints()
            .MapApprovalsCommandEndpoints()
            .MapConfigurationCommandEndpoints()
            .MapIgaCommandEndpoints();

        return versionedGroup;
    }

    internal static RouteGroupBuilder MapUmsQueryEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup
            .MapIdentityQueryEndpoints()
            .MapAuthorizationQueryEndpoints()
            .MapAuditQueryEndpoints()
            .MapApprovalsQueryEndpoints()
            .MapConfigurationQueryEndpoints()
            .MapIgaQueryEndpoints();

        return versionedGroup;
    }

    internal static RouteGroupBuilder MapIdentityCommandEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup.MapTenantEndpoints();
        versionedGroup.MapTenantBranchEndpoints();
        versionedGroup.MapTenantIdentityProviderEndpoints();
        versionedGroup.MapTenantBrandingEndpoints();
        versionedGroup.MapUserAccountEndpoints();
        versionedGroup.MapDelegationEndpoints();
        versionedGroup.MapOnboardingInboxEndpoints();

        return versionedGroup;
    }

    internal static RouteGroupBuilder MapIdentityQueryEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup.MapTenantQueryEndpoints();
        versionedGroup.MapBranchQueryEndpoints();
        versionedGroup.MapBrandingQueryEndpoints();
        versionedGroup.MapIdentityProviderQueryEndpoints();
        versionedGroup.MapUserAccountQueryEndpoints();
        versionedGroup.MapDelegationQueryEndpoints();

        return versionedGroup;
    }

    internal static RouteGroupBuilder MapAuthorizationCommandEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup.MapProfileEndpoints();
        versionedGroup.MapSystemSuiteEndpoints();
        versionedGroup.MapPermissionTemplateEndpoints();
        versionedGroup.MapRoleEndpoints();
        versionedGroup.MapTemplateAssignmentRuleEndpoints();

        return versionedGroup;
    }

    internal static RouteGroupBuilder MapAuthorizationQueryEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup.MapProfileQueryEndpoints();
        versionedGroup.MapSystemSuiteQueryEndpoints();
        versionedGroup.MapPermissionTemplateQueryEndpoints();

        return versionedGroup;
    }

    internal static RouteGroupBuilder MapAuditCommandEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup.MapAuditRecordEndpoints();
        return versionedGroup;
    }

    internal static RouteGroupBuilder MapAuditQueryEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup.MapAuditRecordQueryEndpoints();
        return versionedGroup;
    }

    internal static RouteGroupBuilder MapApprovalsCommandEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup.MapApprovalWorkflowEndpoints();
        versionedGroup.MapApprovalRequestEndpoints();
        versionedGroup.MapDocumentTypeEndpoints();
        versionedGroup.MapUserDocumentEndpoints();
        versionedGroup.MapAccessEnforcementPolicyEndpoints();
        versionedGroup.MapNotificationRuleEndpoints();

        return versionedGroup;
    }

    internal static RouteGroupBuilder MapApprovalsQueryEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup.MapApprovalWorkflowQueryEndpoints();
        versionedGroup.MapApprovalRequestQueryEndpoints();
        versionedGroup.MapDocumentTypeQueryEndpoints();
        versionedGroup.MapUserDocumentQueryEndpoints();
        versionedGroup.MapAccessEnforcementPolicyQueryEndpoints();
        versionedGroup.MapNotificationRuleQueryEndpoints();

        return versionedGroup;
    }

    internal static RouteGroupBuilder MapConfigurationCommandEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup.MapAppConfigurationEndpoints();
        versionedGroup.MapFeatureFlagEndpoints();
        versionedGroup.MapIdpConfigurationEndpoints();
        versionedGroup.MapParameterEndpoints();

        return versionedGroup;
    }

    internal static RouteGroupBuilder MapConfigurationQueryEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup.MapAppConfigurationQueryEndpoints();
        versionedGroup.MapFeatureFlagQueryEndpoints();
        versionedGroup.MapIdpConfigurationQueryEndpoints();
        versionedGroup.MapParameterCatalogQueryEndpoints();

        return versionedGroup;
    }

    internal static RouteGroupBuilder MapIgaCommandEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup.MapPromotionRequestEndpoints();
        versionedGroup.MapRoleMaturityStatusEndpoints();

        return versionedGroup;
    }

    internal static RouteGroupBuilder MapIgaQueryEndpoints(this RouteGroupBuilder versionedGroup)
    {
        versionedGroup.MapPromotionRequestQueryEndpoints();
        versionedGroup.MapRoleMaturityStatusQueryEndpoints();

        return versionedGroup;
    }
}

internal sealed class UmsApiDocumentationBootstrapper : IBootstrapper<IServiceCollection>
{
    private readonly IConfiguration _configuration;

    public UmsApiDocumentationBootstrapper(IServiceCollection services, IConfiguration configuration)
    {
        Result = services;
        _configuration = configuration;
    }

    public IServiceCollection? Result { get; private set; }

    public void Run()
    {
        ArgumentNullException.ThrowIfNull(Result);

        Result.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "UMS Tenant API",
                Version = "v1",
                Description = "User Management System — modular monolith API with REST commands and GraphQL queries, prepared for SQL Server platform persistence.",
            });

            options.AddSecurityDefinition("DevUserId", new OpenApiSecurityScheme
            {
                Name = "X-User-Id",
                Type = SecuritySchemeType.ApiKey,
                In = ParameterLocation.Header,
                Description = "Development-only header. Sets the authenticated user id used by command handlers.",
            });

            // HARDENING-02: JWT Bearer security definition for production Swagger UI
            options.AddSwaggerBearerAuth();

            options.OperationFilter<LanguageHeaderOperationFilter>();
        });

        Result.AddCors(options =>
        {
            options.AddPolicy("DefaultPolicy", policy =>
            {
                policy
                    .WithOrigins(_configuration.GetValue<string>("AllowedOrigins")?.Split(',') ?? Array.Empty<string>())
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithExposedHeaders(
                        ObservabilityHeaders.CorrelationId,
                        ObservabilityHeaders.SessionTrackingId,
                        "api-supported-versions",
                        "api-deprecated-versions");
            });
        });
    }
}
