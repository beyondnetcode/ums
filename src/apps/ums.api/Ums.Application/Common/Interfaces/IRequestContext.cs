namespace Ums.Application.Common.Interfaces;

/// <summary>
/// Domain-agnostic port that exposes request execution correlation data to the application layer.
/// </summary>
public interface IRequestContext
{
    string? SessionTrackingId { get; }

    string? CorrelationId { get; }

    string? TraceId { get; }

    string? SpanId { get; }
}
