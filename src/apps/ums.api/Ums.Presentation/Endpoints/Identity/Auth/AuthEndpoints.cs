namespace Ums.Presentation.Endpoints.Identity.Auth;

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
using Ums.Domain.Authorization;
using Ums.Domain.Identity;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Presentation.Services;
using ProfileAggregate = Ums.Domain.Authorization.Profile.Profile;
using TenantAggregate = Ums.Domain.Identity.Tenant.Tenant;
using UserAccountAggregate = Ums.Domain.Identity.UserAccount.UserAccount;

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
    }

    private static async Task<IResult> HandleLoginAsync(
        LoginRequest request,
        IPasswordHashingService passwordHasher,
        ITenantRepository tenantRepository,
        IUserAccountRepository userAccountRepository,
        IProfileRepository profileRepository,
        IRoleRepository roleRepository,
        IJwtTokenService jwtTokenService,
        HttpContext httpContext)
    {
        // Validate request
        if (string.IsNullOrWhiteSpace(request.TenantCode) ||
            string.IsNullOrWhiteSpace(request.Username) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Results.BadRequest(new LoginErrorResponse(
                ErrorCodes.ValidationError,
                "Tenant code, username and password are required.",
                SupportReferenceId: null));
        }

        // Find tenant by code
        var tenantResult = await tenantRepository.GetByCodeAsync(request.TenantCode.ToUpperInvariant());
        if (tenantResult is null)
        {
            return Results.NotFound(new LoginErrorResponse(
                ErrorCodes.TenantNotFound,
                $"Tenant '{request.TenantCode}' was not found.",
                SupportReferenceId: null));
        }

        var tenant = tenantResult;

        // Check if tenant is active
        if (tenant.Props.Status != Domain.Enums.TenantStatus.Active)
        {
            return Results.BadRequest(new LoginErrorResponse(
                ErrorCodes.TenantInactive,
                $"Tenant '{request.TenantCode}' is not active.",
                SupportReferenceId: null));
        }

        // Find user by email (username = email in this system)
        var userResult = await userAccountRepository.GetByEmailAsync(
            Email.Create(request.Username),
            default);

        if (userResult is null)
        {
            return Results.Json(new LoginErrorResponse(
                ErrorCodes.InvalidCredentials,
                "Invalid username or password.",
                SupportReferenceId: null), statusCode: 401);
        }

        var user = userResult;

        // Check if user is active
        if (user.Props.Status != Domain.Enums.UserStatus.Active)
        {
            return Results.Json(new LoginErrorResponse(
                ErrorCodes.UserNotActive,
                "User account is not active. Please contact your administrator.",
                SupportReferenceId: null), statusCode: 401);
        }

        // Verify password
        var activeCredential = user.PasswordCredentials.FirstOrDefault(c => c.Props.IsActive);
        if (activeCredential is null || !passwordHasher.Verify(request.Password, activeCredential.Props.PasswordHash.GetValue()))
        {
            // Record failed attempt
            user.RecordAuthenticationAttempt(false, "Invalid password", "127.0.0.1", ActorId.Create("auth:system"));
            await userAccountRepository.UpdateAsync(user, default);
            await userAccountRepository.UnitOfWork.SaveEntitiesAsync(default);

            return Results.Json(new LoginErrorResponse(
                ErrorCodes.InvalidCredentials,
                "Invalid username or password.",
                SupportReferenceId: null), statusCode: 401);
        }

        // Record successful authentication
        user.RecordAuthenticationAttempt(true, "Login successful", "127.0.0.1", ActorId.Create("auth:system"));
        await userAccountRepository.UpdateAsync(user, default);
        await userAccountRepository.UnitOfWork.SaveEntitiesAsync(default);

        // Get profile and role
        var profiles = await profileRepository.GetByUserIdAsync(user.Props.Id.GetValue(), default);
        var profile = profiles.FirstOrDefault();
        var role = profile is not null ? await roleRepository.GetByIdAsync(profile.Props.RoleId.GetValue(), default) : null;

        // Create claims for the authenticated user
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Props.Id.GetValue().ToString()),
            new(ClaimTypes.Name, user.Props.Email.GetValue()),
            new("tenant_id", tenant.Props.Id.GetValue().ToString()),
            new("tenant_code", tenant.Props.Code.GetValue()),
            new("username", user.Props.IdentityReference?.GetValue() ?? user.Props.Email.GetValue()),
        };

        if (role is not null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Props.Code.GetValue()));
            claims.Add(new Claim("role_name", role.Props.Value.GetValue()));
        }

        if (profile is not null)
        {
            claims.Add(new Claim("profile_id", profile.Props.Id.GetValue().ToString()));
        }

        // Generate JWT token for API access
        var sessionTrackingId = Guid.NewGuid().ToString();
        var isInternalAdmin = tenant.Props.Code.GetValue() == "INTERNAL_ADMIN";
        var jwtToken = jwtTokenService.GenerateToken(new TokenGenerationRequest(
            UserId: user.Props.Id.GetValue().ToString(),
            Email: user.Props.Email.GetValue(),
            Username: user.Props.IdentityReference?.GetValue() ?? user.Props.Email.GetValue(),
            TenantId: tenant.Props.Id.GetValue(),
            TenantCode: tenant.Props.Code.GetValue(),
            Role: role?.Props.Code.GetValue(),
            RoleName: role?.Props.Value.GetValue(),
            ProfileId: profile?.Props.Id.GetValue().ToString(),
            Permissions: Array.Empty<string>(),
            Language: "en",
            IsInternalAdmin: isInternalAdmin
        ));

        // Sign in using cookie authentication (for web frontend)
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

        await httpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            claimsPrincipal,
            new AuthenticationProperties
            {
                IsPersistent = request.RememberMe,
                ExpiresUtc = request.RememberMe
                    ? DateTimeOffset.UtcNow.AddDays(7)
                    : DateTimeOffset.UtcNow.AddHours(1)
            });

        // Build session ID for response
        var sessionId = Guid.NewGuid().ToString();

        return Results.Ok(new LoginSuccessResponse(
            SessionId: sessionId,
            SessionTrackingId: sessionTrackingId,
            UserId: user.Props.Id.GetValue().ToString(),
            Username: user.Props.IdentityReference?.GetValue() ?? user.Props.Email.GetValue(),
            Email: user.Props.Email.GetValue(),
            TenantId: tenant.Props.Id.GetValue().ToString(),
            TenantCode: tenant.Props.Code.GetValue(),
            TenantName: tenant.Props.Name.GetValue(),
            Role: role?.Props.Code.GetValue(),
            RoleName: role?.Props.Value.GetValue(),
            ProfileId: profile?.Props.Id.GetValue().ToString(),
            Permissions: Array.Empty<string>(),
            Language: "en",
            Token: jwtToken,
            TokenType: "Bearer",
            ExpiresIn: 3600,
            RefreshExpiresIn: request.RememberMe ? 604800 : 86400,
            IsInternalAdmin: isInternalAdmin
        ));
    }

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
    bool IsInternalAdmin = false);

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