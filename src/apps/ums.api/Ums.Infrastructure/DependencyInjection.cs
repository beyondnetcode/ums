namespace Ums.Infrastructure;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Identity;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Services;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();

        services.AddScoped<IUserContext, UserContext>();
        services.AddScoped<ITenantContext, TenantContext>();

        services.AddSingleton<InMemoryTenantRepository>();
        services.AddSingleton<ITenantRepository>(sp => sp.GetRequiredService<InMemoryTenantRepository>());

        return services;
    }
}
