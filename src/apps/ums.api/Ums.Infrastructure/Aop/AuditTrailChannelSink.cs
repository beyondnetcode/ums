namespace Ums.Infrastructure.Aop;

public sealed class AuditTrailChannelSink : IAuditTrailSink
{
    private readonly Channel<AuditTrailEntry> _channel;

    public AuditTrailChannelSink(Channel<AuditTrailEntry> channel)
    {
        _channel = channel;
    }

    public bool TryWrite(AuditTrailEntry entry) => _channel.Writer.TryWrite(entry);
}
