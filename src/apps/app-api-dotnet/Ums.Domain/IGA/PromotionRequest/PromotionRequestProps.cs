namespace Ums.Domain.IGA.PromotionRequest;

using Ums.Domain.IGA.PromotionRequest.PromotionImpactAnalysis;
using PromotionImpactAnalysisEntity = Ums.Domain.IGA.PromotionRequest.PromotionImpactAnalysis.PromotionImpactAnalysis;

public class PromotionRequestProps : IProps
{
    public IdValueObject Id { get; set; }
    public TenantId TenantId { get; set; }
    public UserId UserId { get; set; }
    public RoleId CurrentRoleId { get; set; }
    public RoleId TargetRoleId { get; set; }
    public DateTime RequestedAt { get; set; }
    public ActorId RequestedBy { get; set; }
    public TextValueObject? RequestReason { get; set; }
    public UserId ManagerId { get; set; }
    public ApprovalDecision ManagerApprovalStatus { get; set; }
    public DateTime? ManagerDecisionAt { get; set; }
    public ApprovalDecision SecurityApprovalStatus { get; set; }
    public DateTime? SecurityDecisionAt { get; set; }
    public PromotionStatus Status { get; set; }
    public DateTime? ExecutedAt { get; set; }
    public ActorId? ExecutedBy { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public AuditValueObject Audit { get; private set; }

    public PromotionRequestProps(
        IdValueObject id,
        TenantId tenantId,
        UserId userId,
        RoleId currentRoleId,
        RoleId targetRoleId,
        UserId managerId,
        ActorId requestedBy)
    {
        Id = id;
        TenantId = tenantId;
        UserId = userId;
        CurrentRoleId = currentRoleId;
        TargetRoleId = targetRoleId;
        RequestedAt = DateTime.UtcNow;
        RequestedBy = requestedBy;
        ManagerId = managerId;
        Status = PromotionStatus.Draft;
        Audit = AuditValueObject.Create(requestedBy.GetValue());
    }

    public object Clone() => MemberwiseClone();
}
