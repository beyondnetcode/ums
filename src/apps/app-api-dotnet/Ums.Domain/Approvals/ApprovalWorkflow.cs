namespace Ums.Domain.Approvals;

public sealed class ApprovalWorkflow : ParametricCatalogEntity<ApprovalWorkflow, ApprovalWorkflowProps>
{
    private ApprovalWorkflow(ApprovalWorkflowProps props) : base(props) { }

    public string RequestType => Props.RequestType.GetValue();
    public int RequiredApprovals => Props.RequiredApprovals.GetValue();
    public LifecycleStatus Status => Props.Status;

    public static Result<ApprovalWorkflow> Create(Guid tenantId, string code, string value, string description, string requestType, int requiredApprovals, string version = "1.0.0")
    {
        var validator = new ValidatorRuleManager<IRuleValidator>();

        validator.Add(new RequestTypeRequiredRule(requestType));
        validator.Add(new MinimumApprovalsRule(requiredApprovals));

        var broken = validator.GetBrokenRules();
        if (broken.Any())
        {
            var brokenRules = new BrokenRulesManager();
            brokenRules.Add(broken);
            return Result<ApprovalWorkflow>.Failure(brokenRules.GetBrokenRulesAsString());
        }

        var props = new ApprovalWorkflowProps
        {
            RequestType = RequestTypeVO.Create(requestType),
            RequiredApprovals = RequiredApprovalsVO.Create(requiredApprovals)
        };

        var workflow = new ApprovalWorkflow(props);
        workflow.SetCatalogFields(tenantId, code, value, description, version);
        
        return Result<ApprovalWorkflow>.Success(workflow);
    }
}
