namespace Ums.Shell.Aop.Aspects.Logger.Serilog
{
    public static class ObservabilityHeaders
    {
        public const string CorrelationId = "X-Correlation-Id";

        public const string SessionTrackingId = "X-Session-Tracking-Id";
    }

    public static class ObservabilityKeys
    {
        public const string CorrelationId = "correlation.id";

        public const string SessionTrackingId = "session.tracking_id";
    }
}
