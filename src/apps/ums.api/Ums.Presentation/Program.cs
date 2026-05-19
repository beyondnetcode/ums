using Microsoft.OpenApi.Models;
using Ums.Application;
using Ums.Globalization;
using Ums.Globalization.Access;
using Ums.Infrastructure;
using Ums.Presentation.Endpoints;
using Ums.Presentation.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure();
builder.Services.AddScoped<ILocalizationService, LocalizationService>();

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

var app = builder.Build();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "UMS Tenant API v1");
        c.RoutePrefix = "swagger";
    });
}

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

app.MapTenantEndpoints();
app.MapTenantBranchEndpoints();
app.MapTenantIdentityProviderEndpoints();
app.MapTenantBrandingEndpoints();

app.Run();

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
