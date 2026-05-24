using Asp.Versioning;
using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using Ums.Domain.Identity;
using Ums.Infrastructure;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Options;
using Ums.Presentation.Bootstrapping;
using Ums.Presentation.Extensions;
using Serilog;

// REC-14: Bootstrap Serilog immediately so startup errors are structured.
// Host.UseSerilog() replaces the default Microsoft.Extensions.Logging providers.
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Replace built-in logging with Serilog; reads appsettings Serilog section.
builder.Host.UseSerilog((ctx, cfg) => cfg.ConfigureUmsSerilog(ctx));

ConfigureSecrets(builder);

builder.Services.AddUmsApiServiceBootstrappers(builder.Configuration, builder.Environment);

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

app.UseUmsApiPipeline();
app.MapUmsApiSurface();

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
