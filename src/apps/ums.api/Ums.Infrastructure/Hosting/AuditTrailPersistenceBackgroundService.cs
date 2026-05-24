using Ums.Domain.Audit.AuditRecord;

namespace Ums.Infrastructure.Hosting;

internal sealed class AuditTrailPersistenceBackgroundService(
    Channel<AuditTrailEntry> auditTrailChannel,
    IServiceScopeFactory scopeFactory,
    ILogger<AuditTrailPersistenceBackgroundService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var entry in auditTrailChannel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await PersistAsync(entry, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex,
                    "AuditTrailPersistence: failed to persist audit entry {EventType} for {AffectedEntityType}/{AffectedEntityId}.",
                    entry.EventType,
                    entry.AffectedEntityType,
                    entry.AffectedEntityId);
            }
        }
    }

    private async Task PersistAsync(AuditTrailEntry entry, CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IAuditRecordRepository>();

        var subjectType = ResolveSubjectType(entry.SubjectType);
        var auditResult = ResolveAuditResult(entry.AuditResult);

        var auditRecordResult = AuditRecord.Record(
            entry.WhoActed,
            subjectType,
            entry.WhatChanged,
            entry.EventType,
            auditResult,
            entry.AffectedEntityId,
            entry.AffectedEntityType,
            entry.RootTenantId,
            entry.Metadata);

        if (auditRecordResult.IsFailure)
        {
            logger.LogWarning(
                "AuditTrailPersistence: discarded invalid audit entry {EventType} for {AffectedEntityType}/{AffectedEntityId}: {Error}",
                entry.EventType,
                entry.AffectedEntityType,
                entry.AffectedEntityId,
                auditRecordResult.Error);
            return;
        }

        await repository.AppendAsync(auditRecordResult.Value, cancellationToken);
        await repository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
    }

    private static SubjectType ResolveSubjectType(string value)
        => value switch
        {
            nameof(SubjectType.User) => SubjectType.User,
            nameof(SubjectType.Admin) => SubjectType.Admin,
            nameof(SubjectType.System) => SubjectType.System,
            "BACKGROUND_WORKER" => SubjectType.BackgroundWorker,
            _ => SubjectType.System,
        };

    private static AuditResult ResolveAuditResult(string value)
        => value switch
        {
            nameof(AuditResult.Failure) => AuditResult.Failure,
            nameof(AuditResult.Partial) => AuditResult.Partial,
            _ => AuditResult.Success,
        };
}
