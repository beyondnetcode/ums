using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ums.Domain.Approvals.ApprovalWorkflow;
using Ums.Domain.Approvals.ApprovalWorkflow.ApprovalRequiredDocument;
using Ums.Domain.Approvals.ApprovalRequest;
using Ums.Domain.Approvals.DocumentType;
using Ums.Domain.Approvals.UserDocument;
using Ums.Domain.Approvals.UserDocument.AccessNotification;
using Ums.Domain.Approvals.AccessEnforcementPolicy;
using Ums.Domain.Approvals.NotificationRule;
using Ums.Domain.Enums;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Infrastructure.Persistence.Approvals.Entities;
using BeyondNetCode.Shell.Ddd;
using BeyondNetCode.Shell.Ddd.ValueObjects.Audit;

namespace Ums.Infrastructure.Persistence.Reflection;

using ApprovalWorkflowAggregate = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalWorkflow;
using ApprovalRequiredDocumentEntity = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalRequiredDocument.ApprovalRequiredDocument;
using ApprovalRequestAggregate = Ums.Domain.Approvals.ApprovalRequest.ApprovalRequest;
using DocumentTypeAggregate = Ums.Domain.Approvals.DocumentType.DocumentType;
using UserDocumentAggregate = Ums.Domain.Approvals.UserDocument.UserDocument;
using AccessNotificationEntity = Ums.Domain.Approvals.UserDocument.AccessNotification.AccessNotification;
using AccessEnforcementPolicyAggregate = Ums.Domain.Approvals.AccessEnforcementPolicy.AccessEnforcementPolicy;
using NotificationRuleAggregate = Ums.Domain.Approvals.NotificationRule.NotificationRule;

internal static class ApprovalsAggregateFactory
{
    private static readonly BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static ApprovalWorkflowAggregate RehydrateWorkflow(
        ApprovalWorkflowRecord workflowRecord,
        IReadOnlyCollection<ApprovalRequiredDocumentRecord> documentRecords)
    {
        var props = new ApprovalWorkflowProps(
            IdValueObject.Load(workflowRecord.Id),
            TenantId.Load(workflowRecord.TenantId),
            workflowRecord.SystemSuiteId.HasValue ? SystemSuiteId.Load(workflowRecord.SystemSuiteId.Value) : null,
            Code.Create(workflowRecord.Code),
            Name.Create(workflowRecord.Name),
            Description.Create(workflowRecord.Description),
            DomainEnumerationMapper.FromValue<UserCategory>(workflowRecord.TargetUserCategoryId),
            workflowRecord.RequiresApproval,
            ActorId.Create(workflowRecord.CreatedBy));

        SetAudit(props, workflowRecord.CreatedBy, workflowRecord.CreatedAtUtc, workflowRecord.UpdatedBy, workflowRecord.UpdatedAtUtc, workflowRecord.AuditTimeSpan);

        var workflow = Construct<ApprovalWorkflowAggregate, ApprovalWorkflowProps>(props);
        var requiredDocuments = documentRecords.Select(RehydrateRequiredDocument).ToList();

        SetField(workflow, "_requiredDocuments", requiredDocuments);
        workflow.DomainEvents.MarkChangesAsCommitted();
        workflow.BrokenRules.Clear();

        return workflow;
    }

    private static ApprovalRequiredDocumentEntity RehydrateRequiredDocument(ApprovalRequiredDocumentRecord record)
    {
        var props = new ApprovalRequiredDocumentProps(
            IdValueObject.Load(record.Id),
            ApprovalWorkflowId.Load(record.WorkflowId),
            DocumentTypeId.Load(record.DocumentTypeId),
            record.IsMandatory,
            ActorId.Create(record.CreatedBy));

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);
        return Construct<ApprovalRequiredDocumentEntity, ApprovalRequiredDocumentProps>(props);
    }

    public static ApprovalRequestAggregate RehydrateRequest(ApprovalRequestRecord record)
    {
        var props = new ApprovalRequestProps(
            IdValueObject.Load(record.Id),
            ApprovalWorkflowId.Load(record.WorkflowId),
            UserId.Load(record.TargetUserId),
            record.TargetProfileId.HasValue ? ProfileId.Load(record.TargetProfileId.Value) : null,
            DomainEnumerationMapper.FromValue<ApprovalStatus>(record.StatusId),
            ActorId.Create(record.CreatedBy));

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        var request = Construct<ApprovalRequestAggregate, ApprovalRequestProps>(props);
        request.DomainEvents.MarkChangesAsCommitted();
        request.BrokenRules.Clear();

        return request;
    }

    public static NotificationRuleAggregate RehydrateRule(NotificationRuleRecord record)
    {
        var props = new NotificationRuleProps(
            IdValueObject.Load(record.Id),
            TenantId.Load(record.TenantId),
            DomainEnumerationMapper.FromValue<NotificationChannel>(record.ChannelId),
            TextValueObject.Create(record.Recipient),
            record.IsActive,
            ActorId.Create(record.CreatedBy));

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        var rule = Construct<NotificationRuleAggregate, NotificationRuleProps>(props);
        rule.DomainEvents.MarkChangesAsCommitted();
        rule.BrokenRules.Clear();

        return rule;
    }

    public static DocumentTypeAggregate RehydrateDocumentType(DocumentTypeRecord record)
    {
        var props = new DocumentTypeProps(
            IdValueObject.Load(record.Id),
            TenantId.Load(record.TenantId),
            Code.Create(record.Code),
            Name.Create(record.Name),
            Description.Create(record.Description),
            DomainEnumerationMapper.FromValue<DocumentCriticity>(record.CriticityId),
            ActorId.Create(record.CreatedBy));

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        var documentType = Construct<DocumentTypeAggregate, DocumentTypeProps>(props);
        documentType.DomainEvents.MarkChangesAsCommitted();
        documentType.BrokenRules.Clear();

        return documentType;
    }

    public static UserDocumentAggregate RehydrateUserDocument(
        UserDocumentRecord record,
        IReadOnlyCollection<AccessNotificationRecord> notificationRecords)
    {
        var props = new UserDocumentProps(
            IdValueObject.Load(record.Id),
            UserId.Load(record.UserId),
            DocumentTypeId.Load(record.DocumentTypeId),
            record.IssueDate,
            record.ExpirationDate,
            DomainEnumerationMapper.FromValue<DocumentCriticity>(record.CriticityId),
            TextValueObject.Create(record.FileStoragePath),
            record.FileChecksum,
            ActorId.Create(record.CreatedBy));

        // Restore status (bypasses constructor invariant that always sets PendingReview)
        var statusProp = props.GetType().GetProperty(nameof(UserDocumentProps.Status), InstanceFlags)!;
        statusProp.SetValue(props, DomainEnumerationMapper.FromValue<DocumentStatus>(record.StatusId));

        var notificationStepProp = props.GetType().GetProperty(nameof(UserDocumentProps.NotificationStep), InstanceFlags)!;
        notificationStepProp.SetValue(props, record.NotificationStep);

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        var document = Construct<UserDocumentAggregate, UserDocumentProps>(props);

        var notifications = notificationRecords.Select(RehydrateAccessNotification).ToList();
        SetField(document, "_notifications", notifications);

        document.DomainEvents.MarkChangesAsCommitted();
        document.BrokenRules.Clear();

        return document;
    }

    private static AccessNotificationEntity RehydrateAccessNotification(AccessNotificationRecord record)
    {
        var props = new AccessNotificationProps(
            IdValueObject.Load(record.Id),
            record.Step,
            DomainEnumerationMapper.FromValue<NotificationChannel>(record.ChannelId),
            record.DaysRemaining);

        // Restore persisted SentAt (not the auto-set UtcNow)
        var sentAtProp = props.GetType().GetProperty(nameof(AccessNotificationProps.SentAt), InstanceFlags)!;
        sentAtProp.SetValue(props, record.SentAt);

        return Construct<AccessNotificationEntity, AccessNotificationProps>(props);
    }

    public static AccessEnforcementPolicyAggregate RehydrateAccessEnforcementPolicy(AccessEnforcementPolicyRecord record)
    {
        var props = new AccessEnforcementPolicyProps(
            IdValueObject.Load(record.Id),
            TenantId.Load(record.TenantId),
            record.ProfileId.HasValue ? ProfileId.Load(record.ProfileId.Value) : null,
            record.RoleId.HasValue ? RoleId.Load(record.RoleId.Value) : null,
            DomainEnumerationMapper.FromValue<AccessEnforcementAction>(record.EnforcementActionId),
            record.IsActive,
            ActorId.Create(record.CreatedBy));

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        var policy = Construct<AccessEnforcementPolicyAggregate, AccessEnforcementPolicyProps>(props);
        policy.BrokenRules.Clear();

        return policy;
    }

    private static TEntity Construct<TEntity, TProps>(TProps props)
        where TEntity : class
        where TProps : class
    {
        var ctor = typeof(TEntity).GetConstructor(InstanceFlags, null, [typeof(TProps)], null)
            ?? throw new InvalidOperationException($"Constructor for {typeof(TEntity).Name} not found.");

        return (TEntity)ctor.Invoke([props]);
    }

    private static void SetField<TTarget>(object target, string fieldName, TTarget value)
    {
        var field = target.GetType().GetField(fieldName, InstanceFlags)
            ?? throw new InvalidOperationException($"Field {fieldName} not found on {target.GetType().Name}.");

        field.SetValue(target, value);
    }

    private static void SetAudit(object props, string createdBy, DateTime createdAtUtc, string? updatedBy, DateTime? updatedAtUtc, string auditTimeSpan)
    {
        var property = props.GetType().GetProperty("Audit", InstanceFlags)
            ?? throw new InvalidOperationException($"Audit property not found on {props.GetType().Name}.");

        property.SetValue(props, AuditValueObject.Load(new AuditProps
        {
            CreatedBy = createdBy,
            CreatedAt = createdAtUtc,
            UpdatedBy = updatedBy,
            UpdatedAt = updatedAtUtc,
            TimeSpan = auditTimeSpan,
        }));
    }
}
