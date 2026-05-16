namespace Ums.Domain.IGA;

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

    public UserPromotionProcessProps(IdValueObject id, global::Ums.Domain.Kernel.ValueObjects.TenantId tenantId, global::Ums.Domain.Kernel.ValueObjects.UserId userId, global::Ums.Domain.Iga.ValueObjects.CriteriaId criteriaId, global::Ums.Domain.Authorization.ValueObjects.RoleId targetRoleId, string createdBy)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        CriteriaId = criteriaId;
        TargetRoleId = targetRoleId;
        Status = UserPromotionStatus.Evaluating;
        Audit = AuditValueObject.Create(createdBy);
    }

    public object Clone()
    {
        return this.MemberwiseClone();
    }
}
