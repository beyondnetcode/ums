using Ums.Infrastructure.Services;

namespace Ums.Presentation.IntegrationTest.Infrastructure;

public sealed class RequestContextAccessorTests
{
    [Fact]
    public void Set_ShouldExposeSnapshotThroughRequestContextProperties()
    {
        var accessor = new RequestContextAccessor();

        accessor.Set(new ExecutionContextSnapshot(
            CorrelationId: "corr-123",
            SessionTrackingId: "session-123",
            TraceId: "trace-123",
            SpanId: "span-123"));

        accessor.CorrelationId.Should().Be("corr-123");
        accessor.SessionTrackingId.Should().Be("session-123");
        accessor.TraceId.Should().Be("trace-123");
        accessor.SpanId.Should().Be("span-123");
    }

    [Fact]
    public void Set_WithEmptySnapshot_ShouldReturnNullProperties()
    {
        var accessor = new RequestContextAccessor();

        accessor.Set(ExecutionContextSnapshot.Empty);

        accessor.CorrelationId.Should().BeNull();
        accessor.SessionTrackingId.Should().BeNull();
        accessor.TraceId.Should().BeNull();
        accessor.SpanId.Should().BeNull();
    }

    [Fact]
    public void SetClientTimezone_ShouldExposeThroughClientTimezoneProperty()
    {
        var accessor = new RequestContextAccessor();

        accessor.SetClientTimezone("America/Lima");

        accessor.ClientTimezone.Should().Be("America/Lima");
    }

    [Fact]
    public void SetClientTimezone_WithNull_ShouldReturnNull()
    {
        var accessor = new RequestContextAccessor();
        accessor.SetClientTimezone("America/Lima");

        accessor.SetClientTimezone(null);

        accessor.ClientTimezone.Should().BeNull();
    }
}
