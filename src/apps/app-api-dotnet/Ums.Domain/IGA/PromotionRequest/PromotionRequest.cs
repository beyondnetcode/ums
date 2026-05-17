namespace Ums.Domain.IGA.PromotionRequest;

using Ums.Domain.IGA.PromotionRequest.PromotionImpactAnalysis;
using PromotionImpactAnalysisEntity = Ums.Domain.IGA.PromotionRequest.PromotionImpactAnalysis.PromotionImpactAnalysis;

public sealed class PromotionRequest : AggregateRoot<PromotionRequest, PromotionRequestProps>
{
    private readonly List<PromotionImpactAnalysisEntity> _impactAnalyses = new();

    public PromotionRequestId GetId() => PromotionRequestId.Load(Props.Id.GetValue());

    public TenantId TenantId => Props.TenantId;
    public UserId UserId => Props.UserId;
    public RoleId CurrentRoleId => Props.CurrentRoleId;
    public RoleId TargetRoleId => Props.TargetRoleId;
    public DateTime RequestedAt => Props.RequestedAt;
    public ActorId RequestedBy => Props.RequestedBy;
    public TextValueObject? RequestReason => Props.RequestReason;
    public UserId ManagerId => Props.ManagerId;
    public ApprovalDecision ManagerApprovalStatus => Props.ManagerApprovalStatus;
    public DateTime? ManagerDecisionAt => Props.ManagerDecisionAt;
    public ApprovalDecision SecurityApprovalStatus => Props.SecurityApprovalStatus;
    public DateTime? SecurityDecisionAt => Props.SecurityDecisionAt;
    public PromotionStatus Status => Props.Status;
    public DateTime? ExecutedAt => Props.ExecutedAt;
    public ActorId? ExecutedBy => Props.ExecutedBy;
    public DateTime? VerifiedAt => Props.VerifiedAt;

    public IReadOnlyCollection<PromotionImpactAnalysisEntity> ImpactAnalyses => _impactAnalyses.AsReadOnly();

    private PromotionRequest(PromotionRequestProps props) : base(props)
    {
    }

    public static Result<PromotionRequest> Create(
        TenantId tenantId,
        UserId userId,
        RoleId currentRoleId,
        RoleId targetRoleId,
        UserId managerId,
        TextValueObject? requestReason,
        ActorId requestedBy)
    {
        var props = new PromotionRequestProps(IdValueObject.Create(), tenantId, userId, currentRoleId, targetRoleId, managerId, requestedBy)
        {
            RequestReason = requestReason
        };
        var request = new PromotionRequest(props);

        if (!request.IsValid())
        {
            return Result<PromotionRequest>.Failure(request.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<PromotionRequest>.Success(request);
    }

    public Result Submit(ActorId submittedBy)
    {
        if (Status != PromotionStatus.Draft)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.IGA.PromotionNotInDraft));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = PromotionStatus.PendingManagerApproval;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(submittedBy.GetValue());
        return Result.Success();
    }

    public Result ManagerApprove(ActorId managerId)
    {
        if (Status != PromotionStatus.PendingManagerApproval)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.IGA.PromotionNotPendingManager));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.ManagerApprovalStatus = ApprovalDecision.Approved;
        Props.ManagerDecisionAt = DateTime.UtcNow;
        Props.Status = PromotionStatus.PendingSecurityReview;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(managerId.GetValue());
        return Result.Success();
    }

    public Result ManagerReject(ActorId managerId)
    {
        if (Status != PromotionStatus.PendingManagerApproval)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.IGA.PromotionNotPendingManager));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.ManagerApprovalStatus = ApprovalDecision.Rejected;
        Props.ManagerDecisionAt = DateTime.UtcNow;
        Props.Status = PromotionStatus.Rejected;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(managerId.GetValue());
        return Result.Success();
    }

    public Result SecurityReviewLowRisk(ActorId securityId)
    {
        if (Status != PromotionStatus.PendingSecurityReview)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.IGA.PromotionNotPendingSecurity));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.SecurityApprovalStatus = ApprovalDecision.Approved;
        Props.SecurityDecisionAt = DateTime.UtcNow;
        Props.Status = PromotionStatus.ApprovedReadyToExecute;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(securityId.GetValue());
        return Result.Success();
    }

    public Result SecurityReviewHighRisk(ActorId securityId)
    {
        if (Status != PromotionStatus.PendingSecurityReview)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.IGA.PromotionNotPendingSecurity));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = PromotionStatus.PendingSecurityApproval;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(securityId.GetValue());
        return Result.Success();
    }

    public Result SecurityApprove(ActorId securityId)
    {
        if (Status != PromotionStatus.PendingSecurityApproval)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.IGA.PromotionNotPendingSecurity));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.SecurityApprovalStatus = ApprovalDecision.Approved;
        Props.SecurityDecisionAt = DateTime.UtcNow;
        Props.Status = PromotionStatus.ApprovedReadyToExecute;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(securityId.GetValue());
        return Result.Success();
    }

    public Result SecurityReject(ActorId securityId)
    {
        if (Status != PromotionStatus.PendingSecurityApproval && Status != PromotionStatus.PendingSecurityReview)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.IGA.PromotionNotPendingSecurity));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.SecurityApprovalStatus = ApprovalDecision.Rejected;
        Props.SecurityDecisionAt = DateTime.UtcNow;
        Props.Status = PromotionStatus.Rejected;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(securityId.GetValue());
        return Result.Success();
    }

    public Result Execute(ActorId executedBy)
    {
        if (Status != PromotionStatus.ApprovedReadyToExecute)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.IGA.PromotionNotApproved));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = PromotionStatus.Executed;
        Props.ExecutedAt = DateTime.UtcNow;
        Props.ExecutedBy = executedBy;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(executedBy.GetValue());
        return Result.Success();
    }

    public Result Verify(ActorId verifiedBy)
    {
        if (Status != PromotionStatus.Executed)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.IGA.PromotionAlreadyExecuted));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = PromotionStatus.Verified;
        Props.VerifiedAt = DateTime.UtcNow;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(verifiedBy.GetValue());
        return Result.Success();
    }

    public Result MarkVerificationFailed(ActorId verifiedBy)
    {
        if (Status != PromotionStatus.Executed)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.IGA.PromotionAlreadyExecuted));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = PromotionStatus.VerificationFailed;
        Props.VerifiedAt = DateTime.UtcNow;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(verifiedBy.GetValue());
        return Result.Success();
    }

    public Result AddImpactAnalysis(
        decimal riskScore,
        TextValueObject riskLevel,
        int newPermissionsCount,
        int removedPermissionsCount,
        int affectedSystemsCount,
        TextValueObject? conflictingPermissions,
        TextValueObject? riskFactors,
        TextValueObject? suggestedMitigations,
        ActorId analyzedBy)
    {
        if (_impactAnalyses.Count > 0)
        {
            BrokenRules.Add(new BrokenRule(nameof(ImpactAnalyses), DomainErrors.IGA.ImpactAnalysisAlreadyExists));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        var analysisResult = PromotionImpactAnalysisEntity.Create(
            GetId(),
            riskScore,
            riskLevel,
            newPermissionsCount,
            removedPermissionsCount,
            affectedSystemsCount,
            conflictingPermissions,
            riskFactors,
            suggestedMitigations,
            analyzedBy,
            analyzedBy);

        if (analysisResult.IsFailure)
        {
            return Result.Failure(analysisResult.Error);
        }

        _impactAnalyses.Add(analysisResult.Value);
        TrackingState.MarkAsDirty();
        Props.Audit.Update(analyzedBy.GetValue());
        return Result.Success();
    }
}
