
namespace Ums.Shell.Factory.Interfaces
{
    public interface IFactoryInterceptor
    {
        void OnEntry<TTarget>(TTarget target, string name);

        void OnSuccess<TTarget, TService>(TTarget target, string name, IList<TService> services);

        void OnError<TTarget, TService>(TTarget target, string name, IList<TService> services, Exception exception);

        void OnExit<TTarget, TService>(TTarget target, string name, IList<TService> services);
    }
}
