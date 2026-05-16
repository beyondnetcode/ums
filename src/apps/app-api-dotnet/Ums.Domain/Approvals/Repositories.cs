namespace Ums.Domain.Approvals;

using Ums.Domain.Kernel;

public interface IApprovalWorkflowRepository : ITenantScopedRepository<ApprovalWorkflow>
{
    Task<ApprovalWorkflow?> GetByRequestTypeAsync(Guid tenantId, string requestType, CancellationToken cancellationToken = default);
}

public interface IApprovalRequestRepository : ITenantScopedRepository<ApprovalRequest>
{
    Task<IReadOnlyCollection<ApprovalRequest>> GetPendingAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface IExternalAccessRequestRepository : ITenantScopedRepository<ExternalAccessRequest>
{
    Task<IReadOnlyCollection<ExternalAccessRequest>> GetPendingAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
