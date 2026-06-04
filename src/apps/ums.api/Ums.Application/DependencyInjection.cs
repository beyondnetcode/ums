namespace Ums.Application;

using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Ums.Application.Common.Behaviors;
using Ums.Application.Common.Interfaces;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(configuration => configuration.RegisterServicesFromAssembly(assembly));
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddScoped<ITenantScopePolicy, TenantScopePolicy>();
        services.AddScoped<IUserManagementDelegationAccessService, UserManagementDelegationAccessService>();

        return services;
    }
}
