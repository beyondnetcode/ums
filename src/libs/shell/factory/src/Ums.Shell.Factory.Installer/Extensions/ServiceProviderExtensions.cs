using Ums.Shell.Factory.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace Ums.Shell.Factory.Installer.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static IFactory GetFactory(this IServiceProvider provider)
        {
            return provider.GetService<IFactory>();
        }
    }
}
