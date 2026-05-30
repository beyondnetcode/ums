using System.Collections.Generic;
using System.Reflection;
using Ums.Domain.Enums;
using Ums.Domain.IGA.PromotionRequest;
using Ums.Domain.IGA.PromotionRequest.PromotionImpactAnalysis;
using Ums.Domain.IGA.RoleMaturityStatus;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Infrastructure.Persistence.IGA.Entities;
using BeyondNetCode.Shell.Ddd;
using BeyondNetCode.Shell.Ddd.ValueObjects.Audit;

namespace Ums.Infrastructure.Persistence.Reflection;

using PromotionRequestAggregate = Ums.Domain.IGA.PromotionRequest.PromotionRequest;
using RoleMaturityStatusAggregate = Ums.Domain.IGA.RoleMaturityStatus.RoleMaturityStatus;
using PromotionImpactAnalysisEntity = Ums.Domain.IGA.PromotionRequest.PromotionImpactAnalysis.PromotionImpactAnalysis;

internal static class IgaAggregateFactory
{
    private static readonly BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

    public static PromotionRequestAggregate RehydratePromotionRequest(
        PromotionRequestRecord record,
        IReadOnlyCollection<PromotionImpactAnalysisRecord> impactAnalysisRecords)
    {
        var props = new PromotionRequestProps(
            IdValueObject.Load(record.Id),
            TenantId.Load(record.TenantId),
            UserId.Load(record.UserId),
            RoleId.Load(record.CurrentRoleId),
            RoleId.Load(record.TargetRoleId),
            UserId.Load(record.ManagerId),
            ActorId.Create(record.RequestedBy))
        {
            RequestedAt = record.RequestedAt,
            RequestReason = record.RequestReason is not null ? TextValueObject.Create(record.RequestReason) : null,
            ManagerApprovalStatus = (ApprovalDecision)record.ManagerApprovalStatusId,
            ManagerDecisionAt = record.ManagerDecisionAt,
            SecurityApprovalStatus = (ApprovalDecision)record.SecurityApprovalStatusId,
            SecurityDecisionAt = record.SecurityDecisionAt,
            Status = (PromotionStatus)record.StatusId,
            ExecutedAt = record.ExecutedAt,
            ExecutedBy = record.ExecutedBy is not null ? ActorId.Create(record.ExecutedBy) : null,
            VerifiedAt = record.VerifiedAt,
        };

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        var aggregate = Construct<PromotionRequestAggregate, PromotionRequestProps>(props);
        var analyses = impactAnalysisRecords.Select(RehydrateImpactAnalysis).ToList();
        SetField(aggregate, "_impactAnalyses", analyses);

        aggregate.DomainEvents.MarkChangesAsCommitted();
        aggregate.BrokenRules.Clear();

        return aggregate;
    }

    private static PromotionImpactAnalysisEntity RehydrateImpactAnalysis(PromotionImpactAnalysisRecord record)
    {
        var props = new PromotionImpactAnalysisProps(
            IdValueObject.Load(record.Id),
            PromotionRequestId.Load(record.PromotionRequestId),
            ActorId.Create(record.CreatedBy))
        {
            RiskScore = record.RiskScore,
            RiskLevel = TextValueObject.Create(record.RiskLevel),
            NewPermissionsCount = record.NewPermissionsCount,
            RemovedPermissionsCount = record.RemovedPermissionsCount,
            AffectedSystemsCount = record.AffectedSystemsCount,
            ConflictingPermissions = record.ConflictingPermissions is not null ? TextValueObject.Create(record.ConflictingPermissions) : null,
            RiskFactors = record.RiskFactors is not null ? TextValueObject.Create(record.RiskFactors) : null,
            SuggestedMitigations = record.SuggestedMitigations is not null ? TextValueObject.Create(record.SuggestedMitigations) : null,
            AnalyzedAt = record.AnalyzedAt,
            AnalyzedBy = record.AnalyzedBy is not null ? TextValueObject.Create(record.AnalyzedBy) : null,
        };

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        return Construct<PromotionImpactAnalysisEntity, PromotionImpactAnalysisProps>(props);
    }

    public static RoleMaturityStatusAggregate RehydrateRoleMaturityStatus(RoleMaturityStatusRecord record)
    {
        var props = new RoleMaturityStatusProps(
            IdValueObject.Load(record.Id),
            TenantId.Load(record.TenantId),
            UserId.Load(record.UserId),
            RoleId.Load(record.RoleId),
            (RoleMaturityLevel)record.CurrentMaturityLevelId,
            record.AssignedAt,
            record.CurrentLevelSince,
            ActorId.Create(record.CreatedBy))
        {
            NextEligibleMaturityLevel = record.NextEligibleMaturityLevelId.HasValue
                ? (RoleMaturityLevel)record.NextEligibleMaturityLevelId.Value
                : null,
            EligibleForPromotionAt = record.EligibleForPromotionAt,
            CompletedCertificationsCount = record.CompletedCertificationsCount,
            CompletedTrainingsCount = record.CompletedTrainingsCount,
            PerformanceScore = record.PerformanceScore,
            HasNoComplianceIssues = record.HasNoComplianceIssues,
            BlockingFactor = record.BlockingFactor is not null ? TextValueObject.Create(record.BlockingFactor) : null,
            LastReviewedAt = record.LastReviewedAt,
        };

        SetAudit(props, record.CreatedBy, record.CreatedAtUtc, record.UpdatedBy, record.UpdatedAtUtc, record.AuditTimeSpan);

        var aggregate = Construct<RoleMaturityStatusAggregate, RoleMaturityStatusProps>(props);
        aggregate.DomainEvents.MarkChangesAsCommitted();
        aggregate.BrokenRules.Clear();

        return aggregate;
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
