namespace Ums.Domain.Approvals;
using Ums.Domain.Approvals.ApprovalWorkflow;
using Ums.Domain.Approvals.ApprovalRequest;
using Ums.Domain.Approvals.DocumentType;
using Ums.Domain.Approvals.UserDocument;
using Ums.Domain.Approvals.AccessEnforcementPolicy;
using Ums.Domain.Approvals.NotificationRule;
using ApprovalWorkflowAggregate = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalWorkflow;
using ApprovalRequestAggregate = Ums.Domain.Approvals.ApprovalRequest.ApprovalRequest;
using DocumentTypeAggregate = Ums.Domain.Approvals.DocumentType.DocumentType;
using UserDocumentAggregate = Ums.Domain.Approvals.UserDocument.UserDocument;
using AccessEnforcementPolicyAggregate = Ums.Domain.Approvals.AccessEnforcementPolicy.AccessEnforcementPolicy;
using NotificationRuleAggregate = Ums.Domain.Approvals.NotificationRule.NotificationRule;

public interface IApprovalWorkflowRepository : IAggregateRepository<ApprovalWorkflowAggregate>
{
    Task<ApprovalWorkflowAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApprovalWorkflowAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApprovalWorkflowAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface IApprovalRequestRepository : IAggregateRepository<ApprovalRequestAggregate>
{
    Task<ApprovalRequestAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApprovalRequestAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ApprovalRequestAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<bool> ExistsPendingForScopeAsync(Guid userId, Guid systemId, Guid? branchId, CancellationToken cancellationToken = default);
}

public interface IDocumentTypeRepository : IAggregateRepository<DocumentTypeAggregate>
{
    Task<DocumentTypeAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentTypeAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentTypeAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface IUserDocumentRepository : IAggregateRepository<UserDocumentAggregate>
{
    Task<UserDocumentAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDocumentAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<UserDocumentAggregate>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}

public interface IAccessEnforcementPolicyRepository : IAggregateRepository<AccessEnforcementPolicyAggregate>
{
    Task<AccessEnforcementPolicyAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccessEnforcementPolicyAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<AccessEnforcementPolicyAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
}

public interface INotificationRuleRepository : IAggregateRepository<NotificationRuleAggregate>
{
    Task<NotificationRuleAggregate?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationRuleAggregate>> GetAllAsync(Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<NotificationRuleAggregate>> GetByTenantIdAsync(Guid tenantId, CancellationToken cancellationToken = default);

    /// <summary>Returns true when an active rule with the same tenant, channel, and recipient already exists.</summary>
    Task<bool> ExistsDuplicateAsync(Guid tenantId, string channel, string recipient, CancellationToken cancellationToken = default);
}
