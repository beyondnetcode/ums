namespace Ums.Presentation.Middleware;

using System.Security.Claims;
using Microsoft.AspNetCore.Http;

/// <summary>
/// HARDENING-03: Rejects requests from users whose tokens have been revoked.
///
/// Runs after UseAuthentication so <c>HttpContext.User</c> is already populated.
/// Returns HTTP 401 with a Problem Details body when the authenticated user is in
/// the revocation list (deleted or blocked). This ensures that deleting or blocking
/// a user takes effect immediately, without waiting for JWT expiry.
///
/// Registration order (Program.cs):
///   UseAuthentication → UseAuthorization → UseTokenRevocation → ...
/// </summary>
public sealed class TokenRevocationMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITokenRevocationStore revocationStore)
    {
        // Only check authenticated requests — anonymous routes pass through.
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                      ?? context.User.FindFirst("sub")?.Value;

            if (!string.IsNullOrEmpty(userId) && await revocationStore.IsRevokedAsync(userId, context.RequestAborted))
            {
                context.Response.StatusCode  = StatusCodes.Status401Unauthorized;
                context.Response.ContentType = "application/problem+json";
                await context.Response.WriteAsJsonAsync(new
                {
                    type     = "https://httpstatuses.io/401",
                    title    = "Unauthorized",
                    status   = 401,
                    detail   = "Your session has been revoked. Please sign in again.",
                    traceId  = context.TraceIdentifier,
                });
                return;
            }
        }

        await next(context);
    }
}

public static class TokenRevocationMiddlewareExtensions
{
    public static IApplicationBuilder UseTokenRevocation(this IApplicationBuilder app)
        => app.UseMiddleware<TokenRevocationMiddleware>();
}
