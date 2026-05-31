using System.Text.Json;
using Ums.Application.Common.Interfaces;
using Ums.Domain.Audit.AuditRecord;

namespace Ums.Infrastructure.Identity.Auth;

/// <summary>
/// Records authentication events in the append-only AuditRecord table.
/// Uses IAuditRecordRepository.AppendAsync — no new table required.
/// </summary>
public sealed class AuthAuditService : IAuthAuditService
{
    private readonly IAuditRecordRepository _auditRepo;

    public AuthAuditService(IAuditRecordRepository auditRepo)
    {
        _auditRepo = auditRepo;
    }

    public async Task RecordAuthEventAsync(AuthAuditEvent evt, CancellationToken cancellationToken = default)
    {
        var whatChanged = JsonSerializer.Serialize(new
        {
            method     = evt.AuthMethod,
            tenantCode = evt.TenantCode,
            ip         = evt.ClientIp,
            result     = evt.Succeeded ? "Success" : "Failure",
            reason     = evt.FailureReason,
            idp        = evt.IdpProvider,
        });

        var affectedId   = evt.UserId   != Guid.Empty ? evt.UserId   : Guid.NewGuid();
        var rootTenantId = evt.TenantId != Guid.Empty ? evt.TenantId : Guid.NewGuid();

        var record = AuditRecord.Record(
            whoActed:           evt.UserId != Guid.Empty ? evt.UserId : Guid.NewGuid(),
            subjectType:        SubjectType.User,
            whatChanged:        whatChanged,
            eventType:          evt.EventType,
            auditResult:        evt.Succeeded ? AuditResult.Success : AuditResult.Failure,
            affectedEntityId:   affectedId,
            affectedEntityType: "UserAccount",
            rootTenantId:       rootTenantId);

        if (record.IsSuccess)
            await _auditRepo.AppendAsync(record.Value, cancellationToken);
    }
}
