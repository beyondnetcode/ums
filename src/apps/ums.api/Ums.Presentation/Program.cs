using System.Threading.RateLimiting;
using Asp.Versioning;
using Serilog;
using Asp.Versioning.Builder;
using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Ums.Application;
using Ums.Domain.Identity;
using Ums.Globalization;
using Ums.Globalization.Access;
using Ums.Infrastructure;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Options;
using Ums.Presentation.Endpoints.Identity.Tenant;
using Ums.Presentation.Endpoints.Identity.Tenant.Queries;
using Ums.Presentation.Endpoints.Identity.UserAccount;
using Ums.Presentation.Endpoints.Identity.UserAccount.Queries;
using Ums.Presentation.Endpoints.Identity.UserManagementDelegation;
using Ums.Presentation.Endpoints.Identity.UserManagementDelegation.Queries;
using Ums.Presentation.Endpoints.Authorization.Profile;
using Ums.Presentation.Endpoints.Authorization.Profile.Queries;
using Ums.Presentation.Endpoints.Authorization.SystemSuite;
using Ums.Presentation.Endpoints.Authorization.SystemSuite.Queries;
using Ums.Presentation.Endpoints.Authorization.Template;
using Ums.Presentation.Endpoints.Authorization.Template.Queries;
using Ums.Presentation.Endpoints.Audit.AuditRecord;
using Ums.Presentation.Endpoints.Audit.AuditRecord.Queries;
using Ums.Presentation.Endpoints.Approvals.ApprovalWorkflow;
using Ums.Presentation.Endpoints.Approvals.ApprovalWorkflow.Queries;
using Ums.Presentation.Endpoints.Approvals.ApprovalRequest;
using Ums.Presentation.Endpoints.Approvals.ApprovalRequest.Queries;
using Ums.Presentation.Endpoints.Approvals.DocumentType;
using Ums.Presentation.Endpoints.Approvals.DocumentType.Queries;
using Ums.Presentation.Endpoints.Approvals.UserDocument;
using Ums.Presentation.Endpoints.Approvals.UserDocument.Queries;
using Ums.Presentation.Endpoints.Approvals.AccessEnforcementPolicy;
using Ums.Presentation.Endpoints.Approvals.AccessEnforcementPolicy.Queries;
using Ums.Presentation.Endpoints.Approvals.NotificationRule;
using Ums.Presentation.Endpoints.Approvals.NotificationRule.Queries;
using Ums.Presentation.Endpoints.Configuration.AppConfiguration;
using Ums.Presentation.Endpoints.Configuration.AppConfiguration.Queries;
using Ums.Presentation.Endpoints.Configuration.FeatureFlag;
using Ums.Presentation.Endpoints.Configuration.FeatureFlag.Queries;
using Ums.Presentation.Endpoints.Configuration.IdpConfiguration;
using Ums.Presentation.Endpoints.Configuration.IdpConfiguration.Queries;
using Ums.Presentation.Endpoints.IGA.PromotionRequest;
using Ums.Presentation.Endpoints.IGA.PromotionRequest.Queries;
using Ums.Presentation.Endpoints.IGA.RoleMaturityStatus;
using Ums.Presentation.Endpoints.IGA.RoleMaturityStatus.Queries;
using Ums.Infrastructure.HealthChecks;
using Ums.Presentation.Endpoints;
using Ums.Presentation.Extensions;
using Ums.Presentation.GraphQL;
using Ums.Presentation.Middleware;

// REC-14: Bootstrap Serilog immediately so startup errors are structured.
// Host.UseSerilog() replaces the default Microsoft.Extensions.Logging providers.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Replace built-in logging with Serilog; reads appsettings Serilog section.
builder.Host.UseSerilog((ctx, cfg) => cfg.ConfigureUmsSerilog(ctx));

ConfigureSecrets(builder);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddUmsGraphQl(builder.Environment);
builder.Services.AddMemoryCache(); // required by IdempotencyMiddleware (FIX-06)

// HARDENING-02: JWT Bearer authentication. Disabled in dev (DevAuthMiddleware handles it).
// Production: set Authentication:Enabled=true and configure Authority + Audience.
builder.Services.AddUmsAuthentication(builder.Configuration);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
});

var rateLimit = builder.Configuration.GetSection("ApiSettings:RateLimiting");
var permitLimit = rateLimit.GetValue<int>("PermitLimit", 100);
var windowMinutes = rateLimit.GetValue<int>("WindowMinutes", 1);

builder.Services.AddRateLimiter(options =>
{
    // HARDENING-05: Partition key order — tenant → user → API key → IP.
    // Tenant-level partitioning prevents one tenant from consuming the global budget
    // and starving others. Within a tenant, user-level limits apply.
    static string ResolvePartitionKey(HttpContext ctx, string prefix = "")
    {
        // 1. Tenant ID from JWT (custom claim 'tenant_id' or 'org_id')
        var tenantId = ctx.User.FindFirst("tenant_id")?.Value
                    ?? ctx.User.FindFirst("org_id")?.Value;

        // 2. JWT sub claim (authenticated user)
        var sub = ctx.User.FindFirst("sub")?.Value
               ?? ctx.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (!string.IsNullOrEmpty(tenantId) && !string.IsNullOrEmpty(sub))
            return $"{prefix}tenant:{tenantId}:user:{sub}";

        if (!string.IsNullOrEmpty(sub))
            return $"{prefix}user:{sub}";

        // 3. X-Api-Key header
        var apiKey = ctx.Request.Headers["X-Api-Key"].FirstOrDefault();
        if (!string.IsNullOrEmpty(apiKey))
            return $"{prefix}apikey:{apiKey}";

        // 4. Fallback: remote IP (shared NAT risk — documented)
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

builder.Services.AddProblemDetails();
builder.Services.AddEndpointsApiExplorer();

// REC-02: Real health checks — liveness + readiness + outbox backlog
builder.Services.AddInfrastructureHealthChecks(builder.Configuration);

// REC-06: OpenTelemetry — distributed tracing + metrics
builder.Services.AddUmsObservability(builder.Configuration);
builder.Services.AddSwaggerGen(options =>
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

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultPolicy", policy =>
    {
        policy
            .WithOrigins(builder.Configuration.GetValue<string>("AllowedOrigins")?.Split(',') ?? Array.Empty<string>())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials()
            .WithExposedHeaders("X-Correlation-Id", "api-supported-versions", "api-deprecated-versions");
    });
});

var app = builder.Build();

var persistenceOptions = app.Services.GetRequiredService<IOptions<PersistenceOptions>>().Value;

if (persistenceOptions.Provider == PersistenceProvider.SqlServer && persistenceOptions.InitializePlatformStoreOnStartup)
{
    using var scope = app.Services.CreateScope();
    var platformDbContext = scope.ServiceProvider.GetRequiredService<UmsPlatformDbContext>();
    await SqlServerSchemaBootstrapper.InitializeAsync(platformDbContext);
}

// Seed prototype aggregates only while the aggregate store remains in-memory.
if (app.Environment.IsDevelopment() && persistenceOptions.SeedDevData)
{
    using var scope = app.Services.CreateScope();
    var repository = scope.ServiceProvider.GetRequiredService<ITenantRepository>();

    if (repository is InMemoryTenantRepository inMemoryTenantRepository)
    {
        DevDataSeeder.Seed(inMemoryTenantRepository);

        var inMemoryUserAccountRepository = scope.ServiceProvider.GetService<InMemoryUserAccountRepository>();
        if (inMemoryUserAccountRepository is not null)
        {
            DevDataSeeder.SeedUserAccounts(inMemoryUserAccountRepository);
        }
    }
    else
    {
        await DevDataSeeder.SeedAsync(repository);

        var userAccountRepository = scope.ServiceProvider.GetService<IUserAccountRepository>();
        if (userAccountRepository is not null)
        {
            foreach (var tenantCode in new[] { "RANSA_PERU", "NEPTUNIA", "APM_CALLAO" })
            {
                var tenant = await repository.GetByCodeAsync(tenantCode);
                if (tenant is not null)
                {
                    await DevDataSeeder.SeedUserAccountsAsync(userAccountRepository, tenant.Props.Id.GetValue());
                }
            }
        }
    }
}

var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

var versionedGroup = app.MapGroup("api/v{apiVersion:apiVersion}")
    .WithApiVersionSet(versionSet);

app.UseCorrelationId();
// REC-14: Emit one structured log line per HTTP request (method, path, status, elapsed).
// Placed after CorrelationId so the CorrelationId log scope is already active.
app.UseSerilogRequestLogging(opts =>
{
    opts.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value ?? string.Empty);
        diagnosticContext.Set("CorrelationId", httpContext.TraceIdentifier);
    };
    // Exclude health checks to avoid log noise
    opts.GetLevel = (ctx, _, _) =>
        ctx.Request.Path.StartsWithSegments("/health")
            ? Serilog.Events.LogEventLevel.Verbose
            : Serilog.Events.LogEventLevel.Information;
});
app.UseGlobalExceptionHandler();
app.UseIdempotency(); // FIX-06: deduplicate POST/PUT/PATCH via Idempotency-Key header
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
app.UseCulture();
// HARDENING-02: UseAuthentication validates JWT Bearer tokens in production.
// UseDevAuth runs AFTER so it only sets a ClaimsPrincipal when no real auth is present.
app.UseAuthentication();
app.UseAuthorization();
app.UseDevAuth();
// HARDENING-03: Reject requests from deleted/blocked users whose tokens are still valid.
app.UseTokenRevocation();
app.UseHttpsRedirection();

app.MapGraphQL("/graphql")
    .WithTags("GraphQL - Queries")
    .RequireRateLimiting("graphql");

versionedGroup.MapGraphQL("/graphql")
    .WithTags("GraphQL - Queries")
    .RequireRateLimiting("graphql");

// REC-02: Liveness — always returns 200 if the process is alive (no DB check)
app.MapGet("/health/live", () => Results.Ok(new
{
    Status  = "Alive",
    Service = "UMS API",
    Timestamp = DateTimeOffset.UtcNow,
}))
.WithName("GetLiveness")
.WithTags("Platform")
.ExcludeFromDescription(); // keep Swagger clean

// REC-02: Readiness — checks SQL Server + outbox backlog
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = hc => hc.Tags.Contains("ready"),
    ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync,
})
.WithName("GetReadiness")
.WithTags("Platform");

// REC-02: Full health report (all checks)
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthCheckResponseWriter.WriteJsonAsync,
})
.WithName("GetHealth")
.WithTags("Platform");

// OPS-02: Pact provider state endpoint — Development only.
// PactNet verifier calls this to set up preconditions before each interaction.
if (app.Environment.IsDevelopment())
    app.MapPactProviderStateEndpoints();

versionedGroup.MapTenantEndpoints();
versionedGroup.MapTenantBranchEndpoints();
versionedGroup.MapTenantIdentityProviderEndpoints();
versionedGroup.MapTenantBrandingEndpoints();
versionedGroup.MapUserAccountEndpoints();
versionedGroup.MapDelegationEndpoints();

versionedGroup.MapProfileEndpoints();

versionedGroup.MapSystemSuiteEndpoints();

versionedGroup.MapPermissionTemplateEndpoints();

versionedGroup.MapAuditRecordEndpoints();

versionedGroup.MapApprovalWorkflowEndpoints();
versionedGroup.MapApprovalRequestEndpoints();
versionedGroup.MapDocumentTypeEndpoints();
versionedGroup.MapUserDocumentEndpoints();
versionedGroup.MapAccessEnforcementPolicyEndpoints();
versionedGroup.MapNotificationRuleEndpoints();
versionedGroup.MapAppConfigurationEndpoints();
versionedGroup.MapFeatureFlagEndpoints();
versionedGroup.MapIdpConfigurationEndpoints();
versionedGroup.MapPromotionRequestEndpoints();
versionedGroup.MapRoleMaturityStatusEndpoints();

versionedGroup.MapTenantQueryEndpoints();
versionedGroup.MapBranchQueryEndpoints();
versionedGroup.MapBrandingQueryEndpoints();
versionedGroup.MapIdentityProviderQueryEndpoints();
versionedGroup.MapUserAccountQueryEndpoints();
versionedGroup.MapDelegationQueryEndpoints();
versionedGroup.MapProfileQueryEndpoints();
versionedGroup.MapSystemSuiteQueryEndpoints();
versionedGroup.MapPermissionTemplateQueryEndpoints();
versionedGroup.MapAuditRecordQueryEndpoints();
versionedGroup.MapApprovalWorkflowQueryEndpoints();
versionedGroup.MapApprovalRequestQueryEndpoints();
versionedGroup.MapDocumentTypeQueryEndpoints();
versionedGroup.MapUserDocumentQueryEndpoints();
versionedGroup.MapAccessEnforcementPolicyQueryEndpoints();
versionedGroup.MapNotificationRuleQueryEndpoints();
versionedGroup.MapAppConfigurationQueryEndpoints();
versionedGroup.MapFeatureFlagQueryEndpoints();
versionedGroup.MapIdpConfigurationQueryEndpoints();
versionedGroup.MapPromotionRequestQueryEndpoints();
versionedGroup.MapRoleMaturityStatusQueryEndpoints();

app.Run();

static void ConfigureSecrets(WebApplicationBuilder builder)
{
    var secretSource = builder.Configuration["Secrets:Source"] ?? "AppSettings";

    switch (secretSource)
    {
        case "UserSecrets":
            if (builder.Environment.IsDevelopment())
            {
                builder.Configuration.AddUserSecrets<Program>(optional: true, reloadOnChange: true);
            }
            break;

        case "KeyVault":
            var vaultUrl = builder.Configuration["Secrets:KeyVault:Url"];
            if (!string.IsNullOrEmpty(vaultUrl))
            {
                var credential = builder.Configuration["Secrets:KeyVault:UseManagedIdentity"] == "true"
                    ? (TokenCredential)new ManagedIdentityCredential()
                    : new DefaultAzureCredential();

                builder.Configuration.AddAzureKeyVault(
                    new Uri(vaultUrl),
                    credential,
                    new KeyVaultSecretManager());
            }
            break;

        case "Environment":
            break;

        case "AppSettings":
        default:
            break;
    }
}

internal sealed class LanguageHeaderOperationFilter : Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter
{
    public void Apply(Microsoft.OpenApi.Models.OpenApiOperation operation, Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext context)
    {
        operation.Parameters ??= new List<Microsoft.OpenApi.Models.OpenApiParameter>();
        operation.Parameters.Add(new Microsoft.OpenApi.Models.OpenApiParameter
        {
            Name = "X-Language",
            In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Required = false,
            Description = "Language code (e.g. 'en', 'es'). Falls back to Accept-Language then 'en'.",
            Schema = new Microsoft.OpenApi.Models.OpenApiSchema { Type = "string", Default = new Microsoft.OpenApi.Any.OpenApiString("en") },
        });
    }
}

public partial class Program;
