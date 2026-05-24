using Ums.Shell.Aop.Aspects;

namespace Ums.Shell.Aop.Tests
{
    public class AddAdvice : Advice
    {
        public override void OnEntry(IJoinPoint joinpoint, object[] context)
        {
            joinpoint.Arguments[0]=5;
        }

        public override void OnExit(IJoinPoint joinpoint, object[] context)
        {
            var value = (int)joinpoint.Return + 5;

            joinpoint.Return = value;
        }

        public override void OnSuccess(IJoinPoint joinpoint, object[] context)
        {
            var value = (int)joinpoint.Return + 5;

            joinpoint.Return = value;
        }
    }
}
