using Ums.Shell.Factory.Impl;
using Ums.Shell.Factory.Installer.Impl;
using Ums.Shell.Factory.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Ums.Shell.Factory.Installer.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers the factory infrastructure into the DI container.
        ///
        /// <see cref="FactoryCreator"/> is wired with a <c>Func&lt;Type, object&gt;</c>
        /// that delegates to <see cref="IServiceProvider.GetRequiredService(Type)"/>.
        /// Every concrete implementation type that the factory may create must
        /// therefore be registered in the same DI container.
        /// </summary>
        public static IServiceCollection AddFactory(
            this IServiceCollection services,
            Action<IFactoryBuilder>? action = null)
        {
            // FactoryCreator receives a factory function backed by IServiceProvider.
            // This is the accepted pattern for generic object factories in .NET:
            // the factory is part of the composition root and is allowed to resolve
            // services by type.
            services.TryAddSingleton<IFactoryCreator>(sp =>
                new FactoryCreator(sp.GetRequiredService));

            services.TryAddSingleton<IFactory, Factory.Impl.Factory>();

            services.TryAddSingleton<IFactorySetupProvider, FactorySetupProvider>();

            action?.Invoke(new FactoryBuilder(services));

            return services;
        }
    }
}
