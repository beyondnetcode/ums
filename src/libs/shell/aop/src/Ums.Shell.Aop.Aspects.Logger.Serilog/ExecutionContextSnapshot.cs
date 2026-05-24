namespace Ums.Shell.Aop.Aspects.Logger.Serilog
{
    public sealed record ExecutionContextSnapshot(
        string CorrelationId,
        string SessionTrackingId,
        string TraceId,
        string SpanId)
    {
        public static readonly ExecutionContextSnapshot Empty = new(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);
    }
}
