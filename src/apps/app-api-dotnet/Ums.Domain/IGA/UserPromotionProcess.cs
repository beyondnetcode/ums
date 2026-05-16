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

    public static Result<UserPromotionProcess> Start(Guid tenantId, Guid userId, Guid criteriaId, Guid targetRoleId)
    {
        if (tenantId == Guid.Empty || userId == Guid.Empty || criteriaId == Guid.Empty || targetRoleId == Guid.Empty)
            return Result<UserPromotionProcess>.Failure("Tenant, user, criteria, and target role identifiers are required.");

        var props = new UserPromotionProcessProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.UserId.Load(userId),
            global::Ums.Domain.Iga.ValueObjects.CriteriaId.Load(criteriaId),
            global::Ums.Domain.Authorization.ValueObjects.RoleId.Load(targetRoleId));

        var process = new UserPromotionProcess(props);
        return Result<UserPromotionProcess>.Success(process);
    }

    public Result MarkCriteriaMet(Guid approvalRequestId)
    {
        if (approvalRequestId == Guid.Empty)
            return Result.Failure("Approval request identifier is required.");

        Props.ApprovalRequestId = IdValueObject.Load(approvalRequestId);
        Props.Status = UserPromotionStatus.PendingApproval;
        Props.Audit.Update("system");
        
        return Result.Success();
    }

    public Result Complete()
    {
        if (Props.Status != UserPromotionStatus.PendingApproval && Props.Status != UserPromotionStatus.CriteriaMet)
            return Result.Failure("Promotion can only complete after criteria are met.");

        Props.Status = UserPromotionStatus.Promoted;
        Props.Audit.Update("system");
        
        DomainEvents.ApplyChange(new UserPromotionCompletedEvent(Props.TenantId.GetValue(), GetId(), Props.UserId.GetValue(), Props.TargetRoleId.GetValue()), true);
        return Result.Success();
    }
    
    public Guid GetId() => Props.Id.GetValue();
}
