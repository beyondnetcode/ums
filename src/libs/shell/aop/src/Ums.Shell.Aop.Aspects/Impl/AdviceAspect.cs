using System;

namespace Ums.Shell.Aop.Aspects
{
    public class AdviceAspect : OnMethodBoundaryAspect<AdviceAspectAttribute>
    {
        private IFactory<IAdvice> _factory;

        private IAdvice _advice;

        public AdviceAspect(IFactory<IAdvice> factory)
        {
            HandleException = false;

            _factory = factory;
        }

        protected override void Init(IJoinPoint joinPoint)
        {
            if(CurrentAttribute.Type == null)
            {
                throw new ArgumentNullException(nameof(CurrentAttribute.Type), "The Type should not be null");
            }

            if (!typeof(IAdvice).IsAssignableFrom(CurrentAttribute.Type))
            {
                throw new ArgumentException("The Type used is not valid", nameof(CurrentAttribute.Type));
            }

            _advice = _factory.Create(CurrentAttribute.Type);

            HandleException = CurrentAttribute.HandleException;
        }

        protected override void OnSuccess(IJoinPoint joinPoint)
        {
            _advice.OnSuccess(joinPoint, CurrentAttribute.Context);
        }

        protected override void OnEntry(IJoinPoint joinPoint)
        {
            _advice.OnEntry(joinPoint, CurrentAttribute.Context);
        }

        protected override void OnExit(IJoinPoint joinPoint)
        {
            _advice.OnExit(joinPoint, CurrentAttribute.Context);
        }

        protected override void OnException(IJoinPoint joinPoint, Exception ex)
        {
            _advice.OnException(joinPoint, CurrentAttribute.Context, ex);
        }
    }
}
