using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Ums.Infrastructure.Services;

/// <summary>
/// Infrastructure implementation of IUserContext using ASP.NET Core HttpContext.
/// </summary>
public class UserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? UserId => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    
    public string? UserName => _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name) 
                               ?? _httpContextAccessor.HttpContext?.User?.FindFirstValue("name") 
                               ?? "Anonymous";

    public string? TenantId => _httpContextAccessor.HttpContext?.User?.FindFirstValue("tenant_id");

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated ?? false;
}
