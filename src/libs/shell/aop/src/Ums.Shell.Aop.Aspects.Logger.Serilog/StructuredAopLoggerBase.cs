
namespace Ums.Shell.Aop.Aspects.Logger.Serilog
{
    public abstract class StructuredAopLoggerBase : ILogger
    {
        private readonly IExecutionContextAccessor _executionContextAccessor;

        protected StructuredAopLoggerBase(IExecutionContextAccessor executionContextAccessor)
        {
            _executionContextAccessor = executionContextAccessor;
        }

        protected ExecutionContextSnapshot ResolveExecutionContext(string requestId)
        {
            var current = _executionContextAccessor.Current ?? ExecutionContextSnapshot.Empty;
            var activity = Activity.Current;

            var correlationId = !string.IsNullOrWhiteSpace(current.CorrelationId)
                ? current.CorrelationId
                : activity?.GetBaggageItem(ObservabilityKeys.CorrelationId)
                    ?? requestId
                    ?? string.Empty;

            var sessionTrackingId = !string.IsNullOrWhiteSpace(current.SessionTrackingId)
                ? current.SessionTrackingId
                : activity?.GetBaggageItem(ObservabilityKeys.SessionTrackingId)
                    ?? string.Empty;

            var traceId = !string.IsNullOrWhiteSpace(current.TraceId)
                ? current.TraceId
                : activity?.TraceId.ToString()
                    ?? string.Empty;

            var spanId = !string.IsNullOrWhiteSpace(current.SpanId)
                ? current.SpanId
                : activity?.SpanId.ToString()
                    ?? string.Empty;

            return new ExecutionContextSnapshot(
                correlationId,
                sessionTrackingId,
                traceId,
                spanId);
        }

        protected static string InferBoundedContext(Type targetType)
        {
            var parts = targetType.Namespace?.Split('.') ?? Array.Empty<string>();
            return parts.Length >= 3 ? parts[2] : targetType.Name;
        }

        public abstract void OnExit(IJoinPoint joinPoint, Return @return, string requestId, long duration);

        public abstract void OnExit(IJoinPoint joinPoint, string requestId, long duration);

        public abstract void OnExit(IJoinPoint joinPoint, Return @return, string requestId);

        public abstract void OnExit(IJoinPoint joinPoint, string requestId);

        public abstract void OnEntry(IJoinPoint joinPoint, Argument[] arguments, string requestId);

        public abstract void OnException(IJoinPoint joinPoint, string requestId, Exception ex);
    }
}
