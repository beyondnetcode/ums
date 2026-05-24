namespace Ums.Shell.Aop
{
    public interface IAspectExecutor
    {
        void Execute(IJoinPoint joinPoint);
    }
}
