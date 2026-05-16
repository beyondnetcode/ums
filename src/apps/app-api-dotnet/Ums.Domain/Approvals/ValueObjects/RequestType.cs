namespace Ums.Domain.Approvals.ValueObjects;

public class RequestType : StringValueObject
{
    private RequestType(string value) : base(value) { }
    public static RequestType Create(string value) => new RequestType(value?.Trim() ?? string.Empty);
    public static RequestType Default() => new RequestType(string.Empty);

    public override void AddValidators()
    {
        base.AddValidators();
        if (string.IsNullOrWhiteSpace(GetValue()))
        {
            BrokenRules.Add(new BrokenRule(nameof(RequestType), "Request type is required."));
        }
    }
}
