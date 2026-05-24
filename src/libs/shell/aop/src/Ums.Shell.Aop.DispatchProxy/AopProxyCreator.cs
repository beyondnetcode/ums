namespace Ums.Shell.Aop.DispatchProxy
{
    public static class AopProxyCreator
    {
        public static TService Create<TService, TImplementation>(TService target, IAspectExecutor executor)
            where TService : class
            where TImplementation : class, TService
        {
            object proxy = System.Reflection.DispatchProxy.Create<TService, AopProxy<TService, TImplementation>>();

            ((AopProxy<TService, TImplementation>)proxy).Init(target, executor);

            return (TService)proxy;
        }
    }
}
