namespace Ums.Domain.Approvals;

public class ExternalAccessRequestProps : IProps
{
    public IdValueObject Id { get; private set; }
    public TenantIdVO TenantId { get; private set; }
    public UserIdVO SponsorUserId { get; private set; }
    public OrganizationIdVO TargetOrganizationId { get; private set; }
    public EmailAddress TargetUserEmail { get; private set; }
    public global::Ums.Domain.Authorization.ValueObjects.ProfileId RequestedProfileId { get; private set; }
    public JustificationVO Justification { get; private set; }
    public ApprovalRequestStatus Status { get; set; }
    public IdValueObject? ApprovalRequestId { get; set; }
    public AuditValueObject Audit { get; private set; }

    public ExternalAccessRequestProps(IdValueObject id, TenantIdVO tenantId, UserIdVO sponsorUserId, OrganizationIdVO targetOrganizationId, EmailAddress targetUserEmail, global::Ums.Domain.Authorization.ValueObjects.ProfileId requestedProfileId, JustificationVO justification)
    {
        Id = id;
        TenantId = tenantId;
        SponsorUserId = sponsorUserId;
        TargetOrganizationId = targetOrganizationId;
        TargetUserEmail = targetUserEmail;
        RequestedProfileId = requestedProfileId;
        Justification = justification;
        Status = ApprovalRequestStatus.Pending;
        Audit = AuditValueObject.Create("system");
    }

    public object Clone() => MemberwiseClone();
}
