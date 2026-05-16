namespace Ums.Domain.Audit;

using Ums.Domain.Kernel;

public interface IAuditRecordRepository : ITenantScopedRepository<AuditRecord>
{
    Task<IReadOnlyCollection<AuditRecord>> GetByCorrelationIdAsync(Guid tenantId, string correlationId, CancellationToken cancellationToken = default);
}

public interface IFlagEvaluationLogRepository
{
    Task AddAsync(FlagEvaluationLog log, CancellationToken cancellationToken = default);
}

