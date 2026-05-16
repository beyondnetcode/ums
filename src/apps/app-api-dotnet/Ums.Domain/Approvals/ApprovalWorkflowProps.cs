namespace Ums.Domain.Approvals;

public class ApprovalWorkflowProps : ParametricCatalogProps
{
    public RequestTypeVO RequestType { get; set; } = default!;
    public RequiredApprovalsVO RequiredApprovals { get; set; } = default!;
    public LifecycleStatus Status { get; set; } = LifecycleStatus.Draft;

    public ApprovalWorkflowProps()
    {
        Id = IdValueObject.Create();
        Audit = AuditValueObject.Create("system");
    }
}
