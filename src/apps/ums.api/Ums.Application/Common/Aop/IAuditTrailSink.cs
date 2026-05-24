namespace Ums.Application.Common.Aop;

public interface IAuditTrailSink
{
    bool TryWrite(AuditTrailEntry entry);
}
