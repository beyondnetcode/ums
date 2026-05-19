namespace Ums.Presentation.Middleware;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

public sealed class DevAuthMiddleware
{
    private const string DefaultUserId = "dev-user";
    private const string DefaultUserName = "Developer";
    private const string UserIdHeader = "X-User-Id";
    private const string UserNameHeader = "X-User-Name";

    private readonly RequestDelegate _next;

    public DevAuthMiddleware(RequestDelegate next) => _next = next;

    public Task InvokeAsync(HttpContext context)
    {
        if (context.User?.Identity?.IsAuthenticated != true)
        {
            var userId = context.Request.Headers[UserIdHeader].FirstOrDefault() ?? DefaultUserId;
            var userName = context.Request.Headers[UserNameHeader].FirstOrDefault() ?? DefaultUserName;

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
                new Claim(ClaimTypes.Name, userName),
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
