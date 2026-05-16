using Ums.Shell.Factory.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Ums.Shell.Factory.Installer.Impl
{
    public class ServiceLocator : IScopedServiceLocator
    {
        private readonly IServiceProvider _container;

        public ServiceLocator(IServiceProvider container)
        {
            _container = container;
        }

        public IDisposable BeginScope()
        {
            return _container.CreateScope();
        }

        public TSource Resolve<TSource>() where TSource : class
        {
            return _container.GetRequiredService<TSource>();
        }

        public TSource Resolve<TSource>(string key) where TSource : class
        {
            return Resolve(typeof(TSource), key) as TSource;
        }

        public object Resolve(Type service)
        {
            return _container.GetRequiredService(service);
        }

        public object Resolve(Type service, string key)
        {
            var services = _container.GetServices(service);

            foreach (var s in services)
            {
                if (s.GetType().FullName == key)
                {
                    return s;
                }
            }

            throw new InvalidOperationException();
        }

        public TSource[] ResolveAll<TSource>() where TSource : class
        {
            return _container.GetServices<TSource>()?.ToArray();
        }
    }
}
