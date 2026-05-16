namespace Ums.Domain.Approvals.ValueObjects;

using Ums.Domain.Kernel;

public class RequiredApprovals : IntValueObject
{
    private RequiredApprovals(int value) : base(value) { }
    public static RequiredApprovals Create(int value) => new RequiredApprovals(value);
    public static RequiredApprovals Default() => new RequiredApprovals(1);

    public override void AddValidators()
    {
        base.AddValidators();
        if (GetValue() < 1)
        {
            BrokenRules.Add(new BrokenRule(nameof(RequiredApprovals), DomainErrors.Approval.MinimumApprovalsRequired));
        }
    }
}
