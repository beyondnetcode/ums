namespace Ums.Presentation.Services;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

public interface IJwtTokenService
{
    string GenerateToken(TokenGenerationRequest request);
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
    string Language = "en");

public class JwtTokenService : IJwtTokenService
{
    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public JwtTokenService(IConfiguration configuration)
    {
        _secret = configuration["Jwt:Secret"] ?? throw new ArgumentNullException("Jwt:Secret is not configured");
        _issuer = configuration["Jwt:Issuer"] ?? "ums-api";
        _audience = configuration["Jwt:Audience"] ?? "ums-web-app";
        _expirationMinutes = int.TryParse(configuration["Jwt:ExpirationMinutes"], out var min) ? min : 60;
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

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_expirationMinutes),
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