namespace Ums.Domain.Approvals;

public sealed class ExternalAccessRequest : AggregateRoot<ExternalAccessRequest, ExternalAccessRequestProps>
{
    private ExternalAccessRequest(ExternalAccessRequestProps props) : base(props) { }

    public Guid TenantId => Props.TenantId.GetValue();
    public string Justification => Props.Justification.GetValue();
    public ApprovalRequestStatus Status => Props.Status;

    public static Result<ExternalAccessRequest> Draft(Guid tenantId, Guid sponsorUserId, Guid targetOrganizationId, string targetUserEmail, Guid requestedProfileId, string justification)
    {
        var validator = new ValidatorRuleManager<IRuleValidator>();

        validator.Add(new JustificationRequiredRule(justification));

        var targetEmailResult = EmailAddress.Create(targetUserEmail);
        if (targetEmailResult.IsFailure)
            return Result<ExternalAccessRequest>.Failure(targetEmailResult.Error);

        var broken = validator.GetBrokenRules();
        if (broken.Any())
        {
            var brokenRules = new BrokenRulesManager();
            brokenRules.Add(broken);
            return Result<ExternalAccessRequest>.Failure(brokenRules.GetBrokenRulesAsString());
        }

        var props = new ExternalAccessRequestProps(
            IdValueObject.Create(),
            TenantIdVO.Load(tenantId),
            UserIdVO.Load(sponsorUserId),
            OrganizationIdVO.Load(targetOrganizationId),
            targetEmailResult.Value,
            global::Ums.Domain.Authorization.ValueObjects.ProfileId.Load(requestedProfileId),
            JustificationVO.Create(justification));

        return Result<ExternalAccessRequest>.Success(new ExternalAccessRequest(props));
    }
}
