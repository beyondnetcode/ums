namespace Ums.Domain.Approvals.Rules;

public class RequestTypeRequiredRule : AbstractRuleValidator<object>
{
    private readonly string _requestType;

    public RequestTypeRequiredRule(string requestType) : base(new object())
    {
        _requestType = requestType;
    }

    public override void AddRules(RuleContext? context)
    {
        if (string.IsNullOrWhiteSpace(_requestType))
        {
            AddBrokenRule("RequestType", "Request type is required.");
        }
    }
}
