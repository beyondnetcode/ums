using Ums.Shell.Factory.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Ums.Shell.Factory.Installer.Impl
{
    public class FactoryBuilder : IFactoryBuilder
    {
        private readonly IServiceCollection _serviceCollection;

        public FactoryBuilder(IServiceCollection serviceCollection)
        {
            _serviceCollection = serviceCollection;
        }

        public IFactoryBuilder AddSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            _serviceCollection.AddSingleton<TService, TImplementation>();

            return this;
        }

        public IFactoryBuilder AddTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            _serviceCollection.AddTransient<TService, TImplementation>();

            return this;
        }

        public IFactoryBuilder AddSource<TImplementation>() where TImplementation : class, IFactorySetupSource
        {
            _serviceCollection.AddSingleton<IFactorySetupSource, TImplementation>();

            return this;
        }
    }
}
