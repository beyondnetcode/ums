
namespace Ums.Shell.Factory.Interfaces
{
    public interface IScopedServiceLocator : IServiceLocator
    {
        IDisposable BeginScope();
    }
}
