namespace Ums.Presentation.Endpoints.Identity.Auth;

using Ums.Application.Identity.Auth.Commands;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Ums.Application.Common.Interfaces;
using Ums.Application.Configuration.Services;
using MsConfigProvider = Microsoft.Extensions.Configuration.IConfigurationProvider;
using Ums.Domain.Authorization;
using Ums.Domain.Authorization.Graph;
using Ums.Domain.Identity;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Presentation.Services;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/v1/auth")
            .WithTags("Authentication");

        // Login endpoint
        group.MapPost("/login", HandleLoginAsync)
            .WithName("Login")
            .WithSummary("Authenticate user with tenant, username and password");

        // Logout endpoint
        group.MapPost("/logout", (Delegate)HandleLogoutAsync)
            .WithName("Logout")
            .WithSummary("Terminate the current session");

        // Refresh token endpoint
        group.MapPost("/refresh", HandleRefreshTokenAsync)
            .WithName("RefreshToken")
            .WithSummary("Refresh access token using session cookie")
            .RequireAuthorization();

        // Get current session info
        group.MapGet("/session", HandleGetSessionAsync)
            .WithName("GetSession")
            .WithSummary("Get current authenticated session information")
            .RequireAuthorization();

        // Switch tenant context (internal admins only)
        group.MapPost("/switch-tenant", HandleSwitchTenantAsync)
            .WithName("SwitchTenant")
            .WithSummary("Switch current tenant context (internal admins only)");
            // No RequireAuthorization() - we validate the JWT token directly in the handler

        // Forgot password — public, no auth required
        group.MapPost("/forgot-password", async (
            ForgotPasswordCommand command,
            IMediator mediator,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToOk(context);
        })
        .WithName("ForgotPassword")
        .WithSummary("Request a password reset. Always returns 200 to prevent user enumeration.")
        .AllowAnonymous()
        .Produces<ForgotPasswordResponse>(StatusCodes.Status200OK);

        group.MapPost("/user-signup", async (
            SignupUserCommand command,
            IMediator mediator,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => "/api/v1/auth/user-signup", context);
        })
        .WithName("UserSignup")
        .WithSummary("Request access to an existing tenant.")
        .AllowAnonymous()
        .Produces<UserSignupResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapPost("/tenant-signup", async (
            Ums.Application.Identity.Tenant.SignupRequests.Commands.RequestTenantSignupCommand command,
            IMediator mediator,
            HttpContext context,
            CancellationToken ct) =>
        {
            var result = await mediator.Send(command, ct);
            return result.ToCreated(r => "/api/v1/auth/tenant-signup", context);
        })
        .WithName("TenantSignup")
        .WithSummary("Request onboarding for a new tenant.")
        .AllowAnonymous()
        .Produces<Ums.Application.Identity.Tenant.SignupRequests.DTOs.RequestTenantSignupResponse>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status422UnprocessableEntity);
    }

    /// <summary>
    /// Delegates authentication to AuthenticateUserCommandHandler (full graph engine).
    /// Sets a session cookie for the web frontend and returns the full AuthorizationGraph
    /// in the response body so the UI can build its navigation and permission model.
    /// </summary>
    private static async Task<IResult> HandleLoginAsync(
        LoginRequest  request,
        IMediator     mediator,
        IJwtTokenService jwtTokenService,
        Ums.Application.Configuration.Services.IConfigurationProvider configProvider,
        HttpContext   httpContext,
        CancellationToken cancellationToken)
    {
        var supportReferenceId = httpContext.TraceIdentifier;

        if (string.IsNullOrWhiteSpace(request.TenantCode) ||
            string.IsNullOrWhiteSpace(request.Username)   ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new LoginErrorResponse(
                ErrorCodes.ValidationError,
                "Tenant code, username and password are required.",
                SupportReferenceId: supportReferenceId));
        }

        var clientIp = httpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";

        var command = new AuthenticateUserCommand(
            TenantCode: request.TenantCode.Trim().ToUpperInvariant(),
            Username:   request.Username.Trim(),
            Password:   request.Password,
            ClientIp:   clientIp,
            AccessScope: Ums.Domain.Identity.Auth.AuthAccessScope.PortalManagement,
            RememberMe: request.RememberMe);

        var result = await mediator.Send(command, cancellationToken);

        if (result.IsFailure)
        {
            return MapAuthError(result.Error, supportReferenceId);
        }

        var authResult = result.Value;
        var graph      = authResult.Graph;
        var ctx        = graph.Context;

        // Generate graph JWT
        var jwtToken = jwtTokenService.GenerateGraphToken(graph);

        // Set session cookie for web frontend
        var isInternalAdmin = ctx.Tenant.IsManagementOwner;
        var cookieClaims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, ctx.User.Id.ToString()),
            new(ClaimTypes.Name,           ctx.User.Email),
            new("tenant_id",               ctx.Tenant.Id.ToString()),
            new("tenant_code",             ctx.Tenant.Code),
            new("username",                ctx.User.Username),
            new(ClaimTypes.Role,           ctx.Role.Code),
            new("role_name",               ctx.Role.Name),
            new("profile_id",              ctx.Profile.Id.ToString()),
            new("sys_suite",               ctx.SystemSuite.Code),
            new("auth_method",             graph.Authentication.Method),
            new("is_internal_admin",       isInternalAdmin ? "true" : "false"),
        };
        if (ctx.Branch is not null)
            cookieClaims.Add(new Claim("branch_id", ctx.Branch.Id.ToString()));

        var identity  = new ClaimsIdentity(cookieClaims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                ExpiresUtc   = request.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(1),
            });

        // Resolve tenant-specific parameters from the in-memory cache (loaded at startup).
        // These are included in the session so the frontend never needs a separate round-trip.
        var tenantId = ctx.Tenant.Id;
        var cfg = configProvider.ForTenant(tenantId);
        var sessionParams = new SessionParameters(
            SessionTimeoutMinutes:  cfg.SessionTimeoutMinutes,
            AccessTokenDurationMs:  cfg.AccessTokenDurationMs,
            RefreshTokenDurationMs: cfg.RefreshTokenDurationMs,
            MaxLoginAttempts:       cfg.MaxLoginAttempts,
            MinPasswordLength:      cfg.MinPasswordLength,
            MfaRequiredForAdmin:    cfg.MfaRequiredForAdmin,
            CustomBrandingEnabled:  cfg.CustomBrandingEnabled,
            DefaultLanguage:        cfg.DefaultLanguage,
            DefaultTimezone:        cfg.DefaultTimezone);

        // Build enriched permissions array from Allow options for backward compat
        var permissions = graph.MenuAccess
            .SelectMany(m => m.Menus)
            .SelectMany(m => m.SubMenus)
            .SelectMany(s => s.Options)
            .Where(o => o.Effect == AccessEffect.Allow)
            .Select(o => $"{o.Code}:{o.ActionCode}")
            .ToArray();

        return Results.Ok(new LoginSuccessResponse(
            SessionId:        Guid.NewGuid().ToString(),
            SessionTrackingId: Guid.NewGuid().ToString(),
            UserId:           ctx.User.Id.ToString(),
            Username:         ctx.User.Username,
            Email:            ctx.User.Email,
            TenantId:         tenantId.ToString(),
            TenantCode:       ctx.Tenant.Code,
            TenantName:       ctx.Tenant.Name,
            Role:             ctx.Role.Code,
            RoleName:         ctx.Role.Name,
            ProfileId:        ctx.Profile.Id.ToString(),
            Permissions:      permissions,
            Language:         sessionParams.DefaultLanguage,
            Token:            jwtToken,
            TokenType:        "Bearer",
            ExpiresIn:        authResult.ExpiresIn,
            RefreshExpiresIn: request.RememberMe ? 604800 : 86400,
            IsInternalAdmin:  isInternalAdmin,
            SessionParameters: sessionParams,
            AuthorizationGraph: graph,
            GraphFormat:      authResult.GraphFormat));
    }

    private static IResult MapAuthError(string error, string supportReferenceId) => error switch
    {
        var e when e.StartsWith("AUTH_002") => Results.NotFound(new LoginErrorResponse(
            ErrorCodes.TenantNotFound, "No pudimos iniciar sesión. Verifique el código del tenant.", supportReferenceId)),
        var e when e.StartsWith("AUTH_003") => Results.BadRequest(new LoginErrorResponse(
            ErrorCodes.TenantInactive, "El tenant no está activo. Contacte al administrador.", supportReferenceId)),
        var e when e.StartsWith("AUTH_004") => Results.NotFound(new LoginErrorResponse(
            ErrorCodes.UserNotFound, "No pudimos iniciar sesión. Verifique sus credenciales.", supportReferenceId)),
        var e when e.StartsWith("AUTH_005") => Results.Json(new LoginErrorResponse(
            ErrorCodes.UserNotActive, "Su cuenta no está activa. Contacte al administrador.", supportReferenceId), statusCode: 401),
        var e when e.StartsWith("AUTH_006") => Results.Json(new LoginErrorResponse(
            ErrorCodes.InvalidCredentials, "No pudimos iniciar sesión. Verifique sus credenciales.", supportReferenceId), statusCode: 401),
        _ => Results.Json(new LoginErrorResponse("AUTH_000", "No pudimos iniciar sesión. Intente nuevamente.", supportReferenceId), statusCode: 401),
    };

    private static async Task<IResult> HandleRefreshTokenAsync(
        IJwtTokenService jwtTokenService,
        HttpContext httpContext)
    {
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            return Results.Json(new LoginErrorResponse(
                ErrorCodes.SessionExpired,
                "Session expired or invalid",
                SupportReferenceId: null), statusCode: 401);
        }

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = httpContext.User.FindFirstValue(ClaimTypes.Name);
        var username = httpContext.User.FindFirstValue("username") ?? email ?? "";
        var tenantIdStr = httpContext.User.FindFirstValue("tenant_id");
        var tenantCode = httpContext.User.FindFirstValue("tenant_code") ?? "";
        var role = httpContext.User.FindFirstValue(ClaimTypes.Role);
        var roleName = httpContext.User.FindFirstValue("role_name");
        var profileId = httpContext.User.FindFirstValue("profile_id");

        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantIdStr) || !Guid.TryParse(tenantIdStr, out var tenantId))
        {
            return Results.Unauthorized();
        }

        var newToken = jwtTokenService.GenerateToken(new TokenGenerationRequest(
            UserId: userId,
            Email: email ?? "",
            Username: username,
            TenantId: tenantId,
            TenantCode: tenantCode,
            Role: role,
            RoleName: roleName,
            ProfileId: profileId,
            Permissions: Array.Empty<string>(),
            Language: "en"
        ));

        var sessionTrackingId = httpContext.User.FindFirstValue("session_tracking_id") ?? Guid.NewGuid().ToString();

        return Results.Ok(new RefreshTokenResponse(
            Token: newToken,
            TokenType: "Bearer",
            ExpiresIn: 3600,
            RefreshExpiresIn: 604800,
            SessionTrackingId: sessionTrackingId
        ));
    }

    private static async Task<IResult> HandleLogoutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Ok(new { message = "Logged out successfully" });
    }

    private static IResult HandleGetSessionAsync(HttpContext httpContext, ITenantContext tenantContext)
    {
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            return Results.Unauthorized();
        }

        var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = httpContext.User.FindFirstValue(ClaimTypes.Name);
        var tenantId = httpContext.User.FindFirstValue("tenant_id");
        var tenantCode = httpContext.User.FindFirstValue("tenant_code");
        var tenantName = httpContext.User.FindFirstValue("tenant_name");
        var role = httpContext.User.FindFirstValue(ClaimTypes.Role);
        var roleName = httpContext.User.FindFirstValue("role_name");
        var profileId = httpContext.User.FindFirstValue("profile_id");
        var sessionTrackingId = httpContext.User.FindFirstValue("session_tracking_id") ?? Guid.NewGuid().ToString();
        var isInternalAdmin = httpContext.User.FindFirstValue("is_internal_admin")?.ToLower() == "true";

        return Results.Ok(new LoginSuccessResponse(
            SessionId: Guid.NewGuid().ToString(),
            SessionTrackingId: sessionTrackingId,
            UserId: userId ?? "",
            Username: httpContext.User.FindFirstValue("username") ?? email ?? "",
            Email: email ?? "",
            TenantId: tenantId ?? "",
            TenantCode: tenantCode ?? "",
            TenantName: tenantName ?? "",
            Role: role,
            RoleName: roleName,
            ProfileId: profileId,
            Permissions: Array.Empty<string>(),
            Language: "en",
            Token: null,
            TokenType: null,
            ExpiresIn: null,
            RefreshExpiresIn: null,
            IsInternalAdmin: isInternalAdmin
        ));
    }

    private static async Task<IResult> HandleSwitchTenantAsync(
        SwitchTenantRequest request,
        ITenantContext tenantContext,
        ITenantRepository tenantRepository,
        IConfiguration configuration,
        HttpContext httpContext)
    {
        // Extract JWT token from Authorization header and validate it directly
        // This works regardless of whether ASP.NET Core JWT authentication is configured
        var authHeader = httpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return Results.Unauthorized();
        }

        var tokenString = authHeader.Substring("Bearer ".Length).Trim();
        var secret = configuration["Jwt:Secret"] ?? throw new InvalidOperationException("Jwt:Secret is not configured");

        var tokenHandler = new JwtSecurityTokenHandler();
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5)
        };

        try
        {
            tokenHandler.ValidateToken(tokenString, validationParameters, out var validatedToken);
        }
        catch (SecurityTokenException)
        {
            return Results.Unauthorized();
        }

        var jwtToken = tokenHandler.ReadJwtToken(tokenString);
        var isInternalAdmin = jwtToken.Claims.FirstOrDefault(c => c.Type == "is_internal_admin")?.Value?.ToLower() == "true";
        
        // Initialize TenantContext from JWT claims (this is normally done by authentication middleware)
        var tenantIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
        if (Guid.TryParse(tenantIdClaim, out var userTenantId))
        {
            tenantContext.Initialize(userTenantId, isInternalAdmin);
        }
        else
        {
            return Results.Unauthorized();
        }

        if (!isInternalAdmin)
        {
            return Results.Json(new LoginErrorResponse(
                ErrorCodes.AccessDenied,
                "Only internal administrators can switch tenant context.",
                SupportReferenceId: null), statusCode: 403);
        }

        if (!Guid.TryParse(request.TenantId, out var tenantId))
        {
            return Results.BadRequest(new LoginErrorResponse(
                ErrorCodes.ValidationError,
                "Invalid tenant ID format.",
                SupportReferenceId: null));
        }

        var tenantResult = await tenantRepository.GetByIdAsync(tenantId, default);
        if (tenantResult is null)
        {
            return Results.NotFound(new LoginErrorResponse(
                ErrorCodes.TenantNotFound,
                $"Tenant with ID '{request.TenantId}' was not found.",
                SupportReferenceId: null));
        }

        var tenant = tenantResult;

        if (tenant.Props.Status != Domain.Enums.TenantStatus.Active)
        {
            return Results.BadRequest(new LoginErrorResponse(
                ErrorCodes.TenantInactive,
                $"Tenant '{tenant.Props.Code.GetValue()}' is not active.",
                SupportReferenceId: null));
        }

        try
        {
            if (request.EnableCrossTenantAccess)
            {
                tenantContext.EnableCrossTenantAccess();
            }
            else
            {
                tenantContext.SetOrganizationId(tenantId);
            }
        }
        catch (InvalidOperationException ex)
        {
            return Results.Json(new LoginErrorResponse(
                ErrorCodes.AccessDenied,
                ex.Message,
                SupportReferenceId: null), statusCode: 403);
        }

        var sessionTrackingId = jwtToken.Claims.FirstOrDefault(c => c.Type == "session_tracking_id")?.Value ?? Guid.NewGuid().ToString();

        return Results.Ok(new TenantSwitchResponse(
            PreviousTenantId: tenantContext.OriginalTenantId?.ToString() ?? "",
            CurrentTenantId: tenantId.ToString(),
            CurrentTenantCode: tenant.Props.Code.GetValue(),
            CurrentTenantName: tenant.Props.Name.GetValue(),
            CrossTenantAccessEnabled: request.EnableCrossTenantAccess,
            SessionTrackingId: sessionTrackingId
        ));
    }
}

// ─── Request/Response Models ───────────────────────────────────────────────────

public record LoginRequest(
    string TenantCode,
    string Username,
    string Password,
    bool RememberMe = false);

/// <summary>
/// Effective configuration values for the authenticated tenant, resolved at login time
/// from the in-memory parameter cache (loaded at startup by ConfigurationLoaderHostedService).
/// The frontend uses these values for the entire session without additional API calls.
/// TD-003: When Redis is adopted, the underlying cache swaps transparently.
/// </summary>
public record SessionParameters(
    int    SessionTimeoutMinutes,
    int    AccessTokenDurationMs,
    int    RefreshTokenDurationMs,
    int    MaxLoginAttempts,
    int    MinPasswordLength,
    bool   MfaRequiredForAdmin,
    bool   CustomBrandingEnabled,
    string DefaultLanguage,
    string DefaultTimezone);

public record LoginSuccessResponse(
    string SessionId,
    string SessionTrackingId,
    string UserId,
    string Username,
    string Email,
    string TenantId,
    string TenantCode,
    string TenantName,
    string? Role,
    string? RoleName,
    string? ProfileId,
    string[] Permissions,
    string Language,
    string? Token,
    string? TokenType,
    int? ExpiresIn,
    int? RefreshExpiresIn,
    bool IsInternalAdmin = false,
    SessionParameters? SessionParameters = null,
    // ── Graph fields (null when called from refresh/session endpoints) ──────
    Ums.Domain.Authorization.Graph.AuthorizationGraph? AuthorizationGraph = null,
    string? GraphFormat = null);

public record LoginErrorResponse(
    string Code,
    string Message,
    string? SupportReferenceId);

public record RefreshTokenResponse(
    string Token,
    string TokenType,
    int ExpiresIn,
    int RefreshExpiresIn,
    string SessionTrackingId);

public record SwitchTenantRequest(
    string TenantId,
    bool EnableCrossTenantAccess = false);

public record TenantSwitchResponse(
    string PreviousTenantId,
    string CurrentTenantId,
    string CurrentTenantCode,
    string CurrentTenantName,
    bool CrossTenantAccessEnabled,
    string SessionTrackingId);

// ─── Error Codes ───────────────────────────────────────────────────────────────

public static class ErrorCodes
{
    public const string ValidationError = "AUTH_001";
    public const string TenantNotFound = "AUTH_002";
    public const string TenantInactive = "AUTH_003";
    public const string UserNotFound = "AUTH_004";
    public const string UserNotActive = "AUTH_005";
    public const string InvalidCredentials = "AUTH_006";
    public const string SessionExpired = "AUTH_007";
    public const string AccessDenied = "AUTH_008";
    public const string AdminLacksPermission = "AUTH_009";
    public const string TargetUserOutsideScope = "AUTH_010";
}

public static class UserAccountErrorCodes
{
    public const string FederatedUserPasswordReset = "USER_015";
}

public static class ConfigurationErrorCodes
{
    public const string ValidityPeriodExceedsMaximum = "CONFIG_003";
}
