namespace Ums.Infrastructure.Persistence.Seeders;

using Microsoft.Extensions.DependencyInjection;
using Ums.Domain.Approvals;
using Ums.Domain.Approvals.AccessEnforcementPolicy;
using Ums.Domain.Approvals.ApprovalRequest;
using Ums.Domain.Approvals.ApprovalWorkflow;
using Ums.Domain.Approvals.DocumentType;
using Ums.Domain.Approvals.NotificationRule;
using Ums.Domain.Approvals.UserDocument;
using Ums.Domain.Kernel.ValueObjects;
using ApprovalWorkflowAggregate = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalWorkflow;
using ApprovalRequestAggregate = Ums.Domain.Approvals.ApprovalRequest.ApprovalRequest;
using DocumentTypeAggregate = Ums.Domain.Approvals.DocumentType.DocumentType;
using UserDocumentAggregate = Ums.Domain.Approvals.UserDocument.UserDocument;
using AccessEnforcementPolicyAggregate = Ums.Domain.Approvals.AccessEnforcementPolicy.AccessEnforcementPolicy;
using NotificationRuleAggregate = Ums.Domain.Approvals.NotificationRule.NotificationRule;

public static class ApprovalsDevDataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        var wfRepository = serviceProvider.GetService<IApprovalWorkflowRepository>();
        var inMemoryWfRepository = serviceProvider.GetService<InMemoryApprovalWorkflowRepository>();

        var reqRepository = serviceProvider.GetService<IApprovalRequestRepository>();
        var inMemoryReqRepository = serviceProvider.GetService<InMemoryApprovalRequestRepository>();

        var docTypeRepository = serviceProvider.GetService<IDocumentTypeRepository>();
        var inMemoryDocTypeRepository = serviceProvider.GetService<InMemoryDocumentTypeRepository>();

        var userDocRepository = serviceProvider.GetService<IUserDocumentRepository>();
        var inMemoryUserDocRepository = serviceProvider.GetService<InMemoryUserDocumentRepository>();

        var policyRepository = serviceProvider.GetService<IAccessEnforcementPolicyRepository>();
        var inMemoryPolicyRepository = serviceProvider.GetService<InMemoryAccessEnforcementPolicyRepository>();

        var notificationRepository = serviceProvider.GetService<INotificationRuleRepository>();
        var inMemoryNotificationRepository = serviceProvider.GetService<InMemoryNotificationRuleRepository>();

        var actor = ActorId.Create(CoreDevDataSeeder.SystemActorId);
        var ransaTenantId = TenantId.Load(Guid.Parse(CoreDevDataSeeder.RansaTenantId));
        var adminUserId = UserId.Load(Guid.Parse(CoreDevDataSeeder.RansaAdminUserId));

        // Document Types
        var docTypes = BuildSeedDocumentTypes(ransaTenantId, actor);
        if (inMemoryDocTypeRepository is not null)
            foreach (var docType in docTypes) inMemoryDocTypeRepository.Seed(docType);
        else if (docTypeRepository is not null)
        {
            var existing = await docTypeRepository.GetByTenantIdAsync(ransaTenantId.GetValue(), cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var docType in docTypes) await docTypeRepository.AddAsync(docType, cancellationToken);
                await docTypeRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }

        // Workflows
        var workflows = BuildSeedWorkflows(ransaTenantId, docTypes, actor);
        if (inMemoryWfRepository is not null)
            foreach (var wf in workflows) inMemoryWfRepository.Seed(wf);
        else if (wfRepository is not null)
        {
            var existing = await wfRepository.GetByTenantIdAsync(ransaTenantId.GetValue(), cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var wf in workflows) await wfRepository.AddAsync(wf, cancellationToken);
                await wfRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }

        // Requests
        var requests = BuildSeedRequests(adminUserId, workflows, actor);
        if (inMemoryReqRepository is not null)
            foreach (var req in requests) inMemoryReqRepository.Seed(req);
        else if (reqRepository is not null)
        {
            var existing = await reqRepository.GetByTenantIdAsync(ransaTenantId.GetValue(), cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var req in requests) await reqRepository.AddAsync(req, cancellationToken);
                await reqRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }

        // User Documents
        var userDocs = BuildSeedUserDocs(adminUserId, docTypes, actor);
        if (inMemoryUserDocRepository is not null)
            foreach (var doc in userDocs) inMemoryUserDocRepository.Seed(doc);
        else if (userDocRepository is not null)
        {
            var existing = await userDocRepository.GetByUserIdAsync(adminUserId.GetValue(), cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var doc in userDocs) await userDocRepository.AddAsync(doc, cancellationToken);
                await userDocRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }

        // Policies
        var policies = BuildSeedPolicies(ransaTenantId, actor);
        if (inMemoryPolicyRepository is not null)
            foreach (var pol in policies) inMemoryPolicyRepository.Seed(pol);
        else if (policyRepository is not null)
        {
            var existing = await policyRepository.GetByTenantIdAsync(ransaTenantId.GetValue(), cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var pol in policies) await policyRepository.AddAsync(pol, cancellationToken);
                await policyRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }

        // Notification Rules
        var rules = BuildSeedNotificationRules(ransaTenantId, actor);
        if (inMemoryNotificationRepository is not null)
            foreach (var rule in rules) inMemoryNotificationRepository.Seed(rule);
        else if (notificationRepository is not null)
        {
            var existing = await notificationRepository.GetByTenantIdAsync(ransaTenantId.GetValue(), cancellationToken);
            if (existing.Count == 0)
            {
                foreach (var rule in rules) await notificationRepository.AddAsync(rule, cancellationToken);
                await notificationRepository.UnitOfWork.SaveEntitiesAsync(cancellationToken);
            }
        }
    }

    // DocumentType.Create(TenantId, Code, Name, Description, DocumentCriticity, ActorId)
    private static IReadOnlyList<DocumentTypeAggregate> BuildSeedDocumentTypes(TenantId tenantId, ActorId actor)
    {
        var result = DocumentTypeAggregate.Create(
            tenantId,
            Code.Create("DNI"),
            Name.Create("Documento Nacional de Identidad"),
            Description.Create("Documento de identidad nacional peruano"),
            DocumentCriticity.High,
            actor);

        return result.IsSuccess ? new[] { result.Value } : Array.Empty<DocumentTypeAggregate>();
    }

    // ApprovalWorkflow.Create(TenantId, Code, Name, Description, UserCategory, bool requiresApproval, SystemSuiteId?, ActorId)
    private static IReadOnlyList<ApprovalWorkflowAggregate> BuildSeedWorkflows(TenantId tenantId, IReadOnlyList<DocumentTypeAggregate> docTypes, ActorId actor)
    {
        var wf = ApprovalWorkflowAggregate.Create(
            tenantId,
            Code.Create("ONBOARDING"),
            Name.Create("Supplier Onboarding Workflow"),
            Description.Create("Workflow para onboarding de proveedores externos"),
            UserCategory.External,
            true,
            null,
            actor);

        if (wf.IsSuccess)
        {
            if (docTypes.Count > 0)
                wf.Value.AddRequiredDocument(DocumentTypeId.Load(docTypes[0].Props.Id.GetValue()), true, actor);
            return new[] { wf.Value };
        }

        return Array.Empty<ApprovalWorkflowAggregate>();
    }

    // ApprovalRequest.Create(ApprovalWorkflowId, UserId targetUserId, ProfileId?, ActorId)
    private static IReadOnlyList<ApprovalRequestAggregate> BuildSeedRequests(UserId requesterId, IReadOnlyList<ApprovalWorkflowAggregate> wfs, ActorId actor)
    {
        if (wfs.Count == 0) return Array.Empty<ApprovalRequestAggregate>();

        var req = ApprovalRequestAggregate.Create(
            wfs[0].GetId(),
            requesterId,
            ProfileId.Load(Guid.Parse(CoreDevDataSeeder.DemoAdminProfileId)),
            actor);

        return req.IsSuccess ? new[] { req.Value } : Array.Empty<ApprovalRequestAggregate>();
    }

    // UserDocument.Upload(UserId, DocumentTypeId, DateTime issueDate, DateTime expiration, DocumentCriticity, TextValueObject fileStoragePath, string fileChecksum, ActorId)
    private static IReadOnlyList<UserDocumentAggregate> BuildSeedUserDocs(UserId userId, IReadOnlyList<DocumentTypeAggregate> docTypes, ActorId actor)
    {
        if (docTypes.Count == 0) return Array.Empty<UserDocumentAggregate>();

        var doc = UserDocumentAggregate.Upload(
            userId,
            DocumentTypeId.Load(docTypes[0].Props.Id.GetValue()),
            DateTime.UtcNow.AddMonths(-6),
            DateTime.UtcNow.AddYears(2),
            DocumentCriticity.High,
            TextValueObject.Create("https://storage.ransa.pe/docs/44556677.pdf"),
            "sha256-dev-placeholder",
            actor);

        return doc.IsSuccess ? new[] { doc.Value } : Array.Empty<UserDocumentAggregate>();
    }

    // AccessEnforcementPolicy.Create(TenantId, ProfileId?, RoleId?, AccessEnforcementAction, ActorId)
    private static IReadOnlyList<AccessEnforcementPolicyAggregate> BuildSeedPolicies(TenantId tenantId, ActorId actor)
    {
        var result = AccessEnforcementPolicyAggregate.Create(
            tenantId,
            null,
            null,
            AccessEnforcementAction.BlockUser,
            actor);

        return result.IsSuccess ? new[] { result.Value } : Array.Empty<AccessEnforcementPolicyAggregate>();
    }

    // NotificationRule.Create(TenantId, NotificationChannel, TextValueObject recipient, ActorId)
    private static IReadOnlyList<NotificationRuleAggregate> BuildSeedNotificationRules(TenantId tenantId, ActorId actor)
    {
        var result = NotificationRuleAggregate.Create(
            tenantId,
            NotificationChannel.Email,
            TextValueObject.Create("operaciones@ransa.pe"),
            actor);

        return result.IsSuccess ? new[] { result.Value } : Array.Empty<NotificationRuleAggregate>();
    }
}
