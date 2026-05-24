using System;

namespace Ums.Shell.Aop.Aspects
{
    public interface ILogger
    {
        void OnExit(IJoinPoint joinPoint, Return @return, string requestId, long duration);

        void OnExit(IJoinPoint joinPoint, string requestId, long duration);

        void OnExit(IJoinPoint joinPoint, Return @return, string requestId);

        void OnExit(IJoinPoint joinPoint, string requestId);

        void OnEntry(IJoinPoint joinPoint, Argument[] arguments, string requestId);

        void OnException(IJoinPoint joinPoint, string requestId, Exception ex);
    }
}
