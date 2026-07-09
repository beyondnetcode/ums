using Evolith.Contracts.MasterData;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Ums.Infrastructure.MasterData;

/// <summary>
/// Projects MMS master-Tenant events into the local UMS read model (ADR-0083 / ADR-0106).
/// Idempotent and order-tolerant: the MassTransit inbox dedups by messageId, and the versioned
/// upsert applies only when the incoming sequence is newer than the stored version (stale /
/// out-of-order events are discarded). Deactivated/Deleted map to a soft-delete status.
/// </summary>
public sealed class TenantProjectionConsumer : IConsumer<TenantEvent>
{
    private readonly TenantProjectionDbContext _db;
    private readonly ILogger<TenantProjectionConsumer> _logger;

    public TenantProjectionConsumer(TenantProjectionDbContext db, ILogger<TenantProjectionConsumer> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<TenantEvent> context)
    {
        var e = context.Message;
        var ct = context.CancellationToken;

        var existing = await _db.Tenants.FirstOrDefaultAsync(x => x.TenantId == e.Subject, ct);

        if (existing is not null && e.Sequence <= existing.Version)
        {
            _logger.LogInformation(
                "Tenant projection: discarding stale/out-of-order {Type} seq={Seq} (stored={Stored}) tenant={Tenant} corr={Corr}",
                e.Type, e.Sequence, existing.Version, e.Subject, e.CorrelationId);
            return;
        }

        var record = existing ?? new TenantProjectionRecord { TenantId = e.Subject };
        record.Code = e.Data.Code;
        record.Name = e.Data.Name;
        record.Status = e.Data.Status;
        record.Version = e.Sequence;
        record.UpdatedAt = e.Time;

        if (existing is null) await _db.Tenants.AddAsync(record, ct);
        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Tenant projection applied {Type} seq={Seq} tenant={Tenant} status={Status} corr={Corr}",
            e.Type, e.Sequence, e.Subject, e.Data.Status, e.CorrelationId);
    }
}
