using Asp.Versioning;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Ums.Application;
using Ums.Globalization;
using Ums.Globalization.Access;
using Ums.Infrastructure;
using Ums.Infrastructure.HealthChecks;
using Ums.Presentation.GraphQL;
using Ums.Shell.Bootstrapper.Impl;
using Ums.Shell.Bootstrapper.Interface;

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
            .Add(new UmsApiDocumentationBootstrapper(services, configuration))
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
        Result.AddInfrastructure(_configuration);
        Result.AddScoped<ILocalizationService, LocalizationService>();
        Result.AddUmsGraphQl(_environment);
        Result.AddMemoryCache(); // required by IdempotencyMiddleware (FIX-06)

        // HARDENING-02: JWT Bearer authentication. Disabled in dev (DevAuthMiddleware handles it).
        // Production: set Authentication:Enabled=true and configure Authority + Audience.
        Result.AddUmsAuthentication(_configuration);
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
