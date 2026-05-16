using Ums.Shell.Factory.Interfaces;
using Ums.Shell.Factory.Interfaces;

namespace Ums.Shell.Factory.Impl
{
    public class FactoryCreator : IFactoryCreator
    {
        private readonly IServiceLocator _serviceLocator;

        public FactoryCreator(IServiceLocator serviceLocator)
        {
            _serviceLocator = serviceLocator;
        }

        public T Create<T>(Type type) where T : class
        {
            return _serviceLocator.Resolve<T>(type.FullName);
        }
    }
}
