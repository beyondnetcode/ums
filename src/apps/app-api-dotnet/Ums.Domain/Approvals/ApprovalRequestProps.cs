namespace Ums.Domain.Approvals;

public class ApprovalRequestProps : IProps
{
    public IdValueObject Id { get; private set; }
    public TenantIdVO TenantId { get; private set; }
    public WorkflowIdVO WorkflowId { get; private set; }
    public IdValueObject RequestedBy { get; private set; }
    public IdValueObject? ResolvedBy { get; set; }
    public RequestTypeVO RequestType { get; private set; }
    public JustificationVO Justification { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.Payload Payload { get; private set; }
    public ApprovalRequestStatus Status { get; set; }
    public CommentVO? ResolutionComment { get; set; }
    public AuditValueObject Audit { get; private set; }

    public ApprovalRequestProps(IdValueObject id, TenantIdVO tenantId, WorkflowIdVO workflowId, IdValueObject requestedBy, RequestTypeVO requestType, JustificationVO justification, global::Ums.Domain.Authorization.ValueObjects.Payload payload)
    {
        Id = id;
        TenantId = tenantId;
        WorkflowId = workflowId;
        RequestedBy = requestedBy;
        RequestType = requestType;
        Justification = justification;
        Payload = payload;
        Status = ApprovalRequestStatus.Pending;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone() => MemberwiseClone();
}
