namespace Ums.Infrastructure.Configuration;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ums.Application.Configuration.Services;

public sealed class ConfigurationLoaderHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;

    public ConfigurationLoaderHostedService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<IConfigurationProvider>();
        await provider.LoadAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

public static class ConfigurationLoaderExtensions
{
    public static IServiceCollection AddConfigurationProvider(this IServiceCollection services)
    {
        services.AddSingleton<IConfigurationProvider, ConfigurationProvider>();
        services.AddHostedService<ConfigurationLoaderHostedService>();
        return services;
    }
}