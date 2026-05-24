using System;


namespace Ums.Shell.Aop.Aspects
{
    public class RetryAspect : OnRetryAspect<RetryAspectAttribute>
    {
        public int Count { get; set; }
        protected override bool CanRetry(IJoinPoint joinPoint, Exception ex)
        {
            var attribute = GetAttribute(joinPoint);

            if (Count < attribute.MaxAttempts && (attribute.ExceptionType == null || attribute.ExceptionType == ex.GetType()))
            {
                Count++;

                return true;
            }
            return false;
        }
    }
}
