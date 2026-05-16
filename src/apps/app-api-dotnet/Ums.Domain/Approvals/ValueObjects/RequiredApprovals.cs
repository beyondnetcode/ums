namespace Ums.Domain.Approvals.ValueObjects;

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
            BrokenRules.Add(new BrokenRule(nameof(RequiredApprovals), "Required approvals must be at least 1."));
        }
    }
}
