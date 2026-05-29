namespace Ums.Presentation.Middleware;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

public sealed class DevAuthMiddleware
{
    private const string DefaultUserId = "dev-user";
    private const string DefaultUserName = "Developer";
    private const string DefaultTenantId = "3fa85f64-5717-4562-b3fc-2c963f66afa6";
    private const string UserIdHeader = "X-User-Id";
    private const string UserNameHeader = "X-User-Name";
    private const string TenantIdHeader = "X-Tenant-Id";

    private readonly RequestDelegate _next;
    private readonly IHostEnvironment _environment;

    public DevAuthMiddleware(RequestDelegate next, IHostEnvironment environment)
    {
        _next = next;
        _environment = environment;
    }

    public Task InvokeAsync(HttpContext context)
    {
        if (!_environment.IsDevelopment())
        {
            return _next(context);
        }

        if (context.User?.Identity?.IsAuthenticated != true)
        {
            var userId = context.Request.Headers[UserIdHeader].FirstOrDefault();
            var userName = context.Request.Headers[UserNameHeader].FirstOrDefault();
            var tenantId = context.Request.Headers[TenantIdHeader].FirstOrDefault();

            userId ??= DefaultUserId;
            userName ??= DefaultUserName;
            tenantId ??= DefaultTenantId;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName),
                new Claim("tenant_id", tenantId),
            };
            var identity = new ClaimsIdentity(claims, "Dev");
            context.User = new ClaimsPrincipal(identity);
        }

        return _next(context);
    }
}

public static class DevAuthMiddlewareExtensions
{
    public static IApplicationBuilder UseDevAuth(this IApplicationBuilder app)
        => app.UseMiddleware<DevAuthMiddleware>();
}
