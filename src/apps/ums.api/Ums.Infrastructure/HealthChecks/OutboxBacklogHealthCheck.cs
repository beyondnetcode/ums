namespace Ums.Infrastructure.HealthChecks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Options;

/// <summary>
/// REC-02: Verifica el estado del outbox transaccional.
///
/// Unhealthy  → mensajes no procesados con más de 5 minutos de antigüedad
///              (el dispatcher está caído o bloqueado).
/// Degraded   → mensajes muertos (RetryCount >= MaxRetries, aún sin procesar),
///              o backlog creciente entre 1 y 5 minutos.
/// Healthy    → todo fluye normalmente.
/// </summary>
public sealed class OutboxBacklogHealthCheck(
    UmsPlatformDbContext dbContext,
    IOptions<PersistenceOptions> options) : IHealthCheck
{
    private static readonly TimeSpan UnhealthyThreshold = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan DegradedThreshold  = TimeSpan.FromMinutes(1);
    private const int MaxRetries = 5;

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

            var unprocessed = await dbContext.OutboxMessages
                .Where(m => m.ProcessedOnUtc == null)
                .Select(m => new { m.RetryCount, m.OccurredOnUtc })
                .ToListAsync(cancellationToken);

            if (unprocessed.Count == 0)
                return HealthCheckResult.Healthy("Outbox clear — no pending messages.");

            var deadLetters = unprocessed.Count(m => m.RetryCount >= MaxRetries);
            var oldestAge   = now - unprocessed.Min(m => m.OccurredOnUtc);

            var data = new Dictionary<string, object>
            {
                ["pendingCount"] = unprocessed.Count,
                ["deadLetters"]  = deadLetters,
                ["oldestAgeSeconds"] = (int)oldestAge.TotalSeconds,
            };

            if (deadLetters > 0)
                return HealthCheckResult.Degraded(
                    $"Outbox: {deadLetters} dead-letter message(s) require manual review.",
                    data: data);

            if (oldestAge > UnhealthyThreshold)
                return HealthCheckResult.Unhealthy(
                    $"Outbox backlog stalled: oldest message is {oldestAge.TotalMinutes:F1} min old.",
                    data: data);

            if (oldestAge > DegradedThreshold)
                return HealthCheckResult.Degraded(
                    $"Outbox backlog growing: {unprocessed.Count} pending, oldest {oldestAge.TotalSeconds:F0}s.",
                    data: data);

            return HealthCheckResult.Healthy(
                $"Outbox flowing: {unprocessed.Count} in-flight, oldest {oldestAge.TotalSeconds:F0}s.",
                data);
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Outbox health check failed.", ex);
        }
    }
}
