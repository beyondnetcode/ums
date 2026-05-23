using System.Threading.RateLimiting;
using Asp.Versioning;
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
using Ums.Presentation.Extensions;
using Ums.Presentation.GraphQL;
using Ums.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

ConfigureSecrets(builder);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ILocalizationService, LocalizationService>();
builder.Services.AddUmsGraphQl();
builder.Services.AddMemoryCache(); // required by IdempotencyMiddleware (FIX-06)

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
    options.AddPolicy("graphql", context =>
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: $"graphql:{clientIp}",
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
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: clientIp,
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
app.UseDevAuth();
app.UseHttpsRedirection();

app.MapGraphQL("/graphql")
    .WithTags("GraphQL - Queries")
    .RequireRateLimiting("graphql");

versionedGroup.MapGraphQL("/graphql")
    .WithTags("GraphQL - Queries")
    .RequireRateLimiting("graphql");

app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    Service = "UMS API",
    Language = CultureContext.Current,
    Timestamp = DateTimeOffset.UtcNow,
}))
.WithName("GetHealth")
.WithTags("Platform");

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
