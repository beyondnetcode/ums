using System.Text.Json;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ums.Infrastructure.Persistence;
using Ums.Infrastructure.Persistence.Options;
using Ums.Infrastructure.Persistence.Outbox;
using Ums.Shell.Ddd.Interfaces;

namespace Ums.Infrastructure.Hosting;

/// <summary>
/// Background service that polls the <see cref="OutboxMessage"/> table and dispatches
/// domain events to MediatR notification handlers (FIX-02).
///
/// Design decisions:
/// - Uses <see cref="IServiceScopeFactory"/> to create a fresh DI scope per cycle so that
///   the scoped <see cref="UmsPlatformDbContext"/> is never shared across iterations.
/// - Processes messages in batches of <see cref="BatchSize"/> ordered by OccurredOnUtc
///   (FIFO), matching the <c>IX_OutboxMessages_Dispatch</c> index.
/// - On success: stamps <see cref="OutboxMessage.ProcessedOnUtc"/>.
/// - On failure: increments <see cref="OutboxMessage.RetryCount"/> and records the last
///   error.
/// - REC-13: When <see cref="RetryCount"/> reaches <see cref="MaxRetries"/>, the message
///   is moved to <see cref="DeadLetterMessage"/> (OutboxDeadLetters table) and removed
///   from the hot outbox. This keeps the batch query fast and the readiness health check
///   accurate. Operators can inspect dead-letters via the admin API and replay them.
/// - Only active when <c>Persistence:EnableOutbox = true</c> and
///   <c>Persistence:Provider = SqlServer</c>. In InMemory mode the service exits immediately
///   because there is no outbox table.
/// </summary>
internal sealed class OutboxDispatcherBackgroundService(
    IServiceScopeFactory scopeFactory,
    IOptions<PersistenceOptions> options,
    ILogger<OutboxDispatcherBackgroundService> logger) : BackgroundService
{
    private const int BatchSize  = 50;
    private const int MaxRetries = 5;
    private static readonly TimeSpan PollingInterval  = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ErrorCooldown    = TimeSpan.FromSeconds(30);
    // HARDENING-01: Lease TTL. If a pod crashes mid-dispatch, another pod can re-claim
    // the message after this window expires. Must be > max expected dispatch duration.
    private static readonly TimeSpan LeaseDuration = TimeSpan.FromMinutes(2);
    // Stable pod identity for diagnostics (PID + machine name).
    private static readonly string InstanceId =
        $"{Environment.MachineName}-{Environment.ProcessId}";

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var persistence = options.Value;

        if (persistence.Provider != PersistenceProvider.SqlServer || !persistence.EnableOutbox)
        {
            logger.LogInformation(
                "OutboxDispatcher: disabled (Provider={Provider}, EnableOutbox={EnableOutbox}). Exiting.",
                persistence.Provider, persistence.EnableOutbox);
            return;
        }

        logger.LogInformation(
            "OutboxDispatcher: started. Polling every {Interval}s, batch={Batch}, maxRetries={Max}.",
            PollingInterval.TotalSeconds, BatchSize, MaxRetries);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var dispatched = await DispatchBatchAsync(stoppingToken);

                // Back-pressure: if the batch was full there are likely more messages —
                // skip the wait and go again immediately. Otherwise sleep.
                if (dispatched < BatchSize)
                    await Task.Delay(PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "OutboxDispatcher: unhandled error in dispatch loop. Cooling down for {Cooldown}s.",
                    ErrorCooldown.TotalSeconds);

                await Task.Delay(ErrorCooldown, stoppingToken);
            }
        }

        logger.LogInformation("OutboxDispatcher: stopped.");
    }

    /// <summary>
    /// HARDENING-01: Two-phase dispatch — claim then process.
    ///
    /// Phase 1 (atomic claim): ExecuteUpdateAsync stamps LockedUntil on BatchSize eligible
    /// rows in a single UPDATE … WHERE, which SQL Server executes atomically. Two concurrent
    /// pods cannot claim the same row because the WHERE excludes already-locked rows.
    ///
    /// Phase 2 (read own batch): only fetch rows locked by this instance so the batch
    /// is deterministic even if another pod claims new rows between the two operations.
    ///
    /// Crash recovery: if this pod dies before marking ProcessedOnUtc, the lease expires
    /// after LeaseDuration and another pod re-claims and re-dispatches (idempotency is the
    /// handler's responsibility, as documented in AGENTS.md).
    /// </summary>
    private async Task<int> DispatchBatchAsync(CancellationToken ct)
    {
        await using var scope   = scopeFactory.CreateAsyncScope();
        var dbContext  = scope.ServiceProvider.GetRequiredService<UmsPlatformDbContext>();
        var publisher  = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var now      = DateTime.UtcNow;
        var leaseEnd = now.Add(LeaseDuration);

        // Phase 1 — atomic claim: stamp LockedUntil on up to BatchSize eligible messages.
        // Eligible = not yet processed AND retries not exhausted AND (no lock OR lock expired).
        var claimed = await dbContext.OutboxMessages
            .Where(m =>
                m.ProcessedOnUtc == null &&
                m.RetryCount < MaxRetries &&
                (m.LockedUntil == null || m.LockedUntil < now))
            .Take(BatchSize)
            .ExecuteUpdateAsync(set => set
                .SetProperty(x => x.LockedUntil, leaseEnd)
                .SetProperty(x => x.LockedBy, InstanceId),
            ct);

        if (claimed == 0)
            return 0;

        // Phase 2 — read only the messages claimed by this instance in this cycle.
        var messages = await dbContext.OutboxMessages
            .Where(m =>
                m.ProcessedOnUtc == null &&
                m.LockedBy == InstanceId &&
                m.LockedUntil == leaseEnd)
            .OrderBy(m => m.OccurredOnUtc)
            .Take(BatchSize)
            .ToListAsync(ct);

        if (messages.Count == 0)
            return 0;

        logger.LogDebug("OutboxDispatcher: processing {Count} message(s).", messages.Count);

        var deadLettered = new List<OutboxMessage>();

        foreach (var message in messages)
        {
            if (ct.IsCancellationRequested) break;
            await DispatchMessageAsync(message, publisher, ct);

            // REC-13: If this attempt pushed the message to MaxRetries, move it to DLQ.
            if (message.RetryCount >= MaxRetries && message.ProcessedOnUtc is null)
            {
                deadLettered.Add(message);
            }
        }

        // Move exhausted messages to dead-letter table and remove from hot outbox.
        if (deadLettered.Count > 0)
        {
            var deadLetterEntries = deadLettered
                .Select(DeadLetterMessage.FromOutboxMessage)
                .ToList();

            dbContext.OutboxDeadLetters.AddRange(deadLetterEntries);
            dbContext.OutboxMessages.RemoveRange(deadLettered);

            logger.LogWarning(
                "OutboxDispatcher: {Count} message(s) exhausted retries and were moved to dead-letter store. " +
                "Inspect OutboxDeadLetters table or use the admin API to review.",
                deadLettered.Count);
        }

        await dbContext.SaveChangesAsync(ct);
        return messages.Count;
    }

    private async Task DispatchMessageAsync(
        OutboxMessage message,
        IPublisher publisher,
        CancellationToken ct)
    {
        try
        {
            var eventType = ResolveEventType(message.EventType);
            if (eventType is null)
            {
                logger.LogWarning(
                    "OutboxDispatcher: cannot resolve type '{EventType}' for message {Id}. Skipping.",
                    message.EventType, message.Id);

                // Treat unresolvable types as permanently failed so they do not block the queue.
                message.RetryCount = MaxRetries;
                message.LastError  = $"Type '{message.EventType}' could not be resolved.";
                return;
            }

            var domainEvent = (IDomainEvent?)JsonSerializer.Deserialize(message.Payload, eventType);
            if (domainEvent is null)
            {
                logger.LogWarning(
                    "OutboxDispatcher: deserialization returned null for message {Id} (type={EventType}).",
                    message.Id, message.EventType);

                message.RetryCount = MaxRetries;
                message.LastError  = "Deserialization returned null.";
                return;
            }

            await publisher.Publish(domainEvent, ct);

            message.ProcessedOnUtc = DateTime.UtcNow;

            logger.LogDebug(
                "OutboxDispatcher: dispatched {EventName} (aggregateId={AggregateId}, messageId={Id}).",
                message.EventName, message.AggregateId, message.Id);
        }
        catch (Exception ex)
        {
            message.RetryCount++;
            message.LastError = ex.Message.Length > 4000
                ? ex.Message[..4000]
                : ex.Message;

            logger.LogWarning(ex,
                "OutboxDispatcher: failed to dispatch message {Id} ({EventName}). Retry {Retry}/{Max}.",
                message.Id, message.EventName, message.RetryCount, MaxRetries);
        }
    }

    /// <summary>
    /// Resolves the CLR type from the stored assembly-qualified name.
    /// Tries <see cref="Type.GetType"/> first (works for the current assembly), then scans
    /// loaded assemblies as a fallback for cross-assembly event types.
    /// </summary>
    private static Type? ResolveEventType(string eventType)
    {
        var resolved = Type.GetType(eventType);
        if (resolved is not null) return resolved;

        // Fallback: search loaded assemblies by full name
        return AppDomain.CurrentDomain.GetAssemblies()
            .Select(a => a.GetType(eventType))
            .FirstOrDefault(t => t is not null);
    }
}
