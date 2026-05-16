namespace Ums.Domain.Approvals.Rules;

public class ExternalAccessIdentifiersRequiredRule : AbstractRuleValidator<object>
{
    private readonly Guid _tenantId;
    private readonly Guid _sponsorUserId;
    private readonly Guid _targetOrganizationId;
    private readonly Guid _requestedProfileId;

    public ExternalAccessIdentifiersRequiredRule(Guid tenantId, Guid sponsorUserId, Guid targetOrganizationId, Guid requestedProfileId) : base(new object())
    {
        _tenantId = tenantId;
        _sponsorUserId = sponsorUserId;
        _targetOrganizationId = targetOrganizationId;
        _requestedProfileId = requestedProfileId;
    }

    public override void AddRules(RuleContext? context)
    {
        if (_tenantId == Guid.Empty || _sponsorUserId == Guid.Empty || _targetOrganizationId == Guid.Empty || _requestedProfileId == Guid.Empty)
        {
            AddBrokenRule("Identifiers", "Tenant, sponsor, target organization, and requested profile identifiers are required.");
        }
    }
}
