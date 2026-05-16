namespace Ums.Domain.Approvals.Rules;

using Ums.Domain.Kernel;

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
            AddBrokenRule("RequestType", DomainErrors.Approval.RequestTypeRequired);
        }
    }
}
