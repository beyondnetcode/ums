namespace Ums.Domain.Approvals.ValueObjects;

public class WorkflowId : IdValueObject
{
    private WorkflowId(Guid value) : base(value) { }
    public static new WorkflowId Create() => new WorkflowId(Guid.NewGuid());
    public static new WorkflowId Load(Guid value) => new WorkflowId(value);
    public static new WorkflowId Load(string value) => new WorkflowId(Guid.Parse(value));
}
