
namespace Ums.Infrastructure.Services;

/// <summary>
/// Scoped execution context for the current request. It is populated by middleware and can
/// be reused by loggers, handlers, background dispatch handoff, and observability adapters.
/// </summary>
public sealed class RequestContextAccessor : IRequestContext, IExecutionContextAccessor
{
    private ExecutionContextSnapshot _current = ExecutionContextSnapshot.Empty;

    public string? SessionTrackingId => string.IsNullOrWhiteSpace(_current.SessionTrackingId) ? null : _current.SessionTrackingId;

    public string? CorrelationId => string.IsNullOrWhiteSpace(_current.CorrelationId) ? null : _current.CorrelationId;

    public string? TraceId => string.IsNullOrWhiteSpace(_current.TraceId) ? null : _current.TraceId;

    public string? SpanId => string.IsNullOrWhiteSpace(_current.SpanId) ? null : _current.SpanId;

    public ExecutionContextSnapshot Current => _current;

    public void Set(ExecutionContextSnapshot snapshot)
    {
        _current = snapshot ?? ExecutionContextSnapshot.Empty;
    }
}
