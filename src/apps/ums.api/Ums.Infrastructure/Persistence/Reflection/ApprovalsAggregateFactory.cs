using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Ums.Domain.Approvals.ApprovalWorkflow;
using Ums.Domain.Approvals.ApprovalWorkflow.ApprovalRequiredDocument;
using Ums.Domain.Approvals.ApprovalRequest;
using Ums.Domain.Approvals.NotificationRule;
using Ums.Domain.Enums;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Infrastructure.Persistence.Approvals.Entities;
using Ums.Shell.Ddd;
using Ums.Shell.Ddd.ValueObjects.Audit;

namespace Ums.Infrastructure.Persistence.Reflection;

using ApprovalWorkflowAggregate = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalWorkflow;
using ApprovalRequiredDocumentEntity = Ums.Domain.Approvals.ApprovalWorkflow.ApprovalRequiredDocument.ApprovalRequiredDocument;
using ApprovalRequestAggregate = Ums.Domain.Approvals.ApprovalRequest.ApprovalRequest;
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
