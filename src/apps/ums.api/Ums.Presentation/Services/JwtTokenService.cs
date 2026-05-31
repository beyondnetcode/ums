namespace Ums.Presentation.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Ums.Application.Configuration.Services;

public interface IJwtTokenService
{
    string GenerateToken(TokenGenerationRequest request);

    /// <summary>
    /// Generates a JWT token that embeds the full AuthorizationGraph as claims.
    /// Used by /client/authenticate — the token IS the auth graph for the client system.
    /// </summary>
    string GenerateGraphToken(Ums.Domain.Authorization.Graph.AuthorizationGraph graph);

    string GenerateRefreshToken();
    DateTime GetTokenExpiration(string token);
}

public record TokenGenerationRequest(
    string UserId,
    string Email,
    string Username,
    Guid TenantId,
    string TenantCode,
    string? Role,
    string? RoleName,
    string? ProfileId,
    string[] Permissions,
    string Language = "en",
    bool IsInternalAdmin = false);

public class JwtTokenService : IJwtTokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly IConfigurationProvider _configProvider;

    public JwtTokenService(IConfiguration configuration, IConfigurationProvider configProvider)
    {
        _secret = configuration["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret is not configured");
        _issuer = configuration["Jwt:Issuer"] ?? "ums-api";
        _audience = configuration["Jwt:Audience"] ?? "ums-web-app";
        _configProvider = configProvider;
    }

    public string GenerateToken(TokenGenerationRequest request)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, request.UserId),
            new(JwtRegisteredClaimNames.Email, request.Email),
            new(JwtRegisteredClaimNames.Name, request.Username),
            new("tenant_id", request.TenantId.ToString()),
            new("tenant_code", request.TenantCode),
            new("session_tracking_id", Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

        if (!string.IsNullOrEmpty(request.Role))
        {
            claims.Add(new Claim(ClaimTypes.Role, request.Role));
        }

        if (!string.IsNullOrEmpty(request.RoleName))
        {
            claims.Add(new Claim("role_name", request.RoleName));
        }

        if (!string.IsNullOrEmpty(request.ProfileId))
        {
            claims.Add(new Claim("profile_id", request.ProfileId));
        }

        foreach (var permission in request.Permissions)
        {
            claims.Add(new Claim("permission", permission));
        }

        claims.Add(new Claim("language", request.Language));
        claims.Add(new Claim("is_internal_admin", request.IsInternalAdmin ? "true" : "false"));

        var accessTokenDurationMs = _configProvider.GetValueAs<int>(
            "ACCESS_TOKEN_DURATION_MS",
            request.TenantId,
            3600000);

        var expires = DateTime.UtcNow.AddMilliseconds(accessTokenDurationMs);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateGraphToken(Ums.Domain.Authorization.Graph.AuthorizationGraph graph)
    {
        var securityKey  = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secret));
        var credentials  = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var ctx    = graph.Context;
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   ctx.User.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, ctx.User.Email),
            new(JwtRegisteredClaimNames.Name,  ctx.User.Username),
            new("tenant_id",                   ctx.Tenant.Id.ToString()),
            new("tenant_code",                 ctx.Tenant.Code),
            new("sys_suite",                   ctx.SystemSuite.Code),
            new(ClaimTypes.Role,               ctx.Role.Code),
            new("role_name",                   ctx.Role.Name),
            new("profile_id",                  ctx.Profile.Id.ToString()),
            new("auth_method",                 graph.Authentication.Method),
            new("graph_generated_at",          graph.GeneratedAt.ToString("O")),
            new("graph_valid_until",           graph.ValidUntil.ToString("O")),
            new("session_tracking_id",         Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
        };

        if (ctx.Branch is not null)
            claims.Add(new Claim("branch_id", ctx.Branch.Id.ToString()));

        if (graph.Authentication.Provider is not null)
            claims.Add(new Claim("idp_provider", graph.Authentication.Provider.Name));

        // Embed permissions as compact claims: "TargetCode:ActionCode:Effect"
        foreach (var module in graph.MenuAccess)
        foreach (var menu   in module.Menus)
        foreach (var sub    in menu.SubMenus)
        foreach (var opt    in sub.Options)
            claims.Add(new Claim("perm", $"{opt.Code}:{opt.ActionCode}:{opt.Effect}"));

        foreach (var res in graph.DomainPermissions)
        foreach (var act in res.Actions)
            claims.Add(new Claim("domain_perm", $"{res.ResourceCode}:{act.ActionCode}:{act.Effect}"));

        foreach (var scope in graph.Scopes)
            claims.Add(new Claim("scope", scope));

        foreach (var flag in graph.FeatureFlags.Where(f => f.IsEnabled))
            claims.Add(new Claim("feature", flag.FlagCode));

        var token = new JwtSecurityToken(
            issuer:            _issuer,
            audience:          _audience,
            claims:            claims,
            expires:           graph.ValidUntil,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public DateTime GetTokenExpiration(string token)
    {
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        return jwtToken.ValidTo;
    }
}