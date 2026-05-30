namespace Ums.Presentation.Bootstrapping.Bootstrappers;

using BeyondNetCode.Shell.Bootstrapper.Interface;
using Microsoft.Extensions.DependencyInjection;
using Ums.Infrastructure.Configuration;

public sealed class ConfigurationBootstrapper : IBootstrapper<IServiceCollection>
{
    public ConfigurationBootstrapper(IServiceCollection services)
    {
        Result = services;
    }

    public IServiceCollection? Result { get; private set; }

    public void Run()
    {
        ArgumentNullException.ThrowIfNull(Result);
        Result!.AddConfigurationProvider();
    }
}