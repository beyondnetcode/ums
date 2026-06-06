using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Ums.Presentation.IntegrationTest.Infrastructure;

public sealed class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock)
        : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var tenantId = "3fa85f64-5717-4562-b3fc-2c963f66afa6"; // Default system tenant
        if (Request.Headers.TryGetValue("X-Tenant-Id", out var tenantHeader))
        {
            tenantId = tenantHeader.ToString();
        }

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000111"),
            new Claim("tenant_id", tenantId),
            new Claim("org_id", tenantId),
            new Claim("is_internal_admin", "true"),
            new Claim(ClaimTypes.Role, "SuperAdmin")
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "Test");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
