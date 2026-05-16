namespace Ums.Domain.Approvals;

public sealed class ApprovalRequest : AggregateRoot<ApprovalRequest, ApprovalRequestProps>
{
    private ApprovalRequest(ApprovalRequestProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public Guid WorkflowId => Props.WorkflowId.GetValue();
    public string RequestType => Props.RequestType.GetValue();
    public string Justification => Props.Justification.GetValue();
    public ApprovalRequestStatus Status => Props.Status;

    public static Result<ApprovalRequest> Submit(Guid tenantId, Guid workflowId, Guid requestedBy, string requestType, string justification, string payload)
    {
        var validator = new ValidatorRuleManager<IRuleValidator>();

        validator.Add(new RequestTypeRequiredRule(requestType));
        validator.Add(new JustificationRequiredRule(justification));

        var broken = validator.GetBrokenRules();
        if (broken.Any())
        {
            var brokenRules = new BrokenRulesManager();
            brokenRules.Add(broken);
            return Result<ApprovalRequest>.Failure(brokenRules.GetBrokenRulesAsString());
        }

        var props = new ApprovalRequestProps(
            IdValueObject.Create(),
            TenantIdVO.Load(tenantId),
            WorkflowIdVO.Load(workflowId),
            IdValueObject.Load(requestedBy),
            RequestTypeVO.Create(requestType),
            JustificationVO.Create(justification),
            global::Ums.Domain.Authorization.ValueObjects.Payload.Create(string.IsNullOrWhiteSpace(payload) ? "{}" : payload.Trim()));

        var request = new ApprovalRequest(props);
        request.DomainEvents.ApplyChange(new ApprovalRequestedEvent(tenantId, request.Props.Id.GetValue(), requestType));

        return Result<ApprovalRequest>.Success(request);
    }

    public Result Resolve(Guid resolvedBy, ApprovalDecision decision, string comment)
    {
        var validator = new ValidatorRuleManager<IRuleValidator>();

        validator.Add(new ResolutionCommentRequiredRule(comment));

        var broken = validator.GetBrokenRules();
        if (broken.Any())
        {
            var brokenRules = new BrokenRulesManager();
            brokenRules.Add(broken);
            return Result.Failure(brokenRules.GetBrokenRulesAsString());
        }

        Props.ResolvedBy = IdValueObject.Load(resolvedBy);
        Props.ResolutionComment = CommentVO.Create(comment);
        
        if (decision == ApprovalDecision.Approved)
            Props.Status = ApprovalRequestStatus.Approved;
        else
            Props.Status = ApprovalRequestStatus.Rejected;

        TrackingState.MarkAsDirty();
        Props.Audit.Update("system");

        DomainEvents.ApplyChange(new ApprovalCompletedEvent(Props.TenantId.GetValue(), Props.Id.GetValue(), Props.Status.ToString()));

        return Result.Success();
    }
}
