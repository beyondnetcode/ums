using Ums.Shell.Factory.Interfaces;

namespace Ums.Shell.Factory.Impl
{
    public abstract class AbstractFactoryInterceptor : IFactoryInterceptor
    {
        public static IFactoryInterceptor Instance = new NullFactoryInterceptor();

        public virtual void OnEntry<TTarget>(TTarget target, string name)
        {
        
        }

        public virtual void OnSuccess<TTarget, TService>(TTarget target, string name, IList<TService> services)
        {
        
        }

        public virtual void OnError<TTarget, TService>(TTarget target, string name, IList<TService> services, Exception exception)
        {
        
        }

        public virtual void OnExit<TTarget, TService>(TTarget target, string name, IList<TService> services)
        {
        
        }
    }
}