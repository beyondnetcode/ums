namespace Ums.Shell.Aop.Aspects
{
    public interface IEvaluator
    {
        TOutput Evaluate<TOutput>(IJoinPoint joinPoint, string expression, TOutput errorValue = default(TOutput));
    }
}
