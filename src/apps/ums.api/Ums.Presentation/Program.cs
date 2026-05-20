using System.Threading.RateLimiting;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.OpenApi.Models;
using Ums.Application;
using Ums.Globalization;
using Ums.Globalization.Access;
using Ums.Infrastructure;
using Ums.Infrastructure.Persistence;
using Ums.Presentation.Endpoints.Identity.Tenant;
using Ums.Presentation.Endpoints.Identity.Tenant.Queries;
using Ums.Presentation.Extensions;
using Ums.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

ConfigureSecrets(builder);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

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
        Description = "User Management System — Tenant REST API (in-memory persistence for development).",
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

// Seed in-memory repository with dev prototype data in Development mode
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var repository = scope.ServiceProvider.GetRequiredService<InMemoryTenantRepository>();
    DevDataSeeder.Seed(repository);
}

var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

var versionedGroup = app.MapGroup("api/v{apiVersion:apiVersion}")
    .WithApiVersionSet(versionSet);

app.UseCorrelationId();
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
app.UseCulture();
app.UseDevAuth();
app.UseHttpsRedirection();

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

versionedGroup.MapTenantQueryEndpoints();
versionedGroup.MapBranchQueryEndpoints();
versionedGroup.MapBrandingQueryEndpoints();
versionedGroup.MapIdentityProviderQueryEndpoints();

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
