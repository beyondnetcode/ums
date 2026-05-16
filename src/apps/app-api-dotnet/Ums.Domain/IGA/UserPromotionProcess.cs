namespace Ums.Domain.IGA;

public sealed class UserPromotionProcess : AggregateRoot<UserPromotionProcess, UserPromotionProcessProps>
{
    private UserPromotionProcess(UserPromotionProcessProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new UserPromotionStartedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.UserId.GetValue()), true);
        }
    }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid UserId => Props.UserId.GetValue();
    public Guid CriteriaId => Props.CriteriaId.GetValue();
    public Guid TargetRoleId => Props.TargetRoleId.GetValue();
    public Guid? ApprovalRequestId => Props.ApprovalRequestId?.GetValue();
    public UserPromotionStatus Status => Props.Status;

    public static Result<UserPromotionProcess> Start(Guid tenantId, Guid userId, Guid criteriaId, Guid targetRoleId, string createdBy)
    {
        if (tenantId == Guid.Empty || userId == Guid.Empty || criteriaId == Guid.Empty || targetRoleId == Guid.Empty)
        {
            var brokenRules = new BrokenRulesManager();
            brokenRules.Add(new BrokenRule("Identifiers", DomainErrors.Iga.PromotionIdentifiersRequired));
            return Result<UserPromotionProcess>.Failure(brokenRules.GetBrokenRulesAsString());
        }

        var props = new UserPromotionProcessProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.UserId.Load(userId),
            global::Ums.Domain.Iga.ValueObjects.CriteriaId.Load(criteriaId),
            global::Ums.Domain.Authorization.ValueObjects.RoleId.Load(targetRoleId),
            createdBy);

        var process = new UserPromotionProcess(props);

        if (!process.IsValid())
        {
            return Result<UserPromotionProcess>.Failure(process.BrokenRules.GetBrokenRulesAsString());
        }

        return Result<UserPromotionProcess>.Success(process);
    }

    public Result MarkCriteriaMet(Guid approvalRequestId, string updatedBy)
    {
        if (approvalRequestId == Guid.Empty)
        {
            BrokenRules.Add(new BrokenRule(nameof(approvalRequestId), DomainErrors.Iga.ApprovalIdRequired));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.ApprovalRequestId = IdValueObject.Load(approvalRequestId);
        Props.Status = UserPromotionStatus.PendingApproval;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy);
        
        return Result.Success();
    }

    public Result Complete(string updatedBy)
    {
        if (Props.Status != UserPromotionStatus.PendingApproval && Props.Status != UserPromotionStatus.CriteriaMet)
        {
            BrokenRules.Add(new BrokenRule(nameof(Status), DomainErrors.Iga.PromotionOnlyCompleteAfterCriteria));
        }

        if (!IsValid())
        {
            return Result.Failure(BrokenRules.GetBrokenRulesAsString());
        }

        Props.Status = UserPromotionStatus.Promoted;
        TrackingState.MarkAsDirty();
        Props.Audit.Update(updatedBy);
        
        DomainEvents.ApplyChange(new UserPromotionCompletedEvent(Props.TenantId.GetValue(), GetId(), Props.UserId.GetValue(), Props.TargetRoleId.GetValue()), true);
        return Result.Success();
    }
    
    public Guid GetId() => Props.Id.GetValue();
}
