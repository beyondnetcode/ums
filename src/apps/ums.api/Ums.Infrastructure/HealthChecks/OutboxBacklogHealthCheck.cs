namespace Ums.Infrastructure.HealthChecks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Options;

/// <summary>
/// REC-02 / REC-13: Verifica el estado del outbox transaccional y el dead-letter store.
///
/// Unhealthy  → mensajes no procesados con más de 5 minutos de antigüedad
///              (el dispatcher está caído o bloqueado).
/// Degraded   → backlog creciente entre 1 y 5 minutos, o mensajes en el DLQ
///              no reproducidos (requires operator attention).
/// Healthy    → todo fluye normalmente.
///
/// REC-13: Since exhausted outbox messages are now moved to OutboxDeadLetters,
/// the backlog check only sees truly in-flight messages.  The dead-letter count
/// is surfaced separately as a Degraded signal.
/// </summary>
public sealed class OutboxBacklogHealthCheck(
    UmsPlatformDbContext dbContext,
    IOptions<PersistenceOptions> options) : IHealthCheck
{
    private static readonly TimeSpan UnhealthyThreshold = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DegradedThreshold  = TimeSpan.FromMinutes(1);

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        // Skip when running InMemory — no outbox table
        if (options.Value.Provider != PersistenceProvider.SqlServer)
            return HealthCheckResult.Healthy("Outbox N/A (InMemory provider).");

        try
        {
            var now = DateTimeOffset.UtcNow;

            // REC-13: OutboxMessages now only contain in-flight messages (not exhausted ones).
            var unprocessed = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc == null)
                .Select(m => new { m.OccurredOnUtc })
                .ToListAsync(cancellationToken);

            // Count unrpelayed dead-letter entries (require operator attention)
            var deadLetterCount = await dbContext.OutboxDeadLetters
                .CountAsync(m => !m.ReplayedSuccessfully, cancellationToken);

            var data = new Dictionary<string, object>
            {
                ["pendingCount"]      = unprocessed.Count,
                ["deadLetterCount"]   = deadLetterCount,
            };

            if (unprocessed.Count > 0)
            {
                var oldestAge = now - unprocessed.Min(m => m.OccurredOnUtc);
                data["oldestAgeSeconds"] = (int)oldestAge.TotalSeconds;

                if (oldestAge > UnhealthyThreshold)
                    return HealthCheckResult.Unhealthy(
                        $"Outbox backlog stalled: oldest message is {oldestAge.TotalMinutes:F1} min old.",
                        data: data);

                if (oldestAge > DegradedThreshold)
                    return HealthCheckResult.Degraded(
                        $"Outbox backlog growing: {unprocessed.Count} pending, oldest {oldestAge.TotalSeconds:F0}s.",
                        data: data);
            }

            // Dead-letters are not Unhealthy (they don't block dispatch) but they need attention.
            if (deadLetterCount > 0)
                return HealthCheckResult.Degraded(
                    $"Outbox: {deadLetterCount} dead-letter message(s) require manual review or replay.",
                    data: data);

            return unprocessed.Count == 0
                ? HealthCheckResult.Healthy("Outbox clear — no pending messages.", data)
                : HealthCheckResult.Healthy(
                    $"Outbox flowing: {unprocessed.Count} in-flight.",
                    data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Outbox health check failed.", ex);
        }
    }
}
