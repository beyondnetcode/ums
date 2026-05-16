using Ums.Shell.Ddd.ValueObjects.Common;
namespace Ums.Domain.Approvals.ValueObjects;

using Ums.Shell.Ddd;

public class WorkflowId : IdValueObject
{
    private WorkflowId(Guid value) : base(value) { }
    public static new WorkflowId Create() => new WorkflowId(Guid.NewGuid());
    public static new WorkflowId Load(Guid value) => new WorkflowId(value);
    public static new WorkflowId Load(string value) => new WorkflowId(Guid.Parse(value));
}

public class ApprovalRequestId : IdValueObject
{
    private ApprovalRequestId(Guid value) : base(value) { }
    public static new ApprovalRequestId Create() => new ApprovalRequestId(Guid.NewGuid());
    public static new ApprovalRequestId Load(Guid value) => new ApprovalRequestId(value);
    public static new ApprovalRequestId Load(string value) => new ApprovalRequestId(Guid.Parse(value));
}
