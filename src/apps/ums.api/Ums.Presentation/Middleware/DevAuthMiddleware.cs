namespace Ums.Presentation.Middleware;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Ums.Application.Common.Interfaces;

public sealed class DevAuthMiddleware
{
    private const string DefaultUserId = "dev-user";
    private const string DefaultUserName = "Developer";
    private const string DefaultTenantId = "11111111-1111-1111-1111-111111111111";
    private const string InternalAdminTenantId = "11111111-1111-1111-1111-111111111111";
    private const string UserIdHeader = "X-User-Id";
    private const string UserNameHeader = "X-User-Name";
    private const string TenantIdHeader = "X-Tenant-Id";
    private const string IsInternalAdminHeader = "X-Is-Internal-Admin";

    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;

    public DevAuthMiddleware(RequestDelegate next, IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (!_environment.IsDevelopment())
        {
            await _next(context);
            return;
        }

        // Public authentication endpoints must execute with the real anonymous context.
        // Default dev claims would force the internal admin tenant and break tenant-scoped
        // login, signup, forgot-password, and session bootstrap flows.
        if (context.Request.Path.StartsWithSegments("/api/v1/auth"))
        {
            await _next(context);
            return;
        }

        if (context.User?.Identity?.IsAuthenticated != true)
        {
            var userId = context.Request.Headers[UserIdHeader].FirstOrDefault();
            var userName = context.Request.Headers[UserNameHeader].FirstOrDefault();
            var tenantId = context.Request.Headers[TenantIdHeader].FirstOrDefault();
            var isInternalAdminHeader = context.Request.Headers[IsInternalAdminHeader].FirstOrDefault();

            userId ??= DefaultUserId;
            userName ??= DefaultUserName;
            tenantId ??= DefaultTenantId;

            var isInternalAdmin = isInternalAdminHeader?.ToLower() == "true"
                || tenantId == InternalAdminTenantId;

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, userId),
                new(ClaimTypes.Name, userName),
                new("tenant_id", tenantId),
                new("is_internal_admin", isInternalAdmin.ToString().ToLower()),
            };

            var identity = new ClaimsIdentity(claims, "Dev");
            context.User = new ClaimsPrincipal(identity);

            if (Guid.TryParse(tenantId, out var parsedTenantId))
            {
                tenantContext.Initialize(parsedTenantId, isInternalAdmin);
            }
        }

        await _next(context);
    }
}

public static class DevAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseDevAuth(this IApplicationBuilder app)
        => app.UseMiddleware<DevAuthMiddleware>();
}
