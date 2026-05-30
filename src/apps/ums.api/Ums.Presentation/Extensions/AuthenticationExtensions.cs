namespace Ums.Presentation.Extensions;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// HARDENING-02: Configures JWT Bearer authentication for production.
///
/// Configuration (appsettings.json):
/// <code>
/// "Authentication": {
///   "Enabled": true,
///   "Authority": "https://your-idp.example.com",   // OIDC discovery endpoint base URL
///   "Audience": "ums-api",                          // Expected JWT audience claim
///   "RequireHttpsMetadata": true,
///   "ValidIssuers": ["https://your-idp.example.com"]
/// }
/// </code>
///
/// Claim conventions (populate IUserContext / ITenantContext from these):
///   sub       → user ID (standard OIDC)
///   name      → display name
///   tenant_id → UMS organization / tenant ID (custom claim)
///   email     → user email (optional, only if IdP provides it)
///
/// In development, set "Authentication:Enabled": false to fall back to the
/// DevAuthMiddleware which reads X-User-Id / X-User-Name headers.
/// NEVER enable DevAuthMiddleware in production.
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddUmsAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var enabled   = configuration.GetValue<bool>("Authentication:Enabled", false);
        var authority = configuration["Authentication:Authority"];
        var audience  = configuration["Authentication:Audience"] ?? "ums-api";
        var requireHttps = configuration.GetValue<bool>("Authentication:RequireHttpsMetadata", true);
        var validIssuers = configuration.GetSection("Authentication:ValidIssuers")
            .Get<string[]>() ?? [];

        if (!enabled || string.IsNullOrWhiteSpace(authority))
        {
            // Authentication is disabled (dev/test mode). DevAuthMiddleware handles identity.
            // Still register AddAuthentication so UseAuthentication() does not throw.
            services.AddAuthentication()
                .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
                {
                    options.Cookie.Name = "ums.session";
                    options.Cookie.HttpOnly = true;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                    options.ExpireTimeSpan = TimeSpan.FromHours(1);
                    options.SlidingExpiration = true;
                    options.LoginPath = "/api/v1/auth/login";
                    options.AccessDeniedPath = "/api/v1/auth/denied";
                });
            services.AddAuthorization();
            return services;
        }

        services
            .AddAuthentication(cfg =>
            {
                cfg.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                cfg.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.Cookie.Name = "ums.session";
                options.Cookie.HttpOnly = true;
                options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
                options.ExpireTimeSpan = TimeSpan.FromHours(1);
                options.SlidingExpiration = true;
            })
            .AddJwtBearer(options =>
            {
                options.Authority = authority;
                options.Audience  = audience;
                options.RequireHttpsMetadata = requireHttps;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer    = true,
                    ValidateAudience  = true,
                    ValidateLifetime  = true,
                    ValidIssuers      = validIssuers.Length > 0 ? validIssuers : [authority],
                    ValidAudience     = audience,
                    ClockSkew         = TimeSpan.FromSeconds(30),
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = ctx =>
                    {
                        // Structured log — token validation failures are security events.
                        var logger = ctx.HttpContext.RequestServices
                            .GetRequiredService<ILoggerFactory>()
                            .CreateLogger("UMS.Authentication");

                        logger.LogWarning(
                            "JWT authentication failed. Path={Path} Error={Error}",
                            ctx.Request.Path,
                            ctx.Exception.GetType().Name);

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = ctx =>
                    {
                        // Populate ITenantContext from the JWT claims.
                        // This runs after signature validation, so the claims are trusted.
                        var tenantContext = ctx.HttpContext.RequestServices
                            .GetService<Ums.Application.Common.Interfaces.ITenantContext>();

                        if (tenantContext is null) return Task.CompletedTask;

                        var tenantIdClaim = ctx.Principal?.FindFirst("tenant_id")?.Value
                                         ?? ctx.Principal?.FindFirst("org_id")?.Value;

                        var isInternalAdminClaim = ctx.Principal?.FindFirst("is_internal_admin")?.Value;

                        if (Guid.TryParse(tenantIdClaim, out var tenantId))
                        {
                            var isInternalAdmin = isInternalAdminClaim?.ToLower() == "true";
                            tenantContext.Initialize(tenantId, isInternalAdmin);
                        }

                        return Task.CompletedTask;
                    },
                };
            });

        services.AddAuthorization();
        return services;
    }

    /// <summary>
    /// Configures the Swagger UI to accept Bearer tokens.
    /// Only adds the security definition; does not enforce it on endpoints
    /// (authorization policy enforcement is separate).
    /// </summary>
    public static void AddSwaggerBearerAuth(this Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
    {
        options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
        {
            Name         = "Authorization",
            Type         = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
            Scheme       = "bearer",
            BearerFormat = "JWT",
            In           = Microsoft.OpenApi.Models.ParameterLocation.Header,
            Description  = "Enter a valid JWT access token. Example: 'eyJhbGci...'",
        });

        options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
        {
            {
                new Microsoft.OpenApi.Models.OpenApiSecurityScheme
                {
                    Reference = new Microsoft.OpenApi.Models.OpenApiReference
                    {
                        Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                        Id   = "Bearer",
                    },
                },
                Array.Empty<string>()
            },
        });
    }
}
