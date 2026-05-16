namespace Ums.Domain.IGA;

using Ums.Domain.Kernel;

using Ums.Domain.Common;
using Ums.Domain.Enums;
using Ums.Domain.Events;
using Ums.Shell.Ddd;
using Ums.Domain.Kernel.ValueObjects;
using Ums.Shell.Ddd.Interfaces;
using Ums.Shell.Ddd.ValueObjects.Common;

public class RolePromotionCriteriaProps : ParametricCatalogProps
{
    public global::Ums.Domain.Authorization.ValueObjects.RoleId SourceRoleId { get; set; } = default!;
    public global::Ums.Domain.Authorization.ValueObjects.RoleId TargetRoleId { get; set; } = default!;
    public StringValueObject EvaluationExpression { get; set; } = default!;

    public RolePromotionCriteriaProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}

public sealed class RolePromotionCriteria : ParametricCatalogEntity<RolePromotionCriteria, RolePromotionCriteriaProps>
{
    private RolePromotionCriteria(RolePromotionCriteriaProps props) : base(props) { }

    public Guid SourceRoleId => Props.SourceRoleId.GetValue();
    public Guid TargetRoleId => Props.TargetRoleId.GetValue();
    public string EvaluationExpression => Props.EvaluationExpression.GetValue();

    public static Result<RolePromotionCriteria> Create(
        Guid tenantId,
        string code,
        string value,
        string description,
        Guid sourceRoleId,
        Guid targetRoleId,
        string evaluationExpression,
        string version = "1.0.0")
    {
        if (sourceRoleId == Guid.Empty || targetRoleId == Guid.Empty)
            return Result<RolePromotionCriteria>.Failure("Source and target roles are required.");

        if (sourceRoleId == targetRoleId)
            return Result<RolePromotionCriteria>.Failure("Source and target roles must be different.");

        if (string.IsNullOrWhiteSpace(evaluationExpression))
            return Result<RolePromotionCriteria>.Failure("Evaluation expression is required.");

        var props = new RolePromotionCriteriaProps
        {
            SourceRoleId = global::Ums.Domain.Authorization.ValueObjects.RoleId.Load(sourceRoleId),
            TargetRoleId = global::Ums.Domain.Authorization.ValueObjects.RoleId.Load(targetRoleId),
            EvaluationExpression = global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(evaluationExpression.Trim())
        };

        var criteria = new RolePromotionCriteria(props);
        var result = criteria.SetCatalogFields(tenantId, code, value, description, version);
        
        return result.IsFailure ? Result<RolePromotionCriteria>.Failure(result.Error) : Result<RolePromotionCriteria>.Success(criteria);
    }
}

public class UserPromotionProcessProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.UserId UserId { get; private set; }
    public global::Ums.Domain.Iga.ValueObjects.CriteriaId CriteriaId { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.RoleId TargetRoleId { get; private set; }
    public IdValueObject? ApprovalRequestId { get; set; }
    public UserPromotionStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public UserPromotionProcessProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.UserId userId, global::Ums.Domain.Iga.ValueObjects.CriteriaId criteriaId, global::Ums.Domain.Authorization.ValueObjects.RoleId targetRoleId)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        CriteriaId = criteriaId;
        TargetRoleId = targetRoleId;
        Status = UserPromotionStatus.Evaluating;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

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

public class UserManagementDelegationProps : IProps
{
    public IdValueObject Id { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.TenantId TenantId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.UserId DelegatorUserId { get; private set; }
    public global::Ums.Domain.Kernel.ValueObjects.UserId DelegateUserId { get; private set; }
    public DateTimeOffset EffectiveFrom { get; private set; }
    public DateTimeOffset? EffectiveTo { get; private set; }
    public StringValueObject Scope { get; private set; }
    public DelegationStatus Status { get; set; }
    public AuditValueObject Audit { get; private set; }

    public UserManagementDelegationProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.UserId delegatorUserId, global::Ums.Domain.Kernel.ValueObjects.UserId delegateUserId, DateRange effectiveRange, StringValueObject scope)
    {
        Id = id;
        TenantId = tenantId;
        DelegatorUserId = delegatorUserId;
        DelegateUserId = delegateUserId;
        EffectiveFrom = effectiveRange.StartsAt;
        EffectiveTo = effectiveRange.EndsAt;
        Scope = scope;
        Status = DelegationStatus.Active;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}

public sealed class UserManagementDelegation : AggregateRoot<UserManagementDelegation, UserManagementDelegationProps>
{
    private UserManagementDelegation(UserManagementDelegationProps props) : base(props)
    {
        if (TrackingState.IsNew)
        {
            DomainEvents.ApplyChange(new DelegationGrantedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.DelegateUserId.GetValue()), true);
        }
    }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid DelegatorUserId => Props.DelegatorUserId.GetValue();
    public Guid DelegateUserId => Props.DelegateUserId.GetValue();
    public DateTimeOffset EffectiveFrom => Props.EffectiveFrom;
    public DateTimeOffset? EffectiveTo => Props.EffectiveTo;
    public string Scope => Props.Scope.GetValue();
    public DelegationStatus Status => Props.Status;

    public static Result<UserManagementDelegation> Grant(Guid tenantId, Guid delegatorUserId, Guid delegateUserId, DateRange effectiveRange, string scope)
    {
        if (tenantId == Guid.Empty || delegatorUserId == Guid.Empty || delegateUserId == Guid.Empty)
            return Result<UserManagementDelegation>.Failure("Tenant, delegator, and delegate identifiers are required.");

        if (delegatorUserId == delegateUserId)
            return Result<UserManagementDelegation>.Failure("Delegator and delegate must be different.");

        if (string.IsNullOrWhiteSpace(scope))
            return Result<UserManagementDelegation>.Failure("Delegation scope is required.");

        var props = new UserManagementDelegationProps(
            IdValueObject.Create(),
            global::Ums.Domain.Kernel.ValueObjects.TenantId.Load(tenantId),
            global::Ums.Domain.Kernel.ValueObjects.UserId.Load(delegatorUserId),
            global::Ums.Domain.Kernel.ValueObjects.UserId.Load(delegateUserId),
            effectiveRange,
            global::Ums.Domain.Kernel.ValueObjects.TextValueObject.Create(scope.Trim()));

        var delegation = new UserManagementDelegation(props);
        return Result<UserManagementDelegation>.Success(delegation);
    }

    public void Revoke()
    {
        Props.Status = DelegationStatus.Revoked;
        Props.Audit.Update("system");
    }
    
    public Guid GetId() => Props.Id.GetValue();
}

