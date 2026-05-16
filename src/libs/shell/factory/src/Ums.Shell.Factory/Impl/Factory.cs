using Ums.Shell.Factory.Interfaces;
using Ums.Shell.Factory.Model;

namespace Ums.Shell.Factory.Impl
{
    public class Factory : IFactory
    {
        public IFactorySetupProvider ConfigurationProvider { get; }

        public IFactoryInterceptor Interceptor { get; set; }

        public IFactoryCreator Creator { get; }

        public Factory(IFactorySetupProvider factoryConfigurationProvider, IFactoryCreator factoryCreator)
        {
            ArgumentNullException.ThrowIfNull(factoryConfigurationProvider, nameof(factoryConfigurationProvider));
            ArgumentNullException.ThrowIfNull(factoryCreator, nameof(factoryCreator));
            
            ConfigurationProvider = factoryConfigurationProvider;

            Creator = factoryCreator;
            
            Interceptor = AbstractFactoryInterceptor.Instance;
        }

        public TService[] Create<TTarget, TService>(TTarget instance) where TService : class
        {
            return Create<TTarget, TService>(instance, string.Empty);
        }

        public SetupItem[] ConfigurationFor<TTarget, TService>(TTarget target) where TService : class
        {
            return ConfigurationFor<TTarget, TService>(target, string.Empty);
        }

        public TService[] Create<TTarget, TService>(TTarget target, string name) where TService : class
        {
            var list = Array.Empty<TService>();

            try
            {
                Interceptor.OnEntry(target, name);

                list = ConfigurationFor<TTarget, TService> (target, name).Select(item=> Creator.Create<TService>(item.ImplementationType)).ToArray();

                Interceptor.OnSuccess(target, name, list);
                
            }
            catch (Exception ex)
            {
                Interceptor.OnError(target, name, list, ex);

                throw;
            }
            finally
            {
                Interceptor.OnExit(target, name, list);
            }
            return list;
        }

        public SetupItem[] ConfigurationFor<TTarget, TService>(TTarget target, string name) where TService : class
        {
            return ConfigurationProvider.Provide<TTarget, TService>(target, name);
        }
    }
}
