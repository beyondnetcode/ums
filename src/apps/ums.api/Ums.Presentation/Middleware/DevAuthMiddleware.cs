namespace Ums.Presentation.Middleware;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;

public sealed class DevAuthMiddleware
{
    private const string DefaultUserId = "dev-user";
    private const string DefaultUserName = "Developer";
    private const string UserIdHeader = "X-User-Id";
    private const string UserNameHeader = "X-User-Name";

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

            userId ??= DefaultUserId;
            userName ??= DefaultUserName;

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
