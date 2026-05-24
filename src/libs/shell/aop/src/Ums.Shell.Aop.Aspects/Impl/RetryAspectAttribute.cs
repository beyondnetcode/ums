using System;


namespace Ums.Shell.Aop.Aspects
{
    public class RetryAspectAttribute : AbstractAspectAttribute
    {
        public int MaxAttempts { get; set; }

        public Type ExceptionType { get; set; }
    }
}
