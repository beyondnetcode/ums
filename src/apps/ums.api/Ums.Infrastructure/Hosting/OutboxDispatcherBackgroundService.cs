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
    /// Fetches one batch of unprocessed messages, dispatches each one, and returns the
    /// number of messages that were successfully dequeued from the batch.
    /// REC-13: After dispatching, any message that hit MaxRetries is moved to the
    /// dead-letter table and deleted from the outbox to keep the hot table clean.
    /// </summary>
    private async Task<int> DispatchBatchAsync(CancellationToken ct)
    {
        await using var scope   = scopeFactory.CreateAsyncScope();
        var dbContext  = scope.ServiceProvider.GetRequiredService<UmsPlatformDbContext>();
        var publisher  = scope.ServiceProvider.GetRequiredService<IPublisher>();

        var messages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOnUtc == null && m.RetryCount < MaxRetries)
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
