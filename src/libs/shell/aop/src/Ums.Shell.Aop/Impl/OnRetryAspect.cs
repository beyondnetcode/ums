using System;

namespace Ums.Shell.Aop
{
    public abstract class OnRetryAspect<T> : OnMethodBoundaryAspect<T> where T : AbstractAspectAttribute
    {
        public override void Apply(IJoinPoint joinPoint)
        {
            Init(joinPoint);

            Retry(joinPoint);
        }

        private void Retry(IJoinPoint joinPoint)
        {
            OnEntry(joinPoint);

            bool success = false;

            while (true)
            {
                try
                {
                    if (Continue(joinPoint))
                    {
                        if (GetNext() == null)
                        {
                            joinPoint.Proceed();
                        }
                        else
                        {
                            GetNext().Apply(joinPoint);
                        }
                        OnSuccess(joinPoint);
                        success = true;
                        break;
                    }
                }
                catch (Exception ex)
                {
                    if (!CanRetry(joinPoint, ex))
                    {
                        if (HandleException)
                        {
                            OnException(joinPoint, ex);
                            success = true;
                            break;
                        }
                        else
                        {
                            throw;
                        }
                    }
                }
            }

            if (success)
            {
                OnExit(joinPoint);
            }
        }

        protected virtual bool CanRetry(IJoinPoint joinPoint, Exception ex)
        {
            return false;
        }
    }
}
