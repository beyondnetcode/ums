namespace Ums.Presentation.Endpoints.Identity.Auth;

using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Infrastructure.Persistence.Repositories;
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
        group.MapPost("/logout", HandleLogoutAsync)
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
                supportReferenceId: null));
        }

        // Find tenant by code
        var tenantResult = await tenantRepository.GetByCodeAsync(request.TenantCode.ToUpperInvariant());
        if (tenantResult is null)
        {
            return Results.NotFound(new LoginErrorResponse(
                ErrorCodes.TenantNotFound,
                $"Tenant '{request.TenantCode}' was not found.",
                supportReferenceId: null));
        }

        var tenant = tenantResult;

        // Check if tenant is active
        if (tenant.Props.Status != Domain.Enums.TenantStatus.Active)
        {
            return Results.BadRequest(new LoginErrorResponse(
                ErrorCodes.TenantInactive,
                $"Tenant '{request.TenantCode}' is not active.",
                supportReferenceId: null));
        }

        // Find user by email (username = email in this system)
        var userResult = await userAccountRepository.GetByEmailAsync(
            Email.Create(request.Username),
            tenant.Props.Id.GetValue(),
            cancellationToken: default);

        if (userResult is null)
        {
            return Results.Unauthorized(new LoginErrorResponse(
                ErrorCodes.InvalidCredentials,
                "Invalid username or password.",
                supportReferenceId: null));
        }

        var user = userResult;

        // Check if user is active
        if (user.Props.Status != Domain.Enums.UserStatus.Active)
        {
            return Results.Unauthorized(new LoginErrorResponse(
                ErrorCodes.UserNotActive,
                "User account is not active. Please contact your administrator.",
                supportReferenceId: null));
        }

        // Verify password
        var activeCredential = user.Props.PasswordCredentials.FirstOrDefault(c => c.IsActive);
        if (activeCredential is null || !passwordHasher.Verify(request.Password, activeCredential.PasswordHash.Value))
        {
            // Record failed attempt
            user.RecordAuthenticationAttempt(false, "Invalid password", "127.0.0.1");
            await userAccountRepository.UpdateAsync(user, default);
            await userAccountRepository.UnitOfWork.SaveEntitiesAsync(default);

            return Results.Unauthorized(new LoginErrorResponse(
                ErrorCodes.InvalidCredentials,
                "Invalid username or password.",
                supportReferenceId: null));
        }

        // Record successful authentication
        user.RecordAuthenticationAttempt(true, "Login successful", "127.0.0.1");
        await userAccountRepository.UpdateAsync(user, default);
        await userAccountRepository.UnitOfWork.SaveEntitiesAsync(default);

        // Get profile and role
        var profiles = await profileRepository.GetByUserIdAsync(user.Props.Id.GetValue(), default);
        var profile = profiles.FirstOrDefault();
        var role = profile != null ? await roleRepository.GetByIdAsync(profile.Props.RoleId.GetValue(), default) : null;

        // Create claims for the authenticated user
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Props.Id.GetValue().ToString()),
            new(ClaimTypes.Name, user.Props.Email.Value),
            new("tenant_id", tenant.Props.Id.GetValue().ToString()),
            new("tenant_code", tenant.Props.Code.Value),
            new("username", user.Props.IdentityReference?.Value ?? user.Props.Email.Value),
        };

        if (role != null)
        {
            claims.Add(new Claim(ClaimTypes.Role, role.Props.Code.Value));
            claims.Add(new Claim("role_name", role.Props.Value.Value));
        }

        if (profile != null)
        {
            claims.Add(new Claim("profile_id", profile.Props.Id.GetValue().ToString()));
        }

        // Generate JWT token for API access
        var sessionTrackingId = Guid.NewGuid().ToString();
        var jwtToken = jwtTokenService.GenerateToken(new TokenGenerationRequest(
            UserId: user.Props.Id.GetValue().ToString(),
            Email: user.Props.Email.Value,
            Username: user.Props.IdentityReference?.Value ?? user.Props.Email.Value,
            TenantId: tenant.Props.Id.GetValue(),
            TenantCode: tenant.Props.Code.Value,
            Role: role?.Props.Code.Value,
            RoleName: role?.Props.Value.Value,
            ProfileId: profile?.Props.Id.GetValue().ToString(),
            Permissions: Array.Empty<string>(),
            Language: "en"
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
            sessionId: sessionId,
            sessionTrackingId: sessionTrackingId,
            userId: user.Props.Id.GetValue().ToString(),
            username: user.Props.IdentityReference?.Value ?? user.Props.Email.Value,
            email: user.Props.Email.Value,
            tenantId: tenant.Props.Id.GetValue().ToString(),
            tenantCode: tenant.Props.Code.Value,
            tenantName: tenant.Props.Name.Value,
            role: role?.Props.Code.Value,
            roleName: role?.Props.Value.Value,
            profileId: profile?.Props.Id.GetValue().ToString(),
            permissions: Array.Empty<string>(),
            language: "en",
            token: jwtToken,
            tokenType: "Bearer",
            expiresIn: 3600,
            refreshExpiresIn: request.RememberMe ? 604800 : 86400
        ));
    }

    private static async Task<IResult> HandleRefreshTokenAsync(
        IJwtTokenService jwtTokenService,
        HttpContext httpContext)
    {
        if (!httpContext.User.Identity?.IsAuthenticated ?? true)
        {
            return Results.Unauthorized(new LoginErrorResponse(
                ErrorCodes.SessionExpired,
                "Session expired or invalid",
                supportReferenceId: null));
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
            token: newToken,
            tokenType: "Bearer",
            expiresIn: 3600,
            refreshExpiresIn: 604800,
            sessionTrackingId: sessionTrackingId
        ));
    }

    private static async Task<IResult> HandleLogoutAsync(HttpContext httpContext)
    {
        await httpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.Ok(new { message = "Logged out successfully" });
    }

    private static IResult HandleGetSessionAsync(HttpContext httpContext)
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

        return Results.Ok(new LoginSuccessResponse(
            sessionId: Guid.NewGuid().ToString(),
            sessionTrackingId: sessionTrackingId,
            userId: userId ?? "",
            username: httpContext.User.FindFirstValue("username") ?? email ?? "",
            email: email ?? "",
            tenantId: tenantId ?? "",
            tenantCode: tenantCode ?? "",
            tenantName: tenantName ?? "",
            role: role,
            roleName: roleName,
            profileId: profileId,
            permissions: Array.Empty<string>(),
            language: "en",
            token: null,
            tokenType: null,
            expiresIn: null
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
    int? RefreshExpiresIn);

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
}